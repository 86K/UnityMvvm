/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

#if UNITY_2019_1_OR_NEWER
using Loxodon.Framework.Binding.Reflection;
using Loxodon.Framework.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace Loxodon.Framework.Binding.Proxy.Targets
{
    public class ValueChangedEventProxy<T> : EventTargetProxyBase
    {
        private bool disposed = false;
        protected ICommand command;/* Command Binding */
        protected IInvoker invoker;/* Method Binding or Lua Function Binding */
        protected Delegate handler;/* Delegate Binding */
        protected SendOrPostCallback updateTargetEnableAction;
        private readonly INotifyValueChanged<T> target;
        public ValueChangedEventProxy(INotifyValueChanged<T> target) : base(target)
        {
            this.target = target;
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

        public override Type Type => typeof(object);

        protected virtual void BindEvent()
        {
            target.RegisterValueChangedCallback(OnValueChangedEvent);
        }

        protected virtual void UnbindEvent()
        {
            target.UnregisterValueChangedCallback(OnValueChangedEvent);
        }

        protected virtual bool IsValid(Delegate handler)
        {
            if (handler is Action<T>)
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

        protected virtual bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            var parameters = info.Parameters;
            if (parameters == null || parameters.Length != 1)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(T));
        }

        protected virtual void OnValueChangedEvent(ChangeEvent<T> eventArgs)
        {
            try
            {
                T value = eventArgs.newValue;
                if (command != null)
                {
                    command.Execute(value);
                    return;
                }

                if (invoker != null)
                {
                    invoker.Invoke(value);
                    return;
                }

                if (handler != null)
                {
                    if (handler is Action<T>)
                    {
                        (handler as Action<T>)(value);
                    }
                    else
                    {
                        handler.DynamicInvoke(value);
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("{0}", e));
            }
        }

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
            ICommand command = value as ICommand;
            if (command != null)
            {
                this.command = command;
                BindCommand(this.command);
                UpdateTargetEnable();
                return;
            }

            //Bind Method
            IProxyInvoker proxyInvoker = value as IProxyInvoker;
            if (proxyInvoker != null)
            {
                if (IsValid(proxyInvoker))
                {
                    this.invoker = proxyInvoker;
                    return;
                }

                throw new ArgumentException("Bind method failed.the parameter types do not match.");
            }

            //Bind Delegate
            Delegate handler = value as Delegate;
            if (handler != null)
            {
                if (IsValid(handler))
                {
                    this.handler = handler;
                    return;
                }

                throw new ArgumentException("Bind method failed.the parameter types do not match.");
            }

            //Bind Script Function
            IInvoker invoker = value as IInvoker;
            if (invoker != null)
            {
                this.invoker = invoker;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            if (updateTargetEnableAction == null)
                updateTargetEnableAction = UpdateTargetEnable;
            UISynchronizationContext.Post(updateTargetEnableAction, null);
        }

        protected virtual void UpdateTargetEnable(object state = null)
        {
            var target = Target;
            if (target == null || !(target is VisualElement))
                return;

            bool value = command == null ? false : command.CanExecute(null);
            ((VisualElement)target).SetEnabled(value);
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
}
#endif