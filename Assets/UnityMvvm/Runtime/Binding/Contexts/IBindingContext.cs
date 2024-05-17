using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public interface IBindingContext : IDisposable
    {
        event EventHandler DataContextChanged;

        object Owner { get; }

        object DataContext { get; set; }
        
        void Add(object target, TargetDescription description);
    }
}