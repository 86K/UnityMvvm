

using UnityEngine;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class WindowView : UIView, IWindowView
    {
        private IAnimation activationAnimation;
        private IAnimation passivationAnimation;

        public virtual IAnimation ActivationAnimation
        {
            get => activationAnimation;
            set => activationAnimation = value;
        }

        public virtual IAnimation PassivationAnimation
        {
            get => passivationAnimation;
            set => passivationAnimation = value;
        }

        public virtual List<IUIView> Views
        {
            get
            {
                var transform = Transform;
                List<IUIView> views = new List<IUIView>();
                int count = transform.childCount;
                for (int i = 0; i < count; i++)
                {
                    var child = transform.GetChild(i);
                    var view = child.GetComponent<IUIView>();
                    if (view != null)
                        views.Add(view);
                }
                return views;
            }
        }

        public virtual IUIView GetView(string name)
        {
            return Views.Find(v => v.Name.Equals(name));
        }

        public virtual void AddView(IUIView view, bool worldPositionStays = false)
        {
            if (view == null)
                return;

            Transform t = view.Transform;
            if (t == null || t.parent == transform)
                return;

            view.Owner.layer = gameObject.layer;
            t.SetParent(transform, worldPositionStays);
        }

        public virtual void AddView(IUIView view, UILayout layout)
        {
            if (view == null)
                return;

            Transform t = view.Transform;
            if (t == null)
                return;

            if (t.parent == transform)
            {
                if (layout != null)
                    layout(view.RectTransform);
                return;
            }

            view.Owner.layer = gameObject.layer;
            t.SetParent(transform, false);
            if (layout != null)
                layout(view.RectTransform);
        }

        public virtual void RemoveView(IUIView view, bool worldPositionStays = false)
        {
            if (view == null)
                return;

            Transform t = view.Transform;
            if (t == null || t.parent != transform)
                return;

            t.SetParent(null, worldPositionStays);
        }
    }
}
