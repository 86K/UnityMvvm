using System;

namespace Fusion.Mvvm
{
    public interface IProxyFieldInfo : IProxyMemberInfo
    {
        Type ValueType { get; }

        TypeCode ValueTypeCode { get; }

        object GetValue(object target);

        void SetValue(object target, object value);
    }

    public interface IProxyFieldInfo<TValue> : IProxyFieldInfo
    {
        new TValue GetValue(object target);

        void SetValue(object target, TValue value);
    }
}