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

using Loxodon.Framework.Binding;
using Loxodon.Framework.Observables;
using Loxodon.Framework.Views;
using System.Collections.Specialized;
using UnityEngine;

namespace Loxodon.Framework.Tutorials
{
    public class ListView : UIView
    {
        private ObservableList<ListItemViewModel> items;

        public Transform content;

        public GameObject itemTemplate;

        public ObservableList<ListItemViewModel> Items
        {
            get => items;
            set
            {
                if (items == value)
                    return;

                if (items != null)
                    items.CollectionChanged -= OnCollectionChanged;
          
                items = value;
                OnItemsChanged();

                if (items != null)
                    items.CollectionChanged += OnCollectionChanged;
            }
        }

        protected override void OnDestroy()
        {
            if (items != null)
                items.CollectionChanged -= OnCollectionChanged;
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItem(eventArgs.NewStartingIndex, eventArgs.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveItem(eventArgs.OldStartingIndex, eventArgs.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ReplaceItem(eventArgs.OldStartingIndex, eventArgs.OldItems[0], eventArgs.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetItem();
                    break;
                case NotifyCollectionChangedAction.Move:
                    MoveItem(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex, eventArgs.NewItems[0]);
                    break;
            }
        }

        protected virtual void OnItemsChanged()
        {
            int count = content.childCount;
            for(int i = count - 1; i >= 0; i--)
            {
                Transform child = content.GetChild(i);
                Destroy(child.gameObject);
            }

            for (int i = 0; i < items.Count; i++)
            {
                AddItem(i, items[i]);
            }
        }

        protected virtual void AddItem(int index, object item)
        {
            var itemViewGo = Instantiate(itemTemplate);
            itemViewGo.transform.SetParent(content, false);
            itemViewGo.transform.SetSiblingIndex(index);
            itemViewGo.SetActive(true);

            UIView itemView = itemViewGo.GetComponent<UIView>();
            itemView.SetDataContext(item);
        }

        protected virtual void RemoveItem(int index, object item)
        {
            Transform transform = content.GetChild(index);
            UIView itemView = transform.GetComponent<UIView>();
            if (itemView.GetDataContext() == item)
            {
                itemView.gameObject.SetActive(false);
                Destroy(itemView.gameObject);
            }
        }

        protected virtual void ReplaceItem(int index, object oldItem, object item)
        {
            Transform transform = content.GetChild(index);
            UIView itemView = transform.GetComponent<UIView>();
            if (itemView.GetDataContext() == oldItem)
            {
                itemView.SetDataContext(item);
            }
        }

        protected virtual void MoveItem(int oldIndex, int index, object item)
        {
            Transform transform = content.GetChild(oldIndex);
            UIView itemView = transform.GetComponent<UIView>();
            itemView.transform.SetSiblingIndex(index);
        }

        protected virtual void ResetItem()
        {
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                Transform transform = content.GetChild(i);
                Destroy(transform.gameObject);
            }
        }
    }

}