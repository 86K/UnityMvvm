using UnityEngine.Events;

namespace Fusion.Mvvm
{
    public class UnityPropertyProxy<TValue> : PropertyTargetProxy
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(UnityPropertyProxy<TValue>));

        private readonly UnityEvent<TValue> unityEvent;
        public UnityPropertyProxy(object target, IProxyPropertyInfo propertyInfo, UnityEvent<TValue> unityEvent) : base(target, propertyInfo)
        {
            this.unityEvent = unityEvent;
        }

        public override BindingMode DefaultMode => BindingMode.TwoWay;

        protected override void DoSubscribeForValueChange(object target)
        {
            if (unityEvent == null || target == null)
                return;

            unityEvent.AddListener(OnValueChanged);
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (unityEvent != null)
                unityEvent.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(TValue value)
        {
            RaiseValueChanged();
        }
    }
}
