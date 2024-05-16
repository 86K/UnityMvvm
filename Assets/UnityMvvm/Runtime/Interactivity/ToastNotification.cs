

namespace Fusion.Mvvm
{
    public class ToastNotification
    {
        private readonly float duration;
        private readonly string message;

        public ToastNotification(string message) : this(message, 3f)
        {
        }

        public ToastNotification(string message, float duration)
        {
            this.duration = duration;
            this.message = message;
        }

        public float Duration => duration;

        public string Message => message;
    }
}
