using System;

namespace Fusion.Mvvm
{
    public class EventNodeProxy : SourceProxyBase, ISourceProxy, IModifiable
    {
        private readonly IProxyEventInfo _eventInfo;
        private bool _disposed;
        private readonly bool _isStatic;
        private Delegate _handler;

        public EventNodeProxy(IProxyEventInfo eventInfo) : this(null, eventInfo)
        {
        }

        public EventNodeProxy(object source, IProxyEventInfo eventInfo) : base(source)
        {
            _eventInfo = eventInfo;
            _isStatic = _eventInfo.IsStatic;
        }

        public override Type Type => _eventInfo.HandlerType;

        public void SetValue<TValue>(TValue value)
        {
            SetValue((object)value);
        }

        public void SetValue(object value)
        {
            if (value != null && !(value.GetType() == Type))
                throw new ArgumentException("Binding delegate to event failed, mismatched delegate type", "value");

            Unbind(Source, _handler);
            _handler = (Delegate)value;
            Bind(Source, _handler);
        }

        private void Bind(object target, Delegate handler)
        {
            if (handler == null)
                return;

            if (_eventInfo != null)
                _eventInfo.Add(target, handler);
        }

        private void Unbind(object target, Delegate handler)
        {
            if (handler == null)
                return;

            if (_eventInfo != null)
                _eventInfo.Remove(target, handler);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                var source = Source;
                if (_isStatic || source != null)
                    Unbind(source, _handler);

                _handler = null;
                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}