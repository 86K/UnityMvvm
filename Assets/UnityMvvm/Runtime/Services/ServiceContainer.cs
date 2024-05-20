using System;
using System.Collections.Concurrent;

namespace Fusion.Mvvm
{
    public class ServiceContainer : IServiceContainer, IDisposable
    {
        private ConcurrentDictionary<string, Entry> _nameServiceMappings = new ConcurrentDictionary<string, Entry>();
        private ConcurrentDictionary<Type, Entry> _typeServiceMappings = new ConcurrentDictionary<Type, Entry>();

        public T Resolve<T>()
        {
            if (_typeServiceMappings.TryGetValue(typeof(T), out var entry))
                return (T)entry.Factory.Create();
            return default;
        }

        public T Resolve<T>(string name)
        {
            if (_nameServiceMappings.TryGetValue(name, out var entry))
                return (T)entry.Factory.Create();
            return default;
        }

        public void Register<T>(T target)
        {
            Register0(typeof(T), new SingleInstanceFactory(target));
        }
        
        public void Register<T>(string name, T target)
        {
            Register0(name, new SingleInstanceFactory(target));
        }

        public void Unregister<T>()
        {
            Unregister0(typeof(T));
        }

        /// <summary>
        /// For services registered with a type, if the type is not a generic type, 
        /// it can be retrieved by type or type name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="factory"></param>
        private void Register0(Type type, IFactory factory)
        {
            string name = type.IsGenericType ? null : type.Name;
            Entry entry = new Entry(name, type, factory);
            if (!_typeServiceMappings.TryAdd(type, entry))
                throw new Exception($"Duplicate key {type}");

            if (!string.IsNullOrEmpty(name))
                _nameServiceMappings.TryAdd(name, entry);
        }

        /// <summary>
        /// Services registered with a name can only be retrieved with a name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="factory"></param>
        private void Register0(string name, IFactory factory)
        {
            if (!_nameServiceMappings.TryAdd(name, new Entry(name, null, factory)))
                throw new Exception($"Duplicate key {name}");
        }

        private void Unregister0(Type type)
        {
            if (!_typeServiceMappings.TryRemove(type, out var entry) || entry == null || string.IsNullOrEmpty(entry.Name))
                return;

            if (!_nameServiceMappings.TryGetValue(entry.Name, out var entry2) || entry != entry2)
                return;

            _nameServiceMappings.TryRemove(entry.Name, out _);
        }

        #region IDisposable Support

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var kv in _nameServiceMappings)
                        kv.Value.Dispose();

                    _nameServiceMappings.Clear();
                    _nameServiceMappings = null;

                    foreach (var kv in _typeServiceMappings)
                        kv.Value.Dispose();

                    _typeServiceMappings.Clear();
                    _typeServiceMappings = null;
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

        private class Entry : IDisposable
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

        private interface IFactory : IDisposable
        {
            object Create();
        }

        private class SingleInstanceFactory : IFactory
        {
            private object target;

            public SingleInstanceFactory(object target)
            {
                this.target = target;
            }

            public object Create()
            {
                return target;
            }

            #region IDisposable Support

            private bool disposed;

            private void Dispose(bool disposing)
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