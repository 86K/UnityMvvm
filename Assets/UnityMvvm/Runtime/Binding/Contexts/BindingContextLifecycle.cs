using UnityEngine;

namespace Fusion.Mvvm
{
    public class BindingContextLifecycle : MonoBehaviour
    {
        private IBindingContext _bindingContext;

        public IBindingContext BindingContext
        {
            get => _bindingContext;
            set
            {
                if (_bindingContext == value)
                    return;

                if (_bindingContext != null)
                    _bindingContext.Dispose();

                _bindingContext = value;
            }
        }

        protected void OnDestroy()
        {
            if (_bindingContext != null)
            {
                _bindingContext.Dispose();
                _bindingContext = null;
            }
        }
    }
}