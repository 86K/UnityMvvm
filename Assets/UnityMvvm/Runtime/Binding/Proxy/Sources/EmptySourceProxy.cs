

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

        public virtual object GetValue()
        {
            DebugWarning();
            return null;
        }

        public virtual TValue GetValue<TValue>()
        {
            DebugWarning();
            return default(TValue);
        }

        public virtual void SetValue(object value)
        {
            DebugWarning();
        }

        public virtual void SetValue<TValue>(TValue value)
        {
            DebugWarning();
        }

        [Conditional("DEBUG")]
        private void DebugWarning()
        {
            UnityEngine.Debug.LogWarning(string.Format("this is an empty source proxy,If you see this, then the DataContext is null.The SourceDescription is \"{0}\"", description.ToString()));
        }
    }
}
