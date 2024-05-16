

using System;

namespace Fusion.Mvvm
{
    public interface IProxyFactory
    {
        IProxyType Create(Type type);
    }
}
