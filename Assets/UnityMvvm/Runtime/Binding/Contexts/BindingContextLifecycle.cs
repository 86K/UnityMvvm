

using UnityEngine;
namespace Fusion.Mvvm
{
    public class BindingContextLifecycle : MonoBehaviour
    {
        private IBindingContext bindingContext;
        public IBindingContext BindingContext
        {
            get => bindingContext;
            set
            {
                if (bindingContext == value)
                    return;

                if (bindingContext != null)
                    bindingContext.Dispose();

                bindingContext = value;
            }
        }

        protected virtual void OnDestroy()
        {
            if (bindingContext != null)
            {
                bindingContext.Dispose();
                bindingContext = null;
            }
        }
    }
}
