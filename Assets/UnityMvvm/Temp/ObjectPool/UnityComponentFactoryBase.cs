

using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UnityComponentFactoryBase<T> : IObjectFactory<T> where T : Component
    {
        public virtual T Create(IObjectPool<T> pool)
        {
            T target = Create();
            PooledUnityObject pooledObj = target.gameObject.AddComponent<PooledUnityObject>();
            pooledObj.pool = pool;
            pooledObj.target = target;
            return target;
        }

        protected abstract T Create();

        public abstract void Reset(T obj);

        public virtual void Destroy(T obj)
        {
            Object.Destroy(obj.gameObject);
        }

        public virtual bool Validate(T obj)
        {
            return true;
        }
    }

    class PooledUnityObject : MonoBehaviour, IPooledObject
    {
        internal IObjectPool pool;
        internal object target;

        public void Free()
        {
            if (pool != null)
                pool.Free(target);
        }
    }
}
