// Copyright © 2016 Henrik Steffen Gaﬂmann
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Deeplex.Utils.ObjectModel
{
    public interface IValidatedProperty
        : INotifyPropertyChanged,
            INotifyDataErrorInfo
    {
        bool IsValid { get; }

        bool IsDirty { get; }

        void AddError(string error);
        void AddErrors(IEnumerable<string> errors);
        void ClearErrors();
        void GetErrors();

        void Revert();
    }

    public sealed class ValidatedProperty<T> : ValidateableBase
        ,
        IValidatedProperty
    {
        private readonly Func<T, T> Clone;
        private bool mIsEnabled;
        private T mOriginal;
        private T mValue;
        private bool mValueHasBeenSet;

        public ValidatedProperty(Func<T, T> clone = null)
        {
            ErrorsChanged += (s, e) => RaisePropertyChanged("IsValid");

            if (clone != null)
            {
                Clone = clone;
            }
            else if (typeof(IDeepClonable<T>).IsAssignableFrom(typeof(T)))
            {
                Clone = obj => ((IDeepClonable<T>) obj).Clone();
            }
            else
            {
                Clone = obj => obj;
            }
        }

        public ValidatedProperty(T value, Func<T, T> clone = null)
            : this(clone)
        {
            Value = value;
        }

        public bool IsEnabled
        {
            get { return mIsEnabled; }
            set { SetProperty(ref mIsEnabled, value); }
        }

        public T Original
        {
            get { return mOriginal; }
            set
            {
                mValueHasBeenSet = true;
                SetProperty(ref mOriginal, value);
            }
        }

        public T Value
        {
            get { return mValue; }
            set
            {
                if (!mValueHasBeenSet)
                {
                    Original = Clone(value);
                }
                SetProperty(ref mValue, value);
                RaisePropertyChanged("IsDirty");
            }
        }

        public bool IsDirty
            => Equals(Value, Original);

        public bool IsValid
            => mValidationErrors.Count == 0;

        public void Revert()
            => Value = Original;

        public void AddError(string error)
            => AddError("Value", error);

        public void AddErrors(IEnumerable<string> errors)
            => AddErrors("Value", errors);

        public void ClearErrors()
            => ClearErrors("Value");

        public void GetErrors()
            => GetErrors("Value");

        public void ApplyChange()
            => Original = Value;


        public override string ToString()
            => Value?.ToString() ?? string.Empty;
    }
}