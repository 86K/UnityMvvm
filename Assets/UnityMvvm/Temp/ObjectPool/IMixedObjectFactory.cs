

namespace Fusion.Mvvm
{
    public interface IMixedObjectFactory<T> where T : class
    {
        /// <summary>
        /// Create a <typeparamref name="T"/>.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        T Create(IMixedObjectPool<T> pool, string typeName);

        /// <summary>
        /// Destroy the object.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="obj"></param>
        void Destroy(string typeName, T obj);

        /// <summary>
        /// Reset the object
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="obj"></param>
        void Reset(string typeName, T obj);

        /// <summary>
        /// Validate the object
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Validate(string typeName, T obj);
    }
}
