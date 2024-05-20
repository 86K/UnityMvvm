namespace Fusion.Mvvm
{
    public interface IInvoker
    {
        object Invoke(params object[] args);
    }

    public interface IInvoker<in T> : IInvoker
    {
        object Invoke(T arg);
    }
}