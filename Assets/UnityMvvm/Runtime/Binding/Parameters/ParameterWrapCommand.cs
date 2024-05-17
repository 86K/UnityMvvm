using System;

namespace Fusion.Mvvm
{
    public class ParameterWrapCommand : ParameterWrapBase, ICommand
    {
        private readonly ICommand _wrappedCommand;

        public ParameterWrapCommand(ICommand wrappedCommand, ICommandParameter commandParameter) : base(commandParameter)
        {
            _wrappedCommand = wrappedCommand ?? throw new ArgumentNullException("wrappedCommand");
        }

        public event EventHandler CanExecuteChanged
        {
            add => _wrappedCommand.CanExecuteChanged += value;
            remove => _wrappedCommand.CanExecuteChanged -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _wrappedCommand.CanExecute(GetParameterValue());
        }

        public void Execute(object parameter)
        {
            var param = GetParameterValue();
            if (_wrappedCommand.CanExecute(param))
                _wrappedCommand.Execute(param);
        }
    }

    public class ParameterWrapCommand<T> : ICommand
    {
        private readonly ICommand<T> _wrappedCommand;
        private readonly ICommandParameter<T> _commandParameter;

        public ParameterWrapCommand(ICommand<T> wrappedCommand, ICommandParameter<T> commandParameter)
        {
            _commandParameter = commandParameter ?? throw new ArgumentNullException("commandParameter");
            _wrappedCommand = wrappedCommand ?? throw new ArgumentNullException("wrappedCommand");
        }

        public event EventHandler CanExecuteChanged
        {
            add => _wrappedCommand.CanExecuteChanged += value;
            remove => _wrappedCommand.CanExecuteChanged -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _wrappedCommand.CanExecute(_commandParameter.GetValue());
        }

        public void Execute(object parameter)
        {
            var param = _commandParameter.GetValue();
            if (_wrappedCommand.CanExecute(param))
                _wrappedCommand.Execute(param);
        }
    }
}