

using System;

namespace Fusion.Mvvm
{
    public class ItemTargetProxy<TKey> : ValueTargetProxyBase
    {
        protected readonly IProxyItemInfo itemInfo;
        protected readonly TKey key;
        public ItemTargetProxy(object target, TKey key, IProxyItemInfo itemInfo) : base(target)
        {
            this.key = key;
            this.itemInfo = itemInfo;
        }

        public override Type Type => itemInfo.ValueType;

        public override TypeCode TypeCode => itemInfo.ValueTypeCode;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override object GetValue()
        {
            var target = Target;
            if (target == null)
                return null;

            return itemInfo.GetValue(target, key);
        }

        public override TValue GetValue<TValue>()
        {
            var target = Target;
            if (target == null)
                return default(TValue);

            if (!typeof(TValue).IsAssignableFrom(itemInfo.ValueType))
                throw new InvalidCastException();

            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
                return proxy.GetValue(target, key);

            return (TValue)itemInfo.GetValue(target, key);
        }

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            itemInfo.SetValue(target, key, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = Target;
            if (target == null)
                return;

            var proxy = itemInfo as IProxyItemInfo<TKey, TValue>;
            if (proxy != null)
            {
                proxy.SetValue(target, key, value);
                return;
            }

            itemInfo.SetValue(target, key, value);
        }
    }
}
