using System;

namespace Fusion.Mvvm
{
    public abstract class BindingBase : IBinding
    {
        private IBindingContext _bindingContext;
        private object _dataContext;

        protected BindingBase(IBindingContext bindingContext, object dataContext)
        {
            _bindingContext = bindingContext;
            _dataContext = dataContext;
        }

        public IBindingContext BindingContext
        {
            get => _bindingContext;
            set => _bindingContext = value;
        }

        public object DataContext
        {
            get => _dataContext;
            set
            {
                if (_dataContext == value)
                    return;

                _dataContext = value;
                OnDataContextChanged();
            }
        }

        protected abstract void OnDataContextChanged();

        #region IDisposable Support     

        protected virtual void Dispose(bool disposing)
        {
            _bindingContext = null;
            _dataContext = null;
        }

        ~BindingBase()
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
