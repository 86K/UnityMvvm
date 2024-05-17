using System;
using UnityEngine.EventSystems;

namespace Fusion.Mvvm
{
    public abstract class AbstractBinding : IBinding
    {
        private IBindingContext bindingContext;
        private WeakReference target;
        private object dataContext;

        public AbstractBinding(IBindingContext bindingContext, object dataContext, object target)
        {
            this.bindingContext = bindingContext;
            this.target = new WeakReference(target, false);
            this.dataContext = dataContext;
        }

        public virtual IBindingContext BindingContext
        {
            get => bindingContext;
            set => bindingContext = value;
        }

        public virtual object Target
        {
            get
            {
                var target = this.target != null ? this.target.Target : null;
                return IsAlive(target) ? target : null;
            }
        }

        private bool IsAlive(object target)
        {
            try
            {
                if (target is UIBehaviour behaviour)
                {
                    if (behaviour.IsDestroyed())
                        return false;
                    return true;
                }

                if (target is UnityEngine.Object o)
                {
                    //Check if the object is valid because it may have been destroyed.
                    //Unmanaged objects,the weak caches do not accurately track the validity of objects.
                    var name = o.name;
                    return true;
                }

                return target != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual object DataContext
        {
            get => dataContext;
            set
            {
                if (dataContext == value)
                    return;

                dataContext = value;
                OnDataContextChanged();
            }
        }

        protected abstract void OnDataContextChanged();

        #region IDisposable Support     

        protected virtual void Dispose(bool disposing)
        {
            bindingContext = null;
            dataContext = null;
            target = null;
        }

        ~AbstractBinding()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
