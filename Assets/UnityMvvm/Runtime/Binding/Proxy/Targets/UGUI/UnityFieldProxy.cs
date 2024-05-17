using UnityEngine.Events;

namespace Fusion.Mvvm
{
    public class UnityFieldProxy<TValue> : FieldTargetProxy
    {
        private readonly UnityEvent<TValue> _unityEvent;

        public UnityFieldProxy(object target, IProxyFieldInfo fieldInfo, UnityEvent<TValue> unityEvent) : base(target, fieldInfo)
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