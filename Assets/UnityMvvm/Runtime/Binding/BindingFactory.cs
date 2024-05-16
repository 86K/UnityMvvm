namespace Fusion.Mvvm
{

    public class BindingFactory : IBindingFactory
    {
        private ISourceProxyFactory sourceProxyFactory;
        private ITargetProxyFactory targetProxyFactory;

        public ISourceProxyFactory SourceProxyFactory
        {
            get => sourceProxyFactory;
            set => sourceProxyFactory = value;
        }
        public ITargetProxyFactory TargetProxyFactory
        {
            get => targetProxyFactory;
            set => targetProxyFactory = value;
        }

        public BindingFactory(ISourceProxyFactory sourceProxyFactory, ITargetProxyFactory targetProxyFactory)
        {
            this.sourceProxyFactory = sourceProxyFactory;
            this.targetProxyFactory = targetProxyFactory;
        }

        public IBinding Create(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription)
        {
            return new Binding(bindingContext, source, target, bindingDescription, sourceProxyFactory, targetProxyFactory);
        }
    }
}
