/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Binding;
using Loxodon.Framework.Binding.Builder;
using Loxodon.Framework.Commands;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Interactivity;
using Loxodon.Framework.Localizations;
using Loxodon.Framework.Services;
using Loxodon.Framework.ViewModels;
using Loxodon.Framework.Views;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Loxodon.Framework.Tutorials
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

            /* Initialize the ui view locator and register UIViewLocator */
            IServiceContainer container = context.GetContainer();
            container.Register<IUIViewLocator>(new DefaultUIViewLocator());

            CultureInfo cultureInfo = Locale.GetCultureInfo();
            var localization = Localization.Current;
            localization.CultureInfo = cultureInfo;
            localization.AddDataProvider(new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser()));
            container.Register(localization); 

            /* Initialize the dialog service */
            IDialogService dialogService = new DefaultDialogService();
            container.Register<IDialogService>(dialogService);
        }

        protected override void Start()
        {
            ApplicationContext context = Context.GetApplicationContext();
            IDialogService dialogService = context.GetService<IDialogService>();
            DialogServiceExampleViewModel viewModel = new DialogServiceExampleViewModel(dialogService);
            this.SetDataContext(viewModel);

            /* databinding */
            BindingSet<DialogServiceExample, DialogServiceExampleViewModel> bindingSet = this.CreateBindingSet<DialogServiceExample, DialogServiceExampleViewModel>();

            /* Binding command */
            bindingSet.Bind(openAlert).For(v => v.onClick).To(vm => vm.OpenAlertDialog);
            bindingSet.Bind(openAlert2).For(v => v.onClick).To(vm => vm.OpenAlertDialog2);

            bindingSet.Build();
        }
    }
}
