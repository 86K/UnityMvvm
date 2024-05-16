using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class DialogServiceExampleViewModel : ViewModelBase
    {
        private readonly SimpleCommand openAlertDialog;
        private readonly SimpleCommand openAlertDialog2;

        private readonly IDialogService dialogService;

        public DialogServiceExampleViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            openAlertDialog = new SimpleCommand(() =>
            {
                openAlertDialog.Enabled = false;
                IAsyncResult<int> result = this.dialogService.ShowDialog("Dialog Service Example", "This is a dialog test.", "Yes", "No", null, true);
                result.Callbackable().OnCallback(r =>
                {
                    if (r.Result == AlertDialog.BUTTON_POSITIVE)
                    {
                        Debug.LogFormat("Click: Yes");
                    }
                    else if (r.Result == AlertDialog.BUTTON_NEGATIVE)
                    {
                        Debug.LogFormat("Click: No");
                    }
                    openAlertDialog.Enabled = true;
                });
            });

            openAlertDialog2 = new SimpleCommand(() =>
            {
                openAlertDialog2.Enabled = false;

                AlertDialogViewModel viewModel = new AlertDialogViewModel();
                viewModel.Title = "Dialog Service Example";
                viewModel.Message = "This is a dialog test.";
                viewModel.ConfirmButtonText = "OK";

                IAsyncResult<AlertDialogViewModel> result = this.dialogService.ShowDialog("UI/AlertDialog", viewModel);
                result.Callbackable().OnCallback(r =>
                {
                    AlertDialogViewModel vm = r.Result;
                    if (vm.Result == AlertDialog.BUTTON_POSITIVE)
                    {
                        Debug.LogFormat("Click: OK");
                    }
                    openAlertDialog2.Enabled = true;
                });
            });
        }

        public ICommand OpenAlertDialog => openAlertDialog;
        public ICommand OpenAlertDialog2 => openAlertDialog2;
    }

    public class DialogServiceExample : WindowView
    {
        public Button openAlert;
        public Button openAlert2;

        protected override void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            
            IServiceContainer container = context.GetContainer();
            container.Register<IUIViewLocator>(new DefaultUIViewLocator());

            CultureInfo cultureInfo = Locale.GetCultureInfo();
            var localization = Localization.Current;
            localization.CultureInfo = cultureInfo;
            // localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));
            container.Register(localization); 

            
            IDialogService dialogService = new DefaultDialogService();
            container.Register<IDialogService>(dialogService);
        }

        protected override void Start()
        {
            ApplicationContext context = Context.GetApplicationContext();
            IDialogService dialogService = context.GetService<IDialogService>();
            DialogServiceExampleViewModel viewModel = new DialogServiceExampleViewModel(dialogService);
            this.SetDataContext(viewModel);

            
            BindingSet<DialogServiceExample, DialogServiceExampleViewModel> bindingSet = this.CreateBindingSet<DialogServiceExample, DialogServiceExampleViewModel>();

            
            bindingSet.Bind(openAlert).For(v => v.onClick).To(vm => vm.OpenAlertDialog);
            bindingSet.Bind(openAlert2).For(v => v.onClick).To(vm => vm.OpenAlertDialog2);

            bindingSet.Build();
        }
    }
}
