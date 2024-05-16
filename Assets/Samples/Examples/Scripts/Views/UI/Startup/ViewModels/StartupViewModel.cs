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
using Loxodon.Framework.Commands;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Interactivity;
using Loxodon.Framework.Localizations;
using Loxodon.Framework.Messaging;
using Loxodon.Framework.ViewModels;
using UnityEngine;

namespace Loxodon.Framework.Examples
{
    public class StartupViewModel : ViewModelBase
    {
        private readonly ProgressBar progressBar = new ProgressBar();
        private readonly SimpleCommand command;
        private readonly Localization localization;

        //public InteractionRequest<LoginViewModel> LoginRequest;
        public AsyncInteractionRequest<WindowNotification> LoginRequest { get; private set; }
        public AsyncInteractionRequest<ProgressBar> LoadSceneRequest { get; private set; }
        public InteractionRequest DismissRequest { get; private set; }


        public StartupViewModel() : this(null)
        {
        }

        public StartupViewModel(IMessenger messenger) : base(messenger)
        {
            ApplicationContext context = Context.GetApplicationContext();
            localization = context.GetService<Localization>();
            var accountService = context.GetService<IAccountService>();
            var globalPreferences = context.GetGlobalPreferences();

            //this.LoginRequest = new InteractionRequest<LoginViewModel>(this);          
            LoginRequest = new AsyncInteractionRequest<WindowNotification>(this);
            LoadSceneRequest = new AsyncInteractionRequest<ProgressBar>(this);
            DismissRequest = new InteractionRequest(this);

            var loginViewModel = new LoginViewModel(accountService, localization, globalPreferences);
            //this.command = new SimpleCommand(() =>
            //{
            //    this.LoginRequest.Raise(loginViewModel, vm =>
            //    {
            //        this.command.Enabled = true;

            //        if (vm.Account != null)
            //            this.LoadScene();
            //    });
            //});

            command = new SimpleCommand(async () =>
            {
                command.Enabled = false;
                await LoginRequest.Raise(WindowNotification.CreateShowNotification(loginViewModel, false, true));
                command.Enabled = true;
                if (loginViewModel.Account != null)
                {
                    await LoadSceneRequest.Raise(ProgressBar);
                    DismissRequest.Raise();
                }
            });
        }

        public ProgressBar ProgressBar => progressBar;

        public ICommand Click => command;

        /// <summary>
        /// Simulate a unzip task.
        /// </summary>
        public async void Unzip()
        {
            command.Enabled = false;
            progressBar.Enable = true;
            ProgressBar.Tip = R.startup_progressbar_tip_unziping;

            try
            {
                var progress = 0f;
                while (progress < 1f)
                {
                    progress += 0.01f;
                    ProgressBar.Progress = progress;/* update progress */
                    await new WaitForSecondsRealtime(0.02f);
                }
            }
            finally
            {
                command.Enabled = true;
                progressBar.Enable = false;
                progressBar.Tip = "";
                command.Execute(null);
            }
        }
    }
}