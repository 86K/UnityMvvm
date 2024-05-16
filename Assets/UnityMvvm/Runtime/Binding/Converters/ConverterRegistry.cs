

namespace Fusion.Mvvm
{
    public class ConverterRegistry: KeyValueRegistry<string,IConverter>, IConverterRegistry
    {
        public ConverterRegistry()
        {
            Init();
        }

        protected virtual void Init()
        {
        }
    }
}
