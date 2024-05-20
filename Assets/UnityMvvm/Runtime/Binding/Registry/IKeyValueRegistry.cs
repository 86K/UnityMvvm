namespace Fusion.Mvvm
{
    public interface IKeyValueRegistry<K,V>
    {
        V Find(K key);

        void Register(K key, V value);
    }
}
