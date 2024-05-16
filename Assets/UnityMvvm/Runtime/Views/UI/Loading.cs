

using System;

namespace Fusion.Mvvm
{
    public class Loading : UIBase, IDisposable
    {
        private const string DEFAULT_VIEW_NAME = "UI/Loading";
        private static readonly object _lock = new object();
        private static int refCount = 0;
        private static LoadingWindow window;
        private static string viewName;
        private readonly bool ignoreAnimation;
        public static string ViewName
        {
            get => string.IsNullOrEmpty(viewName) ? DEFAULT_VIEW_NAME : viewName;
            set => viewName = value;
        }

        public static Loading Show(bool ignoreAnimation = false)
        {
            return new Loading(ignoreAnimation);
        }

        protected Loading(bool ignoreAnimation)
        {
            this.ignoreAnimation = ignoreAnimation;
            lock (_lock)
            {
                if (refCount <= 0)
                {
                    IUIViewLocator locator = GetUIViewLocator();
                    window = locator.LoadWindow<LoadingWindow>(ViewName);
                    window.Create();
                    window.Show(this.ignoreAnimation);
                }
                refCount++;
            }
        }

        #region IDisposable Support
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                Executors.RunOnMainThread(() =>
                {
                    lock (_lock)
                    {
                        refCount--;
                        if (refCount <= 0)
                        {
                            window.Dismiss(ignoreAnimation);
                            window = null;
                        }
                    }
                });
            }
        }

        ~Loading()
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
