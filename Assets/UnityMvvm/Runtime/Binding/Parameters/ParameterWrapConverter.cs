using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapConverter : AbstractConverter
    {
        private readonly ICommandParameter commandParameter;
        public ParameterWrapConverter(ICommandParameter commandParameter)
        {
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public override object Convert(object value)
        {
            if (value == null)
                return null;

            if (value is Delegate)
                return new ParameterWrapDelegateInvoker(value as Delegate, commandParameter);

            if (value is ICommand)
                return new ParameterWrapCommand(value as ICommand, commandParameter);

            if (value is IScriptInvoker)
                return new ParameterWrapScriptInvoker(value as IScriptInvoker, commandParameter);

            if (value is IProxyInvoker)
                return new ParameterWrapProxyInvoker(value as IProxyInvoker, commandParameter);

            if (value is IInvoker)
                return new ParameterWrapInvoker(value as IInvoker, commandParameter);

            throw new NotSupportedException($"Unsupported type \"{value.GetType()}\".");
        }

        public override object ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }

    public class ParameterWrapConverter<T> : AbstractConverter
    {
        private readonly ICommandParameter<T> commandParameter;
        public ParameterWrapConverter(ICommandParameter<T> commandParameter)
        {
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
        }

        public override object Convert(object value)
        {
            if (value == null)
                return null;

            if (value is IInvoker<T> invoker)
                return new ParameterWrapInvoker<T>(invoker, commandParameter);

            if (value is ICommand<T> command)
                return new ParameterWrapCommand<T>(command, commandParameter);

            if (value is Action<T> action)
                return new ParameterWrapActionInvoker<T>(action, commandParameter);

            if (value is Delegate)
                return new ParameterWrapDelegateInvoker(value as Delegate, commandParameter);

            if (value is ICommand)
                return new ParameterWrapCommand(value as ICommand, commandParameter);

            if (value is IScriptInvoker)
                return new ParameterWrapScriptInvoker(value as IScriptInvoker, commandParameter);

            if (value is IProxyInvoker)
                return new ParameterWrapProxyInvoker(value as IProxyInvoker, commandParameter);

            if (value is IInvoker)
                return new ParameterWrapInvoker(value as IInvoker, commandParameter);

            throw new NotSupportedException($"Unsupported type \"{value.GetType()}\".");
        }

        public override object ConvertBack(object value)
        {
            throw new NotSupportedException();
        }
    }
}
