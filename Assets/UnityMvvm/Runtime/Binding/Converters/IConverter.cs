namespace Fusion.Mvvm
{
    public interface IConverter
    {
        object Convert(object value);

        object ConvertBack(object value);
    }
}
