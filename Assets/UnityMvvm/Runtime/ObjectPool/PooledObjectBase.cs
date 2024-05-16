
 
namespace Fusion.Mvvm
{
    public abstract class PooledObjectBase<T> : IPooledObject where T : PooledObjectBase<T>
    {
        private readonly IObjectPool<T> pool;
        public PooledObjectBase(IObjectPool<T> pool)
        {
            this.pool = pool;
        }

        public virtual void Free()
        {
            pool.Free((T)this);
        }
    }
}
