using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ListItemEditView : UIView
    {
        public Text title;
        public Text price;
        public Slider priceSlider;
        public Button changeIcon;
        public Image image;
        public Button submit;
        public Button cancel;

        public ListItemEditViewModel ViewModel
        {
            get => (ListItemEditViewModel)this.GetDataContext();
            set => this.SetDataContext(value);
        }

        protected override void Start()
        {
            var bindingSet = this.CreateBindingSet<ListItemEditView, ListItemEditViewModel>();
            bindingSet.Bind(title).For(v => v.text).To(vm => vm.Title);
            bindingSet.Bind(price).For(v => v.text).ToExpression(vm => $"${vm.Price:0.00}").OneWay();
            bindingSet.Bind(priceSlider).For(v => v.value, v => v.onValueChanged).To(vm => vm.Price).TwoWay();
            bindingSet.Bind(image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
            bindingSet.Bind(changeIcon).For(v => v.onClick).To(vm => vm.OnChangeIcon);
            bindingSet.Build();

            cancel.onClick.AddListener(Cancel);
            submit.onClick.AddListener(Submit);
        }

        private void Cancel()
        {            
            ViewModel.Cancelled = true;
            gameObject.SetActive(false);
            //this.Visibility = false;
            this.SetDataContext(null);
        }

        private void Submit()
        {            
            ViewModel.Cancelled = false;
            gameObject.SetActive(false);
            //this.Visibility = false;           
            this.SetDataContext(null);
        }
    }

}