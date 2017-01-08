using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace NotEnoughTime.Utils.ObjectModel
{
    public interface IValidatedProperty
        : INotifyPropertyChanged
        , INotifyDataErrorInfo
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
        , IValidatedProperty
    {
        private readonly Func<T, T> Clone;
        private T mOriginal;
        private T mValue;
        private bool mIsEnabled;
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
                Clone = (obj) => ((IDeepClonable<T>)obj).Clone();
            }
            else
            {
                Clone = (obj) => obj;
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

        public void ApplyChange()
            => Original = Value;


        public override string ToString()
        {
            return Value?.ToString() ?? String.Empty;
        }

        public void AddError(string error)
            => AddError("Value", error);

        public void AddErrors(IEnumerable<string> errors)
            => AddErrors("Value", errors);

        public void ClearErrors()
            => ClearErrors("Value");

        public void GetErrors()
            => GetErrors("Value");
    }
}