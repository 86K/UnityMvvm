using System;

namespace Fusion.Mvvm
{
    public class FieldTargetProxy : ValueTargetProxyBase
    {
        private readonly IProxyFieldInfo _fieldInfo;

        public FieldTargetProxy(object target, IProxyFieldInfo fieldInfo) : base(target)
        {
            _fieldInfo = fieldInfo;
        }

        public override Type Type => _fieldInfo.ValueType;

        public override TypeCode TypeCode => _fieldInfo.ValueTypeCode;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override object GetValue()
        {
            var target = Target;
            if (target == null)
                return null;

            return _fieldInfo.GetValue(target);
        }

        public override TValue GetValue<TValue>()
        {
            var target = Target;
            if (target == null)
                return default;

            if (_fieldInfo is IProxyFieldInfo<TValue> info)
                return info.GetValue(target);

            return (TValue)_fieldInfo.GetValue(target);
        }

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            _fieldInfo.SetValue(target, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = Target;
            if (target == null)
                return;

            if (_fieldInfo is IProxyFieldInfo<TValue> info)
            {
                info.SetValue(target, value);
                return;
            }

            _fieldInfo.SetValue(target, value);
        }
    }
}
