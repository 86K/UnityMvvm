

using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UnityGameObjectFactoryBase : IObjectFactory<GameObject>
    {
        public virtual GameObject Create(IObjectPool<GameObject> pool)
        {
            GameObject target = Create();
            PooledUnityObject pooledObj = target.AddComponent<PooledUnityObject>();
            pooledObj.pool = pool;
            return target;
        }

        protected abstract GameObject Create();

        public abstract void Reset(GameObject obj);

        public virtual void Destroy(GameObject obj)
        {
            Object.Destroy(obj);
        }

        public virtual bool Validate(GameObject obj)
        {
            return true;
        }

        class PooledUnityObject : MonoBehaviour, IPooledObject
        {
            internal IObjectPool<GameObject> pool;

            public void Free()
            {
                if (pool != null)
                    pool.Free(gameObject);
            }
        }
    }
}
