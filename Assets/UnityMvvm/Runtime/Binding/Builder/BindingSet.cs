using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class BindingSet<TTarget, TSource> : BindingSetBase where TTarget : class
    {
        private readonly TTarget _target;

        public BindingSet(IBindingContext bindingContext, TTarget target) : base(bindingContext)
        {
            _target = target;
        }

        public BindingBuilder<TTarget, TSource> Bind()
        {
            var builder = new BindingBuilder<TTarget, TSource>(_bindingContext, _target);
            _bindingBuilders.Add(builder);
            return builder;
        }

        public BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget, TSource>(_bindingContext, target);
            _bindingBuilders.Add(builder);
            return builder;
        }
    }
}