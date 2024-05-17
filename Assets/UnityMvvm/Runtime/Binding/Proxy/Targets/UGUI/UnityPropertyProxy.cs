using UnityEngine.Events;

namespace Fusion.Mvvm
{
    public class UnityPropertyProxy<TValue> : PropertyTargetProxy
    {
        private readonly UnityEvent<TValue> _unityEvent;

        public UnityPropertyProxy(object target, IProxyPropertyInfo propertyInfo, UnityEvent<TValue> unityEvent) : base(target, propertyInfo)
        {
            _unityEvent = unityEvent;
        }

        public override BindingMode DefaultMode => BindingMode.TwoWay;

        protected override void DoSubscribeForValueChange(object target)
        {
            if (_unityEvent == null || target == null)
                return;

            _unityEvent.AddListener(OnValueChanged);
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (_unityEvent != null)
                _unityEvent.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(TValue value)
        {
            RaiseValueChanged();
        }
    }
}