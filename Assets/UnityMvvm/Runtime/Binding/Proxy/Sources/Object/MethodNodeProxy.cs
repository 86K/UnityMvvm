using System;

namespace Fusion.Mvvm
{
    public class MethodNodeProxy : SourceProxyBase, IObtainable
    {
        private readonly IProxyInvoker _invoker;

        public MethodNodeProxy(IProxyMethodInfo methodInfo) : this(null, methodInfo)
        {
        }

        public MethodNodeProxy(object source, IProxyMethodInfo methodInfo) : base(source)
        {
            IProxyMethodInfo info = methodInfo;
            _invoker = new ProxyInvoker(Source, info);
        }

        public override Type Type => typeof(IProxyInvoker);

        public object GetValue()
        {
            return _invoker;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)_invoker;
        }
    }
}
