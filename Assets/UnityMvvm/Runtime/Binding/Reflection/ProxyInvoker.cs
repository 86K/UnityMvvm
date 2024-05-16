

namespace Fusion.Mvvm
{
    public class ProxyInvoker : IProxyInvoker
    {
        private readonly object target;
        private readonly IProxyMethodInfo proxyMethodInfo;
        public ProxyInvoker(object target, IProxyMethodInfo proxyMethodInfo)
        {
            this.target = target;
            this.proxyMethodInfo = proxyMethodInfo;
        }

        public virtual IProxyMethodInfo ProxyMethodInfo => proxyMethodInfo;

        public object Invoke(params object[] args)
        {
            return proxyMethodInfo.Invoke(target, args);
        }
    }
}
