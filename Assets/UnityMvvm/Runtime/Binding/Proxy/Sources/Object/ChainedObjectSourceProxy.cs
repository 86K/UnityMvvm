using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ChainedObjectSourceProxy : NotifiableSourceProxyBase, IObtainable, IModifiable, INotifiable
    {
        private readonly INodeProxyFactory factory;
        private readonly ProxyEntry[] proxies;
        private readonly int count;

        public ChainedObjectSourceProxy(object source, PathToken token, INodeProxyFactory factory) : base(source)
        {
            this.factory = factory;
            count = token.Path.Count;
            proxies = new ProxyEntry[count];
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

        protected ISourceProxy GetProxy()
        {
            ProxyEntry proxyEntry = proxies[count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy;
        }

        protected IObtainable GetObtainable()
        {
            ProxyEntry proxyEntry = proxies[count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy as IObtainable;
        }

        protected IModifiable GetModifiable()
        {
            ProxyEntry proxyEntry = proxies[count - 1];
            if (proxyEntry == null)
                return null;

            return proxyEntry.Proxy as IModifiable;
        }

        public virtual object GetValue()
        {
            IObtainable obtainable = GetObtainable();
            if (obtainable == null)
                return null;
            return obtainable.GetValue();
        }

        public virtual TValue GetValue<TValue>()
        {
            IObtainable obtainable = GetObtainable();
            if (obtainable == null)
                return default(TValue);

            return obtainable.GetValue<TValue>();
        }

        public virtual void SetValue(object value)
        {
            IModifiable modifiable = GetModifiable();
            if (modifiable == null)
                return;

            modifiable.SetValue(value);
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            IModifiable modifiable = GetModifiable();
            if (modifiable == null)
                return;

            modifiable.SetValue<TValue>(value);
        }

        void Bind(object source, PathToken token)
        {
            int index = token.Index;
            ISourceProxy proxy = factory.Create(source, token);
            if (proxy == null)
            {
                var node = token.Current;
                if (node is MemberNode)
                {
                    var memberNode = node as MemberNode;
                    string typeName = source != null ? source.GetType().Name : memberNode.Type.Name;
                    throw new ProxyException("Not found the member named '{0}' in the class '{1}'.", memberNode.Name, typeName);
                }
                throw new ProxyException("Failed to create proxy for \"{0}\".Not found available proxy factory.", token.ToString());
            }

            ProxyEntry entry = new ProxyEntry(proxy, token);
            proxies[index] = entry;

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
                                var proxyEntry = proxies[index];
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

                var child = (proxy as IObtainable).GetValue();
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
            for (int i = proxies.Length - 1; i > index; i--)
            {
                ProxyEntry proxyEntry = proxies[i];
                if (proxyEntry == null)
                    continue;

                var proxy = proxyEntry.Proxy;
                proxyEntry.Proxy = null;
                if (proxy != null)
                    proxy.Dispose();
            }

            ProxyEntry entry = proxies[index];
            var obtainable = entry.Proxy as IObtainable;
            if (obtainable == null)
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
            for (int i = proxies.Length - 1; i >= 0; i--)
            {
                ProxyEntry proxyEntry = proxies[i];
                if (proxyEntry == null)
                    continue;

                proxyEntry.Dispose();
                proxies[i] = null;
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

        public class ProxyEntry : IDisposable
        {
            private ISourceProxy proxy;
            private EventHandler handler;
            public ProxyEntry(ISourceProxy proxy, PathToken token)
            {
                Proxy = proxy;
                Token = token;
            }

            public ISourceProxy Proxy
            {
                get => proxy;
                set
                {
                    if (proxy == value)
                        return;

                    if (handler != null)
                    {
                        var notifiable = proxy as INotifiable;
                        if (notifiable != null)
                            notifiable.ValueChanged -= handler;

                        notifiable = value as INotifiable;
                        if (notifiable != null)
                            notifiable.ValueChanged += handler;
                    }

                    proxy = value;
                }
            }

            public PathToken Token { get; set; }

            public EventHandler Handler
            {
                get => handler;
                set
                {
                    if (handler == value)
                        return;

                    var notifiable = proxy as INotifiable;
                    if (notifiable != null)
                    {
                        if (handler != null)
                            notifiable.ValueChanged -= handler;

                        if (value != null)
                            notifiable.ValueChanged += value;
                    }

                    handler = value;
                }
            }

            #region IDisposable Support
            private bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    Handler = null;
                    if (proxy != null)
                        proxy.Dispose();
                    proxy = null;
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
