using System;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class ExpressionSourceProxy : NotifiableSourceProxyBase, IExpressionSourceProxy
    {
        private bool _disposed;
        private readonly Func<object[], object> _func;
        private readonly List<ISourceProxy> _inners;
        private readonly object[] _args;

        public ExpressionSourceProxy(object source, Func<object[], object> func, Type type, List<ISourceProxy> inners) : base(source)
        {
            Type = type;
            _func = func;
            _inners = inners;

            _args = source != null ? new object[] { source } : null;

            if (_inners == null || _inners.Count <= 0)
                return;

            foreach (ISourceProxy proxy in _inners)
            {
                if (proxy is INotifiable notifiable)
                    notifiable.ValueChanged += OnValueChanged;
            }
        }

        public override Type Type { get; }

        public object GetValue()
        {
            return _func(_args);
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_inners != null && _inners.Count > 0)
                    {
                        foreach (ISourceProxy proxy in _inners)
                        {
                            if (proxy is INotifiable notifiable)
                                notifiable.ValueChanged -= OnValueChanged;
                            proxy.Dispose();
                        }

                        _inners.Clear();
                    }
                }

                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }

    public class ExpressionSourceProxy<T, TResult> : NotifiableSourceProxyBase, IExpressionSourceProxy
    {
        private bool _disposed;
        private readonly Func<T, TResult> _func;
        private readonly List<ISourceProxy> _inners;

        public ExpressionSourceProxy(T source, Func<T, TResult> func, List<ISourceProxy> inners) : base(source)
        {
            _func = func;
            _inners = inners;

            if (_inners == null || _inners.Count <= 0)
                return;

            foreach (ISourceProxy proxy in _inners)
            {
                if (proxy is INotifiable notifiable)
                    notifiable.ValueChanged += OnValueChanged;
            }
        }

        public override Type Type => typeof(TResult);

        public object GetValue()
        {
            return _func((T)Source);
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_inners != null && _inners.Count > 0)
                    {
                        foreach (ISourceProxy proxy in _inners)
                        {
                            if (proxy is INotifiable notifiable)
                                notifiable.ValueChanged -= OnValueChanged;

                            proxy.Dispose();
                        }

                        _inners.Clear();
                    }
                }

                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }

    public class ExpressionSourceProxy<TResult> : NotifiableSourceProxyBase, IExpressionSourceProxy
    {
        private bool _disposed;
        private readonly Func<TResult> _func;
        private readonly List<ISourceProxy> _inners;

        public ExpressionSourceProxy(Func<TResult> func, List<ISourceProxy> inners) : base(null)
        {
            _func = func;
            _inners = inners;

            if (_inners == null || _inners.Count <= 0)
                return;

            foreach (ISourceProxy proxy in _inners)
            {
                if (proxy is INotifiable notifiable)
                    notifiable.ValueChanged += OnValueChanged;
            }
        }

        public override Type Type => typeof(TResult);

        public object GetValue()
        {
            return _func();
        }

        public TValue GetValue<TValue>()
        {
            return (TValue)GetValue();
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_inners != null && _inners.Count > 0)
                    {
                        foreach (ISourceProxy proxy in _inners)
                        {
                            if (proxy is INotifiable notifiable)
                                notifiable.ValueChanged -= OnValueChanged;

                            proxy.Dispose();
                        }

                        _inners.Clear();
                    }
                }

                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}