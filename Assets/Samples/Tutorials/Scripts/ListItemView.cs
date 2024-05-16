using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ListItemView : UIView
    {
        public Text title;
        public Text price;
        public Image image;
        public GameObject border;
        public Button selectButton;
        public Button clickButton;

        protected override void Start()
        {
            BindingSet<ListItemView, ListItemViewModel> bindingSet = this.CreateBindingSet<ListItemView, ListItemViewModel>();
            bindingSet.Bind(title).For(v => v.text).To(vm => vm.Title).OneWay();
            bindingSet.Bind(image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
            bindingSet.Bind(price).For(v => v.text).ToExpression(vm => $"${vm.Price:0.00}").OneWay();
            bindingSet.Bind(border).For(v => v.activeSelf).To(vm => vm.IsSelected).OneWay();
            bindingSet.Bind(selectButton).For(v => v.onClick).To(vm => vm.SelectCommand).CommandParameter(this.GetDataContext);
            bindingSet.Bind(clickButton).For(v => v.onClick).To(vm => vm.ClickCommand).CommandParameter(this.GetDataContext);
            bindingSet.Build();
        }
    }
}
