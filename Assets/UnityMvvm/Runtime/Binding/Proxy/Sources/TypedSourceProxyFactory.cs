

namespace Fusion.Mvvm
{
    public abstract class TypedSourceProxyFactory<T> : ISourceProxyFactory where T : SourceDescription
    {
        public virtual bool IsSupported(SourceDescription description)
        {
            if (!(description is T))
                return false;
            return true;
        }

        public ISourceProxy CreateProxy(object source, SourceDescription description)
        {
            if (!IsSupported(description))
                return null;

            if (TryCreateProxy(source, (T)description, out var proxy))
                return proxy;

            return proxy;
        }

        protected abstract bool TryCreateProxy(object source, T description, out ISourceProxy proxy);
    }
}
