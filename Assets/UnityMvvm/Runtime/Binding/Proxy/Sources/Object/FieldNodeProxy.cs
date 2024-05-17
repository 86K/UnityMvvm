using System;

namespace Fusion.Mvvm
{
    public class FieldNodeProxy : SourceProxyBase, IObtainable, IModifiable
    {
        private readonly IProxyFieldInfo fieldInfo;

        public FieldNodeProxy(IProxyFieldInfo fieldInfo) : this(null, fieldInfo)
        {
        }

        public FieldNodeProxy(object source, IProxyFieldInfo fieldInfo) : base(source)
        {
            this.fieldInfo = fieldInfo;
        }

        public override Type Type => fieldInfo.ValueType;

        public override TypeCode TypeCode => fieldInfo.ValueTypeCode;

        public object GetValue()
        {
            return fieldInfo.GetValue(Source);
        }

        public TValue GetValue<TValue>()
        {
            if (fieldInfo is IProxyFieldInfo<TValue> proxy)
                return proxy.GetValue(Source);

            return (TValue)fieldInfo.GetValue(Source);
        }

        public void SetValue(object value)
        {
            fieldInfo.SetValue(Source, value);
        }

        public void SetValue<TValue>(TValue value)
        {
            if (fieldInfo is IProxyFieldInfo<TValue> proxy)
            {
                proxy.SetValue(Source, value);
                return;
            }

            fieldInfo.SetValue(Source, value);
        }
    }
}