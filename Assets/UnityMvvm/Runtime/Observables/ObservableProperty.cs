using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    [Serializable]
    public abstract class ObservablePropertyBase<T>
    {
        private readonly object _lock = new object();
        private EventHandler _valueChanged;
        protected T _value;

        public event EventHandler ValueChanged
        {
            add
            {
                lock (_lock)
                {
                    _valueChanged += value;
                }
            }
            remove
            {
                lock (_lock)
                {
                    _valueChanged -= value;
                }
            }
        }

        protected ObservablePropertyBase() : this(default)
        {
        }

        protected ObservablePropertyBase(T value)
        {
            _value = value;
        }

        public virtual Type Type => typeof(T);

        protected void RaiseValueChanged()
        {
            _valueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual bool Equals(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }

    [Serializable]
    public class ObservableProperty : ObservablePropertyBase<object>, IObservableProperty
    {
        public ObservableProperty() : this(null)
        {
        }

        public ObservableProperty(object value) : base(value)
        {
        }

        public override Type Type => _value != null ? _value.GetType() : typeof(object);

        public virtual object Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                RaiseValueChanged();
            }
        }

        public override string ToString()
        {
            var v = Value;
            if (v == null)
                return "";

            return v.ToString();
        }
    }

    [Serializable]
    public class ObservableProperty<T> : ObservablePropertyBase<T>, IObservableProperty<T>
    {
        public ObservableProperty() : this(default)
        {
        }

        public ObservableProperty(T value) : base(value)
        {
        }

        public virtual T Value
        {
            get => _value;
            set
            {
                if (Equals(_value, value))
                    return;

                _value = value;
                RaiseValueChanged();
            }
        }

        object IObservableProperty.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        public override string ToString()
        {
            var v = Value;
            if (v == null)
                return "";

            return v.ToString();
        }

        public static implicit operator T(ObservableProperty<T> data)
        {
            return data.Value;
        }

        public static implicit operator ObservableProperty<T>(T data)
        {
            return new ObservableProperty<T>(data);
        }
    }
}