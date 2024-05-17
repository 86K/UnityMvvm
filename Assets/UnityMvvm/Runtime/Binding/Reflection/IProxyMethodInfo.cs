using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    public interface IProxyMethodInfo : IProxyMemberInfo
    {
        Type ReturnType { get; }

        ParameterInfo[] Parameters { get; }
        
        object Invoke(object target, params object[] args);
    }
}