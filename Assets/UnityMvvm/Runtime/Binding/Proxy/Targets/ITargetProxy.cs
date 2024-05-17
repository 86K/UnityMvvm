using System;

namespace Fusion.Mvvm
{
    public interface ITargetProxy : IBindingProxy
    {
        Type Type { get; }

        TypeCode TypeCode { get; }

        BindingMode DefaultMode { get; }
    }
}
