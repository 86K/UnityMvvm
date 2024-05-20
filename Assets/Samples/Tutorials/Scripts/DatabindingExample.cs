

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Fusion.Mvvm
{
    public class DataBindingAccount : ObservableObject
    {
        private int id;
        private string username;
        private string password;
        private string email;
        private DateTime birthday;
        private readonly ObservableProperty<string> address = new ObservableProperty<string>();

        public int ID
        {
            get => id;
            set => Set(ref id, value);
        }

        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }

        public string Password
        {
            get => password;
            set => Set(ref password, value);
        }

        public string Email
        {
            get => email;
            set => Set(ref email, value);
        }

        public DateTime Birthday
        {
            get => birthday;
            set => Set(ref birthday, value);
        }

        public ObservableProperty<string> Address => address;
    }

    public class AccountViewModel : ViewModelBase
    {
        private DataBindingAccount account;
        private bool remember;
        private string username;
        private string email;
        private ObservableDictionary<string, string> errors = new ObservableDictionary<string, string>();

        public DataBindingAccount Account
        {
            get => account;
            set => Set(ref account, value);
        }

        public string Username
        {
            get => username;
            set => Set(ref username, value);
        }

        public string Email
        {
            get => email;
            set => Set(ref email, value);
        }

        public bool Remember
        {
            get => remember;
            set => Set(ref remember, value);
        }

        public ObservableDictionary<string, string> Errors
        {
            get => errors;
            set => Set(ref errors, value);
        }

        public void OnUsernameValueChanged(string value)
        {
            Debug.LogFormat("Username ValueChanged:{0}", value);
        }

        public void OnEmailValueChanged(string value)
        {
            Debug.LogFormat("Email ValueChanged:{0}", value);
        }

        public void OnSubmit()
        {
            if (string.IsNullOrEmpty(Username) || !Regex.IsMatch(Username, "^[a-zA-Z0-9_-]{4,12}$"))
            {
                errors["errorMessage"] = "Please enter a valid username.";
                return;
            }

            if (string.IsNullOrEmpty(Email) || !Regex.IsMatch(Email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                errors["errorMessage"] = "Please enter a valid email.";
                return;
            }

            errors.Clear();
            Account.Username = Username;
            Account.Email = Email;
        }
    }

    public class DatabindingExample : UIView
    {
        public Text description;
        public Text title;
        public Text username;
        public Text password;
        public Text email;
        public Text birthday;
        public Text address;
        public Text remember;

        public Text errorMessage;

        public InputField usernameEdit;
        public InputField emailEdit;
        public Toggle rememberEdit;
        public Button submit;
        
        protected override void Awake()
        {
            Context context = Context.GetGlobalContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();
        }

        protected override void Start()
        {
            DataBindingAccount account = new DataBindingAccount()
            {
                ID = 1,
                Username = "test",
                Password = "test",
                Email = "yangpc.china@gmail.com",
                Birthday = new DateTime(2000, 3, 3)
            };
            account.Address.Value = "beijing";

            AccountViewModel accountViewModel = new AccountViewModel()
            {
                Account = account
            };

            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = accountViewModel;

            
            BindingSet<DatabindingExample, AccountViewModel> bindingSet = this.CreateBindingSet<DatabindingExample, AccountViewModel>();
            //			bindingSet.Bind (this.username).For ("text").To ("Account.Username").OneWay ();
            //			bindingSet.Bind (this.password).For ("text").To ("Account.Password").OneWay ();
            bindingSet.Bind(username).For(v => v.text).To(vm => vm.Account.Username).OneWay();
            bindingSet.Bind(password).For(v => v.text).To(vm => vm.Account.Password).OneWay();
            bindingSet.Bind(email).For(v => v.text).To(vm => vm.Account.Email).OneWay();
            bindingSet.Bind(remember).For(v => v.text).To(vm => vm.Remember).OneWay();
            bindingSet.Bind(birthday).For(v => v.text).ToExpression(vm => string.Format("{0} ({1})",
             vm.Account.Birthday.ToString("yyyy-MM-dd"), (DateTime.Now.Year - vm.Account.Birthday.Year))).OneWay();

            bindingSet.Bind(address).For(v => v.text).To(vm => vm.Account.Address).OneWay();
            // bindingSet.Bind(description).For(v => v.text).ToExpression(vm => localization.GetFormattedText("databinding.tutorials.description", vm.Account.Username, vm.Username)).OneWay();

            bindingSet.Bind(errorMessage).For(v => v.text).To(vm => vm.Errors["errorMessage"]).OneWay();

            bindingSet.Bind(usernameEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Username).TwoWay();
            bindingSet.Bind(usernameEdit).For(v => v.onValueChanged).To<string>(vm => vm.OnUsernameValueChanged);
            bindingSet.Bind(emailEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Email).TwoWay();
            bindingSet.Bind(emailEdit).For(v => v.onValueChanged).To<string>(vm => vm.OnEmailValueChanged);
            bindingSet.Bind(rememberEdit).For(v => v.isOn, v => v.onValueChanged).To(vm => vm.Remember).TwoWay();
            bindingSet.Bind(submit).For(v => v.onClick).To(vm => vm.OnSubmit);
            
            bindingSet.Build();

            // BindingSet<DatabindingExample> staticBindingSet = this.CreateBindingSet<DatabindingExample>();
            //  staticBindingSet.Bind(title).For(v => v.text).To(()=>TestStaticField).OneTime();
            // staticBindingSet.Build();
        }

        private static string TestStaticField = "测试BindingSet<TTarget>进行静态绑定";
        // private static string TestStaticProperty => "测试BindingSet<TTarget>进行静态绑定"; ()=>TestStaticProperty 报错
        static void TestStaticMethod(){}// ()=>TestStaticMethod  Fusion.Mvvm.ProxyInvoker
    }

}