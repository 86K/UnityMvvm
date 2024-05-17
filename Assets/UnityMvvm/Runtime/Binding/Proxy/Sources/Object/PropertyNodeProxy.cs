using System;
using INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Fusion.Mvvm
{
    public class PropertyNodeProxy : NotifiableSourceProxyBase, IObtainable, IModifiable
    {
        private readonly IProxyPropertyInfo _propertyInfo;

        public PropertyNodeProxy(IProxyPropertyInfo propertyInfo) : this(null, propertyInfo)
        {
        }

        public PropertyNodeProxy(object source, IProxyPropertyInfo propertyInfo) : base(source)
        {
            _propertyInfo = propertyInfo;

            if (Source == null)
                return;

            if (Source is INotifyPropertyChanged sourceNotify)
            {
                sourceNotify.PropertyChanged += OnPropertyChanged;
            }
            else
            {
                UnityEngine.Debug.LogWarning(
                    $"The type {propertyInfo.DeclaringType.Name} does not inherit the INotifyPropertyChanged interface and does not support the PropertyChanged event.");
            }
        }

        public override Type Type => _propertyInfo.ValueType;

        public override TypeCode TypeCode => _propertyInfo.ValueTypeCode;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;
            if (string.IsNullOrEmpty(name) || name.Equals(_propertyInfo.Name))
                RaiseValueChanged();
        }

        public object GetValue()
        {
            return _propertyInfo.GetValue(Source);
        }

        public TValue GetValue<TValue>()
        {
            if (_propertyInfo is IProxyPropertyInfo<TValue> proxy)
                return proxy.GetValue(Source);

            return (TValue)_propertyInfo.GetValue(Source);
        }

        public void SetValue(object value)
        {
            _propertyInfo.SetValue(Source, value);
        }

        public void SetValue<TValue>(TValue value)
        {
            if (_propertyInfo is IProxyPropertyInfo<TValue> proxy)
            {
                proxy.SetValue(Source, value);
                return;
            }

            _propertyInfo.SetValue(Source, value);
        }

        #region IDisposable Support

        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (Source != null && Source is INotifyPropertyChanged sourceNotify)
                {
                    sourceNotify.PropertyChanged -= OnPropertyChanged;
                }

                disposedValue = true;
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}