/*
 * MIT License
 *
 * Copyright (c) 2018 Clark Yang
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in 
 * the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
 * of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Loxodon.Framework.Binding.Contexts;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
#endif
namespace Loxodon.Framework.Binding.Builder
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

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<TChildTarget, TSource> Bind<TChildTarget>(string targetName = null) where TChildTarget : VisualElement
//        {
//            UIDocument document = (this.target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            TChildTarget target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<TChildTarget>() : rootVisualElement.Q<TChildTarget>(targetName);
//            var builder = new BindingBuilder<TChildTarget, TSource>(context, target);
//            this.builders.Add(builder);
//            return builder;
//        }
//#endif
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

//#if UNITY_2019_1_OR_NEWER
//        public virtual BindingBuilder<TChildTarget> Bind<TChildTarget>(string targetName = null) where TChildTarget : VisualElement
//        {
//            UIDocument document = (this.target as UnityEngine.Behaviour).GetComponent<UIDocument>();
//            if (document == null)
//                throw new Exception("The UIDocument not found, this is not a UIToolkit view.");

//            VisualElement rootVisualElement = document.rootVisualElement;
//            TChildTarget target = string.IsNullOrEmpty(targetName) ? rootVisualElement.Q<TChildTarget>() : rootVisualElement.Q<TChildTarget>(targetName);
//            var builder = new BindingBuilder<TChildTarget>(context, target);
//            this.builders.Add(builder);
//            return builder;
//        }
// #endif
    }

    public class BindingSet : BindingSetBase
    {
        private readonly object target;
        public BindingSet(IBindingContext context, object target) : base(context)
        {
            this.target = target;
        }

        public virtual BindingBuilder Bind()
        {
            var builder = new BindingBuilder(context, target);
            builders.Add(builder);
            return builder;
        }

        public virtual BindingBuilder Bind(object target)
        {
            var builder = new BindingBuilder(context, target);
            builders.Add(builder);
            return builder;
        }
    }
}
