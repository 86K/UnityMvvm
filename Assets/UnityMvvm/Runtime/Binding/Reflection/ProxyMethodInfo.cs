using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ProxyMethodInfo : IProxyMethodInfo
    {
        protected readonly bool _isValueType;
        protected readonly MethodInfo _methodInfo;

        public ProxyMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            _methodInfo = methodInfo;
            _isValueType = methodInfo.DeclaringType.GetTypeInfo().IsValueType;
        }

        public virtual Type DeclaringType => _methodInfo.DeclaringType;

        public virtual string Name => _methodInfo.Name;

        public bool IsStatic => _methodInfo.IsStatic;

        public virtual Type ReturnType => _methodInfo.ReturnType;

        public virtual ParameterInfo[] Parameters => _methodInfo.GetParameters();

        public virtual ParameterInfo ReturnParameter => _methodInfo.ReturnParameter;

        public virtual object Invoke(object target, params object[] args)
        {
            return _methodInfo.Invoke(target, args);
        }
    }

    public class ProxyFuncInfo<T, TResult> : ProxyMethodInfo
    {
        private readonly Func<T, TResult> function;

        public ProxyFuncInfo(MethodInfo info, Func<T, TResult> function) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            this.function = function ?? MakeFunc();
        }

        public override Type DeclaringType => typeof(T);

        private Func<T, TResult> MakeFunc()
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Func<T, TResult>)_methodInfo.CreateDelegate(typeof(Func<T, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private TResult Invoke(T target)
        {
            if (function != null)
                return function(target);

            return (TResult)_methodInfo.Invoke(target, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target);
        }
    }

    public class ProxyFuncInfo<T, P1, TResult> : ProxyMethodInfo
    {
        private readonly Func<T, P1, TResult> function;

        public ProxyFuncInfo(MethodInfo info, Func<T, P1, TResult> function) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function ?? MakeFunc(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Func<T, P1, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Func<T, P1, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private TResult Invoke(T target, P1 p1)
        {
            if (function != null)
                return function(target, p1);

            return (TResult)_methodInfo.Invoke(target, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0]);
        }
    }

    public class ProxyFuncInfo<T, P1, P2, TResult> : ProxyMethodInfo
    {
        private readonly Func<T, P1, P2, TResult> function;

        public ProxyFuncInfo(MethodInfo info, Func<T, P1, P2, TResult> function) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function ?? MakeFunc(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Func<T, P1, P2, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Func<T, P1, P2, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, P2, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private TResult Invoke(T target, P1 p1, P2 p2)
        {
            if (function != null)
                return function(target, p1, p2);

            return (TResult)_methodInfo.Invoke(target, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0], (P2)args[1]);
        }
    }

    public class ProxyFuncInfo<T, P1, P2, P3, TResult> : ProxyMethodInfo
    {
        private readonly Func<T, P1, P2, P3, TResult> function;

        public ProxyFuncInfo(MethodInfo info, Func<T, P1, P2, P3, TResult> function) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) ||
                !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.function = function ?? MakeFunc(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Func<T, P1, P2, P3, TResult> MakeFunc(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Func<T, P1, P2, P3, TResult>)methodInfo.CreateDelegate(typeof(Func<T, P1, P2, P3, TResult>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private TResult Invoke(T target, P1 p1, P2 p2, P3 p3)
        {
            if (function != null)
                return function(target, p1, p2, p3);

            return (TResult)_methodInfo.Invoke(target, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((T)target, (P1)args[0], (P2)args[1], (P3)args[2]);
        }
    }

    public class ProxyActionInfo<T> : ProxyMethodInfo
    {
        private readonly Action<T> action;
        public override Type DeclaringType => typeof(T);

        public ProxyActionInfo(MethodInfo info, Action<T> action) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(void) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            this.action = action ?? MakeAction(_methodInfo);
        }

        private Action<T> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Action<T>)methodInfo.CreateDelegate(typeof(Action<T>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private void Invoke(T target)
        {
            if (action != null)
            {
                action(target);
                return;
            }

            _methodInfo.Invoke(target, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1> : ProxyMethodInfo
    {
        private readonly Action<T, P1> action;

        public ProxyActionInfo(MethodInfo info, Action<T, P1> action) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(void) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action ?? MakeAction(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Action<T, P1> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Action<T, P1>)methodInfo.CreateDelegate(typeof(Action<T, P1>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private void Invoke(T target, P1 p1)
        {
            if (action != null)
            {
                action(target, p1);
                return;
            }

            _methodInfo.Invoke(target, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0]);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1, P2> : ProxyMethodInfo
    {
        private readonly Action<T, P1, P2> action;

        public ProxyActionInfo(MethodInfo info, Action<T, P1, P2> action) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(void) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            this.action = action ?? MakeAction(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Action<T, P1, P2> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Action<T, P1, P2>)methodInfo.CreateDelegate(typeof(Action<T, P1, P2>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private void Invoke(T target, P1 p1, P2 p2)
        {
            if (action != null)
            {
                action(target, p1, p2);
                return;
            }

            _methodInfo.Invoke(target, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0], (P2)args[1]);
            return null;
        }
    }

    public class ProxyActionInfo<T, P1, P2, P3> : ProxyMethodInfo
    {
        private readonly Action<T, P1, P2, P3> _action;

        public ProxyActionInfo(MethodInfo info, Action<T, P1, P2, P3> action) : base(info)
        {
            if (IsStatic)
                throw new ArgumentException("The method is static!");

            if (!(typeof(void) == _methodInfo.ReturnType) ||
                (_methodInfo.DeclaringType != null && !_methodInfo.DeclaringType.IsAssignableFrom(typeof(T))))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) ||
                !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _action = action ?? MakeAction(_methodInfo);
        }

        public override Type DeclaringType => typeof(T);

        private Action<T, P1, P2, P3> MakeAction(MethodInfo methodInfo)
        {
            try
            {
                if (_isValueType)
                    return null;

                return (Action<T, P1, P2, P3>)methodInfo.CreateDelegate(typeof(Action<T, P1, P2, P3>));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }

            return null;
        }

        private void Invoke(T target, P1 p1, P2 p2, P3 p3)
        {
            if (_action != null)
            {
                _action(target, p1, p2, p3);
                return;
            }

            _methodInfo.Invoke(target, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((T)target, (P1)args[0], (P2)args[1], (P3)args[2]);
            return null;
        }
    }
}