

using System;

namespace Fusion.Mvvm
{
    public class LiteralSourceProxy : SourceProxyBase, ISourceProxy, IObtainable
    {
        public LiteralSourceProxy(object source) : base(source)
        {
        }

        public override Type Type => source != null ? source.GetType() : typeof(object);

        public virtual object GetValue()
        {
            return source;
        }

        public virtual TValue GetValue<TValue>()
        {
            return (TValue)Convert.ChangeType(source, typeof(TValue));
        }
    }
}
