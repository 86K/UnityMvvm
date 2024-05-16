

namespace Fusion.Mvvm
{
    public class DialogNotification : Notification
    {
        private readonly string confirmButtonText;
        private readonly string neutralButtonText;
        private readonly string cancelButtonText;
        private readonly bool canceledOnTouchOutside;

        private int dialogResult;

        public DialogNotification(string title, string message, string confirmButtonText, bool canceledOnTouchOutside = true) : this(title, message, confirmButtonText, null, null, canceledOnTouchOutside)
        {
        }

        public DialogNotification(string title, string message, string confirmButtonText, string cancelButtonText, bool canceledOnTouchOutside = true) : this(title, message, confirmButtonText, null, cancelButtonText, canceledOnTouchOutside)
        {
        }

        public DialogNotification(string title, string message, string confirmButtonText, string neutralButtonText, string cancelButtonText, bool canceledOnTouchOutside = true) : base(title, message)
        {
            this.confirmButtonText = confirmButtonText;
            this.neutralButtonText = neutralButtonText;
            this.cancelButtonText = cancelButtonText;
            this.canceledOnTouchOutside = canceledOnTouchOutside;
        }

        public string ConfirmButtonText => confirmButtonText;

        public string NeutralButtonText => neutralButtonText;

        public string CancelButtonText => cancelButtonText;

        public bool CanceledOnTouchOutside => canceledOnTouchOutside;

        public int DialogResult
        {
            get => dialogResult;
            set => dialogResult = value;
        }
    }
}
