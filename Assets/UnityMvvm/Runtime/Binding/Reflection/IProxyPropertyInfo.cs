using System;

namespace Fusion.Mvvm
{
    public interface IProxyPropertyInfo : IProxyMemberInfo
    {
        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target);

        void SetValue(object target, object value);
    }

    public interface IProxyPropertyInfo<TValue> : IProxyPropertyInfo
    {
        new TValue GetValue(object target);

        void SetValue(object target, TValue value);
    }
}