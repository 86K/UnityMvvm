using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ListItemDetailView : MonoBehaviour
    {
        public GameObject panel;
        public Text title;
        public Text price;
        public Image image;

        public ListItemViewModel Item
        {
            get => (ListItemViewModel)this.GetDataContext();
            set => this.SetDataContext(value);
        }

        private void Start()
        {
            var bindingSet = this.CreateBindingSet<ListItemDetailView, ListItemViewModel>();
            bindingSet.Bind(panel).For(v => v.activeSelf).To(vm => vm.IsSelected);
            bindingSet.Bind(title).For(v => v.text).To(vm => vm.Title);
            bindingSet.Bind(image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
            bindingSet.Bind(price).For(v => v.text).ToExpression(vm => $"${vm.Price:0.00}").OneWay();
            bindingSet.Build();
        }
    }
}
