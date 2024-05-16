using System;

namespace Fusion.Mvvm
{
    public class MethodNodeProxy : SourceProxyBase, IObtainable
    {
        protected IProxyMethodInfo methodInfo;
        protected IProxyInvoker invoker;

        public MethodNodeProxy(IProxyMethodInfo methodInfo) : this(null, methodInfo)
        {
        }

        public MethodNodeProxy(object source, IProxyMethodInfo methodInfo) : base(source)
        {
            this.methodInfo = methodInfo;
            invoker = new ProxyInvoker(this.source, this.methodInfo);
        }

        public override Type Type => typeof(IProxyInvoker);

        public object GetValue()
        {
            return invoker;
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)invoker;
        }
    }
}
