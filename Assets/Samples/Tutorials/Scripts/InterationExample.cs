

using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class InterationViewModel : ViewModelBase
    {
        public readonly InteractionRequest<DialogNotification> AlertDialogRequest = new InteractionRequest<DialogNotification>();
        public readonly AsyncInteractionRequest<DialogNotification> AsyncAlertDialogRequest = new AsyncInteractionRequest<DialogNotification>();
        public readonly InteractionRequest<ToastNotification> ToastRequest = new InteractionRequest<ToastNotification>();
        public readonly InteractionRequest<VisibilityNotification> LoadingRequest = new InteractionRequest<VisibilityNotification>();

        public InterationViewModel()
        {
            OpenAlertDialog = new SimpleCommand(() =>
            {
                OpenAlertDialog.Enabled = false;

                DialogNotification notification = new DialogNotification("Interation Example", "This is a dialog test.", "Yes", "No", true);

                Action<DialogNotification> callback = n =>
                {
                    OpenAlertDialog.Enabled = true;

                    if (n.DialogResult == AlertDialog.BUTTON_POSITIVE)
                    {
                        Debug.LogFormat("Click: Yes");
                    }
                    else if (n.DialogResult == AlertDialog.BUTTON_NEGATIVE)
                    {
                        Debug.LogFormat("Click: No");
                    }
                };

                AlertDialogRequest.Raise(notification, callback);
            });

            AsyncOpenAlertDialog = new SimpleCommand(async () =>
            {
                AsyncOpenAlertDialog.Enabled = false;
                DialogNotification notification = new DialogNotification("Interation Example", "This is a dialog test.", "Yes", "No", true);
                await AsyncAlertDialogRequest.Raise(notification);
                AsyncOpenAlertDialog.Enabled = true;
                if (notification.DialogResult == AlertDialog.BUTTON_POSITIVE)
                {
                    Debug.LogFormat("Click: Yes");
                }
                else if (notification.DialogResult == AlertDialog.BUTTON_NEGATIVE)
                {
                    Debug.LogFormat("Click: No");
                }
            });

            ShowToast = new SimpleCommand(() =>
            {
                ToastNotification notification = new ToastNotification("This is a toast test.", 2f);
                ToastRequest.Raise(notification);
            });

            ShowLoading = new SimpleCommand(() =>
            {
                VisibilityNotification notification = new VisibilityNotification(true);
                LoadingRequest.Raise(notification);
            });

            HideLoading = new SimpleCommand(() =>
            {
                VisibilityNotification notification = new VisibilityNotification(false);
                LoadingRequest.Raise(notification);
            });

        }

        public SimpleCommand OpenAlertDialog { get; }
        public SimpleCommand AsyncOpenAlertDialog { get; }
        public SimpleCommand ShowToast { get; }
        public SimpleCommand ShowLoading { get; }
        public SimpleCommand HideLoading { get; }
    }

    public class InterationExample : WindowView
    {
        public Button openAlert;
        public Button asyncOpenAlert;
        public Button showToast;
        public Button showLoading;
        public Button hideLoading;

        private readonly List<Loading> list = new List<Loading>();

        private LoadingInteractionAction loadingInteractionAction;
        private ToastInteractionAction toastInteractionAction;
        private AsyncDialogInteractionAction dialogInteractionAction;

        protected override void Awake()
        {
            Context context = Context.GetGlobalContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            
            IServiceContainer container = context.GetContainer();
            container.Register<IUIViewLocator>(new DefaultUIViewLocator());
        }

        protected override void Start()
        {
            loadingInteractionAction = new LoadingInteractionAction();
            toastInteractionAction = new ToastInteractionAction(this);
            dialogInteractionAction = new AsyncDialogInteractionAction("UI/AlertDialog");

            InterationViewModel viewModel = new InterationViewModel();
            this.SetDataContext(viewModel);

            
            BindingSet<InterationExample, InterationViewModel> bindingSet = this.CreateBindingSet<InterationExample, InterationViewModel>();

            
            bindingSet.Bind().For(v => v.OnOpenAlert).To(vm => vm.AlertDialogRequest);

            
            bindingSet.Bind().For(v => v.dialogInteractionAction).To(vm => vm.AsyncAlertDialogRequest);

            
            bindingSet.Bind().For(v => v.toastInteractionAction).To(vm => vm.ToastRequest);
            
            //bindingSet.Bind().For(v => v.OnShowToast).To(vm => vm.ToastRequest);

            
            bindingSet.Bind().For(v => v.loadingInteractionAction).To(vm => vm.LoadingRequest);
            
            //bindingSet.Bind().For(v => v.OnShowOrHideLoading).To(vm => vm.LoadingRequest);

            
            bindingSet.Bind(openAlert).For(v => v.onClick).To(vm => vm.OpenAlertDialog);
            bindingSet.Bind(asyncOpenAlert).For(v => v.onClick).To(vm => vm.AsyncOpenAlertDialog);
            bindingSet.Bind(showToast).For(v => v.onClick).To(vm => vm.ShowToast);
            bindingSet.Bind(showLoading).For(v => v.onClick).To(vm => vm.ShowLoading);
            bindingSet.Bind(hideLoading).For(v => v.onClick).To(vm => vm.HideLoading);

            bindingSet.Build();
        }

        private void OnOpenAlert(object sender, InteractionEventArgs args)
        {
            DialogNotification notification = args.Context as DialogNotification;
            var callback = args.Callback;

            if (notification == null)
                return;

            AlertDialog.ShowMessage(notification.Message, notification.Title, notification.ConfirmButtonText, null, notification.CancelButtonText, notification.CanceledOnTouchOutside, (result) =>
              {
                  notification.DialogResult = result;
                  callback?.Invoke();
              });
        }

        private void OnShowToast(object sender, InteractionEventArgs args)
        {
            if (!(args.Context is ToastNotification notification))
                return;

            Toast.Show(this, notification.Message, notification.Duration);
        }

        private void OnShowOrHideLoading(object sender, InteractionEventArgs args)
        {
            if (!(args.Context is VisibilityNotification notification))
                return;

            if (notification.Visible)
            {
                list.Add(Loading.Show());
            }
            else
            {
                if (list.Count <= 0)
                    return;

                Loading loading = list[0];
                loading.Dispose();
                list.RemoveAt(0);
            }
        }
    }
}
