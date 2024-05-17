namespace Fusion.Mvvm
{
    public interface IProxyInvoker : IInvoker
    {
        IProxyMethodInfo ProxyMethodInfo { get; }
    }
}