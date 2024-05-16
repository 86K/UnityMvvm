using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    [DefaultExecutionOrder(100)]
    public abstract class AbstractLocalized<T> : MonoBehaviour where T : Component
    {
        [SerializeField]
        private string key;
        protected T target;
        protected IObservableProperty value;

        protected virtual void OnKeyChanged()
        {
            if (value != null)
                value.ValueChanged -= OnValueChanged;

            if (!enabled || target == null || string.IsNullOrEmpty(key))
                return;

            Localization localization = Localization.Current;
            value = localization.GetValue(key);
            value.ValueChanged += OnValueChanged;
            OnValueChanged(value, EventArgs.Empty);
        }

        public string Key
        {
            get => key;
            set
            {
                if (string.IsNullOrEmpty(value) || value.Equals(key))
                    return;

                key = value;
                OnKeyChanged();
            }
        }

        protected virtual void OnEnable()
        {
            if (target == null)
                target = GetComponent<T>();

            if (target == null)
                return;

            OnKeyChanged();
        }

        protected virtual void OnDisable()
        {
            if (value != null)
            {
                value.ValueChanged -= OnValueChanged;
                value = null;
            }
        }

        protected abstract void OnValueChanged(object sender, EventArgs e);
    }
}
