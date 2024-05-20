using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UIBase
    {
        private const string DEFAULT_VIEW_LOCATOR_KEY = "_DEFAULT_VIEW_LOCATOR";

        protected static IUIViewLocator GetUIViewLocator()
        {
            Context context = Context.GetGlobalContext();
            IUIViewLocator locator = context.GetService<IUIViewLocator>();
            if (locator != null)
                return locator;

            Debug.Log("Not found the \"IUIViewLocator\" in the ApplicationContext.Try loading the Tips using the DefaultUIViewLocator.");

            locator = context.GetService<IUIViewLocator>(DEFAULT_VIEW_LOCATOR_KEY);
            if (locator == null)
            {
                locator = new DefaultUIViewLocator();
                context.GetContainer().Register(DEFAULT_VIEW_LOCATOR_KEY, locator);
            }
            return locator;
        }

        protected static IUIViewGroup GetCurrentViewGroup()
        {
            GlobalWindowManagerBase windowManager = GlobalWindowManagerBase.Root;
            IWindow window = windowManager.Current;
            while (window is WindowContainer windowContainer)
                window = windowContainer.Current;
            return window as IUIViewGroup;
        }
    }
}
