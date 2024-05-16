

namespace Fusion.Mvvm
{
    public interface ITargetProxyFactory
    {
        ITargetProxy CreateProxy(object target, TargetDescription description);
    }
}
