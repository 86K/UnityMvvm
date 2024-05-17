using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class TargetProxyFactory : ITargetProxyFactory, IRegistry<ITargetProxyFactory>
    {
        private readonly List<FactoryPriorityPair<ITargetProxyFactory>> _factories = new List<FactoryPriorityPair<ITargetProxyFactory>>();

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
                throw new Exception(
                    $"Unable to bind the \"{description}\".An exception occurred while creating a proxy for the \"{description.TargetName}\" property of class \"{target.GetType().Name}\".\n{e}");
            }
        }

        private bool TryCreateProxy(object target, TargetDescription description, out ITargetProxy proxy)
        {
            proxy = null;
            foreach (var pair in _factories)
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

            _factories.Add(new FactoryPriorityPair<ITargetProxyFactory>(factory, priority));
            _factories.Sort((x, y) => y.priority.CompareTo(x.priority));
        }

        public void Unregister(ITargetProxyFactory factory)
        {
            if (factory == null)
                return;

            _factories.RemoveAll(pair => pair.factory == factory);
        }
    }
}