


using System;

namespace Fusion.Mvvm
{
    public abstract class BindingProxyBase : IBindingProxy
    {
        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
        }

        ~BindingProxyBase()
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
