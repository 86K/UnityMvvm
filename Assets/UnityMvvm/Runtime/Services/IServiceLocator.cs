namespace Fusion.Mvvm
{
    public interface IServiceLocator
    {
        T Resolve<T>();

        T Resolve<T>(string name);
    }
}