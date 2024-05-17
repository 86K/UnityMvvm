using System;

namespace Fusion.Mvvm
{
    public class ObservableNodeProxy : NotifiableSourceProxyBase, IObtainable, IModifiable
    {
        private readonly IObservableProperty _property;

        public ObservableNodeProxy(IObservableProperty property) : this(null, property)
        {
        }

        public ObservableNodeProxy(object source, IObservableProperty property) : base(source)
        {
            _property = property;
            _property.ValueChanged += OnValueChanged;
        }

        public override Type Type => _property.Type;

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        public object GetValue()
        {
            return _property.Value;
        }

        public TValue GetValue<TValue>()
        {
            if (_property is IObservableProperty<TValue> observableProperty)
                return observableProperty.Value;

            return (TValue)_property.Value;
        }

        public void SetValue(object value)
        {
            _property.Value = value;
        }

        public void SetValue<TValue>(TValue value)
        {
            if (_property is IObservableProperty<TValue> observableProperty)
            {
                observableProperty.Value = value;
                return;
            }

            _property.Value = value;
        }

        #region IDisposable Support

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (_property != null)
                    _property.ValueChanged -= OnValueChanged;

                disposedValue = true;
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}