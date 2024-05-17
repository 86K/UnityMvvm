using System;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class MethodTargetProxy : TargetProxyBase, IObtainable, IProxyInvoker
    {
        private readonly IProxyMethodInfo _methodInfo;
        private SendOrPostCallback _postCallback;

        public MethodTargetProxy(object target, IProxyMethodInfo methodInfo) : base(target)
        {
            _methodInfo = methodInfo;
            if (!(methodInfo.ReturnType == typeof(void)))
                throw new ArgumentException("methodInfo");
        }

        public override BindingMode DefaultMode => BindingMode.OneWayToSource;

        public override Type Type => typeof(IProxyInvoker);

        public IProxyMethodInfo ProxyMethodInfo => _methodInfo;

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
                if (_methodInfo.IsStatic)
                {
                    _methodInfo.Invoke(null, args);
                    return null;
                }

                var target = Target;
                if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                    return null;

                return _methodInfo.Invoke(target, args);
            }

            if (_postCallback == null)
            {
                _postCallback = state =>
                {
                    if (_methodInfo.IsStatic)
                    {
                        _methodInfo.Invoke(null, args);
                        return;
                    }

                    var target = Target;
                    if (target == null || (target is Behaviour behaviour && !behaviour.isActiveAndEnabled))
                        return;

                    _methodInfo.Invoke(target, (object[])state);
                };
            }

            UISynchronizationContext.Post(_postCallback, args);
            return null;
        }
    }
}