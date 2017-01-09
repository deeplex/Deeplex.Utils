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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NotEnoughTime.Utils.ObjectModel
{
    public interface IValidatingViewModel : INotifyDataErrorInfo
    {
        Action<IValidatingViewModel> Validator { get; }

        bool IsValid { get; }

        bool IsDirty { get; }

        void Validate();
        void Revert();
    }

    public abstract class ValidatingViewModel<T> : ValidateableBase,
        IValidatingViewModel
        where T : IValidatingViewModel
    {
        private bool mIsDirty;
        private bool mIsValid = true;

        protected ValidatingViewModel()
        {
            var iPropType = typeof(IValidatedProperty);
            Properties = new ReadOnlyCollection<PropertyInfo>(
                typeof(T).GetRuntimeProperties()
                    .Where(prop => iPropType.IsAssignableFrom(prop.PropertyType))
                    .ToArray()
            );

            // whenever a property is changed, validate the model
            foreach (var property in ValidatedProperties)
            {
                property.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName.Equals("Value"))
                    {
                        Validate();
                    }
                };
            }
        }

        private IReadOnlyList<PropertyInfo> Properties { get; }

        protected IEnumerable<IValidatedProperty> ValidatedProperties
            => Properties.Select(x => x.GetValue(this) as IValidatedProperty)
                .Where(x => x != null);

        public bool IsValid
        {
            get { return mIsValid; }
            set { SetProperty(ref mIsValid, value); }
        }

        public bool IsDirty
        {
            get { return mIsDirty; }
            set { SetProperty(ref mIsDirty, value); }
        }

        public abstract Action<IValidatingViewModel> Validator { get; }

        public void Revert()
        {
            foreach (var property in ValidatedProperties)
            {
                property.Revert();
            }
        }

        public void Validate()
        {
            var properties = ValidatedProperties.ToArray();
            foreach (var property in ValidatedProperties)
            {
                property.ClearErrors();
            }
            ClearErrors("");
            Validator?.Invoke(this);
            IsDirty = properties.Any(x => x.IsDirty);
            IsValid = mValidationErrors.Count == 0 && properties.Any(x => !x.IsValid);
        }
    }
}