

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class TargetProxyFactory : ITargetProxyFactory, ITargetProxyFactoryRegister
    {
        private readonly List<PriorityFactoryPair> factories = new List<PriorityFactoryPair>();

        public ITargetProxy CreateProxy(object target, TargetDescription description)
        {
            try
            {
                if (TryCreateProxy(target, description, out var proxy))
                    return proxy;

                throw new NotSupportedException("Not found available proxy factory.");
            }
            catch (Exception e)
            {
                throw new ProxyException(e, "Unable to bind the \"{0}\".An exception occurred while creating a proxy for the \"{1}\" property of class \"{2}\".", description.ToString(), description.TargetName, target.GetType().Name);
            }
        }

        protected virtual bool TryCreateProxy(object target, TargetDescription description, out ITargetProxy proxy)
        {
            proxy = null;
            foreach (PriorityFactoryPair pair in factories)
            {
                var factory = pair.factory;
                if (factory == null)
                    continue;

                try
                {
                    proxy = factory.CreateProxy(target, description);
                    if (proxy != null)
                        return true;

                }
                catch (MissingMemberException e)
                {
                    if (!TargetNameUtil.IsCollection(description.TargetName))
                        throw e;
                }
                catch (NullReferenceException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"An exception occurred when using the \"{factory.GetType().Name}\" factory to create a proxy for the \"{description.TargetName}\" property of class \"{target.GetType().Name}\";exception:{e}");
                }
            }

            return false;
        }

        public void Register(ITargetProxyFactory factory, int priority = 100)
        {
            if (factory == null)
                return;

            factories.Add(new PriorityFactoryPair(factory, priority));
            factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public void Unregister(ITargetProxyFactory factory)
        {
            if (factory == null)
                return;

            factories.RemoveAll(pair => pair.factory == factory);
        }

        struct PriorityFactoryPair
        {
            public PriorityFactoryPair(ITargetProxyFactory factory, int priority)
            {
                this.factory = factory;
                this.priority = priority;
            }

            public readonly int priority;
            public readonly ITargetProxyFactory factory;
        }
    }
}
