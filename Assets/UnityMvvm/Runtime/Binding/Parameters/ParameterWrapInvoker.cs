using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapInvoker : IInvoker
    {
        private readonly IInvoker _invoker;
        private readonly ICommandParameter _commandParameter;

        public ParameterWrapInvoker(IInvoker invoker, ICommandParameter commandParameter)
        {
            _invoker = invoker ?? throw new ArgumentNullException("invoker");
            _commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public object Invoke(params object[] args)
        {
            return _invoker.Invoke(_commandParameter.GetValue());
        }
    }

    public class ParameterWrapInvoker<T> : IInvoker
    {
        private readonly IInvoker<T> invoker;
        private readonly ICommandParameter<T> commandParameter;

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