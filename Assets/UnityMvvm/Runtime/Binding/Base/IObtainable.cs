namespace Fusion.Mvvm
{
    public interface IObtainable
    {
        object GetValue();

        TValue GetValue<TValue>();
    }
}