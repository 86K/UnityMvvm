using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class UnityEventProxyBase<T> : EventTargetProxyBase where T : UnityEventBase
    {
        private bool _disposed;
        protected ICommand _command;
        protected IInvoker _invoker;
        protected Delegate _handler;

        private IProxyPropertyInfo _interactable;
        private SendOrPostCallback _interactablePostCallback;
        protected readonly T _unityEvent;

        protected UnityEventProxyBase(object target, T unityEvent) : base(target)
        {
            _unityEvent = unityEvent ?? throw new ArgumentNullException("unityEvent");
            BindEvent();
        }

        public override BindingMode DefaultMode => BindingMode.OneWay;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                UnbindCommand(_command);
                UnbindEvent();
                _disposed = true;
                base.Dispose(disposing);
            }
        }

        protected abstract void BindEvent();

        protected abstract void UnbindEvent();

        protected abstract bool IsValid(Delegate handler);

        protected abstract bool IsValid(IProxyInvoker invoker);

        public override void SetValue(object value)
        {
            var target = Target;
            if (target == null)
                return;

            if (_command != null)
            {
                UnbindCommand(_command);
                _command = null;
            }

            _invoker = null;
            _handler = null;

            if (value == null)
                return;

            //Bind Command
            if (value is ICommand command)
            {
                if (_interactable == null)
                {
                    var interactablePropertyInfo = target.GetType().GetProperty("interactable");
                    if (interactablePropertyInfo != null)
                        _interactable = interactablePropertyInfo.AsProxy();
                }

                _command = command;
                BindCommand(_command);
                UpdateTargetInteractable();
                return;
            }

            //Bind Method
            if (value is IProxyInvoker proxyInvoker)
            {
                if (!IsValid(proxyInvoker))
                    throw new ArgumentException("Bind method failed.the parameter types do not match.");
                _invoker = proxyInvoker;
                return;
            }

            //Bind Delegate
            if (value is Delegate handler)
            {
                if (!IsValid(handler))
                    throw new ArgumentException("Bind method failed.the parameter types do not match.");
                _handler = handler;
                return;
            }

            //Bind Script Function
            if (value is IInvoker invoker)
            {
                _invoker = invoker;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        private void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (UISynchronizationContext.InThread)
            {
                UpdateTargetInteractable(null);
            }
            else
            {
                if (_interactablePostCallback == null)
                    _interactablePostCallback = UpdateTargetInteractable;
                UISynchronizationContext.Post(_interactablePostCallback, null);
            }
        }

        private void UpdateTargetInteractable(object state = null)
        {
            var target = Target;
            if (_interactable == null || target == null)
                return;

            bool value = _command?.CanExecute(null) ?? false;
            if (_interactable is IProxyPropertyInfo<bool> info)
            {
                info.SetValue(target, value);
                return;
            }

            _interactable.SetValue(target, value);
        }

        private void BindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        private void UnbindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged -= OnCanExecuteChanged;
        }
    }

    public class UnityEventProxy : UnityEventProxyBase<UnityEvent>
    {
        public UnityEventProxy(object target, UnityEvent unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type => typeof(UnityEvent);

        protected override void BindEvent()
        {
            _unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            _unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction || handler is Action)
                return true;

            MethodInfo info = handler.Method;
            if (!(info.ReturnType == typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 0)
                return false;
            return true;
        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!(info.ReturnType == typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters != null && parameters.Length != 0)
                return false;
            return true;
        }

        private void OnEvent()
        {
            try
            {
                if (_command != null)
                {
                    _command.Execute(null);
                    return;
                }

                if (_invoker != null)
                {
                    _invoker.Invoke();
                    return;
                }

                if (_handler != null)
                {
                    if (_handler is Action action)
                        action();
                    else if (_handler is UnityAction unityAction)
                        unityAction();
                    else
                        _handler.DynamicInvoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }
    }

    public class UnityEventProxy<T> : UnityEventProxyBase<UnityEvent<T>>
    {
        public UnityEventProxy(object target, UnityEvent<T> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type => typeof(UnityEvent<T>);

        protected override void BindEvent()
        {
            _unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            _unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction<T> || handler is Action<T>)
                return true;

            MethodInfo info = handler.Method;
            if (!(info.ReturnType == typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 1)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(T));
        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!(info.ReturnType == typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T));
        }

        private void OnEvent(T parameter)
        {
            try
            {
                if (_command != null)
                {
                    if (_command is ICommand<T> genericCommand)
                        genericCommand.Execute(parameter);
                    else
                        _command.Execute(parameter);
                    return;
                }

                if (_invoker != null)
                {
                    if (_invoker is IInvoker<T> genericInvoker)
                        genericInvoker.Invoke(parameter);
                    else
                        _invoker.Invoke(parameter);
                    return;
                }

                if (_handler != null)
                {
                    if (_handler is Action<T> action)
                        action(parameter);
                    else if (_handler is UnityAction<T> unityAction)
                        unityAction(parameter);
                    else
                        _handler.DynamicInvoke(parameter);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }
    }
}