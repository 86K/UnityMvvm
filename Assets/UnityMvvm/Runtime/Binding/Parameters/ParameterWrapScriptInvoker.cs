using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapScriptInvoker : ParameterWrapBase, IInvoker
    {
        private readonly IScriptInvoker invoker;

        public ParameterWrapScriptInvoker(IScriptInvoker invoker, ICommandParameter commandParameter) : base(commandParameter)
        {
            this.invoker = invoker ?? throw new ArgumentNullException("invoker");
        }

        public object Invoke(params object[] args)
        {
            return invoker.Invoke(GetParameterValue());
        }
    }
}
