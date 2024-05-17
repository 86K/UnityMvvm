using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    internal static class DelegateExtensions
    {
        internal static Type ReturnType(this Delegate del)
        {
            MethodInfo info = del.GetType().GetMethod("Invoke");
            if (info == null)
                return null;

            return info.ReturnType;
        }

        internal static Type ParameterType(this Delegate del)
        {
            MethodInfo info = del.GetType().GetMethod("Invoke");
            if (info == null)
                return null;

            ParameterInfo[] parameters = info.GetParameters();
            if (parameters.Length <= 0)
                return null;

            return parameters[0].ParameterType;
        }
    }
}
