using System;

namespace Fusion.Mvvm
{
    public class EventNodeProxy : SourceProxyBase, ISourceProxy, IModifiable
    {
        protected readonly IProxyEventInfo eventInfo;
        private bool disposed = false;
        private readonly bool isStatic = false;
        protected Delegate handler;

        public EventNodeProxy(IProxyEventInfo eventInfo) : this(null, eventInfo)
        {
        }

        public EventNodeProxy(object source, IProxyEventInfo eventInfo) : base(source)
        {
            this.eventInfo = eventInfo;
            isStatic = this.eventInfo.IsStatic;
        }

        public override Type Type => eventInfo.HandlerType;

        public virtual void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        public virtual void SetValue(object value)
        {
            if (value != null && !value.GetType().Equals(Type))
                throw new ArgumentException("Binding delegate to event failed, mismatched delegate type", "value");

            Unbind(Source, handler);
            handler = (Delegate)value;
            Bind(Source, handler);
        }

        protected virtual void Bind(object target, Delegate handler)
        {
            if (handler == null)
                return;

            if (eventInfo != null)
                eventInfo.Add(target, handler);
        }

        protected virtual void Unbind(object target, Delegate handler)
        {
            if (handler == null)
                return;

            if (eventInfo != null)
                eventInfo.Remove(target, handler);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                var source = Source;
                if (isStatic || source != null)
                    Unbind(source, handler);

                handler = null;
                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
