namespace Fusion.Mvvm
{
    public interface IServiceRegistry
    {
        void Register<T>(T target);

        void Register<T>(string name, T target);
        
        void Unregister<T>();
    }
}