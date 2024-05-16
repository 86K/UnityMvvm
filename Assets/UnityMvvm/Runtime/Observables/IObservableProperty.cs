

using System;

namespace Fusion.Mvvm
{
    public interface IObservableProperty
    {
        event EventHandler ValueChanged;

        Type Type { get; }

        object Value { get; set; }
    }

    public interface IObservableProperty<T> : IObservableProperty
    {
        new T Value { get; set; }
    }
}
