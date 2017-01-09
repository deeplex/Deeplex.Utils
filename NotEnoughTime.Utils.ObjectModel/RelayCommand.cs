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
using System.Reflection;
using System.Windows.Input;

namespace NotEnoughTime.Utils.ObjectModel
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> mCanExecute;
        private readonly Action<T> mExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            mExecute = execute ?? throw new ArgumentNullException(nameof(execute));
            mCanExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => (parameter is T
            || (!typeof(T).GetTypeInfo().IsValueType && parameter == null)) // if T isn't a value type, parameter may also equal null
            && (mCanExecute == null || mCanExecute((T)parameter));

        public void Execute(object parameter)
            => mExecute?.Invoke((T)parameter);

        public void RaiseCanExecuteChanged(EventArgs args)
            => CanExecuteChanged?.Invoke(this, args);
    }

    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            : base(execute, canExecute)
        {
        }
    }
}
