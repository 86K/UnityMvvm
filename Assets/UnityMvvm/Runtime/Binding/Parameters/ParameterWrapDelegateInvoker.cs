using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class ParameterWrapDelegateInvoker : ParameterWrapBase, IInvoker
    {
        private readonly Delegate _handler;

        public ParameterWrapDelegateInvoker(Delegate handler, ICommandParameter commandParameter) : base(commandParameter)
        {
            _handler = handler ?? throw new ArgumentNullException("handler");
            if (!IsValid(handler))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return _handler.DynamicInvoke(GetParameterValue());
        }

        private bool IsValid(Delegate handler)
        {
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (info.ReturnType != typeof(void))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 1)
                return false;

            return parameterTypes[0].IsAssignableFrom(GetParameterValueType());
        }
    }

    public class ParameterWrapActionInvoker<T> : IInvoker
    {
        private readonly Action<T> _handler;
        private readonly ICommandParameter<T> _commandParameter;

        public ParameterWrapActionInvoker(Action<T> handler, ICommandParameter<T> commandParameter)
        {
            _commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
            _handler = handler ?? throw new ArgumentNullException("handler");
        }

        public object Invoke(params object[] args)
        {
            _handler(_commandParameter.GetValue());
            return null;
        }
    }
}