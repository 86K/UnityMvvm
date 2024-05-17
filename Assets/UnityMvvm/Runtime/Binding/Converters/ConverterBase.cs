namespace Fusion.Mvvm
{
    public abstract class ConverterBase : IConverter
    {
        public virtual object Convert(object value)
        {
            return value;
        }

        public virtual object ConvertBack(object value)
        {
            return value;
        }
    }
}