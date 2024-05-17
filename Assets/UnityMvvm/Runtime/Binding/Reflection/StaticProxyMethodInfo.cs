using System;
using System.Reflection;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class StaticProxyFuncInfo<T, TResult> : ProxyMethodInfo
    {
        private readonly Func<TResult> _function;

        public StaticProxyFuncInfo(MethodInfo info, Func<TResult> function) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            _function = function ?? MakeFunc(_methodInfo);
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

        private TResult Invoke()
        {
            if (_function != null)
                return _function();

            return (TResult)_methodInfo.Invoke(null, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke();
        }
    }

    public class StaticProxyFuncInfo<T, P1, TResult> : ProxyMethodInfo
    {
        private readonly Func<P1, TResult> _function;

        public StaticProxyFuncInfo(MethodInfo info, Func<P1, TResult> function) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _function = function ?? MakeFunc(_methodInfo);
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

        private TResult Invoke(P1 p1)
        {
            if (_function != null)
                return _function(p1);

            return (TResult)_methodInfo.Invoke(null, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0]);
        }
    }

    public class StaticProxyFuncInfo<T, P1, P2, TResult> : ProxyMethodInfo
    {
        private readonly Func<P1, P2, TResult> _function;

        public StaticProxyFuncInfo(MethodInfo info, Func<P1, P2, TResult> function) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _function = function ?? MakeFunc(_methodInfo);
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

        private TResult Invoke(P1 p1, P2 p2)
        {
            if (_function != null)
                return _function(p1, p2);

            return (TResult)_methodInfo.Invoke(null, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0], (P2)args[1]);
        }
    }

    public class StaticProxyFuncInfo<T, P1, P2, P3, TResult> : ProxyMethodInfo
    {
        private readonly Func<P1, P2, P3, TResult> _function;

        public StaticProxyFuncInfo(MethodInfo info, Func<P1, P2, P3, TResult> function) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(TResult) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) ||
                !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _function = function ?? MakeFunc(_methodInfo);
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

        private TResult Invoke(P1 p1, P2 p2, P3 p3)
        {
            if (_function != null)
                return _function(p1, p2, p3);

            return (TResult)_methodInfo.Invoke(null, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            return Invoke((P1)args[0], (P2)args[1], (P3)args[2]);
        }
    }

    public class StaticProxyActionInfo<T> : ProxyMethodInfo
    {
        private readonly Action _action;

        public StaticProxyActionInfo(MethodInfo info, Action action) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            _action = action ?? MakeAction(_methodInfo);
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

        private void Invoke()
        {
            if (_action != null)
            {
                _action();
                return;
            }

            _methodInfo.Invoke(null, null);
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke();
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1> : ProxyMethodInfo
    {
        private readonly Action<P1> _action;

        public StaticProxyActionInfo(MethodInfo info, Action<P1> action) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 1 || !(typeof(P1) == parameters[0].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _action = action ?? MakeAction(_methodInfo);
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

        private void Invoke(P1 p1)
        {
            if (_action != null)
            {
                _action(p1);
                return;
            }

            _methodInfo.Invoke(null, new object[] { p1 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0]);
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1, P2> : ProxyMethodInfo
    {
        private readonly Action<P1, P2> _action;

        public StaticProxyActionInfo(MethodInfo info, Action<P1, P2> action) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 2 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _action = action ?? MakeAction(_methodInfo);
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

        private void Invoke(P1 p1, P2 p2)
        {
            if (_action != null)
            {
                _action(p1, p2);
                return;
            }

            _methodInfo.Invoke(null, new object[] { p1, p2 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0], (P2)args[1]);
            return null;
        }
    }

    public class StaticProxyActionInfo<T, P1, P2, P3> : ProxyMethodInfo
    {
        private readonly Action<P1, P2, P3> _action;

        public StaticProxyActionInfo(MethodInfo info, Action<P1, P2, P3> action) : base(info)
        {
            if (!_methodInfo.IsStatic)
                throw new ArgumentException("The method isn't static!");

            if (!(typeof(void) == _methodInfo.ReturnType) || !(typeof(T) == _methodInfo.DeclaringType))
                throw new ArgumentException("The method types do not match!");

            ParameterInfo[] parameters = _methodInfo.GetParameters();
            if (parameters.Length != 3 || !(typeof(P1) == parameters[0].ParameterType) || !(typeof(P2) == parameters[1].ParameterType) ||
                !(typeof(P3) == parameters[2].ParameterType))
                throw new ArgumentException("The method types do not match!");

            _action = action ?? MakeAction(_methodInfo);
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

        private void Invoke(P1 p1, P2 p2, P3 p3)
        {
            if (_action != null)
            {
                _action(p1, p2, p3);
                return;
            }

            _methodInfo.Invoke(null, new object[] { p1, p2, p3 });
        }

        public override object Invoke(object target, params object[] args)
        {
            Invoke((P1)args[0], (P2)args[1], (P3)args[2]);
            return null;
        }
    }
}