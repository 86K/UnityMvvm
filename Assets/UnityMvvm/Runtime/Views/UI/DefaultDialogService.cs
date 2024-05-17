using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class DefaultDialogService : IDialogService
    {
        public virtual IAsyncResult<int> ShowDialog(string title, string message)
        {
            return ShowDialog(title, message, null, null, null, true);
        }

        public virtual IAsyncResult<int> ShowDialog(string title, string message, string buttonText)
        {
            return ShowDialog(title, message, buttonText, null, null, false);
        }

        public virtual IAsyncResult<int> ShowDialog(string title, string message, string confirmButtonText, string cancelButtonText)
        {
            return ShowDialog(title, message, confirmButtonText, cancelButtonText, null, false);
        }

        public virtual IAsyncResult<int> ShowDialog(string title, string message, string confirmButtonText, string cancelButtonText, string neutralButtonText)
        {
            return ShowDialog(title, message, confirmButtonText, cancelButtonText, neutralButtonText, false);
        }

        public virtual IAsyncResult<int> ShowDialog(string title, string message, string confirmButtonText, string cancelButtonText, string neutralButtonText, bool canceledOnTouchOutside)
        {
            AsyncResult<int> result = new AsyncResult<int>();
            try
            {
                AlertDialog.ShowMessage(message, title, confirmButtonText, neutralButtonText, cancelButtonText, canceledOnTouchOutside, (which) => { result.SetResult(which); });
            }
            catch (Exception e)
            {
                result.SetException(e);
            }
            return result;
        }

        public virtual IAsyncResult<TViewModel> ShowDialog<TViewModel>(string viewName, TViewModel viewModel) where TViewModel : IViewModel
        {
            AsyncResult<TViewModel> result = new AsyncResult<TViewModel>();
            Window window = null;
            try
            {
                ApplicationContext context = Context.GetApplicationContext();
                IUIViewLocator locator = context.GetService<IUIViewLocator>();
                if (locator == null)
                {
                    Debug.Log("Not found the \"IUIViewLocator\".");

                    throw new NotFoundException("Not found the \"IUIViewLocator\".");
                }

                if (string.IsNullOrEmpty(viewName))
                    throw new ArgumentNullException("The view name is null.");

                window = locator.LoadView<Window>(viewName);
                if (window == null)
                {
                    Debug.LogWarning($"Not found the dialog window named \"{viewName}\".");

                    throw new NotFoundException($"Not found the dialog window named \"{viewName}\".");
                }

                if (window is AlertDialogWindowBase @base && viewModel is AlertDialogViewModel model)
                    @base.ViewModel = model;
                else
                    window.SetDataContext(viewModel);

                EventHandler handler = null;
                handler = (sender, eventArgs) =>
                {
                    window.OnDismissed -= handler;
                    result.SetResult(viewModel);
                };
                window.Create();
                window.OnDismissed += handler;
                window.Show(true);
            }
            catch (Exception e)
            {
                result.SetException(e);
                if (window != null)
                    window.Dismiss();
            }
            return result;
        }

        public IAsyncResult<IViewModel> ShowDialog(string viewName, IViewModel viewModel)
        {
            return ShowDialog<IViewModel>(viewName, viewModel);
        }
    }
}
