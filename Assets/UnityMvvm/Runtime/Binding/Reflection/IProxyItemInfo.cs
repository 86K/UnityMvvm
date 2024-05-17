using System;

namespace Fusion.Mvvm
{
    public interface IProxyItemInfo : IProxyMemberInfo
    {
        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target, object key);

        void SetValue(object target, object key, object value);
    }

    public interface IProxyItemInfo<in TKey, TValue> : IProxyItemInfo
    {
        TValue GetValue(object target, TKey key);

        void SetValue(object target, TKey key, TValue value);
    }
    
    public interface IProxyItemInfo<in T, in TKey, TValue> : IProxyItemInfo<TKey, TValue>
    {
        TValue GetValue(T target, TKey key);
    
        void SetValue(T target, TKey key, TValue value);
    }
}