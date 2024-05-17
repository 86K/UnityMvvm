using System;

namespace Fusion.Mvvm
{
    public class ConstantCommandParameter<T> : ICommandParameter<T>
    {
        private readonly T _parameter;

        public ConstantCommandParameter(T parameter)
        {
            _parameter = parameter;
        }

        public T GetValue()
        {
            return _parameter;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        object ICommandParameter.GetValue()
        {
            return GetValue();
        }
    }
}