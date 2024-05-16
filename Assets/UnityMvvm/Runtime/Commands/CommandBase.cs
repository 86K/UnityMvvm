

using System;

namespace Fusion.Mvvm
{
    public abstract class CommandBase : ICommand
    {
        private readonly object _lock = new object();
        private EventHandler canExecuteChanged;

        public event EventHandler CanExecuteChanged
        {
            add { lock (_lock) { canExecuteChanged += value; } }
            remove { lock (_lock) { canExecuteChanged -= value; } }
        }

        public virtual void RaiseCanExecuteChanged()
        {
            var handler = canExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public abstract bool CanExecute(object parameter);

        public abstract void Execute(object parameter);
    }
}
