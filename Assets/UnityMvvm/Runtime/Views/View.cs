

using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class View : MonoBehaviour, IView
    {
        [NonSerialized]
        private readonly IAttributes attributes = new Attributes();

        public virtual string Name
        {
            get => gameObject != null ? gameObject.name : null;
            set
            {
                if (gameObject == null)
                    return;

                gameObject.name = value;
            }
        }

        public virtual Transform Parent => transform != null ? transform.parent : null;

        public virtual GameObject Owner => gameObject;

        public virtual Transform Transform => transform;

        public virtual bool Visibility
        {
            get => gameObject != null ? gameObject.activeSelf : false;
            set
            {
                if (gameObject == null)
                    return;
                if (gameObject.activeSelf == value)
                    return;

                gameObject.SetActive(value);
            }
        }

        protected virtual void OnEnable()
        {
            OnVisibilityChanged();
        }

        protected virtual void OnDisable()
        {
            OnVisibilityChanged();
        }

        public virtual IAttributes ExtraAttributes => attributes;

        protected virtual void OnVisibilityChanged()
        {
        }
    }
}
