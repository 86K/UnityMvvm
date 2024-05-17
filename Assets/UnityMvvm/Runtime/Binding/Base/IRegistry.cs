namespace Fusion.Mvvm
{
    public interface IRegistry<in T>
    {
        void Register(T type, int priority = 100);

        void Unregister(T type);
    }
}