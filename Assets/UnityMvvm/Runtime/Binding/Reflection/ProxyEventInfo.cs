

using System;
using System.Reflection;

namespace Fusion.Mvvm
{
    public class ProxyEventInfo : IProxyEventInfo
    {
        protected EventInfo eventInfo;
        public ProxyEventInfo(EventInfo eventInfo)
        {
            this.eventInfo = eventInfo;
        }

        public Type DeclaringType => eventInfo.DeclaringType;

        public string Name => eventInfo.Name;

        public bool IsStatic => eventInfo.IsStatic();

        public Type HandlerType => eventInfo.EventHandlerType;

        public void Add(object target, Delegate handler)
        {
            eventInfo.AddEventHandler(target, handler);
        }

        public void Remove(object target, Delegate handler)
        {
            eventInfo.RemoveEventHandler(target, handler);
        }
    }
}
