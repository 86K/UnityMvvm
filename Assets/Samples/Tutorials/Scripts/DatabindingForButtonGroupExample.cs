using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ButtonGroupViewModel : ViewModelBase
    {
        private string text;
        private readonly SimpleCommand<string> click;
        public ButtonGroupViewModel()
        {
            click = new SimpleCommand<string>(OnClick);
        }

        public string Text
        {
            get => text;
            set => Set(ref text, value);
        }

        public ICommand Click => click;

        public void OnClick(string buttonText)
        {
            Executors.RunOnCoroutineNoReturn(DoClick(buttonText));
        }

        private IEnumerator DoClick(string buttonText)
        {
            click.Enabled = false;
            Text = $"Click Button:{buttonText}.Restore button status after one second";
            Debug.LogFormat("Click Button:{0}", buttonText);

            //Restore button status after one second
            yield return new WaitForSeconds(1f);
            click.Enabled = true;
        }

    }

    public class DatabindingForButtonGroupExample : UIView
    {
        public Button button1;
        public Button button2;
        public Button button3;
        public Button button4;
        public Button button5;
        public Text text;

        protected override void Awake()
        {
            Context context = Context.GetGlobalContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();
        }

        protected override void Start()
        {
            ButtonGroupViewModel viewModel = new ButtonGroupViewModel();

            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = viewModel;

            
            BindingSet<DatabindingForButtonGroupExample, ButtonGroupViewModel> bindingSet = this.CreateBindingSet<DatabindingForButtonGroupExample, ButtonGroupViewModel>();
            bindingSet.Bind(button1).For(v => v.onClick).To(vm => vm.Click).CommandParameter(() => button1.name);
            bindingSet.Bind(button2).For(v => v.onClick).To(vm => vm.Click).CommandParameter(() => button2.name);
            bindingSet.Bind(button3).For(v => v.onClick).To(vm => vm.Click).CommandParameter(() => button3.name);
            bindingSet.Bind(button4).For(v => v.onClick).To(vm => vm.Click).CommandParameter(() => button4.name);
            bindingSet.Bind(button5).For(v => v.onClick).To(vm => vm.Click).CommandParameter(() => button5.name);

            bindingSet.Bind(text).For(v => v.text).To(vm => vm.Text).OneWay();

            bindingSet.Build();
        }
    }
}
