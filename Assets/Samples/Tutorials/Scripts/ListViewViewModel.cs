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
using Loxodon.Framework.Observables;
using Loxodon.Framework.ViewModels;
using UnityEngine;

namespace Loxodon.Framework.Tutorials
{
    public class ListViewViewModel : ViewModelBase
    {
        private ListItemViewModel selectedItem;
        private readonly SimpleCommand<ListItemViewModel> itemSelectCommand;
        private readonly SimpleCommand<ListItemViewModel> itemClickCommand;
        private readonly AsyncInteractionRequest<VisibilityNotification> itemEditRequest;
        private ObservableList<ListItemViewModel> items;

        public ListViewViewModel()
        {
            itemEditRequest = new AsyncInteractionRequest<VisibilityNotification>(this);
            itemClickCommand = new SimpleCommand<ListItemViewModel>(OnItemClick);
            itemSelectCommand = new SimpleCommand<ListItemViewModel>(OnItemSelect);
            items = CreateList();
        }

        public ObservableList<ListItemViewModel> Items
        {
            get => items;
            set => Set(ref items, value);
        }

        public ListItemViewModel SelectedItem
        {
            get => selectedItem;
            set => Set(ref selectedItem, value);
        }

        public IInteractionRequest ItemEditRequest => itemEditRequest;

        public ListItemViewModel SelectItem(int index)
        {
            if (index < 0 || index >= items.Count)
                throw new System.Exception();

            var item = items[index];
            item.IsSelected = true;
            SelectedItem = item;

            if (items != null && item.IsSelected)
            {
                foreach (var i in items)
                {
                    if (i == item)
                        continue;
                    i.IsSelected = false;
                }
            }

            return item;
        }

        private async void OnItemClick(ListItemViewModel item)
        {
            ListItemEditViewModel editViewModel = new ListItemEditViewModel(item);
            await itemEditRequest.Raise(VisibilityNotification.CreateShowNotification(editViewModel, true));

            if (editViewModel.Cancelled)
                return;

            //Apply changes
            item.Icon = editViewModel.Icon;
            item.Price = editViewModel.Price;
            item.Title = editViewModel.Title;
        }

        private void OnItemSelect(ListItemViewModel item)
        {
            item.IsSelected = !item.IsSelected;
            if (items != null && item.IsSelected)
            {
                foreach (var i in items)
                {
                    if (i == item)
                        continue;
                    i.IsSelected = false;
                }
            }

            if (item.IsSelected)
                SelectedItem = item;
        }

        public void AddItem()
        {
            int i = items.Count;
            items.Add(NewItem(i));
        }

        public void RemoveItem()
        {
            if (items.Count <= 0)
                return;

            int index = Random.Range(0, items.Count - 1);
            var item = items[index];
            if (item.IsSelected)
                SelectedItem = null;

            items.RemoveAt(index);
        }

        public void ClearItem()
        {
            if (items.Count <= 0)
                return;

            items.Clear();
            SelectedItem = null;
        }

        public void ChangeItemIcon()
        {
            if (items.Count <= 0)
                return;

            foreach (var item in items)
            {
                int iconIndex = Random.Range(1, 30);
                item.Icon = $"EquipImages_{iconIndex}";
            }
        }

        public void ChangeItems()
        {
            SelectedItem = null;
            Items = CreateList();
        }

        private ObservableList<ListItemViewModel> CreateList()
        {
            var items = new ObservableList<ListItemViewModel>();
            for (int i = 0; i < 3; i++)
            {
                items.Add(NewItem(i));
            }
            return items;
        }

        private ListItemViewModel NewItem(int id)
        {
            int iconIndex = Random.Range(1, 30);
            float price = Random.Range(0f, 100f);
            return new ListItemViewModel(itemSelectCommand, itemClickCommand) { Title = "Equip " + id, Icon = $"EquipImages_{iconIndex}", Price = price };
        }
    }
}
