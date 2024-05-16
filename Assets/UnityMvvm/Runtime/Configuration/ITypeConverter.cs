using System;

namespace Fusion.Mvvm
{
    public interface ITypeConverter
    {
        bool Support(Type type);
        
        object Convert(Type type, object value);
    }
}