using UnityEngine;
using System.Collections.Generic;
using System;

namespace Fusion.Mvvm
{
    public static class BehaviourBindingExtension
    {
        private static IBinder binder;

        private static IBinder Binder
        {
            get
            {
                if (binder == null)
                    binder = Context.GetApplicationContext().GetService<IBinder>();

                if (binder == null)
                    throw new Exception("Data binding service is not initialized, please create a BindingServiceBundle service before using it.");

                return binder;
            }
        }

        /// <summary>
        /// 绑定上下文。（调用的Behaviour和注册的IBinder）
        /// 
        /// 在UI上附加一个BindingContextLifecycle，其持有一个BindingContext。
        /// </summary>
        /// <param name="behaviour"></param>
        /// <returns></returns>
        public static IBindingContext BindingContext(this Behaviour behaviour)
        {
            if (behaviour == null || behaviour.gameObject == null)
                return null;

            BindingContextLifecycle bindingContextLifecycle = behaviour.GetComponent<BindingContextLifecycle>();
            if (bindingContextLifecycle == null)
                bindingContextLifecycle = behaviour.gameObject.AddComponent<BindingContextLifecycle>();

            IBindingContext bindingContext = bindingContextLifecycle.BindingContext;
            if (bindingContext == null)
            {
                bindingContext = new BindingContext(behaviour, Binder);
                bindingContextLifecycle.BindingContext = bindingContext;
            }
            return bindingContext;
        }

        public static BindingSet<TBehaviour, TSource> CreateBindingSet<TBehaviour, TSource>(this TBehaviour behaviour) where TBehaviour : Behaviour
        {
            IBindingContext context = behaviour.BindingContext();
            return new BindingSet<TBehaviour, TSource>(context, behaviour);
        }

        public static BindingSet<TBehaviour, TSource> CreateBindingSet<TBehaviour, TSource>(this TBehaviour behaviour, TSource dataContext) where TBehaviour : Behaviour
        {
            IBindingContext context = behaviour.BindingContext();
            context.DataContext = dataContext;
            return new BindingSet<TBehaviour, TSource>(context, behaviour);
        }
        
        public static void SetDataContext(this Behaviour behaviour, object dataContext)
        {
            behaviour.BindingContext().DataContext = dataContext;
        }

        public static object GetDataContext(this Behaviour behaviour)
        {
            return behaviour.BindingContext().DataContext;
        }
    }
}
