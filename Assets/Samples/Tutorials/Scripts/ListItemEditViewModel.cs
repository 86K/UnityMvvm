using UnityEngine;

namespace Fusion.Mvvm
{
    public class ListItemEditViewModel : ViewModelBase
    {
        private string title;
        private string icon;
        private float price;
        private bool cancelled;

        public ListItemEditViewModel(ListItemViewModel vm)
        {
            title = vm.Title;
            icon = vm.Icon;
            price = vm.Price;
        }

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }
        public string Icon
        {
            get => icon;
            set => Set(ref icon, value);
        }

        public float Price
        {
            get => price;
            set => Set(ref price, value);
        }

        public bool Cancelled
        {
            get => cancelled;
            set => Set(ref cancelled, value);
        }

        public void OnChangeIcon()
        {
            int iconIndex = Random.Range(1, 30);
            Icon = $"EquipImages_{iconIndex}";
        }
    }
}
