

using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ProxyPropertyInfo : IProxyPropertyInfo
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ProxyPropertyInfo));

        private readonly bool isValueType;
        private TypeCode typeCode;
        protected PropertyInfo propertyInfo;
        protected MethodInfo getMethod;
        protected MethodInfo setMethod;

        public ProxyPropertyInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            this.propertyInfo = propertyInfo;
            //this.isValueType = this.propertyInfo.DeclaringType.GetTypeInfo().IsValueType;
            isValueType = this.propertyInfo.DeclaringType.IsValueType;

            if (this.propertyInfo.CanRead)
                getMethod = propertyInfo.GetGetMethod();

            if (this.propertyInfo.CanWrite && !isValueType)
                setMethod = propertyInfo.GetSetMethod();
        }

        public virtual bool IsValueType => isValueType;

        public virtual Type ValueType => propertyInfo.PropertyType;

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

        public virtual Type DeclaringType => propertyInfo.DeclaringType;

        public virtual string Name => propertyInfo.Name;

        public virtual bool IsStatic => propertyInfo.IsStatic();

        public virtual object GetValue(object target)
        {
            if (getMethod == null)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is not public");

            return getMethod.Invoke(target, null);
        }

        public virtual void SetValue(object target, object value)
        {
            if (!propertyInfo.CanWrite)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is read-only.");

            if (IsValueType)
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setMethod == null)
                throw new MemberAccessException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is not public");

            setMethod.Invoke(target, new object[] { value });
        }
    }

    public class ProxyPropertyInfo<T, TValue> : ProxyPropertyInfo, IProxyPropertyInfo<T, TValue>
    {
        
        private readonly Func<T, TValue> getter;
        private readonly Action<T, TValue> setter;

        public ProxyPropertyInfo(string propertyName) : this(typeof(T).GetProperty(propertyName))
        {
        }

        public ProxyPropertyInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            if (!typeof(TValue).Equals(this.propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The property types do not match!");

            if (IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is static.");

            getter = MakeGetter(propertyInfo);
            setter = MakeSetter(propertyInfo);
        }

        public ProxyPropertyInfo(string propertyName, Func<T, TValue> getter, Action<T, TValue> setter) : this(typeof(T).GetProperty(propertyName), getter, setter)
        {
        }

        public ProxyPropertyInfo(PropertyInfo propertyInfo, Func<T, TValue> getter, Action<T, TValue> setter) : base(propertyInfo)
        {
            if (!typeof(TValue).Equals(this.propertyInfo.PropertyType) || !propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
                throw new ArgumentException("The property types do not match!");

            if (IsStatic)
                throw new ArgumentException($"The property \"{propertyInfo.DeclaringType}.{Name}\" is static.");

            this.getter = getter;
            this.setter = setter;
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

        TValue IProxyPropertyInfo<TValue>.GetValue(object target)
        {
            return GetValue((T)target);
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
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

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
                throw new NotSupportedException($"The type \"{propertyInfo.DeclaringType}\" is a value type, and non-reference types cannot support assignment operations.");

            if (setter != null)
            {
                setter((T)target, (TValue)value);
                return;
            }

            base.SetValue(target, value);
        }

    }
}
