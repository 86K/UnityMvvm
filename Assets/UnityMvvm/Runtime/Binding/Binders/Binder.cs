using System;
using System.Collections.Generic;
using System.Linq;

namespace Fusion.Mvvm
{
    public class Binder : IBinder
    {
        private readonly ISourceProxyFactory sourceProxyFactory;
        private readonly ITargetProxyFactory targetProxyFactory;
        
        public Binder(ISourceProxyFactory sourceProxyFactory, ITargetProxyFactory targetProxyFactory)
        {
            this.sourceProxyFactory = sourceProxyFactory;
            this.targetProxyFactory = targetProxyFactory;
        }
        
        public IBinding Bind(IBindingContext bindingContext, object source, object target, TargetDescription targetDescription)
        {
            return new Binding(bindingContext, source, target, targetDescription, sourceProxyFactory, targetProxyFactory);
        }

        public IEnumerable<IBinding> Bind(IBindingContext bindingContext, object source, object target, IEnumerable<TargetDescription> bindingDescriptions)
        {
            if (bindingDescriptions == null)
                return Array.Empty<IBinding>();

            return bindingDescriptions.Select(description => Bind(bindingContext, source, target, description));
        }
    }
}