

namespace Fusion.Mvvm
{
    public interface IObjectFactory<T> where T : class
    {
        /// <summary>
        /// Create a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        T Create(IObjectPool<T> pool);

        /// <summary>
        /// Destroy the object.
        /// </summary>
        /// <param name="obj"></param>
        void Destroy(T obj);

        /// <summary>
        /// Reset the object
        /// </summary>
        /// <param name="obj"></param>
        void Reset(T obj);

        /// <summary>
        /// Validate the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Validate(T obj);
    }
}
