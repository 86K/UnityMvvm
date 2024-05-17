

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
        private bool disposed;
        protected ICommand command;
        protected IInvoker invoker;
        protected Delegate handler;

        protected IProxyPropertyInfo interactable;
        protected SendOrPostCallback interactablePostCallback;
        protected T unityEvent;

        public UnityEventProxyBase(object target, T unityEvent) : base(target)
        {
            this.unityEvent = unityEvent ?? throw new ArgumentNullException("unityEvent");
            BindEvent();
        }

        public override BindingMode DefaultMode => BindingMode.OneWay;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UnbindCommand(command);
                UnbindEvent();
                disposed = true;
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

            if (this.command != null)
            {
                UnbindCommand(this.command);
                this.command = null;
            }

            if (this.invoker != null)
                this.invoker = null;

            if (this.handler != null)
                this.handler = null;

            if (value == null)
                return;

            //Bind Command
            if (value is ICommand command)
            {
                if (interactable == null)
                {
                    var interactablePropertyInfo = target.GetType().GetProperty("interactable");
                    if (interactablePropertyInfo != null)
                        interactable = interactablePropertyInfo.AsProxy();
                }

                this.command = command;
                BindCommand(this.command);
                UpdateTargetInteractable();
                return;
            }

            //Bind Method
            if (value is IProxyInvoker proxyInvoker)
            {
                if (!IsValid(proxyInvoker))
                    throw new ArgumentException("Bind method failed.the parameter types do not match.");
                this.invoker = proxyInvoker;
                return;
            }

            //Bind Delegate
            if (value is Delegate handler)
            {
                if (!IsValid(handler))
                    throw new ArgumentException("Bind method failed.the parameter types do not match.");
                this.handler = handler;
                return;
            }

            //Bind Script Function
            if (value is IInvoker invoker)
            {
                this.invoker = invoker;
                return;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (UISynchronizationContext.InThread)
            {
                UpdateTargetInteractable(null);
            }
            else
            {
                if (interactablePostCallback == null)
                    interactablePostCallback = UpdateTargetInteractable;
                UISynchronizationContext.Post(interactablePostCallback, null);
            }
        }

        protected virtual void UpdateTargetInteractable(object state = null)
        {
            var target = Target;
            if (interactable == null || target == null)
                return;

            bool value = command == null ? false : command.CanExecute(null);
            if (interactable is IProxyPropertyInfo<bool>)
            {
                (interactable as IProxyPropertyInfo<bool>).SetValue(target, value);
                return;
            }

            interactable.SetValue(target, value);
        }

        protected virtual void BindCommand(ICommand command)
        {
            if (command == null)
                return;

            command.CanExecuteChanged += OnCanExecuteChanged;
        }

        protected virtual void UnbindCommand(ICommand command)
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
            unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction || handler is Action)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 0)
                return false;
            return true;
        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters != null && parameters.Length != 0)
                return false;
            return true;
        }

        protected virtual void OnEvent()
        {
            try
            {
                if (command != null)
                {
                    command.Execute(null);
                    return;
                }

                if (invoker != null)
                {
                    invoker.Invoke();
                    return;
                }

                if (handler != null)
                {
                    if (handler is Action action)
                        action();
                    else if (handler is UnityAction unityAction)
                        unityAction();
                    else
                        handler.DynamicInvoke();
                    return;
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
            unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction<T> || handler is Action<T>)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 1)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(T));
        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T));
        }

        protected virtual void OnEvent(T parameter)
        {
            try
            {
                if (command != null)
                {
                    if (command is ICommand<T> genericCommand)
                        genericCommand.Execute(parameter);
                    else
                        command.Execute(parameter);
                    return;
                }

                if (invoker != null)
                {
                    if (invoker is IInvoker<T> genericInvoker)
                        genericInvoker.Invoke(parameter);
                    else
                        invoker.Invoke(parameter);
                    return;
                }

                if (handler != null)
                {
                    if (handler is Action<T> action)
                        action(parameter);
                    else if (handler is UnityAction<T> unityAction)
                        unityAction(parameter);
                    else
                        handler.DynamicInvoke(parameter);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }
    }

    public class UnityEventProxy<T0, T1> : UnityEventProxyBase<UnityEvent<T0, T1>>
    {
        
        public UnityEventProxy(object target, UnityEvent<T0, T1> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type => typeof(UnityEvent<T0, T1>);

        protected override void BindEvent()
        {
            unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction<T0, T1> || handler is Action<T0, T1>)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 2)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(T0))
                && parameterTypes[1].IsAssignableFrom(typeof(T1));
        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 2)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T0))
                && parameters[1].ParameterType.IsAssignableFrom(typeof(T1));
        }

        protected virtual void OnEvent(T0 t0, T1 t1)
        {
            try
            {
                if (command != null)
                {
                    command.Execute(new object[] { t0, t1 });
                    return;
                }

                if (invoker != null)
                {
                    if (invoker is IInvoker<T0, T1> genericInvoker)
                        genericInvoker.Invoke(t0, t1);
                    else
                        invoker.Invoke(t0, t1);
                    return;
                }

                if (handler != null)
                {
                    if (handler is Action<T0, T1> action)
                        action(t0, t1);
                    else if (handler is UnityAction<T0, T1> unityAction)
                        unityAction(t0, t1);
                    else
                        handler.DynamicInvoke(t0, t1);

                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }
    }

    public class UnityEventProxy<T0, T1, T2> : UnityEventProxyBase<UnityEvent<T0, T1, T2>>
    {
        
        public UnityEventProxy(object target, UnityEvent<T0, T1, T2> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type => typeof(UnityEvent<T0, T1, T2>);

        protected override void BindEvent()
        {
            unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction<T0, T1, T2> || handler is Action<T0, T1, T2>)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 3)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(T0))
                && parameterTypes[1].IsAssignableFrom(typeof(T1))
                && parameterTypes[2].IsAssignableFrom(typeof(T2));

        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 3)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T0))
                && parameters[1].ParameterType.IsAssignableFrom(typeof(T1))
                && parameters[2].ParameterType.IsAssignableFrom(typeof(T2));
        }

        protected virtual void OnEvent(T0 t0, T1 t1, T2 t2)
        {
            try
            {
                if (command != null)
                {
                    command.Execute(new object[] { t0, t1, t2 });
                    return;
                }

                if (invoker != null)
                {
                    if (invoker is IInvoker<T0, T1, T2> genericInvoker)
                        genericInvoker.Invoke(t0, t1, t2);
                    else
                        invoker.Invoke(t0, t1, t2);
                    return;
                }

                if (handler != null)
                {
                    if (handler is Action<T0, T1, T2> action)
                        action(t0, t1, t2);
                    else if (handler is UnityAction<T0, T1, T2> unityAction)
                        unityAction(t0, t1, t2);
                    else
                        handler.DynamicInvoke(t0, t1, t2);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }
    }

    public class UnityEventProxy<T0, T1, T2, T3> : UnityEventProxyBase<UnityEvent<T0, T1, T2, T3>>
    {
        
        public UnityEventProxy(object target, UnityEvent<T0, T1, T2, T3> unityEvent) : base(target, unityEvent)
        {
        }

        public override Type Type => typeof(UnityEvent<T0, T1, T2, T3>);

        protected override void BindEvent()
        {
            unityEvent.AddListener(OnEvent);
        }

        protected override void UnbindEvent()
        {
            unityEvent.RemoveListener(OnEvent);
        }

        protected override bool IsValid(Delegate handler)
        {
            if (handler is UnityAction<T0, T1, T2, T3> || handler is Action<T0, T1, T2, T3>)
                return true;
#if NETFX_CORE
            MethodInfo info = handler.GetMethodInfo();
#else
            MethodInfo info = handler.Method;
#endif
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 4)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(T0))
                && parameterTypes[1].IsAssignableFrom(typeof(T1))
                && parameterTypes[2].IsAssignableFrom(typeof(T2))
                && parameterTypes[3].IsAssignableFrom(typeof(T3));

        }

        protected override bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 4)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T0))
                && parameters[1].ParameterType.IsAssignableFrom(typeof(T1))
                && parameters[2].ParameterType.IsAssignableFrom(typeof(T2))
                && parameters[3].ParameterType.IsAssignableFrom(typeof(T3));
        }

        protected virtual void OnEvent(T0 t0, T1 t1, T2 t2, T3 t3)
        {
            try
            {
                if (command != null)
                {
                    command.Execute(new object[] { t0, t1, t2, t3 });
                    return;
                }

                if (invoker != null)
                {
                    if (invoker is IInvoker<T0, T1, T2, T3> genericInvoker)
                        genericInvoker.Invoke(t0, t1, t2, t3);
                    else
                        invoker.Invoke(t0, t1, t2, t3);
                    return;
                }

                if (handler != null)
                {
                    if (handler is UnityAction<T0, T1, T2, T3> action)
                        action(t0, t1, t2, t3);
                    else if (handler is UnityAction<T0, T1, T2, T3> unityAction)
                        unityAction(t0, t1, t2, t3);
                    else
                        handler.DynamicInvoke(t0, t1, t2, t3);
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