using System;

namespace Fusion.Mvvm
{
    public class ObservableTargetProxy : ValueTargetProxyBase
    {
        private readonly IObservableProperty _observableProperty;

        public ObservableTargetProxy(object target, IObservableProperty observableProperty) : base(target)
        {
            _observableProperty = observableProperty;
        }

        public override Type Type => _observableProperty.Type;

        public override BindingMode DefaultMode => BindingMode.TwoWay;

        public override object GetValue()
        {
            return _observableProperty.Value;
        }

        public override TValue GetValue<TValue>()
        {
            if (_observableProperty is IObservableProperty<TValue> property)
                return property.Value;

            return (TValue)_observableProperty.Value;
        }

        public override void SetValue(object value)
        {
            _observableProperty.Value = value;
        }

        public override void SetValue<TValue>(TValue value)
        {
            if (_observableProperty is IObservableProperty<TValue> property)
            {
                property.Value = value;
                return;
            }

            _observableProperty.Value = value;
        }

        protected override void DoSubscribeForValueChange(object target)
        {
            _observableProperty.ValueChanged += OnValueChanged;
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            _observableProperty.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }
    }
}
