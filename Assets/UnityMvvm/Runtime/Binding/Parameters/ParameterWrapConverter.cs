using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ParameterWrapConverter<T> : ConverterBase
    {
        private readonly ICommandParameter<T> _commandParameter;

        public ParameterWrapConverter(ICommandParameter<T> commandParameter)
        {
            _commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public override object Convert(object value)
        {
            if (value == null)
                return null;

            if (value is IInvoker<T> invoker)
                return new ParameterWrapInvoker<T>(invoker, _commandParameter);

            if (value is ICommand<T> command)
                return new ParameterWrapCommand<T>(command, _commandParameter);

            if (value is Action<T> action)
                return new ParameterWrapActionInvoker<T>(action, _commandParameter);

            if (value is Delegate @delegate)
                return new ParameterWrapDelegateInvoker(@delegate, _commandParameter);

            if (value is ICommand value1)
                return new ParameterWrapCommand(value1, _commandParameter);

            if (value is IScriptInvoker scriptInvoker)
                return new ParameterWrapScriptInvoker(scriptInvoker, _commandParameter);

            if (value is IProxyInvoker proxyInvoker)
                return new ParameterWrapProxyInvoker(proxyInvoker, _commandParameter);

            if (value is IInvoker invoker1)
                return new ParameterWrapInvoker(invoker1, _commandParameter);

            throw new NotSupportedException($"Unsupported type \"{value.GetType()}\".");
        }

        public override object ConvertBack(object value)
        {
            Debug.Log($"非法转换");
            throw new NotSupportedException();
        }
    }
}