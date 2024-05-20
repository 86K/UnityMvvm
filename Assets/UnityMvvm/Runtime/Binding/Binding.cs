using System;
using UnityEngine.Events;
using UnityEngine;
using System.Threading;

namespace Fusion.Mvvm
{
    public class Binding : BindingBase
    {
        private readonly ISourceProxyFactory _sourceProxyFactory;
        private readonly ITargetProxyFactory _targetProxyFactory;

        private bool _disposed;
        private BindingMode _bindingMode = BindingMode.Default;
        private TargetDescription _targetDescription;
        private ISourceProxy _sourceProxy;
        private ITargetProxy _targetProxy;

        private EventHandler _sourceValueChangedHandler;
        private EventHandler _targetValueChangedHandler;

        private readonly IConverter _converter;
        private bool _isUpdatingSource;
        private bool _isUpdatingTarget;
        private readonly string _targetTypeName;
        private SendOrPostCallback _updateTargetAction;

        public Binding(IBindingContext bindingContext, object source, object target, TargetDescription targetDescription,
            ISourceProxyFactory sourceProxyFactory, ITargetProxyFactory targetProxyFactory) : base(bindingContext, source)
        {
            _targetTypeName = target.GetType().Name;
            _targetDescription = targetDescription;

            _converter = targetDescription.Converter;
            _sourceProxyFactory = sourceProxyFactory;
            _targetProxyFactory = targetProxyFactory;

            CreateTargetProxy(target, _targetDescription);
            CreateSourceProxy(DataContext, _targetDescription.Source);
            UpdateDataOnBind();
        }

        private string GetViewName()
        {
            if (BindingContext == null)
                return "unknown";

            var owner = BindingContext.Owner;
            if (owner == null)
                return "unknown";

            string typeName = owner.GetType().Name;
            string name = owner is Behaviour behaviour ? behaviour.name : "";
            return string.IsNullOrEmpty(name) ? typeName : $"{typeName}[{name}]";
        }

        protected override void OnDataContextChanged()
        {
            if (_targetDescription.Source.IsStatic)
                return;

            CreateSourceProxy(DataContext, _targetDescription.Source);
            UpdateDataOnBind();
        }

        private BindingMode BindingMode
        {
            get
            {
                if (_bindingMode != BindingMode.Default)
                    return _bindingMode;

                _bindingMode = _targetDescription.Mode;
                if (_bindingMode == BindingMode.Default)
                    _bindingMode = _targetProxy.DefaultMode;

                if (_bindingMode == BindingMode.Default)
                    Debug.Log("Not set the BindingMode!");

                return _bindingMode;
            }
        }

        private void UpdateDataOnBind()
        {
            try
            {
                if (UpdateTargetOnFirstBind(BindingMode) && _sourceProxy != null)
                {
                    UpdateTargetFromSource();
                }

                if (UpdateSourceOnFirstBind(BindingMode) && _targetProxy != null && _targetProxy is IObtainable)
                {
                    UpdateSourceFromTarget();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"An exception occurs in UpdateTargetOnBind.exception: {e}");
            }
        }

        private void CreateSourceProxy(object source, SourceDescription description)
        {
            DisposeSourceProxy();

            _sourceProxy = _sourceProxyFactory.CreateProxy(description.IsStatic ? null : source, description);

            if (IsSubscribeSourceValueChanged(BindingMode) && _sourceProxy is INotifiable notifiable)
            {
                _sourceValueChangedHandler = (sender, args) => UpdateTargetFromSource();
                notifiable.ValueChanged += _sourceValueChangedHandler;
            }
        }

        private void DisposeSourceProxy()
        {
            try
            {
                if (_sourceProxy != null)
                {
                    if (_sourceValueChangedHandler != null)
                    {
                        ((INotifiable)_sourceProxy).ValueChanged -= _sourceValueChangedHandler;
                        _sourceValueChangedHandler = null;
                    }

                    _sourceProxy.Dispose();
                    _sourceProxy = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void CreateTargetProxy(object target, TargetDescription description)
        {
            DisposeTargetProxy();

            _targetProxy = _targetProxyFactory.CreateProxy(target, description);

            if (IsSubscribeTargetValueChanged(BindingMode) && _targetProxy is INotifiable notifiable)
            {
                _targetValueChangedHandler = (sender, args) => UpdateSourceFromTarget();
                notifiable.ValueChanged += _targetValueChangedHandler;
            }
        }

        private void DisposeTargetProxy()
        {
            try
            {
                if (_targetProxy != null)
                {
                    if (_targetValueChangedHandler != null)
                    {
                        ((INotifiable)_targetProxy).ValueChanged -= _targetValueChangedHandler;
                        _targetValueChangedHandler = null;
                    }

                    _targetProxy.Dispose();
                    _targetProxy = null;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void UpdateTargetFromSource()
        {
            if (UISynchronizationContext.InThread)
            {
                DoUpdateTargetFromSource(null);
            }
            else
            {
#if UNITY_WEBGL
                if (updateTargetAction == null)
                    updateTargetAction = DoUpdateTargetFromSource;
#else
                if (_updateTargetAction == null)
                    Interlocked.CompareExchange(ref _updateTargetAction, DoUpdateTargetFromSource, null);
#endif
                //Run on the main thread
                UISynchronizationContext.Post(_updateTargetAction, null);
            }
        }

        private void DoUpdateTargetFromSource(object state)
        {
            try
            {
                if (_isUpdatingSource)
                    return;

                _isUpdatingTarget = true;

                IObtainable obtainable = _sourceProxy as IObtainable;
                if (obtainable == null)
                    return;

                IModifiable modifier = _targetProxy as IModifiable;
                if (modifier == null)
                    return;

                TypeCode typeCode = _sourceProxy.TypeCode;
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    {
                        var value = obtainable.GetValue<bool>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Byte:
                    {
                        var value = obtainable.GetValue<byte>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Char:
                    {
                        var value = obtainable.GetValue<char>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.DateTime:
                    {
                        var value = obtainable.GetValue<DateTime>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Decimal:
                    {
                        var value = obtainable.GetValue<decimal>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Double:
                    {
                        var value = obtainable.GetValue<double>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int16:
                    {
                        var value = obtainable.GetValue<short>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int32:
                    {
                        var value = obtainable.GetValue<int>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int64:
                    {
                        var value = obtainable.GetValue<long>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.SByte:
                    {
                        var value = obtainable.GetValue<sbyte>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Single:
                    {
                        var value = obtainable.GetValue<float>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.String:
                    {
                        var value = obtainable.GetValue<string>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt16:
                    {
                        var value = obtainable.GetValue<ushort>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt32:
                    {
                        var value = obtainable.GetValue<uint>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt64:
                    {
                        var value = obtainable.GetValue<ulong>();
                        SetTargetValue(modifier, value);
                        break;
                    }
                    case TypeCode.Object:
                    {
                        Type valueType = _sourceProxy.Type;
                        if (valueType == typeof(Vector2))
                        {
                            var value = obtainable.GetValue<Vector2>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(Vector3))
                        {
                            var value = obtainable.GetValue<Vector3>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(Vector4))
                        {
                            var value = obtainable.GetValue<Vector4>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(Color))
                        {
                            var value = obtainable.GetValue<Color>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(Rect))
                        {
                            var value = obtainable.GetValue<Rect>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(Quaternion))
                        {
                            var value = obtainable.GetValue<Quaternion>();
                            SetTargetValue(modifier, value);
                        }
                        else if (valueType == typeof(TimeSpan))
                        {
                            var value = obtainable.GetValue<TimeSpan>();
                            SetTargetValue(modifier, value);
                        }
                        else
                        {
                            var value = obtainable.GetValue();
                            SetTargetValue(modifier, value);
                        }

                        break;
                    }
                    default:
                    {
                        var value = obtainable.GetValue();
                        SetTargetValue(modifier, value);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"An exception occurs when the target property is updated.Please check the binding \"{_targetTypeName}{_targetDescription}\" in the view \"{GetViewName()}\".exception: {e}");
            }
            finally
            {
                _isUpdatingTarget = false;
            }
        }

        private void UpdateSourceFromTarget()
        {
            try
            {
                if (_isUpdatingTarget)
                    return;

                _isUpdatingSource = true;


                IObtainable obtainable = _targetProxy as IObtainable;
                if (obtainable == null)
                    return;

                IModifiable modifier = _sourceProxy as IModifiable;
                if (modifier == null)
                    return;

                TypeCode typeCode = _targetProxy.TypeCode;
                switch (typeCode)
                {
                    case TypeCode.Boolean:
                    {
                        var value = obtainable.GetValue<bool>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Byte:
                    {
                        var value = obtainable.GetValue<byte>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Char:
                    {
                        var value = obtainable.GetValue<char>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.DateTime:
                    {
                        var value = obtainable.GetValue<DateTime>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Decimal:
                    {
                        var value = obtainable.GetValue<decimal>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Double:
                    {
                        var value = obtainable.GetValue<double>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int16:
                    {
                        var value = obtainable.GetValue<short>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int32:
                    {
                        var value = obtainable.GetValue<int>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Int64:
                    {
                        var value = obtainable.GetValue<long>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.SByte:
                    {
                        var value = obtainable.GetValue<sbyte>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Single:
                    {
                        var value = obtainable.GetValue<float>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.String:
                    {
                        var value = obtainable.GetValue<string>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt16:
                    {
                        var value = obtainable.GetValue<ushort>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt32:
                    {
                        var value = obtainable.GetValue<uint>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.UInt64:
                    {
                        var value = obtainable.GetValue<ulong>();
                        SetSourceValue(modifier, value);
                        break;
                    }
                    case TypeCode.Object:
                    {
                        Type valueType = _targetProxy.Type;
                        if (valueType == typeof(Vector2))
                        {
                            var value = obtainable.GetValue<Vector2>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(Vector3))
                        {
                            var value = obtainable.GetValue<Vector3>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(Vector4))
                        {
                            var value = obtainable.GetValue<Vector4>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(Color))
                        {
                            var value = obtainable.GetValue<Color>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(Rect))
                        {
                            var value = obtainable.GetValue<Rect>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(Quaternion))
                        {
                            var value = obtainable.GetValue<Quaternion>();
                            SetSourceValue(modifier, value);
                        }
                        else if (valueType == typeof(TimeSpan))
                        {
                            var value = obtainable.GetValue<TimeSpan>();
                            SetSourceValue(modifier, value);
                        }
                        else
                        {
                            var value = obtainable.GetValue();
                            SetSourceValue(modifier, value);
                        }

                        break;
                    }
                    default:
                    {
                        var value = obtainable.GetValue();
                        SetSourceValue(modifier, value);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"An exception occurs when the source property is updated.Please check the binding \"{_targetTypeName}{_targetDescription}\" in the view \"{GetViewName()}\".exception: {e}");
            }
            finally
            {
                _isUpdatingSource = false;
            }
        }

        private void SetTargetValue<T>(IModifiable modifier, T value)
        {
            if (_converter == null && typeof(T) == _targetProxy.Type)
            {
                modifier.SetValue(value);
                return;
            }

            object safeValue = value;
            if (_converter != null)
                safeValue = _converter.Convert(value);

            if (!typeof(UnityEventBase).IsAssignableFrom(_targetProxy.Type))
                safeValue = _targetProxy.Type.ToSafe(safeValue);

            modifier.SetValue(safeValue);
        }

        private void SetSourceValue<T>(IModifiable modifier, T value)
        {
            if (_converter == null && typeof(T) == _sourceProxy.Type)
            {
                modifier.SetValue(value);
                return;
            }

            object safeValue = value;
            if (_converter != null)
                safeValue = _converter.ConvertBack(safeValue);

            safeValue = _sourceProxy.Type.ToSafe(safeValue);

            modifier.SetValue(safeValue);
        }

        private bool IsSubscribeSourceValueChanged(BindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case BindingMode.Default:
                    return true;

                case BindingMode.OneWay:
                case BindingMode.TwoWay:
                    return true;

                case BindingMode.OneTime:
                case BindingMode.OneWayToSource:
                    return false;

                default:
                    throw new Exception("Unexpected BindingMode");
            }
        }

        private bool IsSubscribeTargetValueChanged(BindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case BindingMode.Default:
                    return true;

                case BindingMode.OneWay:
                case BindingMode.OneTime:
                    return false;

                case BindingMode.TwoWay:
                case BindingMode.OneWayToSource:
                    return true;

                default:
                    throw new Exception("Unexpected BindingMode");
            }
        }

        private bool UpdateTargetOnFirstBind(BindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case BindingMode.Default:
                    return true;

                case BindingMode.OneWay:
                case BindingMode.OneTime:
                case BindingMode.TwoWay:
                    return true;

                case BindingMode.OneWayToSource:
                    return false;

                default:
                    throw new Exception("Unexpected BindingMode");
            }
        }

        private bool UpdateSourceOnFirstBind(BindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case BindingMode.OneWayToSource:
                    return true;

                case BindingMode.Default:
                    return false;

                case BindingMode.OneWay:
                case BindingMode.OneTime:
                case BindingMode.TwoWay:
                    return false;

                default:
                    throw new Exception("Unexpected BindingMode");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DisposeSourceProxy();
                DisposeTargetProxy();
                _targetDescription = null;
                _disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}