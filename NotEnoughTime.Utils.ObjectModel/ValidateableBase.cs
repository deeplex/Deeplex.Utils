using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NotEnoughTime.Utils.ObjectModel
{
    public abstract class ValidateableBase : BindableBase
        , INotifyDataErrorInfo
    {
        protected Dictionary<string, IList<string>> mValidationErrors
            = new Dictionary<string, IList<string>>();

        public bool HasErrors => mValidationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void RaiseErrorsChanged(DataErrorsChangedEventArgs args)
        {
            ErrorsChanged?.Invoke(this, args);
            RaisePropertyChanged("HasErrors");
        }

        protected void AddError(string propertyName, string error)
        {
            if (!mValidationErrors.TryGetValue(propertyName, out var errorList))
            {
                mValidationErrors.Add(propertyName, new List<string> { error });
            }
            else
            {
                errorList.Add(error);
            }
            RaiseErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        protected void AddErrors(string propertyName, IEnumerable<string> errors)
        {
            if (!mValidationErrors.TryGetValue(propertyName, out var errorList))
            {
                mValidationErrors.Add(propertyName, new List<string>(errors));
            }
            else
            {
                foreach (var error in errors)
                {
                    errorList.Add(error);
                }
            }
            RaiseErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors(string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                mValidationErrors.TryGetValue(propertyName, out var errors);
                errors?.Clear();
            }
            else
            {
                foreach (var errorList in mValidationErrors.Select(x => x.Value))
                {
                    errorList.Clear();
                }
            }
            RaiseErrorsChanged(new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                mValidationErrors.TryGetValue(propertyName, out var errors);
                return (errors?.Any() ?? false) ? errors : null;
            }
            else
            {
                return mValidationErrors.SelectMany(x => x.Value);
            }
        }
    }
}
