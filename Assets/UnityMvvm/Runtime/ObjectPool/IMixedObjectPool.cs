

using System;

namespace Fusion.Mvvm
{
    public interface IMixedObjectPool<T> : IDisposable where T : class
    {
        /// <summary>
        /// Gets an object from the pool if one is available, otherwise creates one.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        T Allocate(string typeName);

        /// <summary>
        /// Return an object to the pool,if the number of objects in the pool is greater than or equal to the maximum value, the object is destroyed.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="obj"></param>
        void Free(string typeName, T obj);
    }
}
