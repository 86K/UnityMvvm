using System;

namespace Fusion.Mvvm
{
    public abstract class CommandBase : ICommand
    {
        private readonly object _lock = new object();
        private EventHandler _canExecuteChanged;

        public event EventHandler CanExecuteChanged
        {
            add
            {
                lock (_lock)
                {
                    _canExecuteChanged += value;
                }
            }
            remove
            {
                lock (_lock)
                {
                    _canExecuteChanged -= value;
                }
            }
        }

        protected void RaiseCanExecuteChanged()
        {
            var handler = _canExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }
}