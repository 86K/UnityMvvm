using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public interface IBinder
    {
        IBinding Bind(IBindingContext bindingContext, object source, object target, TargetDescription targetDescription);

        IEnumerable<IBinding> Bind(IBindingContext bindingContext, object source, object target, IEnumerable<TargetDescription> bindingDescriptions);

    }
}