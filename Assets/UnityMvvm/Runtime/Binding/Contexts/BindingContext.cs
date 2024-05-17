using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class BindingContext : IBindingContext
    {
        private readonly string DEFAULT_KEY = "_KEY_";
        private readonly Dictionary<object, List<IBinding>> _bindings = new Dictionary<object, List<IBinding>>();

        private IBinder _binder;
        private object _owner;
        private object _dataContext;
        private readonly object _lock = new object();
        private EventHandler _dataContextChanged;

        [Obsolete("这个的实用性好像不强？没有被用到！")]
        public event EventHandler DataContextChanged
        {
            add
            {
                lock (_lock)
                {
                    _dataContextChanged += value;
                }
            }
            remove
            {
                lock (_lock)
                {
                    _dataContextChanged -= value;
                }
            }
        }

        public BindingContext(object owner, IBinder binder, object dataContext = null)
        {
            _owner = owner;
            _binder = binder;
            DataContext = dataContext;
        }

        private IBinder Binder => _binder;

        public object Owner => _owner;

        public object DataContext
        {
            get => _dataContext;
            set
            {
                if (_dataContext == value)
                    return;

                _dataContext = value;
                OnDataContextChanged();
                RaiseDataContextChanged();
            }
        }

        private void RaiseDataContextChanged()
        {
            try
            {
                var handler = _dataContextChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void OnDataContextChanged()
        {
            try
            {
                foreach (var kv in _bindings)
                {
                    foreach (var binding in kv.Value)
                    {
                        binding.DataContext = DataContext;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
        
        private List<IBinding> GetOrCreateList()
        {
            if (_bindings.TryGetValue(DEFAULT_KEY, out var list))
                return list;

            list = new List<IBinding>();
            _bindings.Add(DEFAULT_KEY, list);
            return list;
        }
        
        void Add(IBinding binding)
        {
            if (binding == null)
                return;

            List<IBinding> list = GetOrCreateList();
            binding.BindingContext = this;
            list.Add(binding);
        }

        public void Add(object target, TargetDescription description)
        {
            IBinding binding = Binder.Bind(this, DataContext, target, description);
            Add(binding);
        }

        void Clear()
        {
            try
            {
                foreach (var kv in _bindings)
                {
                    foreach (var binding in kv.Value)
                    {
                        binding.Dispose();
                    }
                }
            }
            finally
            {
                _bindings.Clear();
            }
        }

        #region IDisposable Support

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Clear();
                    _owner = null;
                    _binder = null;
                }

                disposed = true;
            }
        }

        ~BindingContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}