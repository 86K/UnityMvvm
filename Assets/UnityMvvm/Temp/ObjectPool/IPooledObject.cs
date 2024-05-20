

namespace Fusion.Mvvm
{
    public interface IPooledObject
    {
        /// <summary>
        /// Return the object to the pool.
        /// </summary>
        void Free();
    }
}
