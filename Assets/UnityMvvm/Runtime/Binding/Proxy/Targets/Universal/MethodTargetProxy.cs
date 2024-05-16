using System;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class MethodTargetProxy : TargetProxyBase, IObtainable, IProxyInvoker
    {
        protected readonly IProxyMethodInfo methodInfo;
        protected SendOrPostCallback postCallback;
        public MethodTargetProxy(object target, IProxyMethodInfo methodInfo) : base(target)
        {
            this.methodInfo = methodInfo;
            if (!methodInfo.ReturnType.Equals(typeof(void)))
                throw new ArgumentException("methodInfo");
        }

        public override BindingMode DefaultMode => BindingMode.OneWayToSource;

        public override Type Type => typeof(IProxyInvoker);

        public IProxyMethodInfo ProxyMethodInfo => methodInfo;

        public object GetValue()
        {
            return this;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }

        public object Invoke(params object[] args)
        {
            if (UISynchronizationContext.InThread)
            {
                if (methodInfo.IsStatic)
                {
                    methodInfo.Invoke(null, args);
                    return null;
                }

                var target = Target;
                if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                    return null;

                return methodInfo.Invoke(target, args);
            }
            else
            {
                if (postCallback == null)
                {
                    postCallback = state =>
                    {
                        if (methodInfo.IsStatic)
                        {
                            methodInfo.Invoke(null, args);
                            return;
                        }

                        var target = Target;
                        if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                            return;

                        methodInfo.Invoke(target, (object[])state);
                    };
                }
                UISynchronizationContext.Post(postCallback, args);
                return null;
            }
        }
    }
}
