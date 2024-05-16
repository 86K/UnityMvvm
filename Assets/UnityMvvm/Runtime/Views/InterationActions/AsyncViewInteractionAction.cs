using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fusion.Mvvm
{
    public class AsyncViewInteractionAction : AsyncLoadableInteractionActionBase<VisibilityNotification>
    {
        private readonly IViewGroup viewGroup;
        private UIView view;
        private readonly bool autoDestroy;
        public AsyncViewInteractionAction(string viewName, IViewGroup viewGroup, bool autoDestroy = true) : this(viewName, viewGroup, null, autoDestroy)
        {
        }

        public AsyncViewInteractionAction(string viewName, IViewGroup viewGroup, IUIViewLocator locator, bool autoDestroy = true) : base(viewName, locator)
        {
            this.viewGroup = viewGroup;
            this.autoDestroy = autoDestroy;
        }

        public AsyncViewInteractionAction(UIView view, bool autoDestroy = false) : base(null, null, null)
        {
            this.view = view;
            this.autoDestroy = autoDestroy;
        }

        public UIView View => view;

        public override Task Action(VisibilityNotification notification)
        {
            if (notification.Visible)
                return Show(notification.ViewModel, notification.WaitDisabled);
            else
                return Hide();
        }

        protected virtual async Task Show(object viewModel, bool waitDisabled)
        {
            try
            {
                if (view == null)
                    view = await LoadViewAsync<UIView>();

                if (view == null)
                    throw new NotFoundException($"Not found the view named \"{ViewName}\".");

                if (viewGroup != null)
                    viewGroup.AddView(view);

                if (autoDestroy)
                {
                    view.WaitDisabled().Callbackable().OnCallback(r =>
                    {
                        view = null;
                    });
                }

                if (viewModel != null)
                    view.SetDataContext(viewModel);

                view.Visibility = true;

                if (waitDisabled)
                    await view.WaitDisabled();
            }
            catch (Exception e)
            {
                if (autoDestroy)
                    Destroy();
                throw e;
            }
        }

        protected Task Hide()
        {
            if (view != null)
            {
                view.Visibility = false;
                if (autoDestroy)
                    Destroy();
            }
            return Task.CompletedTask;
        }

        private void Destroy()
        {
            if (view == null)
                return;

            GameObject go = view.Owner;
            if (go != null)
                Object.Destroy(go);
            view = null;
        }
    }
}
