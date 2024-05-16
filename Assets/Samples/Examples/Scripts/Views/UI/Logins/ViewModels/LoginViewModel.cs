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

using Loxodon.Framework.Commands;
using Loxodon.Framework.Interactivity;
using Loxodon.Framework.Localizations;
using Loxodon.Framework.Observables;
using Loxodon.Framework.Prefs;
using Loxodon.Framework.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Loxodon.Framework.Examples
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

        private readonly Preferences globalPreferences;
        private readonly IAccountService accountService;
        private readonly Localization localization;

        private readonly InteractionRequest interactionFinished;
        private readonly InteractionRequest<ToastNotification> toastRequest;

        public LoginViewModel(IAccountService accountService, Localization localization, Preferences globalPreferences)
        {
            this.localization = localization;
            this.accountService = accountService;
            this.globalPreferences = globalPreferences;

            interactionFinished = new InteractionRequest(this);
            toastRequest = new InteractionRequest<ToastNotification>(this);

            if (username == null)
            {
                username = globalPreferences.GetString(LAST_USERNAME_KEY, "");
            }

            loginCommand = new SimpleCommand(Login);
            cancelCommand = new SimpleCommand(() =>
            {
                interactionFinished.Raise();/* Request to close the login window */
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
                errors["username"] = localization.GetText("login.validation.username.error", "Please enter a valid username.");
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
                errors["password"] = localization.GetText("login.validation.password.error", "Please enter a valid password.");
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
                Debug.LogWarning(string.Format("login start. username:{0} password:{1}", username, password));

                this.account = null;
                loginCommand.Enabled = false;/*by databinding, auto set button.interactable = false. */
                if (!(ValidateUsername() && ValidatePassword()))
                    return;

                Account account = await accountService.Login(username, password);
                if (account != null)
                {
                    /* login success */
                    globalPreferences.SetString(LAST_USERNAME_KEY, username);
                    globalPreferences.Save();
                    this.account = account;
                    interactionFinished.Raise();/* Interaction completed, request to close the login window */
                }
                else
                {
                    /* Login failure */
                    var tipContent = localization.GetText("login.failure.tip", "Login failure.");
                    toastRequest.Raise(new ToastNotification(tipContent, 2f));/* show toast */
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("Exception:{0}", e));

                var tipContent = localization.GetText("login.exception.tip", "Login exception.");
                toastRequest.Raise(new ToastNotification(tipContent, 2f));/* show toast */
            }
            finally
            {
                loginCommand.Enabled = true;/*by databinding, auto set button.interactable = true. */
            }
        }

        public Task<Account> GetAccount()
        {
            return accountService.GetAccount(Username);
        }
    }
}