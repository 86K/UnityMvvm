

namespace Fusion.Mvvm
{
    public abstract class ToastViewBase : UIView
    {
        protected string content;
        public string Content 
        {
            get => content;
            set 
            {
                if (string.Equals(content,value))
                    return;

                content = value;
                OnContentChanged();
            }
        }

        protected abstract void OnContentChanged();
    }
}
