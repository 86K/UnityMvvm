

using System.Collections.Specialized;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class Item : ObservableObject
    {
        private string title;
        private string iconPath;
        private string content;

        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        public string IconPath
        {
            get => iconPath;
            set => Set(ref iconPath, value);
        }

        public string Content
        {
            get => content;
            set => Set(ref content, value);
        }

        public override string ToString()
        {
            return $"[Item: Title={Title}, IconPath={IconPath}, Content={Content}]";
        }
    }

    public class ObservableListExample : MonoBehaviour
    {
        private ObservableList<Item> list;

        protected void Start()
        {
            list = new ObservableList<Item>();
            list.CollectionChanged += OnCollectionChanged;

            list.Add(new Item() { Title = "title1", IconPath = "xxx/xxx/icon1.png", Content = "this is a test." });
            list[0] = new Item() { Title = "title2", IconPath = "xxx/xxx/icon2.png", Content = "this is a test." };

            list.Clear();
        }

        protected void OnDestroy()
        {
            if (list != null)
            {
                list.CollectionChanged -= OnCollectionChanged;
                list = null;
            }
        }

        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Item item in eventArgs.NewItems)
                    {
                        Debug.LogFormat("ADD item:{0}", item);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Item item in eventArgs.OldItems)
                    {
                        Debug.LogFormat("REMOVE item:{0}", item);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Item item in eventArgs.OldItems)
                    {
                        Debug.LogFormat("REPLACE before item:{0}", item);
                    }
                    foreach (Item item in eventArgs.NewItems)
                    {
                        Debug.LogFormat("REPLACE after item:{0}", item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.LogFormat("RESET");
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }
    }
}