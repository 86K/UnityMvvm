using System;
using System.Threading;
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
    public class ObservableList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly PropertyChangedEventArgs _countEventArgs = new PropertyChangedEventArgs("Count");
        private readonly PropertyChangedEventArgs _indexerEventArgs = new PropertyChangedEventArgs("Item[]");

        private readonly object _propertyChangedLock = new object();
        private readonly object _collectionChangedLock = new object();
        private PropertyChangedEventHandler _propertyChanged;
        private NotifyCollectionChangedEventHandler _collectionChanged;

        private SimpleMonitor _monitor = new SimpleMonitor();
        private List<T> _items;

        [NonSerialized] private Object _syncRoot;

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

        public ObservableList()
        {
            _items = new List<T>();
        }

        public ObservableList(List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _items = new List<T>();
            foreach (T item in list)
            {
                _items.Add(item);
            }
        }

        public int Count => _items.Count;

        protected IList<T> Items => _items;

        public T this[int index]
        {
            get => _items[index];
            set
            {
                if (IsReadOnly)
                    throw new NotSupportedException("ReadOnlyCollection");

                if (index < 0 || index >= _items.Count)
                    throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

                SetItem(index, value);
            }
        }

        public void Add(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            int index = _items.Count;
            InsertItem(index, item);
        }

        public void Clear()
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            ClearItems();
        }

        public void CopyTo(T[] array, int index)
        {
            _items.CopyTo(array, index);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index > _items.Count)
                throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

            InsertItem(index, item);
        }

        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            int index = _items.IndexOf(item);
            if (index < 0)
                return false;
            RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index >= _items.Count)
                throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

            RemoveItem(index);
        }

        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            int index = _items.Count;
            InsertItem(index, collection);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index > _items.Count)
                throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

            InsertItem(index, collection);
        }

        public void RemoveRange(int index, int count)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (index < 0 || index >= _items.Count)
                throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

            RemoveItem(index, count);
        }

        public List<T>.Enumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        bool IsReadOnly => Items.IsReadOnly;

        bool ICollection<T>.IsReadOnly => IsReadOnly;

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    if (_items is ICollection c)
                    {
                        _syncRoot = c.SyncRoot;
                    }
                    else
                    {
#if UNITY_WEBGL
                        this.syncRoot = new object();
#else
                        Interlocked.CompareExchange(ref _syncRoot, new object(), null);
#endif
                    }
                }

                return _syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (array.Rank != 1)
                throw new ArgumentException("RankMultiDimNotSupported");

            if (array.GetLowerBound(0) != 0)
                throw new ArgumentException("NonZeroLowerBound");

            if (index < 0)
                throw new ArgumentOutOfRangeException($"ArgumentOutOfRangeException:{index}");

            if (array.Length - index < Count)
                throw new ArgumentException("ArrayPlusOffTooSmall");

            if (array is T[] tArray)
            {
                _items.CopyTo(tArray, index);
            }
            else
            {
                Type targetType = array.GetType().GetElementType();
                Type sourceType = typeof(T);
                if (targetType != null && !(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    throw new ArgumentException("InvalidArrayType");

                if (!(array is object[] objects))
                    throw new ArgumentException("InvalidArrayType");

                int count = _items.Count;
                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        objects[index++] = _items[i];
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException("InvalidArrayType");
                }
            }
        }

        object IList.this[int index]
        {
            get => _items[index];
            set
            {
                if (value == null && default(T) != null)
                    throw new ArgumentNullException("value");

                try
                {
                    this[index] = (T)value;
                }
                catch (InvalidCastException e)
                {
                    throw new ArgumentException("", e);
                }
            }
        }

        bool IList.IsReadOnly => IsReadOnly;

        bool IList.IsFixedSize
        {
            get
            {
                if (_items is IList list)
                {
                    return list.IsFixedSize;
                }

                return IsReadOnly;
            }
        }

        int IList.Add(object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (value == null && default(T) != null)
                throw new ArgumentNullException("value");

            try
            {
                Add((T)value);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("", e);
            }

            return Count - 1;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value))
            {
                return IndexOf((T)value);
            }

            return -1;
        }

        void IList.Insert(int index, object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (value == null && default(T) != null)
                throw new ArgumentNullException("value");

            try
            {
                Insert(index, (T)value);
            }
            catch (InvalidCastException e)
            {
                throw new ArgumentException("", e);
            }
        }

        void IList.Remove(object value)
        {
            if (IsReadOnly)
                throw new NotSupportedException("ReadOnlyCollection");

            if (IsCompatibleObject(value))
            {
                Remove((T)value);
            }
        }

        protected virtual void ClearItems()
        {
            CheckReentrancy();
            _items.Clear();
            OnPropertyChanged(_countEventArgs);
            OnPropertyChanged(_indexerEventArgs);
            OnCollectionReset();
        }

        protected virtual void RemoveItem(int index)
        {
            CheckReentrancy();
            T removedItem = this[index];

            _items.RemoveAt(index);

            OnPropertyChanged(_countEventArgs);
            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        protected virtual void RemoveItem(int index, int count)
        {
            CheckReentrancy();

            List<T> list = _items as List<T>;
            List<T> changedItems = list.GetRange(index, count);
            list.RemoveRange(index, count);

            OnPropertyChanged(_countEventArgs);
            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, changedItems, index);
        }

        protected virtual void InsertItem(int index, T item)
        {
            CheckReentrancy();

            _items.Insert(index, item);

            OnPropertyChanged(_countEventArgs);
            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected virtual void InsertItem(int index, IEnumerable<T> collection)
        {
            CheckReentrancy();

            _items.InsertRange(index, collection);

            OnPropertyChanged(_countEventArgs);
            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, ToList(collection), index);
        }

        protected virtual void SetItem(int index, T item)
        {
            CheckReentrancy();
            T originalItem = this[index];

            _items[index] = item;

            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            CheckReentrancy();

            T removedItem = this[oldIndex];

            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, removedItem);

            OnPropertyChanged(_indexerEventArgs);
            OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
        }


        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_propertyChanged != null)
            {
                _propertyChanged(this, e);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_collectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    _collectionChanged(this, e);
                }
            }
        }

        protected IDisposable BlockReentrancy()
        {
            _monitor.Enter();
            return _monitor;
        }

        protected void CheckReentrancy()
        {
            if (_monitor.Busy)
            {
                if ((_collectionChanged != null) && (_collectionChanged.GetInvocationList().Length > 1))
                    throw new InvalidOperationException();
            }
        }

        private IList ToList(IEnumerable<T> collection)
        {
            if (collection is IList list1)
                return list1;

            List<T> list = new List<T>();
            list.AddRange(collection);
            return list;
        }

        private static bool IsCompatibleObject(object value)
        {
            return value is T || (value == null && default(T) == null);
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (_collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList changedItems, int index)
        {
            if (_collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            if (_collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            if (_collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            if (_collectionChanged != null)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        [Serializable()]
        private class SimpleMonitor : IDisposable
        {
            private int _busyCount;
            public bool Busy => _busyCount > 0;

            public void Enter()
            {
                ++_busyCount;
            }

            public void Dispose()
            {
                --_busyCount;
            }
        }
    }
}