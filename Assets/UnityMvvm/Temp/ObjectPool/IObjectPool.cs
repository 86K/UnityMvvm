

using System;

namespace Fusion.Mvvm
{
    public interface IObjectPool : IDisposable
    {
        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns></returns>
        object Allocate();

        /// <summary>
        /// Return an object to the pool,if the number of objects in the pool is greater than or equal to the maximum value, the object is destroyed.
        /// </summary>
        /// <param name="obj"></param>
        void Free(object obj);
    }

    public interface IObjectPool<T> : IObjectPool, IDisposable where T : class
    {
        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <returns></returns>
        new T Allocate();

        /// <summary>
        /// Return an object to the pool,if the number of objects in the pool is greater than or equal to the maximum value, the object is destroyed.
        /// </summary>
        /// <param name="obj"></param>
        void Free(T obj);
    }
}
