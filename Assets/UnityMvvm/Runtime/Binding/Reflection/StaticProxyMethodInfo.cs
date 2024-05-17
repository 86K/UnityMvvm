

using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class StaticProxyFuncInfo<T, TResult> : ProxyMethodInfo, IStaticProxyFuncInfo<T, TResult>
    {
        private readonly Func<TResult> function;

        public StaticProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public StaticProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Func<TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes, Func<TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }

        public StaticProxyFuncInfo(MethodInfo info, Func<TResult> function) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = MakeFunc(methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Func<TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                return (Func<TResult>)methodInfo.CreateDelegate(typeof(Func<TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual TResult Invoke()
        {
            if (function != null)
                return function();

            return (TResult)methodInfo.Invoke(null, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke();
        }
    }

    public class StaticProxyFuncInfo<T, P1, TResult> : ProxyMethodInfo, IStaticProxyFuncInfo<T, P1, TResult>
    {
        private readonly Func<P1, TResult> function;
        public StaticProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public StaticProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Func<P1, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes, Func<P1, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }


        public StaticProxyFuncInfo(MethodInfo info, Func<P1, TResult> function) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = MakeFunc(methodInfo);

        }

        public override Type DeclaringType => typeof(T);

        private Func<P1, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                return (Func<P1, TResult>)methodInfo.CreateDelegate(typeof(Func<P1, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual TResult Invoke(P1 p1)
        {
            if (function != null)
                return function(p1);

            return (TResult)methodInfo.Invoke(null, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0]);
        }
    }

    public class StaticProxyFuncInfo<T, P1, P2, TResult> : ProxyMethodInfo, IStaticProxyFuncInfo<T, P1, P2, TResult>
    {
        private readonly Func<P1, P2, TResult> function;

        public StaticProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }

        public StaticProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Func<P1, P2, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }

        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes, Func<P1, P2, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }


        public StaticProxyFuncInfo(MethodInfo info, Func<P1, P2, TResult> function) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = MakeFunc(methodInfo);

        }

        public override Type DeclaringType => typeof(T);

        private Func<P1, P2, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                return (Func<P1, P2, TResult>)methodInfo.CreateDelegate(typeof(Func<P1, P2, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual TResult Invoke(P1 p1, P2 p2)
        {
            if (function != null)
                return function(p1, p2);

            return (TResult)methodInfo.Invoke(null, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0], (P2)args[1]);
        }
    }

    public class StaticProxyFuncInfo<T, P1, P2, P3, TResult> : ProxyMethodInfo, IStaticProxyFuncInfo<T, P1, P2, P3, TResult>
    {
        private readonly Func<P1, P2, P3, TResult> function;

        public StaticProxyFuncInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }
        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }
        public StaticProxyFuncInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyFuncInfo(string methodName, Func<P1, P2, P3, TResult> function) : this(typeof(T).GetMethod(methodName), function)
        {
        }
        public StaticProxyFuncInfo(string methodName, Type[] parameterTypes, Func<P1, P2, P3, TResult> function) : this(typeof(T).GetMethod(methodName, parameterTypes), function)
        {
        }
        public StaticProxyFuncInfo(MethodInfo info, Func<P1, P2, P3, TResult> function) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) || !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function;
            if (this.function == null)
                this.function = MakeFunc(methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Func<P1, P2, P3, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                return (Func<P1, P2, P3, TResult>)methodInfo.CreateDelegate(typeof(Func<P1, P2, P3, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual TResult Invoke(P1 p1, P2 p2, P3 p3)
        {
            if (function != null)
                return function(p1, p2, p3);

            return (TResult)methodInfo.Invoke(null, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0], (P2)args[1], (P3)args[2]);
        }
    }

    public class StaticProxyActionInfo<T> : ProxyMethodInfo, IStaticProxyActionInfo<T>
    {
        private readonly Action action;

        public StaticProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }
        public StaticProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyActionInfo(string methodName, Action action) : this(typeof(T).GetMethod(methodName), action)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes, Action action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public StaticProxyActionInfo(MethodInfo info, Action action) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = MakeAction(methodInfo);

        }

        public override Type DeclaringType => typeof(T);


        private Action MakeAction(MethodInfo methodInfo)
        {
            try
            {
                return (Action)methodInfo.CreateDelegate(typeof(Action));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual void Invoke()
        {
            if (action != null)
            {
                action();
                return;
            }

            methodInfo.Invoke(null, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke();
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1> : ProxyMethodInfo, IStaticProxyActionInfo<T, P1>
    {
        private readonly Action<P1> action;

        public StaticProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }
        public StaticProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyActionInfo(string methodName, Action<P1> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes, Action<P1> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }

        public StaticProxyActionInfo(MethodInfo info, Action<P1> action) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = MakeAction(methodInfo);

        }

        public override Type DeclaringType => typeof(T);

        private Action<P1> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                return (Action<P1>)methodInfo.CreateDelegate(typeof(Action<P1>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual void Invoke(P1 p1)
        {
            if (action != null)
            {
                action(p1);
                return;
            }

            methodInfo.Invoke(null, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0]);
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1, P2> : ProxyMethodInfo, IStaticProxyActionInfo<T, P1, P2>
    {
        private readonly Action<P1, P2> action;
        public StaticProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }
        public StaticProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }

        public StaticProxyActionInfo(string methodName, Action<P1, P2> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes, Action<P1, P2> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }
        public StaticProxyActionInfo(MethodInfo info, Action<P1, P2> action) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = MakeAction(methodInfo);

        }

        public override Type DeclaringType => typeof(T);

        private Action<P1, P2> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                return (Action<P1, P2>)methodInfo.CreateDelegate(typeof(Action<P1, P2>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual void Invoke(P1 p1, P2 p2)
        {
            if (action != null)
            {
                action(p1, p2);
                return;
            }

            methodInfo.Invoke(null, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0], (P2)args[1]);
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1, P2, P3> : ProxyMethodInfo, IStaticProxyActionInfo<T, P1, P2, P3>
    {
        private readonly Action<P1, P2, P3> action;

        public StaticProxyActionInfo(string methodName) : this(typeof(T).GetMethod(methodName), null)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes) : this(typeof(T).GetMethod(methodName, parameterTypes), null)
        {
        }
        public StaticProxyActionInfo(MethodInfo info) : this(info, null)
        {
        }
        public StaticProxyActionInfo(string methodName, Action<P1, P2, P3> action) : this(typeof(T).GetMethod(methodName), action)
        {
        }
        public StaticProxyActionInfo(string methodName, Type[] parameterTypes, Action<P1, P2, P3> action) : this(typeof(T).GetMethod(methodName, parameterTypes), action)
        {
        }
        public StaticProxyActionInfo(MethodInfo info, Action<P1, P2, P3> action) : base(info)
        {
            if (!methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == methodInfo.ReturnType) || !(typeof(T) == methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) || !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action;
            if (this.action == null)
                this.action = MakeAction(methodInfo);
        }

        public override Type DeclaringType => typeof(T);


        private Action<P1, P2, P3> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                return (Action<P1, P2, P3>)methodInfo.CreateDelegate(typeof(Action<P1, P2, P3>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
            return null;
        }

        public virtual void Invoke(P1 p1, P2 p2, P3 p3)
        {
            if (action != null)
            {
                action(p1, p2, p3);
                return;
            }

            methodInfo.Invoke(null, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0], (P2)args[1], (P3)args[2]);
            return null;
        }
    }
}
