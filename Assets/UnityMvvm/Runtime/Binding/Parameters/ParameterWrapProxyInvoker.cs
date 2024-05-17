using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapProxyInvoker : ParameterWrapBase, IInvoker
    {
        private readonly IProxyInvoker _invoker;

        public ParameterWrapProxyInvoker(IProxyInvoker invoker, ICommandParameter commandParameter) : base(commandParameter)
        {
            _invoker = invoker ?? throw new ArgumentNullException("invoker");
            if (!IsValid(invoker))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return _invoker.Invoke(GetParameterValue());
        }

        private bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (info.ReturnType != typeof(void))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(GetParameterValueType());
        }
    }
}
