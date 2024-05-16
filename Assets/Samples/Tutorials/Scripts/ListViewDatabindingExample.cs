using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Mvvm
{
    public class ListViewDatabindingExample : MonoBehaviour
    {
        private ListViewViewModel viewModel;

        public Button addButton;

        public Button removeButton;

        public Button clearButton;

        public Button changeIconButton;

        public Button changeItems;

        public ListView listView;

        public ListItemDetailView detailView;

        public ListItemEditView editView;

        private AsyncViewInteractionAction editViewInteractionAction;

        void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            foreach (var sprite in Resources.LoadAll<Sprite>("EquipTextures"))
            {
                if (sprite != null)
                    sprites.Add(sprite.name, sprite);
            }
            IConverterRegistry converterRegistry = context.GetContainer().Resolve<IConverterRegistry>();
            converterRegistry.Register("spriteConverter", new SpriteConverter(sprites));
        }

        void Start()
        {
            editViewInteractionAction = new AsyncViewInteractionAction(editView);
            viewModel = new ListViewViewModel();
            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = viewModel;

            BindingSet<ListViewDatabindingExample, ListViewViewModel> bindingSet = this.CreateBindingSet<ListViewDatabindingExample, ListViewViewModel>();
            bindingSet.Bind(listView).For(v => v.Items).To(vm => vm.Items).OneWay();
            bindingSet.Bind(detailView).For(v => v.Item).To(vm => vm.SelectedItem);
            bindingSet.Bind().For(v => v.editViewInteractionAction).To(vm => vm.ItemEditRequest);

            bindingSet.Bind(addButton).For(v => v.onClick).To(vm => vm.AddItem);
            bindingSet.Bind(removeButton).For(v => v.onClick).To(vm => vm.RemoveItem);
            bindingSet.Bind(clearButton).For(v => v.onClick).To(vm => vm.ClearItem);
            bindingSet.Bind(changeIconButton).For(v => v.onClick).To(vm => vm.ChangeItemIcon);
            bindingSet.Bind(changeItems).For(v => v.onClick).To(vm => vm.ChangeItems);

            bindingSet.Build();

            viewModel.SelectItem(0);
        }
    }
}