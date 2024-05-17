namespace Fusion.Mvvm
{
    public interface IModifiable
    {
        void SetValue(object value);

        void SetValue<TValue>(TValue value);
    }
}
