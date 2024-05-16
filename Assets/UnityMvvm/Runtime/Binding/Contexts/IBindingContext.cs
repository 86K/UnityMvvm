

using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public interface IBindingContext : IDisposable
    {
        event EventHandler DataContextChanged;

        object Owner { get; }

        object DataContext { get; set; }

        void Add(IBinding binding,object key=null);

        void Add(IEnumerable<IBinding> bindings,object key = null);

        void Add(object target, TargetDescription description,object key = null);

        void Add(object target, IEnumerable<TargetDescription> descriptions, object key = null);

        void Clear(object key);

        void Clear();
    }
}