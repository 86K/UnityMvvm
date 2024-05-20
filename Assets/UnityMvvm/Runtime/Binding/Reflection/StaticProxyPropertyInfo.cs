using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class StaticProxyPropertyInfo<T, TValue> : ProxyPropertyInfo
    {
        private readonly Func<TValue> _getter;
        private readonly Action<TValue> _setter;

        /* 构造函数在ProxyType中创建实例时使用 */
        
        public StaticProxyPropertyInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            if (propertyInfo.DeclaringType != null && (!(typeof(TValue) == _propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The property types do not match!");

            if (!IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" isn't static.");

            _getter = MakeGetter(propertyInfo);
            _setter = MakeSetter(propertyInfo);
        }

        public StaticProxyPropertyInfo(PropertyInfo propertyInfo, Func<TValue> getter, Action<TValue> setter) : base(propertyInfo)
        {
            if (propertyInfo.DeclaringType != null && (!(typeof(TValue) == _propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The property types do not match!");

            if (!IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" isn't static.");

            _getter = getter;
            _setter = setter;
        }

        public override Type DeclaringType => typeof(T);

        private Action<TValue> MakeSetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (IsValueType)
                    return null;

                var setMethod = propertyInfo.GetSetMethod();
                if (setMethod == null)
                    return null;
                return (Action<TValue>)setMethod.CreateDelegate(typeof(Action<TValue>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private Func<TValue> MakeGetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (IsValueType)
                    return null;

                var getMethod = propertyInfo.GetGetMethod();
                if (getMethod == null)
                    return null;
                return (Func<TValue>)getMethod.CreateDelegate(typeof(Func<TValue>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        public TValue GetValue(T target)
        {
            if (_getter != null)
                return _getter();

            return (TValue)base.GetValue(null);
        }

        public override object GetValue(object target)
        {
            if (_getter != null)
                return _getter();

            return base.GetValue(target);
        }

        private void SetValue(T target, TValue value)
        {
            if (_setter != null)
            {
                _setter(value);
                return;
            }

            base.SetValue(null, value);
        }

        public void SetValue(object target, TValue value)
        {
            SetValue((T)target, value);
        }

        public override void SetValue(object target, object value)
        {
            if (_setter != null)
            {
                _setter((TValue)value);
                return;
            }

            base.SetValue(null, value);
        }
    }
}