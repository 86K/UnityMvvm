namespace Fusion.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, IViewModel
    {
        public virtual void Dispose()
        {
        }
    }
}