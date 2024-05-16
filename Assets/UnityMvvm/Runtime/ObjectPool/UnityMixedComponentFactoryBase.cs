

using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UnityMixedComponentFactoryBase<T> : IMixedObjectFactory<T> where T : Component
    {
        public virtual T Create(IMixedObjectPool<T> pool, string typeName)
        {
            T target = Create(typeName);
            PooledUnityObject pooledObj = target.gameObject.AddComponent<PooledUnityObject>();
            pooledObj.pool = pool;
            pooledObj.target = target;
            pooledObj.typeName = typeName;
            return target;
        }

        protected abstract T Create(string typeName);

        public abstract void Reset(string typeName, T obj);

        public virtual void Destroy(string typeName, T obj)
        {
            Object.Destroy(obj.gameObject);
        }

        public virtual bool Validate(string typeName, T obj)
        {
            return true;
        }

        class PooledUnityObject : MonoBehaviour, IPooledObject
        {
            internal IMixedObjectPool<T> pool;
            internal T target;
            internal string typeName;

            public void Free()
            {
                if (pool != null)
                    pool.Free(typeName, target);
            }
        }
    }
}
