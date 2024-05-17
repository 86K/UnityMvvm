

using System;
using System.Reflection;
using System.Linq.Expressions;
using UnityEngine;

namespace Fusion.Mvvm
{
#pragma warning disable 0414
    public class ProxyFieldInfo : IProxyFieldInfo
    {
        private readonly bool isValueType;
        private TypeCode typeCode;
        protected FieldInfo fieldInfo;

        public ProxyFieldInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                throw new ArgumentNullException("fieldInfo");

            this.fieldInfo = fieldInfo;
            //this.isValueType = this.fieldInfo.DeclaringType.GetTypeInfo().IsValueType;
            isValueType = this.fieldInfo.DeclaringType.IsValueType;
        }

        public virtual bool IsValueType => isValueType;

        public virtual Type ValueType => fieldInfo.FieldType;

        public TypeCode ValueTypeCode
        {
            get
            {
                if (typeCode == TypeCode.Empty)
                {
#if NETFX_CORE
                    typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(ValueType);
#else
                    typeCode = Type.GetTypeCode(ValueType);
#endif
                }
                return typeCode;
            }
        }

        public virtual Type DeclaringType => fieldInfo.DeclaringType;

        public virtual string Name => fieldInfo.Name;

        public virtual bool IsStatic => fieldInfo.IsStatic();

        public virtual object GetValue(object target)
        {
            return fieldInfo.GetValue(target);
        }

        public virtual void SetValue(object target, object value)
        {
            if (fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException($"The type \"{fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            fieldInfo.SetValue(target, value);
        }
    }

#pragma warning disable 0414
    public class ProxyFieldInfo<T, TValue> : ProxyFieldInfo, IProxyFieldInfo<T, TValue>
    {
        private readonly Func<T, TValue> getter;
        private readonly Action<T, TValue> setter;

        public ProxyFieldInfo(string fieldName) : this(typeof(T).GetField(fieldName))
        {
        }

        public ProxyFieldInfo(FieldInfo fieldInfo) : base(fieldInfo)
        {
            if (!typeof(TValue).Equals(this.fieldInfo.FieldType) || !DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The field types do not match!");

            getter = MakeGetter(fieldInfo);
            setter = MakeSetter(fieldInfo);
        }

        public ProxyFieldInfo(string fieldName, Func<T, TValue> getter, Action<T, TValue> setter) : this(typeof(T).GetField(fieldName), getter, setter)
        {
        }

        public ProxyFieldInfo(FieldInfo fieldInfo, Func<T, TValue> getter, Action<T, TValue> setter) : base(fieldInfo)
        {
            if (!typeof(TValue).Equals(this.fieldInfo.FieldType) || !DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The field types do not match!");

            this.getter = getter;
            this.setter = setter;
        }

        private Action<T, TValue> MakeSetter(FieldInfo fieldInfo)
        {
            if (IsValueType)
                return null;

            if (fieldInfo.IsInitOnly)
                return null;

            try
            {
                bool expressionSupportRestricted = false;
#if ENABLE_IL2CPP
                //Only reference types are supported; value types are not supported
                expressionSupportRestricted = true;
#endif
                if (!expressionSupportRestricted || !(typeof(T).IsValueType || typeof(TValue).IsValueType))
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
                bool expressionSupportRestricted = false;
#if ENABLE_IL2CPP
                //Only reference types are supported; value types are not supported
                expressionSupportRestricted = true;
#endif
                if (!expressionSupportRestricted || !(typeof(T).IsValueType || typeof(TValue).IsValueType))
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
            if (getter != null)
                return getter(target);

            return (TValue)fieldInfo.GetValue(target);
        }

        public override object GetValue(object target)
        {
            if (getter != null)
                return getter((T)target);

            return fieldInfo.GetValue(target);
        }

        TValue IProxyFieldInfo<TValue>.GetValue(object target)
        {
            return GetValue((T)target);
        }

        public void SetValue(T target, TValue value)
        {
            if (fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException($"The type \"{fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter(target, value);
                return;
            }

            fieldInfo.SetValue(target, value);
        }

        public override void SetValue(object target, object value)
        {
            if (fieldInfo.IsInitOnly)
                throw new MemberAccessException($"The field \"{fieldInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException($"The type \"{fieldInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter((T)target, (TValue)value);
                return;
            }

            fieldInfo.SetValue(target, value);
        }

        public void SetValue(object target, TValue value)
        {
            SetValue((T)target, value);
        }
    }
}
