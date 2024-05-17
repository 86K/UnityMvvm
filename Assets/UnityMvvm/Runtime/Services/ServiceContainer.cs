

using System;

namespace Fusion.Mvvm
{
    public class ServiceContainer : IServiceContainer, IDisposable
    {
        private readonly object _lock = new object();
        private ConcurrentDictionary<string, Entry> nameServiceMappings = new ConcurrentDictionary<string, Entry>();
        private ConcurrentDictionary<Type, Entry> typeServiceMappings = new ConcurrentDictionary<Type, Entry>();

        public virtual object Resolve(Type type)
        {
            if (typeServiceMappings.TryGetValue(type, out var entry))
                return entry.Factory.Create();
            return null;
        }

        public virtual T Resolve<T>()
        {
            if (typeServiceMappings.TryGetValue(typeof(T), out var entry))
                return (T)entry.Factory.Create();
            return default;
        }

        public virtual object Resolve(string name)
        {
            if (nameServiceMappings.TryGetValue(name, out var entry))
                return entry.Factory.Create();
            return null;
        }

        public virtual T Resolve<T>(string name)
        {
            if (nameServiceMappings.TryGetValue(name, out var entry))
                return (T)entry.Factory.Create();
            return default;
        }

        public virtual void Register<T>(Func<T> factory)
        {
            Register0(typeof(T), new GenericFactory<T>(factory));
        }

        public virtual void Register(Type type, object target)
        {
            Register0(type, new SingleInstanceFactory(target));
        }

        public virtual void Register(string name, object target)
        {
            Register0(name, new SingleInstanceFactory(target));
        }

        public virtual void Register<T>(T target)
        {
            Register0(typeof(T), new SingleInstanceFactory(target));
        }

        public virtual void Register<T>(string name, Func<T> factory)
        {
            Register0(name, new GenericFactory<T>(factory));
        }

        public virtual void Register<T>(string name, T target)
        {
            Register0(name, new SingleInstanceFactory(target));
        }

        public virtual void Unregister(Type type)
        {
            Unregister0(type);
        }

        public virtual void Unregister<T>()
        {
            Unregister0(typeof(T));
        }

        public virtual void Unregister(string name)
        {
            Unregister0(name);
        }

        //internal void Register0(string name, Type type, IFactory factory)
        //{
        //    lock (_lock)
        //    {
        //        var entry = new Entry(name, type, factory);
        //        if (!nameServiceMappings.TryAdd(name, entry))
        //            throw new DuplicateRegisterServiceException(string.Format("Duplicate key {0}", name));

        //        typeServiceMappings.TryAdd(type, entry);
        //    }
        //}

        /// <summary>
        /// For services registered with a type, if the type is not a generic type, 
        /// it can be retrieved by type or type name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="factory"></param>
        internal void Register0(Type type, IFactory factory)
        {
            lock (_lock)
            {
                string name = type.IsGenericType ? null : type.Name;
                Entry entry = new Entry(name, type, factory);
                if (!typeServiceMappings.TryAdd(type, entry))
                    throw new DuplicateRegisterServiceException($"Duplicate key {type}");

                if (!string.IsNullOrEmpty(name))
                    nameServiceMappings.TryAdd(name, entry);
            }
        }

        /// <summary>
        /// Services registered with a name can only be retrieved with a name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factory"></param>
        internal void Register0(string name, IFactory factory)
        {
            lock (_lock)
            {
                if (!nameServiceMappings.TryAdd(name, new Entry(name, null, factory)))
                    throw new DuplicateRegisterServiceException($"Duplicate key {name}");
            }
        }

        internal void Unregister0(string name)
        {
            lock (_lock)
            {
                if (!nameServiceMappings.TryRemove(name, out var entry) || entry == null || entry.Type == null)
                    return;

                if (!typeServiceMappings.TryGetValue(entry.Type, out var entry2) || entry != entry2)
                    return;

                typeServiceMappings.TryRemove(entry.Type, out _);
            }
        }

        internal void Unregister0(Type type)
        {
            lock (_lock)
            {
                if (!typeServiceMappings.TryRemove(type, out var entry) || entry == null || string.IsNullOrEmpty(entry.Name))
                    return;

                if (!nameServiceMappings.TryGetValue(entry.Name, out var entry2) || entry != entry2)
                    return;

                nameServiceMappings.TryRemove(entry.Name, out _);
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
                    foreach (var kv in nameServiceMappings)
                        kv.Value.Dispose();

                    nameServiceMappings.Clear();
                    nameServiceMappings = null;

                    foreach (var kv in typeServiceMappings)
                        kv.Value.Dispose();

                    typeServiceMappings.Clear();
                    typeServiceMappings = null;
                }
                disposed = true;
            }
        }

        ~ServiceContainer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        internal class Entry : IDisposable
        {
            public Entry(string name, Type type, IFactory factory)
            {
                Name = name;
                Type = type;
                Factory = factory;
            }

            public string Name { get; }
            public Type Type { get; }
            public IFactory Factory { get; }
            public void Dispose()
            {
                Factory.Dispose();
            }
        }

        internal interface IFactory : IDisposable
        {
            object Create();
        }

        internal class GenericFactory<T> : IFactory
        {
            private readonly Func<T> func;

            public GenericFactory(Func<T> func)
            {
                this.func = func;
            }

            public virtual object Create()
            {
                return func();
            }

            public void Dispose()
            {
            }
        }

        internal class SingleInstanceFactory : IFactory
        {
            private object target;

            public SingleInstanceFactory(object target)
            {
                this.target = target;
            }

            public virtual object Create()
            {
                return target;
            }

            #region IDisposable Support
            private bool disposed;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (target is IDisposable disposable)
                            disposable.Dispose();
                        target = null;
                    }

                    disposed = true;
                }
            }

            ~SingleInstanceFactory()
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
}
