using System;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class AsyncWindowInteractionAction : AsyncLoadableInteractionActionBase<WindowNotification>
    {
        private Window window;
        public AsyncWindowInteractionAction(string viewName) : this(viewName, null,null)
        {
        }

        public AsyncWindowInteractionAction(string viewName, IUIViewLocator locator) : base(viewName, locator)
        {
        }

        public AsyncWindowInteractionAction(string viewName, IWindowManager windowManager) : base(viewName, windowManager)
        {
        }

        public AsyncWindowInteractionAction(string viewName, IUIViewLocator locator, IWindowManager windowManager) : base(viewName, locator, windowManager)
        {
        }

        public Window Window => window;

        public override Task Action(WindowNotification notification)
        {
            bool ignoreAnimation = notification.IgnoreAnimation;
            switch (notification.ActionType)
            {
                case WindowActionType.CREATE:
                    return Create(notification.ViewModel);
                case WindowActionType.SHOW:
                    return Show(notification.ViewModel, notification.WaitDismissed, ignoreAnimation);
                case WindowActionType.HIDE:
                    return Hide(ignoreAnimation);
                case WindowActionType.DISMISS:
                    return Dismiss(ignoreAnimation);
            }
            return Task.CompletedTask;
        }

        protected async Task Create(object viewModel)
        {
            try
            {
                window = await LoadWindowAsync<Window>();
                if (window == null)
                    throw new NotFoundException($"Not found the window named \"{ViewName}\".");

                if (viewModel != null)
                    window.SetDataContext(viewModel);

                window.Create();
            }
            catch (Exception e)
            {
                window = null;
                throw e;
            }
        }

        protected async Task Show(object viewModel, bool waitDismissed, bool ignoreAnimation = false)
        {
            try
            {
                if (window == null)
                    await Create(viewModel);

                window.WaitDismissed().Callbackable().OnCallback(r =>
                {
                    window = null;
                });

                await window.Show(ignoreAnimation);

                if (waitDismissed)
                    await window.WaitDismissed();
            }
            catch (Exception e)
            {
                if (window != null)
                    await window.Dismiss(ignoreAnimation);

                window = null;
                throw e;
            }
        }

        protected async Task Hide(bool ignoreAnimation = false)
        {
            if (window != null)
                await window.Hide(ignoreAnimation);
        }

        protected async Task Dismiss(bool ignoreAnimation = false)
        {
            if (window != null)
                await window.Dismiss(ignoreAnimation);
        }
    }
}
