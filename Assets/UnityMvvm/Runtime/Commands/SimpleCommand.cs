using System;

namespace Fusion.Mvvm
{
    public class SimpleCommand : CommandBase
    {
        private bool _enabled = true;
        private readonly Action _execute;

        public SimpleCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Enabled;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null)
                _execute();
        }
    }

    public class SimpleCommand<T> : CommandBase, ICommand<T>
    {
        private bool _enabled = true;
        private readonly Action<T> _execute;

        public SimpleCommand(Action<T> execute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Enabled;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null)
                _execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public bool CanExecute(T parameter)
        {
            return Enabled;
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }
    }
}

