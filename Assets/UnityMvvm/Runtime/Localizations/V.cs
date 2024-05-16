

using System;

namespace Fusion.Mvvm
{
    public class V<T> : IObservableProperty<T>
    {
        private readonly object _lock = new object();
        private EventHandler valueChanged;
        private readonly string key;
        private IObservableProperty property;

        public event EventHandler ValueChanged
        {
            add { lock (_lock) { valueChanged += value; } }
            remove { lock (_lock) { valueChanged -= value; } }
        }

        public V(string key) : base()
        {
            this.key = key;
        }

        public virtual Type Type => typeof(T);

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        protected void RaiseValueChanged()
        {
            var handler = valueChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected IObservableProperty Property
        {
            get
            {
                if (property != null)
                    return property;

                lock (this)
                {
                    if (property == null)
                    {
                        property = Localization.Current.GetValue(key);
                        property.ValueChanged += OnValueChanged;
                    }
                    return property;
                }
            }
        }

        public T Value
        {
            get
            {
                var p = Property as IObservableProperty<T>;
                if (p != null)
                    return p.Value;

                return (T)Property.Value;
            }
            set
            {
                var p = Property as IObservableProperty<T>;
                if (p != null)
                {
                    p.Value = value;
                    return;
                }

                Property.Value = value;
            }
        }

        object IObservableProperty.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        public static implicit operator T(V<T> data)
        {
            return data.Value;
        }

        public override string ToString()
        {
            var v = Value;
            if (v == null)
                return "";

            return v.ToString();
        }
    }
}
