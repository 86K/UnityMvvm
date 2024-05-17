using System;

namespace Fusion.Mvvm
{
    [Obsolete("Use for lua...")]
    public class ParameterWrapScriptInvoker : ParameterWrapBase, IInvoker
    {
        private readonly IScriptInvoker _invoker;

        public ParameterWrapScriptInvoker(IScriptInvoker invoker, ICommandParameter commandParameter) : base(commandParameter)
        {
            _invoker = invoker ?? throw new ArgumentNullException("invoker");
        }

        public object Invoke(params object[] args)
        {
            return _invoker.Invoke(GetParameterValue());
        }
    }
}
