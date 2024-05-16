

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class BindingContext : IBindingContext
    {
        private readonly string DEFAULT_KEY = "_KEY_";
        private readonly Dictionary<object, List<IBinding>> bindings = new Dictionary<object, List<IBinding>>();

        private IBinder binder;
        private object owner;
        private object dataContext;
        private readonly object _lock = new object();
        private EventHandler dataContextChanged;

        public event EventHandler DataContextChanged
        {
            add { lock (_lock) { dataContextChanged += value; } }
            remove { lock (_lock) { dataContextChanged -= value; } }
        }

        public BindingContext(IBinder binder) : this(null, binder, (object)null)
        {
        }

        public BindingContext(object owner, IBinder binder) : this(owner, binder, (object)null)
        {
        }

        public BindingContext(object owner, IBinder binder, object dataContext)
        {
            this.owner = owner;
            this.binder = binder;
            DataContext = dataContext;
        }

        public BindingContext(object owner, IBinder binder, IDictionary<object, IEnumerable<TargetDescription>> firstBindings) : this(owner, binder, null, firstBindings)
        {
        }

        public BindingContext(object owner, IBinder binder, object dataContext, IDictionary<object, IEnumerable<TargetDescription>> firstBindings)
        {
            this.owner = owner;
            this.binder = binder;
            DataContext = dataContext;

            if (firstBindings != null && firstBindings.Count > 0)
            {
                foreach (var kvp in firstBindings)
                {
                    Add(kvp.Key, kvp.Value);
                }
            }
        }

        protected IBinder Binder => binder;

        public object Owner => owner;

        public object DataContext
        {
            get => dataContext;
            set
            {
                if (dataContext == value)
                    return;

                dataContext = value;
                OnDataContextChanged();
                RaiseDataContextChanged();
            }
        }

        protected void RaiseDataContextChanged()
        {
            try
            {
                var handler = dataContextChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        protected virtual void OnDataContextChanged()
        {
            try
            {
                foreach (var kv in bindings)
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

        protected List<IBinding> GetOrCreateList(object key)
        {
            if (key == null)
                key = DEFAULT_KEY;

            List<IBinding> list;
            if (bindings.TryGetValue(key, out list))
                return list;

            list = new List<IBinding>();
            bindings.Add(key, list);
            return list;
        }


        public virtual void Add(IBinding binding, object key = null)
        {
            if (binding == null)
                return;

            List<IBinding> list = GetOrCreateList(key);
            binding.BindingContext = this;
            list.Add(binding);
        }

        public virtual void Add(IEnumerable<IBinding> bindings, object key = null)
        {
            if (bindings == null)
                return;

            List<IBinding> list = GetOrCreateList(key);
            foreach (IBinding binding in bindings)
            {
                binding.BindingContext = this;
                list.Add(binding);
            }
        }

        public virtual void Add(object target, TargetDescription description, object key = null)
        {
            IBinding binding = Binder.Bind(this, DataContext, target, description);
            Add(binding, key);
        }

        public virtual void Add(object target, IEnumerable<TargetDescription> descriptions, object key = null)
        {
            IEnumerable<IBinding> bindings = Binder.Bind(this, DataContext, target, descriptions);
            Add(bindings, key);
        }

        public virtual void Clear(object key)
        {
            if (key == null)
                return;

            List<IBinding> list;
            if (!bindings.TryGetValue(key, out list))
                return;

            bindings.Remove(key);
            if (list != null && list.Count > 0)
            {
                foreach (IBinding binding in list)
                {
                    binding.Dispose();
                }
            }
        }

        public virtual void Clear()
        {
            try
            {
                foreach (var kv in bindings)
                {
                    foreach (var binding in kv.Value)
                    {
                        binding.Dispose();
                    }
                }
            }
            finally
            {
                bindings.Clear();
            }
        }

        #region IDisposable Support
        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Clear();
                    owner = null;
                    binder = null;
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