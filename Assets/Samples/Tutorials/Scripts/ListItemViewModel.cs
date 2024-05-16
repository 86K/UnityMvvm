namespace Fusion.Mvvm
{
    public class ListItemViewModel : ViewModelBase
    {
        private string title;
        private string icon;
        private float price;
        private bool selected;
        private readonly ICommand clickCommand;
        private readonly ICommand selectCommand;

        public ListItemViewModel(ICommand selectCommand, ICommand clickCommand)
        {
            this.selectCommand = selectCommand;
            this.clickCommand = clickCommand;
        }

        public ICommand ClickCommand => clickCommand;

        public ICommand SelectCommand => selectCommand;

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

        public bool IsSelected
        {
            get => selected;
            set => Set(ref selected, value);
        }
    }
}
