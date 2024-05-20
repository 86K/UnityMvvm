using System;

namespace Fusion.Mvvm
{
    /// <summary>
    /// 条件命令。
    /// 当条件不存在或者条件满足时，才会执行命令委托。
    /// </summary>
    public class ConditionCommand : CommandBase
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public ConditionCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");

            if (canExecute != null)
                _canExecute = canExecute;
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

    public class ConditionCommand<T> : CommandBase, ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<bool> _canExecute;

        public ConditionCommand(Action<T> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");

            if (canExecute != null)
                _canExecute = canExecute;
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

