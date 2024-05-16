

using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapInvoker : IInvoker
    {
        protected readonly IInvoker invoker;
        protected readonly ICommandParameter commandParameter;
        public ParameterWrapInvoker(IInvoker invoker, ICommandParameter commandParameter)
        {
            this.invoker = invoker ?? throw new ArgumentNullException("invoker");
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public object Invoke(params object[] args)
        {
            return invoker.Invoke(commandParameter.GetValue());
        }
    }

    public class ParameterWrapInvoker<T> : IInvoker
    {
        protected readonly IInvoker<T> invoker;
        protected readonly ICommandParameter<T> commandParameter;
        public ParameterWrapInvoker(IInvoker<T> invoker, ICommandParameter<T> commandParameter)
        {
            this.invoker = invoker ?? throw new ArgumentNullException("invoker");
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public object Invoke(params object[] args)
        {
            return invoker.Invoke(commandParameter.GetValue());
        }
    }
}
