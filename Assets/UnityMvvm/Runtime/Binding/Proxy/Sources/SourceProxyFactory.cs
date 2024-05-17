

using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class SourceProxyFactory : ISourceProxyFactory, ISourceProxyFactoryRegistry
    {
        private readonly List<PriorityFactoryPair> factories = new List<PriorityFactoryPair>();

        public ISourceProxy CreateProxy(object source, SourceDescription description)
        {
            try
            {
                if (!description.IsStatic && source == null)
                    return new EmptSourceProxy(description);

                ISourceProxy proxy = null;
                if (TryCreateProxy(source, description, out proxy))
                    return proxy;

                throw new NotSupportedException("Not found available proxy factory.");
            }
            catch (Exception e)
            {
                throw new ProxyException(e, "An exception occurred while creating a proxy for the \"{0}\".", description.ToString());
            }
        }

        protected virtual bool TryCreateProxy(object source, SourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            foreach (PriorityFactoryPair pair in factories)
            {
                var factory = pair.factory;
                if (factory == null)
                    continue;

                try
                {
                    proxy = factory.CreateProxy(source, description);
                    if (proxy != null)
                        return true;
                }
                catch (MissingMemberException e)
                {
                    throw e;
                }
                catch (NullReferenceException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(
                        $"An exception occurred when using the \"{factory.GetType().Name}\" factory to create a proxy for the \"{description.ToString()}\";exception:{e}");
                }
            }

            proxy = null;
            return false;
        }

        public void Register(ISourceProxyFactory factory, int priority = 100)
        {
            if (factory == null)
                return;

            factories.Add(new PriorityFactoryPair(factory, priority));
            factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public void Unregister(ISourceProxyFactory factory)
        {
            if (factory == null)
                return;

            factories.RemoveAll(pair => pair.factory == factory);
        }

        struct PriorityFactoryPair
        {
            public PriorityFactoryPair(ISourceProxyFactory factory, int priority)
            {
                this.factory = factory;
                this.priority = priority;
            }
            public readonly int priority;
            public readonly ISourceProxyFactory factory;
        }
    }
}
