

using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UnityMixedGameObjectFactoryBase : IMixedObjectFactory<GameObject>
    {
        public virtual GameObject Create(IMixedObjectPool<GameObject> pool, string typeName)
        {
            GameObject target = Create(typeName);
            PooledUnityObject pooledObj = target.gameObject.AddComponent<PooledUnityObject>();
            pooledObj.pool = pool;
            pooledObj.target = target;
            pooledObj.typeName = typeName;
            return target;
        }

        protected abstract GameObject Create(string typeName);

        public abstract void Reset(string typeName, GameObject obj);

        public virtual void Destroy(string typeName, GameObject obj)
        {
            Object.Destroy(obj);
        }

        public virtual bool Validate(string typeName, GameObject obj)
        {
            return true;
        }

        class PooledUnityObject : MonoBehaviour, IPooledObject
        {
            internal IMixedObjectPool<GameObject> pool;
            internal GameObject target;
            internal string typeName;

            public void Free()
            {
                if (pool != null)
                    pool.Free(typeName, target);
            }
        }
    }
}
