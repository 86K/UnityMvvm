using UnityEngine;

namespace Fusion.Mvvm
{
    public class StartupViewModel : ViewModelBase
    {
        private readonly ProgressBar progressBar = new ProgressBar();
        private readonly SimpleCommand command;

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
            var accountService = context.GetService<IAccountService>();
            var globalPreferences = context.GetGlobalPreferences();

            //this.LoginRequest = new InteractionRequest<LoginViewModel>(this);          
            LoginRequest = new AsyncInteractionRequest<WindowNotification>(this);
            LoadSceneRequest = new AsyncInteractionRequest<ProgressBar>(this);
            DismissRequest = new InteractionRequest(this);

            var loginViewModel = new LoginViewModel(accountService, globalPreferences);
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
            // ProgressBar.Tip = R.startup_progressbar_tip_unziping;

            try
            {
                var progress = 0f;
                while (progress < 1f)
                {
                    progress += 0.01f;
                    ProgressBar.Progress = progress;
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