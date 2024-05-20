using System;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public abstract class AsyncLoadableInteractionActionBase<TNotification> : AsyncInteractionActionBase<TNotification>
    {
        private readonly string viewName;
        private IUIViewLocator locator;
        private readonly IWindowManager windowManager;

        public AsyncLoadableInteractionActionBase(string viewName, IUIViewLocator locator) : this(viewName, locator, null)
        {
        }

        public AsyncLoadableInteractionActionBase(string viewName, IWindowManager windowManager) : this(viewName, null, windowManager)
        {
        }

        public AsyncLoadableInteractionActionBase(string viewName, IUIViewLocator locator, IWindowManager windowManager)
        {
            this.viewName = viewName;
            this.locator = locator;
            this.windowManager = windowManager;
        }

        protected string ViewName => viewName;

        protected IUIViewLocator Locator
        {
            get
            {
                if (locator == null)
                {
                    Context context = Context.GetGlobalContext();
                    locator = context.GetService<IUIViewLocator>();
                }
                return locator;
            }
        }

        protected async Task<T> LoadViewAsync<T>() where T : IView
        {
            var locator = Locator;
            if (locator == null)
                throw new Exception("Not found the \"IUIViewLocator\".");

            if (string.IsNullOrEmpty(viewName))
                throw new ArgumentNullException("The view name is null.");

            return await locator.LoadViewAsync<T>(viewName);
        }

        protected async Task<T> LoadWindowAsync<T>() where T : IWindow
        {
            var locator = Locator;
            if (locator == null)
                throw new Exception("Not found the \"IUIViewLocator\".");

            if (string.IsNullOrEmpty(viewName))
                throw new ArgumentNullException("The view name is null.");

            return await locator.LoadWindowAsync<T>(windowManager, viewName);
        }
    }
}
