

namespace Fusion.Mvvm
{
    public interface IBindingFactory
    {
        IBinding Create(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription);
    }
}
