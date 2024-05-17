using System;

namespace Fusion.Mvvm
{
    public class ItemTargetProxy<TKey> : ValueTargetProxyBase
    {
        private readonly IProxyItemInfo _itemInfo;
        private readonly TKey _key;

        public ItemTargetProxy(object target, TKey key, IProxyItemInfo itemInfo) : base(target)
        {
            _key = key;
            _itemInfo = itemInfo;
        }

        public override Type Type => _itemInfo.ValueType;

        public override TypeCode TypeCode => _itemInfo.ValueTypeCode;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override object GetValue()
        {
            var target = Target;
            if (target == null)
                return null;

            return _itemInfo.GetValue(target, _key);
        }

        public override TValue GetValue<TValue>()
        {
            var target = Target;
            if (target == null)
                return default;

            if (!typeof(TValue).IsAssignableFrom(_itemInfo.ValueType))
                throw new InvalidCastException();

            if (_itemInfo is IProxyItemInfo<TKey, TValue> proxy)
                return proxy.GetValue(target, _key);

            return (TValue)_itemInfo.GetValue(target, _key);
        }

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            _itemInfo.SetValue(target, _key, value);
        }

        public override void SetValue<TValue>(TValue value)
        {
            var target = Target;
            if (target == null)
                return;

            if (_itemInfo is IProxyItemInfo<TKey, TValue> proxy)
            {
                proxy.SetValue(target, _key, value);
                return;
            }

            _itemInfo.SetValue(target, _key, value);
        }
    }
}