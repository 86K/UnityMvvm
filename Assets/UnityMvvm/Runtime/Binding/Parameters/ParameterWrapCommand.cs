using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapCommand : ParameterWrapBase, ICommand
    {
        private readonly object _lock = new object();
        private readonly ICommand wrappedCommand;
        public ParameterWrapCommand(ICommand wrappedCommand, ICommandParameter commandParameter) : base(commandParameter)
        {
            this.wrappedCommand = wrappedCommand ?? throw new ArgumentNullException("wrappedCommand");
        }

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { wrappedCommand.CanExecuteChanged += value; } }
            remove { lock (_lock) { wrappedCommand.CanExecuteChanged -= value; } }
        }

        public bool CanExecute(object parameter)
        {
            return wrappedCommand.CanExecute(GetParameterValue());
        }

        public void Execute(object parameter)
        {
            var param = GetParameterValue();
            if (wrappedCommand.CanExecute(param))
                wrappedCommand.Execute(param);
        }
    }

    public class ParameterWrapCommand<T> : ICommand
    {
        private readonly object _lock = new object();
        private readonly ICommand<T> wrappedCommand;
        private readonly ICommandParameter<T> commandParameter;
        public ParameterWrapCommand(ICommand<T> wrappedCommand, ICommandParameter<T> commandParameter)
        {
            this.commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
            this.wrappedCommand = wrappedCommand ?? throw new ArgumentNullException("wrappedCommand");
        }

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { wrappedCommand.CanExecuteChanged += value; } }
            remove { lock (_lock) { wrappedCommand.CanExecuteChanged -= value; } }
        }

        public bool CanExecute(object parameter)
        {
            return wrappedCommand.CanExecute(commandParameter.GetValue());
        }

        public void Execute(object parameter)
        {
            var param = commandParameter.GetValue();
            if (wrappedCommand.CanExecute(param))
                wrappedCommand.Execute(param);
        }
    }
}
