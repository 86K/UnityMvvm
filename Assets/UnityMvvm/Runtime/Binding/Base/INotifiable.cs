using System;

namespace Fusion.Mvvm
{
    public interface INotifiable
    {
        event EventHandler ValueChanged;
    }
}