

namespace Fusion.Mvvm
{
    public class Notification
    {
        private readonly string title;
        private readonly string message;

        public Notification(string message) : this(null, message)
        {
        }

        public Notification(string title, string message)
        {
            this.title = title;
            this.message = message;
        }

        public string Title => title;

        public string Message => message;
    }
}
