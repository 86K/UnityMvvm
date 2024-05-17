using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class ProxyEventInfo : IProxyEventInfo
    {
        private readonly EventInfo _eventInfo;

        public ProxyEventInfo(EventInfo eventInfo)
        {
            _eventInfo = eventInfo;
        }

        public Type DeclaringType => _eventInfo.DeclaringType;

        public string Name => _eventInfo.Name;

        public bool IsStatic => _eventInfo.IsStatic();

        public Type HandlerType => _eventInfo.EventHandlerType;

        public void Add(object target, Delegate handler)
        {
            _eventInfo.AddEventHandler(target, handler);
        }

        public void Remove(object target, Delegate handler)
        {
            _eventInfo.RemoveEventHandler(target, handler);
        }
    }
}