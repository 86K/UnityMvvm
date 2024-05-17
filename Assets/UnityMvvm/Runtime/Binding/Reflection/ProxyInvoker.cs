namespace Fusion.Mvvm
{
    public class ProxyInvoker : IProxyInvoker
    {
        private readonly object _target;
        private readonly IProxyMethodInfo _proxyMethodInfo;

        public ProxyInvoker(object target, IProxyMethodInfo proxyMethodInfo)
        {
            _target = target;
            _proxyMethodInfo = proxyMethodInfo;
        }

        public IProxyMethodInfo ProxyMethodInfo => _proxyMethodInfo;

        public object Invoke(params object[] args)
        {
            return _proxyMethodInfo.Invoke(_target, args);
        }
    }
}