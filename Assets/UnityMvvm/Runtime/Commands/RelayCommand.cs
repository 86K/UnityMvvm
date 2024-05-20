using System;

namespace Fusion.Mvvm
{
    public class RelayCommand : CommandBase
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute) : this(execute, null)
        {
        }

        public RelayCommand(Action execute, bool keepStrongRef) : this(execute, null, keepStrongRef)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this._execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this._canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null)
                _execute();
        }
    }

    public class RelayCommand<T> : CommandBase, ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action<T> execute) : this(execute, null)
        {
        }

        public RelayCommand(Action<T> execute, bool keepStrongRef) : this(execute, null, keepStrongRef)
        {
        }

        public RelayCommand(Action<T> execute, Func<bool> canExecute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this._execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this._canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null)
                _execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public void Execute(T parameter)
        {
            _execute(parameter);
        }
    }
}

