// Copyright © 2016 Henrik Steffen Gaßmann
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Deeplex.Utils.ObjectModel
{
    public abstract class ValidateableBase : BindableBase,
        INotifyDataErrorInfo
    {
        protected Dictionary<string, IList<string>> mValidationErrors
            = new Dictionary<string, IList<string>>();

        public bool HasErrors => mValidationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        protected void RaiseErrorsChanged(DataErrorsChangedEventArgs args)
        {
            ErrorsChanged?.Invoke(this, args);
            RaisePropertyChanged("HasErrors");
        }

        protected void AddError(string propertyName, string error)
        {
            if (!mValidationErrors.TryGetValue(propertyName, out var errorList))
            {
                mValidationErrors.Add(propertyName, new List<string>
                {
                    error
                });
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
    }
}