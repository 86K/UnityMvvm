

using System;

namespace Fusion.Mvvm
{
    public class RelayCommand : CommandBase
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

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

            this.execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this.canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && execute != null)
                execute();
        }
    }

    public class RelayCommand<T> : CommandBase, ICommand<T>
    {
        private readonly Action<T> execute;
        private readonly Func<bool> canExecute;

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

            this.execute = keepStrongRef ? execute : execute.AsWeak();

            if (canExecute != null)
                this.canExecute = keepStrongRef ? canExecute : canExecute.AsWeak();
        }

        public override bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public bool CanExecute(T parameter)
        {
            return canExecute == null || canExecute();
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && execute != null)
                execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public void Execute(T parameter)
        {
            execute(parameter);
        }
    }
}

