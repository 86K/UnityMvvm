using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class InteractionNodeProxy : SourceProxyBase, IModifiable
    {
        
        private readonly IInteractionRequest request;

        private bool disposed;
        protected IInvoker invoker;
        protected Delegate handler;

        public InteractionNodeProxy(IInteractionRequest request) : this(null, request)
        {
        }

        public InteractionNodeProxy(object source, IInteractionRequest request) : base(source)
        {
            this.request = request;
            BindEvent();
        }

        public override Type Type => typeof(EventHandler<InteractionEventArgs>);

        public virtual void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        public virtual void SetValue(object value)
        {
            if (value != null && !(value is IInvoker || value is Delegate))
                throw new ArgumentException("Binding object to InteractionRequest failed, unsupported object type", "value");

            if (this.invoker != null)
                this.invoker = null;

            if (this.handler != null)
                this.handler = null;

            if (value == null)
                return;

            //Bind Method
            if (value is IProxyInvoker proxyInvoker)
            {
                if (IsValid(proxyInvoker))
                {
                    invoker = proxyInvoker;
                    return;
                }

                throw new ArgumentException("Binding the IProxyInvoker to InteractionRequest failed, mismatched parameter type.");
            }
            else if (value is IInvoker invoker)
            {
                this.invoker = invoker;
            }

            //Bind Delegate
            if (value is Delegate handler)
            {
                if (IsValid(handler))
                {
                    this.handler = handler;
                    return;
                }

                throw new ArgumentException("Binding the Delegate to InteractionRequest failed, mismatched parameter type.");
            }
        }

        protected virtual bool IsValid(Delegate handler)
        {
            if (handler is EventHandler<InteractionEventArgs>)
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

            return parameterTypes[0].IsAssignableFrom(typeof(object)) && parameterTypes[1].IsAssignableFrom(typeof(InteractionEventArgs));
        }

        protected virtual bool IsValid(IProxyInvoker invoker)
        {
            IProxyMethodInfo info = invoker.ProxyMethodInfo;
            if (!info.ReturnType.Equals(typeof(void)))
                return false;

            ParameterInfo[] parameters = info.Parameters;
            if (parameters == null || parameters.Length != 2)
                return false;

            return parameters[0].ParameterType.IsAssignableFrom(typeof(object)) && parameters[1].ParameterType.IsAssignableFrom(typeof(InteractionEventArgs));
        }

        protected virtual void BindEvent()
        {
            if (request != null)
                request.Raised += OnRaised;
        }

        protected virtual void UnbindEvent()
        {
            if (request != null)
                request.Raised -= OnRaised;
        }

        protected virtual void OnRaised(object sender, InteractionEventArgs args)
        {
            try
            {
                if (invoker != null)
                {
                    invoker.Invoke(sender, args);
                    return;
                }

                if (handler != null)
                {
                    if (handler is EventHandler<InteractionEventArgs> eventHandler)
                        eventHandler(sender, args);
                    else
                        handler.DynamicInvoke(sender, args);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                UnbindEvent();
                handler = null;
                invoker = null;
                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
