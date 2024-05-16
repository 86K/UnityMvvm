

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;

namespace Fusion.Mvvm
{
    public class VisualElementFieldProxy<TValue> : FieldTargetProxy
    {
        private readonly INotifyValueChanged<TValue> notifyValueChanged;
        public VisualElementFieldProxy(object target, IProxyFieldInfo fieldInfo) : base(target, fieldInfo)
        {
            //不能所有的属性都自动绑定，可能绑定的其他值，与事件不匹配的值
            if (target is INotifyValueChanged<TValue>)
                notifyValueChanged = (INotifyValueChanged<TValue>)target;
            else
                notifyValueChanged = null;
        }

        public override BindingMode DefaultMode => BindingMode.TwoWay;

        protected override void DoSubscribeForValueChange(object target)
        {
            if (notifyValueChanged == null || target == null)
                return;

            notifyValueChanged.RegisterValueChangedCallback(OnValueChanged);
        }

        protected override void DoUnsubscribeForValueChange(object target)
        {
            if (notifyValueChanged != null)
                notifyValueChanged.UnregisterValueChangedCallback(OnValueChanged);
        }

        private void OnValueChanged(ChangeEvent<TValue> e)
        {
            RaiseValueChanged();
        }
    }
}
#endif
