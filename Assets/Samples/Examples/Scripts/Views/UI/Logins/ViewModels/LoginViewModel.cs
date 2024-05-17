using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class LoginViewModel : ViewModelBase
    {
        private const string LAST_USERNAME_KEY = "LAST_USERNAME";

        private readonly ObservableDictionary<string, string> errors = new ObservableDictionary<string, string>();
        private string username;
        private string password;
        private readonly SimpleCommand loginCommand;
        private readonly SimpleCommand cancelCommand;

        private Account account;
        
        private readonly IAccountService accountService;

        private readonly InteractionRequest interactionFinished;
        private readonly InteractionRequest<ToastNotification> toastRequest;

        public LoginViewModel(IAccountService accountService)
        {
            this.accountService = accountService;

            interactionFinished = new InteractionRequest(this);
            toastRequest = new InteractionRequest<ToastNotification>(this);

            loginCommand = new SimpleCommand(Login);
            cancelCommand = new SimpleCommand(() =>
            {
                interactionFinished.Raise();
            });
        }

        public IInteractionRequest InteractionFinished => interactionFinished;

        public IInteractionRequest ToastRequest => toastRequest;

        public ObservableDictionary<string, string> Errors => errors;

        public string Username
        {
            get => username;
            set
            {
                if (Set(ref username, value))
                {
                    ValidateUsername();
                }
            }
        }

        public string Password
        {
            get => password;
            set
            {
                if (Set(ref password, value))
                {
                    ValidatePassword();
                }
            }
        }

        private bool ValidateUsername()
        {
            if (string.IsNullOrEmpty(username) || !Regex.IsMatch(username, "^[a-zA-Z0-9_-]{4,12}$"))
            {
                // errors["username"] = localization.GetText("login.validation.username.error", "Please enter a valid username.");
                return false;
            }
            else
            {
                errors.Remove("username");
                return true;
            }
        }

        private bool ValidatePassword()
        {
            if (string.IsNullOrEmpty(password) || !Regex.IsMatch(password, "^[a-zA-Z0-9_-]{4,12}$"))
            {
                // errors["password"] = localization.GetText("login.validation.password.error", "Please enter a valid password.");
                return false;
            }
            else
            {
                errors.Remove("password");
                return true;
            }
        }

        public ICommand LoginCommand => loginCommand;

        public ICommand CancelCommand => cancelCommand;

        public Account Account => account;

        public async void Login()
        {
            try
            {
                Debug.LogWarning($"login start. username:{username} password:{password}");

                this.account = null;
                loginCommand.Enabled = false;
                if (!(ValidateUsername() && ValidatePassword()))
                    return;

                Account account = await accountService.Login(username, password);
                if (account != null)
                {
                    this.account = account;
                    interactionFinished.Raise();
                }
                else
                {
                    
                    // var tipContent = localization.GetText("login.failure.tip", "Login failure.");
                    toastRequest.Raise(new ToastNotification("", 2f));
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exception:{e}");

                // var tipContent = localization.GetText("login.exception.tip", "Login exception.");
                toastRequest.Raise(new ToastNotification("", 2f));
            }
            finally
            {
                loginCommand.Enabled = true;
            }
        }

        public Task<Account> GetAccount()
        {
            return accountService.GetAccount(Username);
        }
    }
}