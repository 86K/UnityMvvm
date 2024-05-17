using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ChainedObjectSourceProxy : NotifiableSourceProxyBase, IObtainable, IModifiable
    {
        private readonly INodeProxyFactory _factory;
        private readonly ProxyEntry[] _proxies;
        private readonly int _count;

        public ChainedObjectSourceProxy(object source, PathToken token, INodeProxyFactory factory) : base(source)
        {
            _factory = factory;
            _count = token.Path.Count;
            _proxies = new ProxyEntry[_count];
            Bind(source, token);
        }

        public override Type Type
        {
            get
            {
                var proxy = GetProxy();
                if (proxy == null)
                    return typeof(object);

                return proxy.Type;
            }
        }

        public override TypeCode TypeCode
        {
            get
            {
                var proxy = GetProxy();
                if (proxy == null)
                    return TypeCode.Object;

                return proxy.TypeCode;
            }
        }

        private ISourceProxy GetProxy()
        {
            ProxyEntry proxyEntry = _proxies[_count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy;
        }

        private IObtainable GetObtainable()
        {
            ProxyEntry proxyEntry = _proxies[_count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy as IObtainable;
        }

        private IModifiable GetModifiable()
        {
            ProxyEntry proxyEntry = _proxies[_count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy as IModifiable;
        }

        public object GetValue()
        {
            IObtainable obtainable = GetObtainable();
            if (obtainable == null)
                return null;
            return obtainable.GetValue();
        }

        public TValue GetValue<TValue>()
        {
            IObtainable obtainable = GetObtainable();
            if (obtainable == null)
                return default;

            return obtainable.GetValue<TValue>();
        }

        public void SetValue(object value)
        {
            IModifiable modifiable = GetModifiable();
            if (modifiable == null)
                return;

            modifiable.SetValue(value);
        }

        public void SetValue<TValue>(TValue value)
        {
            IModifiable modifiable = GetModifiable();
            if (modifiable == null)
                return;

            modifiable.SetValue<TValue>(value);
        }

        void Bind(object source, PathToken token)
        {
            int index = token.Index;
            ISourceProxy proxy = _factory.Create(source, token);
            if (proxy == null)
            {
                var node = token.Current;
                if (node is MemberNode memberNode)
                {
                    string typeName = source != null ? source.GetType().Name : memberNode.Type.Name;
                    throw new Exception($"Not found the member named '{memberNode.Name}' in the class '{typeName}'.");
                }

                throw new Exception($"Failed to create proxy for \"{token.ToString()}\".Not found available proxy factory.");
            }

            ProxyEntry entry = new ProxyEntry(proxy, token);
            _proxies[index] = entry;

            if (token.HasNext())
            {
                if (proxy is INotifiable)
                {
                    entry.Handler = (sender, args) =>
                    {
                        lock (_lock)
                        {
                            try
                            {
                                var proxyEntry = _proxies[index];
                                if (proxyEntry == null || sender != proxyEntry.Proxy)
                                    return;

                                Rebind(index);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"{e}");
                            }
                        }
                    };
                }

                var child = (proxy as IObtainable)?.GetValue();
                if (child != null)
                    Bind(child, token.NextToken());
                else
                    RaiseValueChanged();
            }
            else
            {
                if (proxy is INotifiable)
                    entry.Handler = (sender, args) => { RaiseValueChanged(); };
                RaiseValueChanged();
            }
        }

        void Rebind(int index)
        {
            for (int i = _proxies.Length - 1; i > index; i--)
            {
                ProxyEntry proxyEntry = _proxies[i];
                if (proxyEntry == null)
                    continue;

                var proxy = proxyEntry.Proxy;
                proxyEntry.Proxy = null;
                if (proxy != null)
                    proxy.Dispose();
            }

            ProxyEntry entry = _proxies[index];
            if (!(entry.Proxy is IObtainable obtainable))
            {
                RaiseValueChanged();
                return;
            }

            var source = obtainable.GetValue();
            if (source == null)
            {
                RaiseValueChanged();
                return;
            }

            Bind(source, entry.Token.NextToken());
        }

        void Unbind()
        {
            for (int i = _proxies.Length - 1; i >= 0; i--)
            {
                ProxyEntry proxyEntry = _proxies[i];
                if (proxyEntry == null)
                    continue;

                proxyEntry.Dispose();
                _proxies[i] = null;
            }
        }

        #region IDisposable Support

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unbind();
                disposedValue = true;
                base.Dispose(disposing);
            }
        }

        #endregion

        private sealed class ProxyEntry : IDisposable
        {
            private ISourceProxy _proxy;
            private EventHandler _handler;

            public ProxyEntry(ISourceProxy proxy, PathToken token)
            {
                Proxy = proxy;
                Token = token;
            }

            public ISourceProxy Proxy
            {
                get => _proxy;
                set
                {
                    if (_proxy == value)
                        return;

                    if (_handler != null)
                    {
                        if (_proxy is INotifiable notifiable)
                            notifiable.ValueChanged -= _handler;

                        notifiable = value as INotifiable;
                        if (notifiable != null)
                            notifiable.ValueChanged += _handler;
                    }

                    _proxy = value;
                }
            }

            public PathToken Token { get; }

            public EventHandler Handler
            {
                get => _handler;
                set
                {
                    if (_handler == value)
                        return;

                    if (_proxy is INotifiable notifiable)
                    {
                        if (_handler != null)
                            notifiable.ValueChanged -= _handler;

                        if (value != null)
                            notifiable.ValueChanged += value;
                    }

                    _handler = value;
                }
            }

            #region IDisposable Support

            private bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    Handler = null;
                    if (_proxy != null)
                        _proxy.Dispose();
                    _proxy = null;
                    disposedValue = true;
                }
            }

            ~ProxyEntry()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}