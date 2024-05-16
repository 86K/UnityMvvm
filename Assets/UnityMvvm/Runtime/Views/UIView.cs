

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fusion.Mvvm
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class UIView : UIBehaviour, IUIView
    {
        private IAnimation enterAnimation;
        private IAnimation exitAnimation;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private readonly object _lock = new object();
        private EventHandler onDisabled;
        private EventHandler onEnabled;

        [NonSerialized]
        private readonly IAttributes attributes = new Attributes();

        public event EventHandler OnDisabled
        {
            add { lock (_lock) { onDisabled += value; } }
            remove { lock (_lock) { onDisabled -= value; } }
        }

        public event EventHandler OnEnabled
        {
            add { lock (_lock) { onEnabled += value; } }
            remove { lock (_lock) { onEnabled -= value; } }
        }

        public virtual string Name
        {
            get => !IsDestroyed() && gameObject != null ? gameObject.name : null;
            set
            {
                if (IsDestroyed() || gameObject == null)
                    return;

                gameObject.name = value;
            }
        }

        public virtual Transform Parent => !IsDestroyed() && transform != null ? transform.parent : null;

        public virtual GameObject Owner => IsDestroyed() ? null : gameObject;

        public virtual Transform Transform => IsDestroyed() ? null : transform;

        public virtual RectTransform RectTransform
        {
            get
            {
                if (IsDestroyed())
                    return null;

                return rectTransform ?? (rectTransform = GetComponent<RectTransform>());
            }
        }

        public virtual bool Visibility
        {
            get => !IsDestroyed() && gameObject != null ? gameObject.activeSelf : false;
            set
            {
                if (IsDestroyed() || gameObject == null)
                    return;

                if (gameObject.activeSelf == value)
                    return;

                gameObject.SetActive(value);
            }
        }

        public virtual IAnimation EnterAnimation
        {
            get => enterAnimation;
            set => enterAnimation = value;
        }

        public virtual IAnimation ExitAnimation
        {
            get => exitAnimation;
            set => exitAnimation = value;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisibilityChanged();
            RaiseOnEnabled();
        }

        protected override void OnDisable()
        {
            OnVisibilityChanged();
            base.OnDisable();
            RaiseOnDisabled();
        }

        protected void RaiseOnEnabled()
        {
            try
            {
                if (onEnabled != null)
                    onEnabled(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        protected void RaiseOnDisabled()
        {
            try
            {
                if (onDisabled != null)
                    onDisabled(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public virtual float Alpha
        {
            get => !IsDestroyed() && gameObject != null ? CanvasGroup.alpha : 0f;
            set { if (!IsDestroyed() && gameObject != null) CanvasGroup.alpha = value; }
        }

        public virtual bool Interactable
        {
            get
            {
                if (IsDestroyed() || gameObject == null)
                    return false;

                if (GlobalSetting.useBlocksRaycastsInsteadOfInteractable)
                    return CanvasGroup.blocksRaycasts;
                return CanvasGroup.interactable;
            }
            set
            {
                if (IsDestroyed() || gameObject == null)
                    return;

                if (GlobalSetting.useBlocksRaycastsInsteadOfInteractable)
                    CanvasGroup.blocksRaycasts = value;
                else
                    CanvasGroup.interactable = value;
            }
        }

        public virtual CanvasGroup CanvasGroup
        {
            get
            {
                if (IsDestroyed())
                    return null;

                return canvasGroup ?? (canvasGroup = GetComponent<CanvasGroup>());
            }
        }

        public virtual IAttributes ExtraAttributes => attributes;

        protected virtual void OnVisibilityChanged()
        {
        }
    }
}

