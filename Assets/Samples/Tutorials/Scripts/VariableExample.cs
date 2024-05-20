

using System;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class VariableViewModel : ViewModelBase
    {
        private bool remember;
        private string username;
        private string email;
        private Color color;
        private Vector3 vector;

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

        public Vector3 Vector
        {
            get => vector;
            set => Set(ref vector, value);
        }

        public Color Color
        {
            get => color;
            set => Set(ref color, value);
        }

        public void OnSubmit()
        {
            Debug.LogFormat("username:{0} email:{1} remember:{2} vector:{3} color:{4}", username, email, remember, vector, color);
        }
    }
    
    public class VariableExample : UIView
    {
        public VariableArray variables;

        protected override void Awake()
        {
            Context context = Context.GetGlobalContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();
        }

        protected override void Start()
        {
            VariableViewModel viewModel = new VariableViewModel()
            {
                Username = "test",
                Email = "yangpc.china@gmail.com",
                Remember = true
            };

            viewModel.Color = variables.Get<Color>("color");
            viewModel.Vector = variables.Get<Vector3>("vector");

            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = viewModel;

            
            BindingSet<VariableExample, VariableViewModel> bindingSet = this.CreateBindingSet<VariableExample, VariableViewModel>();
            bindingSet.Bind(variables.Get<InputField>("username")).For(v => v.text, v => v.onEndEdit).To(vm => vm.Username).TwoWay();
            bindingSet.Bind(variables.Get<InputField>("email")).For(v => v.text, v => v.onEndEdit).To(vm => vm.Email).TwoWay();
            bindingSet.Bind(variables.Get<Toggle>("remember")).For(v => v.isOn, v => v.onValueChanged).To(vm => vm.Remember).TwoWay();
            bindingSet.Bind(variables.Get<Button>("submit")).For(v => v.onClick).To(vm => vm.OnSubmit);
            bindingSet.Build();
        }

    }
}
