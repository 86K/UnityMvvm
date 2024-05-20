using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fusion.Mvvm
{
    /// <summary>
    /// 组合命令。
    /// 所有的命令组合满足时，执行命令。
    /// </summary>
    public class CompositeCommand : CommandBase
    {
        private readonly List<ICommand> _commands = new List<ICommand>();
        private readonly bool _monitorCommandActivity;
        private readonly EventHandler _onCanExecuteChangedHandler;

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeCommand"/>.
        /// </summary>
        public CompositeCommand() : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeCommand"/>.
        /// </summary>
        /// <param name="monitorCommandActivity">Indicates when the command activity is going to be monitored.</param>
        public CompositeCommand(bool monitorCommandActivity)
        {
            _monitorCommandActivity = monitorCommandActivity;
            _onCanExecuteChangedHandler = new EventHandler(OnCanExecuteChanged);
        }

        /// <summary>
        /// Adds a command to the collection and signs up for the <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        ///  <remarks>
        /// If this command is set to monitor command activity, and <paramref name="command"/> 
        /// implements the <see cref="IActiveAware"/> interface, this method will subscribe to its
        /// <see cref="IActiveAware.IsActiveChanged"/> event.
        /// </remarks>
        /// <param name="command">The command to register.</param>
        public virtual void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (command == this)
                throw new ArgumentException("Cannot register a CompositeCommand in itself.");

            lock (_commands)
            {
                if (_commands.Contains(command))
                    throw new InvalidOperationException("Cannot register the same command twice in the same CompositeCommand.");

                _commands.Add(command);
            }

            command.CanExecuteChanged += _onCanExecuteChangedHandler;
            RaiseCanExecuteChanged();

            if (_monitorCommandActivity)
            {
                if (command is IActiveAware activeAwareCommand)
                    activeAwareCommand.IsActiveChanged += OnIsActiveChanged;
            }
        }

        /// <summary>
        /// Removes a command from the collection and removes itself from the <see cref="ICommand.CanExecuteChanged"/> event of it.
        /// </summary>
        /// <param name="command">The command to unregister.</param>
        public virtual void UnregisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            lock (_commands)
            {
                if (!_commands.Remove(command))
                    return;
            }

            command.CanExecuteChanged -= _onCanExecuteChangedHandler;
            RaiseCanExecuteChanged();

            if (_monitorCommandActivity)
            {
                if (command is IActiveAware activeAwareCommand)
                    activeAwareCommand.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        private void OnIsActiveChanged(object sender, EventArgs e)
        {
            RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Forwards <see cref="ICommand.CanExecute"/> to the registered commands and returns
        /// <see langword="true" /> if all of the commands return <see langword="true" />.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        /// If the command does not require data to be passed, this object can be set to <see langword="null" />.
        /// </param>
        /// <returns><see langword="true" /> if all of the commands return <see langword="true" />; otherwise, <see langword="false" />.</returns>
        public override bool CanExecute(object parameter)
        {
            ICommand[] commandList;
            lock (_commands)
            {
                commandList = _commands.ToArray();
            }

            if (commandList.Length <= 0)
                return false;

            foreach (ICommand command in commandList)
            {
                if (!ShouldExecute(command))
                    continue;

                if (!command.CanExecute(parameter))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Forwards <see cref="ICommand.Execute"/> to the registered commands.
        /// </summary>
        /// <param name="parameter">Data used by the command.
        /// If the command does not require data to be passed, this object can be set to <see langword="null" />.
        /// </param>
        public override void Execute(object parameter)
        {
            Queue<ICommand> commands;
            lock (_commands)
            {
                commands = new Queue<ICommand>(_commands.Where(ShouldExecute).ToList());
            }

            while (commands.Count > 0)
            {
                try
                {
                    ICommand command = commands.Dequeue();
                    command.Execute(parameter);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        /// <summary>
        /// Evaluates if a command should execute.
        /// </summary>
        /// <param name="command">The command to evaluate.</param>
        /// <returns>A <see cref="bool"/> value indicating whether the command should be used 
        /// when evaluating <see cref="CompositeCommand.CanExecute"/> and <see cref="CompositeCommand.Execute"/>.</returns>
        /// <remarks>
        /// If this command is set to monitor command activity, and <paramref name="command"/>
        /// implements the <see cref="IActiveAware"/> interface, 
        /// this method will return <see langword="false" /> if the command's <see cref="IActiveAware.IsActive"/> 
        /// property is <see langword="false" />; otherwise it always returns <see langword="true" />.</remarks>
        protected virtual bool ShouldExecute(ICommand command)
        {
            if (!_monitorCommandActivity)
                return true;

            if (!(command is IActiveAware activeAwareCommand))
                return true;

            return activeAwareCommand.IsActive;
        }

        /// <summary>
        /// Gets the list of all the registered commands.
        /// </summary>
        /// <value>A list of registered commands.</value>
        /// <remarks>This returns a copy of the commands subscribed to the CompositeCommand.</remarks>
        public IList<ICommand> RegisteredCommands
        {
            get
            {
                IList<ICommand> commandList;
                lock (_commands)
                {
                    commandList = _commands.ToList();
                }

                return commandList;
            }
        }
    }
}