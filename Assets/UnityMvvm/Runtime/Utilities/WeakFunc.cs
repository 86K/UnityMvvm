

using System;

namespace Fusion.Mvvm
{
    public class WeakFunc<TResult> : WeakBase<Func<TResult>>
    {
        public WeakFunc(Func<TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        public WeakFunc(object target, Func<TResult> func) : base(target, func)
        {
        }

        public virtual TResult Execute()
        {
            if (!IsAlive)
                return default;

            if (del != null)
                return del();

            var target = targetReference.Target;
            return (TResult)targetMethod.Invoke(target, null);
        }

        public override object Execute(params object[] parameters)
        {
            return Execute();
        }
    }

    public class WeakFunc<T, TResult> : WeakBase<Func<T, TResult>>
    {
        public WeakFunc(Func<T, TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        public WeakFunc(object target, Func<T, TResult> func) : base(target, func)
        {
        }

        public virtual TResult Execute(T parameter)
        {
            if (!IsAlive)
                return default;

            if (del != null)
                return del(parameter);

            var target = targetReference.Target;
            return (TResult)targetMethod.Invoke(target, new object[] { parameter });
        }

        public override object Execute(params object[] parameters)
        {
            return Execute((T)parameters[0]);
        }
    }

    public class WeakFunc<T1, T2, TResult> : WeakBase<Func<T1, T2, TResult>>
    {
        public WeakFunc(Func<T1, T2, TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        public WeakFunc(object target, Func<T1, T2, TResult> func) : base(target, func)
        {
        }

        public virtual TResult Execute(T1 t1, T2 t2)
        {
            if (!IsAlive)
                return default;

            if (del != null)
                return del(t1, t2);

            var target = targetReference.Target;
            return (TResult)targetMethod.Invoke(target, new object[] { t1, t2 });
        }

        public override object Execute(params object[] parameters)
        {
            return Execute((T1)parameters[0], (T2)parameters[1]);
        }
    }

    public class WeakFunc<T1, T2, T3, TResult> : WeakBase<Func<T1, T2, T3, TResult>>
    {
        public WeakFunc(Func<T1, T2, T3, TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        public WeakFunc(object target, Func<T1, T2, T3, TResult> func) : base(target, func)
        {
        }

        public virtual TResult Execute(T1 t1, T2 t2, T3 t3)
        {
            if (!IsAlive)
                return default;

            if (del != null)
                return del(t1, t2, t3);

            var target = targetReference.Target;
            return (TResult)targetMethod.Invoke(target, new object[] { t1, t2, t3 });
        }

        public override object Execute(params object[] parameters)
        {
            return Execute((T1)parameters[0], (T2)parameters[1], (T3)parameters[2]);
        }
    }

    public class WeakFunc<T1, T2, T3, T4, TResult> : WeakBase<Func<T1, T2, T3, T4, TResult>>
    {
        public WeakFunc(Func<T1, T2, T3, T4, TResult> func) : this(func == null ? null : func.Target, func)
        {
        }

        public WeakFunc(object target, Func<T1, T2, T3, T4, TResult> func) : base(target, func)
        {
        }

        public virtual TResult Execute(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (!IsAlive)
                return default;

            if (del != null)
                return del(t1, t2, t3, t4);

            var target = targetReference.Target;
            return (TResult)targetMethod.Invoke(target, new object[] { t1, t2, t3, t4 });
        }

        public override object Execute(params object[] parameters)
        {
            return Execute((T1)parameters[0], (T2)parameters[1], (T3)parameters[2], (T4)parameters[3]);
        }
    }
}