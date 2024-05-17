using System;
using System.Diagnostics;

namespace Fusion.Mvvm
{
    public class EmptSourceProxy : SourceProxyBase, IObtainable, IModifiable
    {
        private readonly SourceDescription description;

        public EmptSourceProxy(SourceDescription description) : base(null)
        {
            this.description = description;
        }

        public override Type Type => typeof(object);

        public object GetValue()
        {
            DebugWarning();
            return null;
        }

        public TValue GetValue<TValue>()
        {
            DebugWarning();
            return default;
        }

        public void SetValue(object value)
        {
            DebugWarning();
        }

        public void SetValue<TValue>(TValue value)
        {
            DebugWarning();
        }

        [Conditional("DEBUG")]
        private void DebugWarning()
        {
            UnityEngine.Debug.LogWarning(
                $"this is an empty source proxy,If you see this, then the DataContext is null.The SourceDescription is \"{description}\"");
        }
    }
}