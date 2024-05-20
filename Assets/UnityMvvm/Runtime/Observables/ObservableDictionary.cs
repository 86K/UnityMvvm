using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NotifyCollectionChangedEventHandler = System.Collections.Specialized.NotifyCollectionChangedEventHandler;
using INotifyCollectionChanged = System.Collections.Specialized.INotifyCollectionChanged;
using NotifyCollectionChangedAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NotifyCollectionChangedEventArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;
using PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;

namespace Fusion.Mvvm
{
    [Serializable]
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly PropertyChangedEventArgs _countEventArgs = new PropertyChangedEventArgs("Count");
        private readonly PropertyChangedEventArgs _indexerEventArgs = new PropertyChangedEventArgs("Item[]");
        private readonly PropertyChangedEventArgs _keysEventArgs = new PropertyChangedEventArgs("Keys");
        private readonly PropertyChangedEventArgs _valuesEventArgs = new PropertyChangedEventArgs("Values");

        private readonly object _propertyChangedLock = new object();
        private readonly object _collectionChangedLock = new object();
        private PropertyChangedEventHandler _propertyChanged;
        private NotifyCollectionChangedEventHandler _collectionChanged;

        protected Dictionary<TKey, TValue> _dictionary;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (_propertyChangedLock)
                {
                    _propertyChanged += value;
                }
            }
            remove
            {
                lock (_propertyChangedLock)
                {
                    _propertyChanged -= value;
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (_collectionChangedLock)
                {
                    _collectionChanged += value;
                }
            }
            remove
            {
                lock (_collectionChangedLock)
                {
                    _collectionChanged -= value;
                }
            }
        }

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary);
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(int capacity)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_dictionary.ContainsKey(key))
                    return default;
                return _dictionary[key];
            }
            set => Insert(key, value, false);
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            Insert(key, value, true);
        }

        public bool Remove(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _dictionary.TryGetValue(key, out var value);
            var removed = _dictionary.Remove(key);
            if (removed)
            {
                OnPropertyChanged(NotifyCollectionChangedAction.Remove);
                if (_collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
            }

            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }

        public void Clear()
        {
            if (_dictionary.Count > 0)
            {
                _dictionary.Clear();
                OnPropertyChanged(NotifyCollectionChangedAction.Reset);
                if (_collectionChanged != null)
                    OnCollectionChanged();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary)_dictionary).CopyTo(array, arrayIndex);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        public void AddRange(IDictionary<TKey, TValue> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            if (items.Count > 0)
            {
                if (_dictionary.Count > 0)
                {
                    if (items.Keys.Any((k) => _dictionary.ContainsKey(k)))
                        throw new ArgumentException("An item with the same key has already been added.");
                    foreach (var item in items)
                        ((IDictionary<TKey, TValue>)_dictionary).Add(item);
                }
                else
                {
                    _dictionary = new Dictionary<TKey, TValue>(items);
                }

                OnPropertyChanged(NotifyCollectionChangedAction.Add);
                if (_collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
            }
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            if (_dictionary.TryGetValue(key, out var item))
            {
                if (add)
                    throw new ArgumentException("An item with the same key has already been added.");

                if (EqualityComparer<TValue>.Default.Equals(item, value))
                    return;

                _dictionary[key] = value;
                OnPropertyChanged(NotifyCollectionChangedAction.Replace);
                if (_collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value),
                        new KeyValuePair<TKey, TValue>(key, item));
            }
            else
            {
                _dictionary[key] = value;
                OnPropertyChanged(NotifyCollectionChangedAction.Add);
                if (_collectionChanged != null)
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        private void OnPropertyChanged(NotifyCollectionChangedAction action)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                {
                    OnPropertyChanged(_countEventArgs);
                    OnPropertyChanged(_indexerEventArgs);
                    OnPropertyChanged(_keysEventArgs);
                    OnPropertyChanged(_valuesEventArgs);
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    OnPropertyChanged(_indexerEventArgs);
                    OnPropertyChanged(_valuesEventArgs);
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                default:
                {
                    OnPropertyChanged(_countEventArgs);
                    OnPropertyChanged(_indexerEventArgs);
                    OnPropertyChanged(_keysEventArgs);
                    OnPropertyChanged(_valuesEventArgs);
                    break;
                }
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            if (_propertyChanged != null)
                _propertyChanged(this, eventArgs);
        }

        private void OnCollectionChanged()
        {
            if (_collectionChanged != null)
                _collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
        {
            if (_collectionChanged != null)
                _collectionChanged(this, new NotifyCollectionChangedEventArgs(action, changedItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
        {
            if (_collectionChanged != null)
                _collectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
        {
            if (_collectionChanged != null)
                _collectionChanged(this, new NotifyCollectionChangedEventArgs(action, newItems));
        }

        object IDictionary.this[object key]
        {
            get => ((IDictionary)_dictionary)[key];
            set => Insert((TKey)key, (TValue)value, false);
        }

        ICollection IDictionary.Keys => ((IDictionary)_dictionary).Keys;

        ICollection IDictionary.Values => ((IDictionary)_dictionary).Values;

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_dictionary).Contains(key);
        }

        void IDictionary.Add(object key, object value)
        {
            Add((TKey)key, (TValue)value);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)_dictionary).GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            Remove((TKey)key);
        }

        bool IDictionary.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;

        void ICollection.CopyTo(Array array, int index)
        {
            ((IDictionary)_dictionary).CopyTo(array, index);
        }

        object ICollection.SyncRoot => ((IDictionary)_dictionary).SyncRoot;

        bool ICollection.IsSynchronized => ((IDictionary)_dictionary).IsSynchronized;
    }
}