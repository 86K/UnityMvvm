

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class Context : IDisposable
    {
        private static ApplicationContext context = null;
        private static Dictionary<string, Context> contexts = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnInitialize()
        {
            //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER //&& UNITY_EDITOR
            try
            {
                if (context != null)
                    context.Dispose();

                if (contexts != null)
                {
                    foreach (var context in contexts.Values)
                        context.Dispose();
                    contexts.Clear();
                }
            }
            catch (Exception) { }
#endif

            context = new ApplicationContext();
            contexts = new Dictionary<string, Context>();
        }

        public static ApplicationContext GetApplicationContext()
        {
            return context;
        }

        public static void SetApplicationContext(ApplicationContext context)
        {
            Context.context = context;
        }

        public static Context GetContext(string key)
        {
            Context context = null;
            contexts.TryGetValue(key, out context);
            return context;
        }

        public static T GetContext<T>(string key) where T : Context
        {
            return (T)GetContext(key);
        }

        public static void AddContext(string key, Context context)
        {
            contexts.Add(key, context);
        }

        public static void RemoveContext(string key)
        {
            contexts.Remove(key);
        }

        private readonly bool innerContainer = false;
        private readonly Context contextBase;
        private readonly IServiceContainer container;
        private readonly Dictionary<string, object> attributes;

        public Context() : this(null, null)
        {
        }

        public Context(IServiceContainer container, Context contextBase)
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

        public virtual bool Contains(string name, bool cascade = true)
        {
            if (attributes.ContainsKey(name))
                return true;

            if (cascade && contextBase != null)
                return contextBase.Contains(name, cascade);

            return false;
        }

        public virtual object Get(string name, bool cascade = true)
        {
            return Get<object>(name, cascade);
        }

        public virtual T Get<T>(string name, bool cascade = true)
        {
            object v;
            if (attributes.TryGetValue(name, out v))
                return (T)v;

            if (cascade && contextBase != null)
                return contextBase.Get<T>(name, cascade);

            return default(T);
        }

        public virtual void Set(string name, object value)
        {
            Set<object>(name, value);
        }

        public virtual void Set<T>(string name, T value)
        {
            attributes[name] = value;
        }

        public virtual object Remove(string name)
        {
            return Remove<object>(name);
        }

        public virtual T Remove<T>(string name)
        {
            if (!attributes.ContainsKey(name))
                return default(T);

            object v = attributes[name];
            attributes.Remove(name);
            return (T)v;
        }

        public virtual IEnumerator GetEnumerator()
        {
            return attributes.GetEnumerator();
        }

        public virtual IServiceContainer GetContainer()
        {
            return container;
        }

        public virtual object GetService(Type type)
        {
            object result = container.Resolve(type);
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService(type);

            return null;
        }

        public virtual object GetService(string name)
        {
            object result = container.Resolve(name);
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService(name);

            return null;
        }

        public virtual T GetService<T>()
        {
            T result = container.Resolve<T>();
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService<T>();

            return default(T);
        }

        public virtual T GetService<T>(string name)
        {
            T result = container.Resolve<T>(name);
            if (result != null)
                return result;

            if (contextBase != null)
                return contextBase.GetService<T>(name);

            return default(T);
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (innerContainer && container != null)
                    {
                        IDisposable dis = container as IDisposable;
                        if (dis != null)
                            dis.Dispose();
                    }
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
