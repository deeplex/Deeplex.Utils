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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Deeplex.Utils.ObjectModel
{
    public class ObservableDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>,
            IReadOnlyDictionary<TKey, TValue>,
            INotifyCollectionChanged,
            INotifyPropertyChanged
    {
        private readonly Dictionary<TKey, TValue> mImpl
            = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get { return mImpl[key]; }
            set
            {
                var replaced = mImpl.TryGetValue(key, out var oldValue);

                mImpl[key] = value;

                NotifyCollectionChangedEventArgs args;
                if (replaced)
                {
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                        new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, oldValue));
                }
                else
                {
                    args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                        new KeyValuePair<TKey, TValue>(key, value));
                }
                RaiseCollectionChanged(args);
            }
        }

        public int Count => mImpl.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((IDictionary<TKey, TValue>) mImpl).IsReadOnly;

        public ICollection<TKey> Keys => mImpl.Keys;

        public ICollection<TValue> Values => mImpl.Values;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            mImpl.Add(item.Key, item.Value);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Add(TKey key, TValue value)
        {
            mImpl.Add(key, value);

            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                new KeyValuePair<TKey, TValue>(key, value)));
        }

        public void Clear()
        {
            mImpl.Clear();
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [SuppressMessage("Microsoft.Design", "CA1033",
            Justification = "ContainsKey should be used instead of this.")]
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
            => ((ICollection<KeyValuePair<TKey, TValue>>) mImpl).Contains(item);

        public bool ContainsKey(TKey key)
            => mImpl.ContainsKey(key);

        [SuppressMessage("Microsoft.Design", "CA1033",
            Justification = "This method isn't implemented by the standard dictionary.")]
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => ((ICollection<KeyValuePair<TKey, TValue>>) mImpl).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            => mImpl.GetEnumerator();

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
            => Remove(item.Key);

        public bool Remove(TKey key)
        {
            mImpl.TryGetValue(key, out var value);

            var removed = mImpl.Remove(key);
            if (removed)
            {
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                    new KeyValuePair<TKey, TValue>(key, value)));
            }
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
            => mImpl.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable) mImpl).GetEnumerator();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
            => ((IReadOnlyDictionary<TKey, TValue>) mImpl).Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
            => ((IReadOnlyDictionary<TKey, TValue>) mImpl).Values;

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        }
    }
}