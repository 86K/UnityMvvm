using System.Collections.Specialized;
using UnityEngine;

namespace Fusion.Mvvm
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