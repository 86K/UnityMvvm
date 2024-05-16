

using System;

namespace Fusion.Mvvm
{
    public class SimpleCommand : CommandBase
    {
        private bool enabled = true;
        private readonly Action execute;

        public SimpleCommand(Action execute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;

                enabled = value;
                RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Enabled;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && execute != null)
                execute();
        }
    }

    public class SimpleCommand<T> : CommandBase, ICommand<T>
    {
        private bool enabled = true;
        private readonly Action<T> execute;

        public SimpleCommand(Action<T> execute, bool keepStrongRef = false)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = keepStrongRef ? execute : execute.AsWeak();
        }

        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled == value)
                    return;

                enabled = value;
                RaiseCanExecuteChanged();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return Enabled;
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter) && execute != null)
                execute((T)Convert.ChangeType(parameter, typeof(T)));
        }

        public bool CanExecute(T parameter)
        {
            return Enabled;
        }

        public void Execute(T parameter)
        {
            execute(parameter);
        }
    }
}

