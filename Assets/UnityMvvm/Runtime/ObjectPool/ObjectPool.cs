

using System;
using System.Threading;

namespace Fusion.Mvvm
{
    public class ObjectPool<T> : IObjectPool<T> where T : class
    {
        private readonly Entry[] entries = null;
        private readonly int maxSize;
        private readonly int initialSize;
        protected readonly IObjectFactory<T> factory;

        public ObjectPool(IObjectFactory<T> factory) : this(factory, 0, Environment.ProcessorCount * 2)
        {
        }

        public ObjectPool(IObjectFactory<T> factory, int maxSize) : this(factory, 0, maxSize)
        {
        }

        public ObjectPool(IObjectFactory<T> factory, int initialSize, int maxSize)
        {
            this.factory = factory;
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            entries = new Entry[maxSize];

            if (maxSize < initialSize)
                throw new ArgumentException("the maxSize must be greater than or equal to the initialSize");

            for (int i = 0; i < initialSize; i++)
            {
                entries[i].value = factory.Create(this);
            }
        }

        public int MaxSize => maxSize;

        public int InitialSize => initialSize;

        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns></returns>
        public virtual T Allocate()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);

            T value = default(T);
            for (var i = 0; i < entries.Length; i++)
            {
                value = entries[i].value;
                if (value == null)
                    continue;
#if UNITY_WEBGL
                entries[i].value = null;
                return value;
#else
                if (Interlocked.CompareExchange(ref entries[i].value, null, value) == value)
                    return value;
#endif
            }

            return factory.Create(this);
        }

        /// <summary>
        /// Return an object to the pool,if the number of objects in the pool is greater than or equal to the maximum value, the object is destroyed.
        /// </summary>
        /// <param name="obj"></param>
        public virtual void Free(T obj)
        {
            if (obj == null)
                return;

            if (disposed || !factory.Validate(obj))
            {
                factory.Destroy(obj);
                return;
            }

            factory.Reset(obj);
            for (var i = 0; i < entries.Length; i++)
            {
#if UNITY_WEBGL
                if (entries[i].value == null)
                {
                    entries[i].value = obj;
                    return;
                }
#else
                if (Interlocked.CompareExchange(ref entries[i].value, obj, null) == null)
                    return;
#endif
            }

            factory.Destroy(obj);
        }

        object IObjectPool.Allocate()
        {
            return Allocate();
        }

        void IObjectPool.Free(object obj)
        {
            Free((T)obj);
        }

        protected virtual void Clear()
        {
            for (var i = 0; i < entries.Length; i++)
            {
#if UNITY_WEBGL
                var value = entries[i].value;
                entries[i].value = null;
#else
                var value = Interlocked.Exchange(ref entries[i].value, null);
#endif                
                if (value != null)
                    factory.Destroy(value);
            }
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Clear();
                disposed = true;
            }
        }

        ~ObjectPool()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private struct Entry
        {
            public T value;
        }
    }
}
