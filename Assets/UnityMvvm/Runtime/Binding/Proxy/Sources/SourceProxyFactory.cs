using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class SourceProxyFactory : ISourceProxyFactory, IRegistry<ISourceProxyFactory>
    {
        private readonly List<FactoryPriorityPair<ISourceProxyFactory>> _factories = new List<FactoryPriorityPair<ISourceProxyFactory>>();

        public ISourceProxy CreateProxy(object source, SourceDescription description)
        {
            try
            {
                if (!description.IsStatic && source == null)
                    return new EmptSourceProxy(description);

                if (TryCreateProxy(source, description, out var proxy))
                    return proxy;

                throw new NotSupportedException("Not found available proxy factory.");
            }
            catch (Exception e)
            {
                throw new Exception("An exception occurred while creating a proxy for the \"{description.ToString()}\".\n{e}");
            }
        }

        private bool TryCreateProxy(object source, SourceDescription description, out ISourceProxy proxy)
        {
            proxy = null;
            foreach (var pair in _factories)
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
                        $"An exception occurred when using the \"{factory.GetType().Name}\" factory to create a proxy for the \"{description}\";exception:{e}");
                }
            }

            proxy = null;
            return false;
        }

        public void Register(ISourceProxyFactory factory, int priority = 100)
        {
            if (factory == null)
                return;

            _factories.Add(new FactoryPriorityPair<ISourceProxyFactory>(factory, priority));
            _factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public void Unregister(ISourceProxyFactory factory)
        {
            if (factory == null)
                return;

            _factories.RemoveAll(pair => pair.factory == factory);
        }
    }
}
