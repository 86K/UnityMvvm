using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapProxyInvoker : ParameterWrapBase, IInvoker
    {
        private readonly IProxyInvoker invoker;

        public ParameterWrapProxyInvoker(IProxyInvoker invoker, ICommandParameter commandParameter) : base(commandParameter)
        {
            this.invoker = invoker ?? throw new ArgumentNullException("invoker");
            if (!IsValid(invoker))
                throw new ArgumentException("Bind method failed.the parameter types do not match.");
        }

        public object Invoke(params object[] args)
        {
            return invoker.Invoke(GetParameterValue());
        }

        protected bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(GetParameterValueType());
        }
    }
}
