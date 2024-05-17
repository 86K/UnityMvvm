using System;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Fusion.Mvvm
{
    public class PropertyTargetProxy : ValueTargetProxyBase
    {
        private readonly IProxyPropertyInfo _propertyInfo;

        public PropertyTargetProxy(object target, IProxyPropertyInfo propertyInfo) : base(target)
        {
            _propertyInfo = propertyInfo;
        }

        public override Type Type => _propertyInfo.ValueType;

        public override TypeCode TypeCode => _propertyInfo.ValueTypeCode;

        public override BindingMode DefaultMode => BindingMode.TwoWay;

        public override object GetValue()
        {
            var target = Target;
            if (target == null)
                return null;

            return _propertyInfo.GetValue(target);
        }

        public override TValue GetValue<TValue>()
        {
            var target = Target;
            if (target == null)
                return default;

            if (_propertyInfo is IProxyPropertyInfo<TValue> info)
                return info.GetValue(target);

            return (TValue)_propertyInfo.GetValue(target);
        }

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            _propertyInfo.SetValue(target, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = Target;
            if (target == null)
                return;

            if (_propertyInfo is IProxyPropertyInfo<TValue> info)
            {
                info.SetValue(target, value);
                return;
            }

            _propertyInfo.SetValue(target, value);
        }

        protected override void DoSubscribeForValueChange(object target)
        {
            if (target is INotifyPropertyChanged targetNotify)
            {
                targetNotify.PropertyChanged += OnPropertyChanged;
            }
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (target is INotifyPropertyChanged targetNotify)
            {
                targetNotify.PropertyChanged -= OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;
            if (string.IsNullOrEmpty(name) || name.Equals(_propertyInfo.Name))
            {
                var target = Target;
                if (target == null)
                    return;

                RaiseValueChanged();
            }
        }
    }
}