using System;

namespace Fusion.Mvvm
{
    public class EventTargetProxy : EventTargetProxyBase
    {
        private bool disposed;
        protected readonly IProxyEventInfo eventInfo;
        protected Delegate handler;

        public EventTargetProxy(object target, IProxyEventInfo eventInfo) : base(target)
        {
            this.eventInfo = eventInfo;
        }

        public override Type Type => eventInfo.HandlerType;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override void SetValue(object value)
        {
            if (value != null && !value.GetType().Equals(Type))
                throw new ArgumentException("Binding delegate to event failed, mismatched delegate type", "value");

            var target = Target;
            if (target == null)
                return;

            Unbind(target);

            if (value == null)
                return;

            if (value.GetType().Equals(Type))
            {
                handler = (Delegate)value;
                Bind(target);
                return;
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        protected virtual void Bind(object target)
        {
            if (handler == null)
                return;

            if (eventInfo != null)
                eventInfo.Add(target, handler);
        }

        protected virtual void Unbind(object target)
        {
            if (handler == null)
                return;

            if (eventInfo != null)
                eventInfo.Remove(target, handler);

            handler = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                var target = Target;
                if (eventInfo.IsStatic || target != null)
                    Unbind(target);

                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
