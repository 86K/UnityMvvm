using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class AlertDialogWindowBase : Window
    {
        public GameObject Content;

        protected IUIView contentView;

        protected AlertDialogViewModel viewModel;

        public virtual IUIView ContentView
        {
            get => contentView;
            set
            {
                if (contentView == value)
                    return;

                if (contentView != null)
                    Destroy(contentView.Owner);

                contentView = value;
                if (contentView != null && contentView.Owner != null && Content != null)
                {
                    contentView.Visibility = true;
                    contentView.Transform.SetParent(Content.transform, false);
                }
            }
        }

        public virtual AlertDialogViewModel ViewModel
        {
            get => viewModel;
            set
            {
                if (viewModel == value)
                    return;

                viewModel = value;
                OnChangeViewModel();
            }
        }
        protected override void OnCreate(IBundle bundle)
        {
            WindowType = WindowType.DIALOG;
        }

        protected abstract void OnChangeViewModel();

        public abstract void Cancel();
    }
}
