using System;

namespace Fusion.Mvvm
{
    public class ToastInteractionAction : InteractionActionBase<ToastNotification>
    {
        private readonly string viewName;
        private readonly IUIViewGroup viewGroup;

        public ToastInteractionAction(IUIViewGroup viewGroup) : this(viewGroup, null)
        {
        }

        public ToastInteractionAction(IUIViewGroup viewGroup, string viewName)
        {
            this.viewGroup = viewGroup;
            this.viewName = viewName;
        }

        public override void Action(ToastNotification notification, Action callback)
        {
            if (notification == null)
                return;

            Toast.Show(viewName, viewGroup, notification.Message, notification.Duration, null, callback);
        }
    }
}
