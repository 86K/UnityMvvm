using System;

namespace Fusion.Mvvm
{
    public class FieldTargetProxy : ValueTargetProxyBase
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(FieldTargetProxy));

        protected readonly IProxyFieldInfo fieldInfo;

        public FieldTargetProxy(object target, IProxyFieldInfo fieldInfo) : base(target)
        {
            this.fieldInfo = fieldInfo;
        }

        public override Type Type => fieldInfo.ValueType;

        public override TypeCode TypeCode => fieldInfo.ValueTypeCode;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override object GetValue()
        {
            var target = Target;
            if (target == null)
                return null;

            return fieldInfo.GetValue(target);
        }

        public override TValue GetValue<TValue>()
        {
            var target = Target;
            if (target == null)
                return default(TValue);

            if (fieldInfo is IProxyFieldInfo<TValue>)
                return ((IProxyFieldInfo<TValue>)fieldInfo).GetValue(target);

            return (TValue)fieldInfo.GetValue(target);
        }

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            fieldInfo.SetValue(target, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = Target;
            if (target == null)
                return;

            if (fieldInfo is IProxyFieldInfo<TValue>)
            {
                ((IProxyFieldInfo<TValue>)fieldInfo).SetValue(target, value);
                return;
            }

            fieldInfo.SetValue(target, value);
        }
    }
}
