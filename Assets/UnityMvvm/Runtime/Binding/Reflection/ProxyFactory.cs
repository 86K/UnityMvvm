using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class ProxyFactory
    {
        public static readonly ProxyFactory Default = new ProxyFactory();
        private readonly ConcurrentDictionary<Type, ProxyType> _types = new ConcurrentDictionary<Type, ProxyType>();

        //For compatibility with the "Configurable Enter Play Mode" feature
#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void OnInitialize()
        {
            Default._types.Clear();
        }
#endif

        internal ProxyType GetType(Type type, bool create = true)
        {
            if (_types.TryGetValue(type, out var ret) && ret != null)
                return ret;

            return create ? _types.GetOrAdd(type, (t) => new ProxyType(t, this)) : null;
        }

        public IProxyType Get(Type type)
        {
            return GetType(type);
        }

        public void Register(IProxyMemberInfo proxyMemberInfo)
        {
            if (proxyMemberInfo == null)
                return;

            ProxyType proxyType = GetType(proxyMemberInfo.DeclaringType);
            proxyType.Register(proxyMemberInfo);
        }

        public void Unregister(IProxyMemberInfo proxyMemberInfo)
        {
            if (proxyMemberInfo == null)
                return;

            ProxyType proxyType = GetType(proxyMemberInfo.DeclaringType);
            proxyType.Unregister(proxyMemberInfo);
        }
    }
}