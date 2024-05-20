namespace Fusion.Mvvm
{
    public abstract class ServiceBundleBase : IServiceBundle
    {
        private readonly IServiceContainer _container;

        protected ServiceBundleBase(IServiceContainer container)
        {
            _container = container;
        }

        public void Start()
        {
            OnStart(_container);
        }

        protected abstract void OnStart(IServiceContainer container);

        public void Stop()
        {
            OnStop(_container);
        }

        protected abstract void OnStop(IServiceContainer container);
    }
}