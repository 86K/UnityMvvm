using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class ParameterWrapDelegateInvoker : ParameterWrapBase, IInvoker
    {
        private readonly Delegate handler;

        public ParameterWrapDelegateInvoker(Delegate handler, ICommandParameter commandParameter) : base(commandParameter)
        {
            this.handler = handler ?? throw new ArgumentNullException("handler");
            if (!IsValid(handler))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return handler.DynamicInvoke(GetParameterValue());
        }

        protected virtual bool IsValid(Delegate handler)
        {
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 1)
                return false;

            return parameterTypes[0].IsAssignableFrom(GetParameterValueType());
        }
    }

    public class ParameterWrapActionInvoker<T> : IInvoker
    {
        private readonly Action<T> handler;
        private readonly ICommandParameter<T> commandParameter;
        public ParameterWrapActionInvoker(Action<T> handler, ICommandParameter<T> commandParameter)
        {
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
            this.handler = handler ?? throw new ArgumentNullException("handler");
        }

        public object Invoke(params object[] args)
        {
            handler(commandParameter.GetValue());
            return null;
        }
    }
}
