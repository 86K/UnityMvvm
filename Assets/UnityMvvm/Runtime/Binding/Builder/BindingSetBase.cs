using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class BindingSetBase : IBindingBuilder
    {
        protected readonly IBindingContext _bindingContext;
        protected readonly List<IBindingBuilder> _bindingBuilders = new List<IBindingBuilder>();

        protected BindingSetBase(IBindingContext bindingContext)
        {
            _bindingContext = bindingContext;
        }

        public virtual void Build()
        {
            foreach (var builder in _bindingBuilders)
            {
                try
                {
                    builder.Build();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{e}");
                }
            }

            _bindingBuilders.Clear();
        }
    }
}