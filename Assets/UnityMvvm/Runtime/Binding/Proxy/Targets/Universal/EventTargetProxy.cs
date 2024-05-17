using System;

namespace Fusion.Mvvm
{
    public class EventTargetProxy : EventTargetProxyBase
    {
        private bool _disposed;
        private readonly IProxyEventInfo _eventInfo;
        private Delegate _handler;

        public EventTargetProxy(object target, IProxyEventInfo eventInfo) : base(target)
        {
            _eventInfo = eventInfo;
        }

        public override Type Type => _eventInfo.HandlerType;

        public override BindingMode DefaultMode => BindingMode.OneWay;

        public override void SetValue(object value)
        {
            if (value != null && !(value.GetType() == Type))
                throw new ArgumentException("Binding delegate to event failed, mismatched delegate type", "value");

            var target = Target;
            if (target == null)
                return;

            Unbind(target);

            if (value == null)
                return;

            if (value.GetType() == Type)
            {
                _handler = (Delegate)value;
                Bind(target);
            }
        }

        public override void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        private void Bind(object target)
        {
            if (_handler == null)
                return;

            if (_eventInfo != null)
                _eventInfo.Add(target, _handler);
        }

        private void Unbind(object target)
        {
            if (_handler == null)
                return;

            if (_eventInfo != null)
                _eventInfo.Remove(target, _handler);

            _handler = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                var target = Target;
                if (_eventInfo.IsStatic || target != null)
                    Unbind(target);

                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
