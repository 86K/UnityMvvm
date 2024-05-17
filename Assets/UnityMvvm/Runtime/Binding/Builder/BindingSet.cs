using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class BindingSetBase : IBindingBuilder
    {
        protected IBindingContext context;
        protected readonly List<IBindingBuilder> builders = new List<IBindingBuilder>();

        public BindingSetBase(IBindingContext context)
        {
            this.context = context;
        }

        public virtual void Build()
        {
            foreach (var builder in builders)
            {
                try
                {
                    builder.Build();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(string.Format("{0}", e));
                }
            }
            builders.Clear();
        }
    }

    public class BindingSet<TTarget, TSource> : BindingSetBase where TTarget : class
    {
        private readonly TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget, TSource> Bind()
        {
            var builder = new BindingBuilder<TTarget, TSource>(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget, TSource>(context, target);
            builders.Add(builder);
            return builder;
        }
    }

    public class BindingSet<TTarget> : BindingSetBase where TTarget : class
    {
        private readonly TTarget target;
        public BindingSet(IBindingContext context, TTarget target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder<TTarget> Bind()
        {
            var builder = new BindingBuilder<TTarget>(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder<TChildTarget> Bind<TChildTarget>(TChildTarget target) where TChildTarget : class
        {
            var builder = new BindingBuilder<TChildTarget>(context, target);
            builders.Add(builder);
            return builder;
        }
    }
}
