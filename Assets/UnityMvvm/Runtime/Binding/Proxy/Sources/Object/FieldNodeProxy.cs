using System;

namespace Fusion.Mvvm
{
    public class FieldNodeProxy : SourceProxyBase, IObtainable, IModifiable
    {
        protected IProxyFieldInfo fieldInfo;

        public FieldNodeProxy(IProxyFieldInfo fieldInfo) : this(null, fieldInfo)
        {
        }

        public FieldNodeProxy(object source, IProxyFieldInfo fieldInfo) : base(source)
        {
            this.fieldInfo = fieldInfo;
        }

        public override Type Type => fieldInfo.ValueType;

        public override TypeCode TypeCode => fieldInfo.ValueTypeCode;

        public virtual object GetValue()
        {
            return fieldInfo.GetValue(source);
        }

        public virtual TValue GetValue<TValue>()
        {
            var proxy = fieldInfo as IProxyFieldInfo<TValue>;
            if (proxy != null)
                return proxy.GetValue(source);

            return (TValue)fieldInfo.GetValue(source);
        }

        public virtual void SetValue(object value)
        {
            fieldInfo.SetValue(source, value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            var proxy = fieldInfo as IProxyFieldInfo<TValue>;
            if (proxy != null)
            {
                proxy.SetValue(source, value);
                return;
            }

            fieldInfo.SetValue(source, value);
        }
    }
}
