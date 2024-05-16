

using System;

namespace Fusion.Mvvm
{
    public class AlertDialogViewModel : ViewModelBase
    {
        protected string title;
        protected string message;
        protected string confirmButtonText;
        protected string neutralButtonText;
        protected string cancelButtonText;
        protected bool canceledOnTouchOutside;
        protected bool closed;
        protected int result;
        protected Action<int> click;

        /// <summary>
        /// The title of the dialog box. This may be null.
        /// </summary>
        public virtual string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        /// <summary>
        /// The message to be shown to the user.
        /// </summary>
        public virtual string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        /// <summary>
        /// The text shown in the "confirm" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string ConfirmButtonText
        {
            get => confirmButtonText;
            set => Set(ref confirmButtonText, value);
        }

        /// <summary>
        /// The text shown in the "neutral" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string NeutralButtonText
        {
            get => neutralButtonText;
            set => Set(ref neutralButtonText, value);
        }

        /// <summary>
        /// The text shown in the "cancel" button in the dialog box. 
        /// If left null, the button will be invisible.
        /// </summary>
        public virtual string CancelButtonText
        {
            get => cancelButtonText;
            set => Set(ref cancelButtonText, value);
        }

        /// <summary>
        /// Whether the dialog box is canceled when 
        /// touched outside the window's bounds. 
        /// </summary>
        public virtual bool CanceledOnTouchOutside
        {
            get => canceledOnTouchOutside;
            set => Set(ref canceledOnTouchOutside, value);
        }

        /// <summary>
        /// A callback that should be executed after
        /// the dialog box is closed by the user. The callback method will get a boolean
        /// parameter indicating if the "confirm" button (true) or the "cancel" button
        /// (false) was pressed by the user.
        /// </summary>
        public virtual Action<int> Click
        {
            get => click;
            set => Set(ref click, value);
        }

        /// <summary>
        /// The dialog box has been closed.
        /// </summary>
        public virtual bool Closed
        {
            get => closed;
            protected set => Set(ref closed, value);
        }

        /// <summary>
        /// result
        /// </summary>
        public virtual int Result => result;

        public virtual void OnClick(int which)
        {
            try
            {
                result = which;
                var click = Click;
                if (click != null)
                    click(which);
            }
            catch (Exception) { }
            finally
            {
                Closed = true;
            }
        }
    }
}
