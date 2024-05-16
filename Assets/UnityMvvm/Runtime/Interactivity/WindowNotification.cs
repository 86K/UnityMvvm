

namespace Fusion.Mvvm
{
    public class WindowNotification
    {
        public static WindowNotification CreateShowNotification(bool ignoreAnimation = true, bool waitDismissed = false)
        {
            return new WindowNotification(WindowActionType.SHOW, ignoreAnimation, null, waitDismissed);
        }

        public static WindowNotification CreateShowNotification(object viewModel, bool ignoreAnimation = true, bool waitDismissed = false)
        {
            return new WindowNotification(WindowActionType.SHOW, ignoreAnimation, viewModel, waitDismissed);
        }

        public static WindowNotification CreateHideNotification(bool ignoreAnimation = true)
        {
            return new WindowNotification(WindowActionType.HIDE, ignoreAnimation);
        }

        public static WindowNotification CreateDismissNotification(bool ignoreAnimation = true)
        {
            return new WindowNotification(WindowActionType.DISMISS, ignoreAnimation);
        }

        public WindowNotification(WindowActionType actionType) : this(actionType, true, null)
        {
        }

        public WindowNotification(WindowActionType actionType, bool ignoreAnimation) : this(actionType, ignoreAnimation, null)
        {
        }

        public WindowNotification(WindowActionType actionType, object viewModel, bool waitDismissed = false) : this(actionType, true, viewModel, waitDismissed)
        {
        }

        public WindowNotification(WindowActionType actionType, bool ignoreAnimation, object viewModel, bool waitDismissed = false)
        {
            IgnoreAnimation = ignoreAnimation;
            ActionType = actionType;
            ViewModel = viewModel;
            WaitDismissed = waitDismissed;
        }

        public bool IgnoreAnimation { get; private set; }

        public WindowActionType ActionType { get; private set; }

        public object ViewModel { get; private set; }

        public bool WaitDismissed { get; private set; }
    }

    public enum WindowActionType
    {
        CREATE,
        SHOW,
        HIDE,
        DISMISS
    }
}
