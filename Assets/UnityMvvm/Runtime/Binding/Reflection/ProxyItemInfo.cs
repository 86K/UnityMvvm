

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class ProxyItemInfo : IProxyItemInfo
    {
        private readonly bool isValueType;
        private TypeCode typeCode;
        protected PropertyInfo propertyInfo;
        protected MethodInfo getMethod;
        protected MethodInfo setMethod;

        public ProxyItemInfo(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            if (!propertyInfo.Name.Equals("Item"))
                throw new ArgumentException("The property types do not match!");

            this.propertyInfo = propertyInfo;
            //this.isValueType = this.propertyInfo.DeclaringType.GetTypeInfo().IsValueType;
            isValueType = this.propertyInfo.DeclaringType.IsValueType;

            if (this.propertyInfo.CanRead)
                getMethod = propertyInfo.GetGetMethod();

            if (this.propertyInfo.CanWrite)
                setMethod = propertyInfo.GetSetMethod();
        }

        public bool IsValueType => isValueType;

        public Type ValueType => propertyInfo.PropertyType;

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

        public Type DeclaringType => propertyInfo.DeclaringType;

        public string Name => propertyInfo.Name;

        public bool IsStatic => propertyInfo.IsStatic();

        public object GetValue(object target, object key)
        {
            if (target is IList list)
            {
                int index = (int)key;

                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException("key",
                        $"The index is out of range, the key value is {index}, it is not between 0 and {list.Count}");

                return list[index];
            }

            if (target is IDictionary dict)
            {
                if (!dict.Contains(key))
                    return null;

                return dict[key];
            }

            if (getMethod == null)
                throw new MemberAccessException();

            return getMethod.Invoke(target, new object[] { key });
        }

        public void SetValue(object target, object key, object value)
        {
            if (target is IList list)
            {
                int index = (int)key;

                if (index < 0 || index >= list.Count)
                    throw new ArgumentOutOfRangeException("key",
                        $"The index is out of range, the key value is {index}, it is not between 0 and {list.Count}");

                list[index] = value;
                return;
            }

            if (target is IDictionary dictionary)
            {
                dictionary[key] = value;
                return;
            }

            if (setMethod == null)
                throw new MemberAccessException();

            setMethod.Invoke(target, new object[] { key, value });
        }
    }

    public class ListProxyItemInfo<T, TValue> : ProxyItemInfo, IProxyItemInfo<T, int, TValue> where T : IList<TValue>
    {
        public ListProxyItemInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            if (!(typeof(TValue) == this.propertyInfo.PropertyType) || !typeof(IList<TValue>).IsAssignableFrom(propertyInfo.DeclaringType))
                throw new ArgumentException("The property types do not match!");
        }

        public TValue GetValue(T target, int key)
        {
            if (key < 0 || key >= target.Count)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {key}, it is not between 0 and {target.Count}");

            return target[key];
        }

        public TValue GetValue(object target, int key)
        {
            return GetValue((T)target, key);
        }

        public void SetValue(T target, int key, TValue value)
        {
            if (key < 0 || key >= target.Count)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {key}, it is not between 0 and {target.Count}");

            target[key] = value;
        }

        public void SetValue(object target, int key, TValue value)
        {
            SetValue((T)target, key, value);
        }
    }

    public class DictionaryProxyItemInfo<T, TKey, TValue> : ProxyItemInfo, IProxyItemInfo<T, TKey, TValue> where T : IDictionary<TKey, TValue>
    {
        public DictionaryProxyItemInfo(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            if (!(typeof(TValue) == this.propertyInfo.PropertyType) || !typeof(IDictionary<TKey, TValue>).IsAssignableFrom(propertyInfo.DeclaringType))
                throw new ArgumentException("The property types do not match!");
        }

        public TValue GetValue(T target, TKey key)
        {
            if (!target.ContainsKey(key))
                return default;

            return target[key];
        }

        public TValue GetValue(object target, TKey key)
        {
            return GetValue((T)target, key);
        }

        public void SetValue(T target, TKey key, TValue value)
        {
            target[key] = value;
        }

        public void SetValue(object target, TKey key, TValue value)
        {
            SetValue((T)target, key, value);
        }
    }

    public class ArrayProxyItemInfo : IProxyItemInfo
    {
        private TypeCode typeCode;
        protected readonly Type type;
        public ArrayProxyItemInfo(Type type)
        {
            this.type = type;
            if (this.type == null || !this.type.IsArray)
                throw new ArgumentException();
        }

        public Type ValueType => type.HasElementType ? type.GetElementType() : typeof(object);

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

        public Type DeclaringType => type;

        public string Name => "Item";

        public bool IsStatic => false;

        public virtual object GetValue(object target, object key)
        {
            int index = (int)key;
            Array array = target as Array;
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {index}, it is not between 0 and {array.Length}");

            return array.GetValue(index);
        }

        public virtual void SetValue(object target, object key, object value)
        {
            int index = (int)key;
            Array array = target as Array;
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {index}, it is not between 0 and {array.Length}");

            array.SetValue(value, index);
        }
    }

    public class ArrayProxyItemInfo<T, TValue> : ArrayProxyItemInfo, IProxyItemInfo<T, int, TValue> where T : IList<TValue>
    {
        public ArrayProxyItemInfo() : base(typeof(T))
        {
        }

        public TValue GetValue(T target, int key)
        {
            if (key < 0 || key >= target.Count)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {key}, it is not between 0 and {target.Count}");

            return target[key];
        }

        public TValue GetValue(object target, int key)
        {
            return GetValue((T)target, key);
        }

        public void SetValue(T target, int key, TValue value)
        {
            if (key < 0 || key >= target.Count)
                throw new ArgumentOutOfRangeException("key",
                    $"The index is out of range, the key value is {key}, it is not between 0 and {target.Count}");

            target[key] = value;
        }

        public void SetValue(object target, int key, TValue value)
        {
            SetValue((T)target, key, value);
        }
    }
}
