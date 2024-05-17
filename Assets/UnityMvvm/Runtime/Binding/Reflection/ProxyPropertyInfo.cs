using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ProxyPropertyInfo : IProxyPropertyInfo
    {
        private readonly bool _isValueType;
        private TypeCode _typeCode;
        protected readonly PropertyInfo _propertyInfo;
        private readonly MethodInfo _getMethod;
        private readonly MethodInfo _setMethod;

        public ProxyPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            _propertyInfo = propertyInfo;
            _isValueType = _propertyInfo.DeclaringType.GetTypeInfo().IsValueType;

            if (_propertyInfo.CanRead)
                _getMethod = propertyInfo.GetGetMethod();

            if (_propertyInfo.CanWrite && !_isValueType)
                _setMethod = propertyInfo.GetSetMethod();
        }

        public virtual bool IsValueType => _isValueType;

        public virtual Type ValueType => _propertyInfo.PropertyType;

        public TypeCode ValueTypeCode
        {
            get
            {
                if (_typeCode == TypeCode.Empty)
                {
                    _typeCode = Type.GetTypeCode(ValueType);
                }

                return _typeCode;
            }
        }

        public virtual Type DeclaringType => _propertyInfo.DeclaringType;

        public string Name => _propertyInfo.Name;

        public bool IsStatic => _propertyInfo.IsStatic();

        public virtual object GetValue(object target)
        {
            if (_getMethod == null)
                throw new MemberAccessException($"The property \"{_propertyInfo.DeclaringType}.{Name}\" is not public");

            return _getMethod.Invoke(target, null);
        }

        public virtual void SetValue(object target, object value)
        {
            if (!_propertyInfo.CanWrite)
                throw new MemberAccessException($"The property \"{_propertyInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (_setMethod == null)
                throw new MemberAccessException($"The property \"{_propertyInfo.DeclaringType}.{Name}\" is not public");

            _setMethod.Invoke(target, new object[] { value });
        }
    }

    public class ProxyPropertyInfo<T, TValue> : ProxyPropertyInfo
    {
        private readonly Func<T, TValue> getter;
        private readonly Action<T, TValue> setter;

        public ProxyPropertyInfo(string propertyName, Func<T, TValue> getter, Action<T, TValue> setter) : this(typeof(T).GetProperty(propertyName),
            getter, setter)
        {
        }

        private ProxyPropertyInfo(PropertyInfo propertyInfo, Func<T, TValue> getter = null, Action<T, TValue> setter = null) : base(propertyInfo)
        {
            if (!(typeof(TValue) == _propertyInfo.PropertyType) ||
                (propertyInfo.DeclaringType != null && !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The property types do not match!");

            if (IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is static.");

            this.getter = getter ?? MakeGetter(propertyInfo);
            this.setter = setter ?? MakeSetter(propertyInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Action<T, TValue> MakeSetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (IsValueType)
                    return null;

                var setMethod = propertyInfo.GetSetMethod();
                if (setMethod == null)
                    return null;

                return (Action<T, TValue>)setMethod.CreateDelegate(typeof(Action<T, TValue>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private Func<T, TValue> MakeGetter(PropertyInfo propertyInfo)
        {
            try
            {
                if (IsValueType)
                    return null;

                var getMethod = propertyInfo.GetGetMethod();
                if (getMethod == null)
                    return null;

                return (Func<T, TValue>)getMethod.CreateDelegate(typeof(Func<T, TValue>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        public TValue GetValue(T target)
        {
            if (getter != null)
                return getter(target);

            return (TValue)base.GetValue(target);
        }

        public override object GetValue(object target)
        {
            if (getter != null)
                return getter((T)target);

            return base.GetValue(target);
        }

        public void SetValue(T target, TValue value)
        {
            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter(target, value);
                return;
            }

            base.SetValue(target, value);
        }

        public void SetValue(object target, TValue value)
        {
            SetValue((T)target, value);
        }

        public override void SetValue(object target, object value)
        {
            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter((T)target, (TValue)value);
                return;
            }

            base.SetValue(target, value);
        }
    }
}