using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class Context : IDisposable
    {
        private static Context _applicationContext;
        private static Dictionary<string, Context> _contexts;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitialize()
        {
            //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER //&& UNITY_EDITOR
            try
            {
                if (_applicationContext != null)
                    _applicationContext.Dispose();

                if (_contexts != null)
                {
                    foreach (var context in _contexts.Values)
                        context.Dispose();
                    _contexts.Clear();
                }
            }
            catch (Exception)
            {
                // ignored
            }
#endif

            _applicationContext = new Context();
            _contexts = new Dictionary<string, Context>();
        }

        /// <summary>
        /// 获取全局上下文。
        /// </summary>
        /// <returns></returns>
        public static Context GetGlobalContext()
        {
            return _applicationContext;
        }

        private static Context GetContext(string key)
        {
            _contexts.TryGetValue(key, out var context);
            return context;
        }

        public static T GetContext<T>(string key) where T : Context
        {
            return (T)GetContext(key);
        }

        public static void AddContext(string key, Context context)
        {
            _contexts.Add(key, context);
        }

        public static void RemoveContext(string key)
        {
            _contexts.Remove(key);
        }

        private readonly bool innerContainer;
        private readonly Context contextBase;
        private readonly IServiceContainer container;
        private readonly Dictionary<string, object> attributes;

        public Context() : this(null, null)
        {
        }

        protected Context(IServiceContainer container, Context contextBase)
        {
            attributes = new Dictionary<string, object>();
            this.contextBase = contextBase;
            this.container = container;
            if (this.container == null)
            {
                innerContainer = true;
                this.container = new ServiceContainer();
            }
        }

        public bool Contains(string name, bool cascade = true)
        {
            if (attributes.ContainsKey(name))
                return true;

            if (cascade && contextBase != null)
                return contextBase.Contains(name, cascade);

            return false;
        }

        public object Get(string name, bool cascade = true)
        {
            return Get<object>(name, cascade);
        }

        public T Get<T>(string name, bool cascade = true)
        {
            if (attributes.TryGetValue(name, out var v))
                return (T)v;

            if (cascade && contextBase != null)
                return contextBase.Get<T>(name, cascade);

            return default;
        }

        public void Set(string name, object value)
        {
            Set<object>(name, value);
        }

        public void Set<T>(string name, T value)
        {
            attributes[name] = value;
        }

        public object Remove(string name)
        {
            return Remove<object>(name);
        }

        public T Remove<T>(string name)
        {
            if (!attributes.ContainsKey(name))
                return default;

            object v = attributes[name];
            attributes.Remove(name);
            return (T)v;
        }

        public IEnumerator GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        public IServiceContainer GetContainer()
        {
            return container;
        }

        public T GetService<T>()
        {
            T result = container.Resolve<T>();
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService<T>();

            return default;
        }

        public T GetService<T>(string name)
        {
            T result = container.Resolve<T>(name);
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService<T>(name);

            return default;
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (innerContainer && container is IDisposable dis) dis.Dispose();
                }

                disposed = true;
            }
        }

        ~Context()
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