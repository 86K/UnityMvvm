namespace Fusion.Mvvm
{
    internal struct FactoryPriorityPair<T>
    {
        public readonly int priority;
        public readonly T factory;

        public FactoryPriorityPair(T factory, int priority)
        {
            this.factory = factory;
            this.priority = priority;
        }
    }
}