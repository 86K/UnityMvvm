using System;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    public class AsyncDialogInteractionAction : AsyncLoadableInteractionActionBase<object>
    {
        private Window window;

        public AsyncDialogInteractionAction(string viewName) : base(viewName, null, null)
        {
        }

        public AsyncDialogInteractionAction(string viewName, IUIViewLocator locator) : base(viewName, locator)
        {
        }
        public Window Window => window;

        public override Task Action(object context)
        {
            if (context is WindowNotification notification)
            {
                bool ignoreAnimation = notification.IgnoreAnimation;
                switch (notification.ActionType)
                {
                    case WindowActionType.CREATE:
                        return Create(notification.ViewModel);
                    case WindowActionType.SHOW:
                        return Show(notification.ViewModel, ignoreAnimation);
                    case WindowActionType.HIDE:
                        return Hide(ignoreAnimation);
                    case WindowActionType.DISMISS:
                        return Dismiss(ignoreAnimation);
                }
                return Task.CompletedTask;
            }

            return Show(context);
        }

        protected async Task Create(object viewModel)
        {
            try
            {
                window = await LoadWindowAsync<Window>();
                if (window == null)
                    throw new NotFoundException($"Not found the dialog window named \"{ViewName}\".");

                if (window is AlertDialogWindowBase @base && viewModel is AlertDialogViewModel model)
                {
                    @base.ViewModel = model;
                }
                else if (window is AlertDialogWindowBase windowBase && viewModel is DialogNotification notification)
                {
                    AlertDialogViewModel dialogViewModel = new AlertDialogViewModel();
                    dialogViewModel.Message = notification.Message;
                    dialogViewModel.Title = notification.Title;
                    dialogViewModel.ConfirmButtonText = notification.ConfirmButtonText;
                    dialogViewModel.NeutralButtonText = notification.NeutralButtonText;
                    dialogViewModel.CancelButtonText = notification.CancelButtonText;
                    dialogViewModel.CanceledOnTouchOutside = notification.CanceledOnTouchOutside;
                    dialogViewModel.Click = (result) => notification.DialogResult = result;
                    windowBase.ViewModel = dialogViewModel;
                }
                else
                {
                    if (viewModel != null)
                        window.SetDataContext(viewModel);
                }

                window.Create();
            }
            catch (Exception e)
            {
                window = null;
                throw e;
            }
        }

        protected async Task Show(object viewModel, bool ignoreAnimation = false)
        {
            try
            {
                if (window == null)
                    await Create(viewModel);

                await window.Show(ignoreAnimation);
                await window.WaitDismissed();
                window = null;
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
