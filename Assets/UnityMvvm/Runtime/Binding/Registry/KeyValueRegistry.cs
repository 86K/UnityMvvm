

using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Mvvm
{
    
    public class KeyValueRegistry<K,V> : IKeyValueRegistry<K,V>
    {
        private readonly Dictionary<K, V> lookups = new Dictionary<K, V>();

        public virtual V Find(K key)
        {
            V toReturn;
            lookups.TryGetValue(key, out toReturn);
            return toReturn;
        }

        public virtual V Find(K key,V defaultValue)
        {
            V toReturn;
            if (lookups.TryGetValue(key, out toReturn))
                return toReturn;

            return defaultValue;
        }

        public virtual void Register(K key, V value)
        {
            if (lookups.ContainsKey(key))
            {
                Debug.LogWarning($"The Key({key}) already exists");
            }
            lookups[key] = value;
        }
    }
}
