using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ProxyFieldInfo : IProxyFieldInfo
    {
        private TypeCode _typeCode;
        protected readonly FieldInfo _fieldInfo;

        public ProxyFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            _fieldInfo = fieldInfo;
            IsValueType = _fieldInfo.DeclaringType.GetTypeInfo().IsValueType;
        }

        protected bool IsValueType { get; }

        public virtual Type ValueType => _fieldInfo.FieldType;

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

        public virtual Type DeclaringType => _fieldInfo.DeclaringType;

        public virtual string Name => _fieldInfo.Name;

        public virtual bool IsStatic => _fieldInfo.IsStatic();

        public virtual object GetValue(object target)
        {
            return _fieldInfo.GetValue(target);
        }

        public virtual void SetValue(object target, object value)
        {
            if (_fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{_fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            _fieldInfo.SetValue(target, value);
        }
    }
    
    public class ProxyFieldInfo<T, TValue> : ProxyFieldInfo
    {
        private readonly Func<T, TValue> _getter;
        private readonly Action<T, TValue> _setter;

        public ProxyFieldInfo(string fieldName, Func<T, TValue> getter, Action<T, TValue> setter) : this(typeof(T).GetField(fieldName), getter,
            setter)
        {
        }

        private ProxyFieldInfo(FieldInfo fieldInfo, Func<T, TValue> getter, Action<T, TValue> setter) : base(fieldInfo)
        {
            if (!(typeof(TValue) == _fieldInfo.FieldType) || !DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The field types do not match!");

            _getter = getter;
            _setter = setter;
        }

        private Action<T, TValue> MakeSetter(FieldInfo fieldInfo)
        {
            if (IsValueType)
                return null;

            if (fieldInfo.IsInitOnly)
                return null;

            try
            {
                if (!(typeof(T).IsValueType || typeof(TValue).IsValueType))
                {
                    var targetExp = Expression.Parameter(typeof(T), "target");
                    var paramExp = Expression.Parameter(typeof(TValue), "value");
                    var fieldExp = Expression.Field(fieldInfo.IsStatic ? null : targetExp, fieldInfo);
                    var assignExp = Expression.Assign(fieldExp, paramExp);
                    var lambda = Expression.Lambda<Action<T, TValue>>(assignExp, targetExp, paramExp);
                    return lambda.Compile();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private Func<T, TValue> MakeGetter(FieldInfo fieldInfo)
        {
            try
            {
                if (!(typeof(T).IsValueType || typeof(TValue).IsValueType))
                {
                    var targetExp = Expression.Parameter(typeof(T), "target");
                    var fieldExp = Expression.Field(fieldInfo.IsStatic ? null : targetExp, fieldInfo);
                    var lambda = Expression.Lambda<Func<T, TValue>>(fieldExp, targetExp);
                    return lambda.Compile();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        public override Type DeclaringType => typeof(T);

        public TValue GetValue(T target)
        {
            if (_getter != null)
                return _getter(target);

            return (TValue)_fieldInfo.GetValue(target);
        }

        public override object GetValue(object target)
        {
            if (_getter != null)
                return _getter((T)target);

            return _fieldInfo.GetValue(target);
        }

        public void SetValue(T target, TValue value)
        {
            if (_fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{_fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (_setter != null)
            {
                _setter(target, value);
                return;
            }

            _fieldInfo.SetValue(target, value);
        }

        public override void SetValue(object target, object value)
        {
            if (_fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{_fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException(
                    $"The type \"{_fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (_setter != null)
            {
                _setter((T)target, (TValue)value);
                return;
            }

            _fieldInfo.SetValue(target, value);
        }

        public void SetValue(object target, TValue value)
        {
            SetValue((T)target, value);
        }
    }
}