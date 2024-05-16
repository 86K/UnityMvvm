using System;

namespace Fusion.Mvvm
{
    public static class WindowExtensions
    {
        /// <summary>
        /// wait until the window is dismissed
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static IAsyncResult WaitDismissed(this Window window)
        {
            AsyncResult result = new AsyncResult();
            EventHandler handler = null;
            handler = (sender, eventArgs) =>
            {
                window.OnDismissed -= handler;
                result.SetResult(null);
            };
            window.OnDismissed += handler;
            return result;
        }

        /// <summary>
        /// wait until the view is disabled.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static IAsyncResult WaitDisabled(this UIView view)
        {
            AsyncResult result = new AsyncResult();
            EventHandler handler = null;
            handler = (sender, eventArgs) =>
            {
                view.OnDisabled -= handler;
                result.SetResult(null);
            };
            view.OnDisabled += handler;
            return result;
        }
    }
}
