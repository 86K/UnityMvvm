using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class InteractionNodeProxy : SourceProxyBase, IModifiable
    {
        private readonly IInteractionRequest _request;
        private bool _disposed;
        private IInvoker _invoker;
        private Delegate _handler;

        public InteractionNodeProxy(IInteractionRequest request) : this(null, request)
        {
        }

        public InteractionNodeProxy(object source, IInteractionRequest request) : base(source)
        {
            _request = request;
            BindEvent();
        }

        public override Type Type => typeof(EventHandler<InteractionEventArgs>);

        public void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        public void SetValue(object value)
        {
            if (value != null && !(value is IInvoker || value is Delegate))
                throw new ArgumentException("Binding object to InteractionRequest failed, unsupported object type", "value");

            _invoker = null;
            _handler = null;

            if (value == null)
                return;

            //Bind Method
            if (value is IProxyInvoker proxyInvoker)
            {
                if (IsValid(proxyInvoker))
                {
                    _invoker = proxyInvoker;
                    return;
                }

                throw new ArgumentException("Binding the IProxyInvoker to InteractionRequest failed, mismatched parameter type.");
            }

            if (value is IInvoker invoker)
            {
                _invoker = invoker;
            }

            //Bind Delegate
            if (value is Delegate handler)
            {
                if (IsValid(handler))
                {
                    _handler = handler;
                    return;
                }

                throw new ArgumentException("Binding the Delegate to InteractionRequest failed, mismatched parameter type.");
            }
        }

        private bool IsValid(Delegate handler)
        {
            if (handler is EventHandler<InteractionEventArgs>)
                return true;

            MethodInfo info = handler.Method;
            if (!(info.ReturnType == typeof(void)))
                return false;

            List<Type> parameterTypes = info.GetParameterTypes();
            if (parameterTypes.Count != 2)
                return false;

            return parameterTypes[0].IsAssignableFrom(typeof(object)) && parameterTypes[1].IsAssignableFrom(typeof(InteractionEventArgs));
        }

        private bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!(info.ReturnType == typeof(void)))
                return false;

            ParameterInfo[] parameters = info.Parameters;
            if (parameters == null || parameters.Length != 2)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(object)) &&
                   parameters[1].ParameterType.IsAssignableFrom(typeof(InteractionEventArgs));
        }

        private void BindEvent()
        {
            if (_request != null)
                _request.Raised += OnRaised;
        }

        private void UnbindEvent()
        {
            if (_request != null)
                _request.Raised -= OnRaised;
        }

        private void OnRaised(object sender, InteractionEventArgs args)
        {
            try
            {
                if (_invoker != null)
                {
                    _invoker.Invoke(sender, args);
                    return;
                }

                if (_handler != null)
                {
                    if (_handler is EventHandler<InteractionEventArgs> eventHandler)
                        eventHandler(sender, args);
                    else
                        _handler.DynamicInvoke(sender, args);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                UnbindEvent();
                _handler = null;
                _invoker = null;
                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}