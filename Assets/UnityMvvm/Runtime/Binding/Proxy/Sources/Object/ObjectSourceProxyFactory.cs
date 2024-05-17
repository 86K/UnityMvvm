using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class ObjectSourceProxyFactory : TypedSourceProxyFactory<ObjectSourceDescription>, INodeProxyFactory, IRegistry<INodeProxyFactory>
    {
        private readonly List<FactoryPriorityPair<INodeProxyFactory>> factories = new List<FactoryPriorityPair<INodeProxyFactory>>();

        protected override bool TryCreateProxy(object source, ObjectSourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            var path = description.Path;
            if (path.Count == 1)
            {
                proxy = Create(source, path.AsPathToken());
                if (proxy != null)
                    return true;
                return false;
            }

            proxy = new ChainedObjectSourceProxy(source, path.AsPathToken(), this);
            return true;
        }

        public ISourceProxy Create(object source, PathToken token)
        {
            foreach (var pair in factories)
            {
                var factory = pair.factory;
                if (factory == null)
                    continue;

                var proxy = factory.Create(source, token);
                if (proxy != null)
                    return proxy;
            }

            return null;
        }

        public void Register(INodeProxyFactory factory, int priority = 100)
        {
            if (factory == null)
                return;

            factories.Add(new FactoryPriorityPair<INodeProxyFactory>(factory, priority));
            factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public void Unregister(INodeProxyFactory factory)
        {
            if (factory == null)
                return;

            factories.RemoveAll(pair => pair.factory == factory);
        }
    }
}