using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class DialogInteractionAction : InteractionActionBase<object>
    {
        private readonly string viewName;
        public DialogInteractionAction(string viewName)
        {
            this.viewName = viewName;
        }

        public override void Action(object viewModel, Action callback)
        {
            Window window = null;
            try
            {
                Context context = Context.GetGlobalContext();
                IUIViewLocator locator = context.GetService<IUIViewLocator>();
                if (locator == null)
                    throw new Exception("Not found the \"IUIViewLocator\".");

                if (string.IsNullOrEmpty(viewName))
                    throw new ArgumentNullException("The view name is null.");

                window = locator.LoadView<Window>(viewName);
                if (window == null)
                    throw new Exception($"Not found the dialog window named \"{viewName}\".");

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
                    window.SetDataContext(viewModel);
                }
                                
                window.Create();
                window.WaitDismissed().Callbackable().OnCallback((r) =>
                {
                    callback?.Invoke();
                    callback = null;
                });
                window.Show(true);
            }
            catch (Exception e)
            {
                callback?.Invoke();
                callback = null;

                if (window != null)
                    window.Dismiss();

                Debug.LogWarning(e);
            }
        }
    }
}
