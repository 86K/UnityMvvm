

using System;

namespace Fusion.Mvvm
{
    public interface ICommandParameter
    {
        object GetValue();

        Type GetValueType();
    }

    public interface ICommandParameter<T> : ICommandParameter
    {
        new T GetValue();
    }
}
