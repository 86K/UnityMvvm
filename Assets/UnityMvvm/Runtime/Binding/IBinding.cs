using System;

namespace Fusion.Mvvm
{
    public interface IBinding : IDisposable
    {
        IBindingContext BindingContext { get; set; }

        object DataContext { get; set; }
    }
}
