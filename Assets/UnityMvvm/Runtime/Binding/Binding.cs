

using System;
using UnityEngine.Events;
using UnityEngine;
using System.Threading;

namespace Fusion.Mvvm
{
    public class Binding : AbstractBinding
    {
        private readonly ISourceProxyFactory sourceProxyFactory;
        private readonly ITargetProxyFactory targetProxyFactory;

        private bool disposed = false;
        private BindingMode bindingMode = BindingMode.Default;
        private BindingDescription bindingDescription;
        private ISourceProxy sourceProxy;
        private ITargetProxy targetProxy;

        private EventHandler sourceValueChangedHandler;
        private EventHandler targetValueChangedHandler;

        private readonly IConverter converter;
        private bool isUpdatingSource;
        private bool isUpdatingTarget;
        private readonly string targetTypeName;
        private SendOrPostCallback updateTargetAction;

        public Binding(IBindingContext bindingContext, object source, object target, BindingDescription bindingDescription, ISourceProxyFactory sourceProxyFactory, ITargetProxyFactory targetProxyFactory) : base(bindingContext, source, target)
        {
            targetTypeName = target.GetType().Name;
            this.bindingDescription = bindingDescription;

            converter = bindingDescription.Converter;
            this.sourceProxyFactory = sourceProxyFactory;
            this.targetProxyFactory = targetProxyFactory;

            CreateTargetProxy(target, this.bindingDescription);
            CreateSourceProxy(DataContext, this.bindingDescription.Source);
            UpdateDataOnBind();
        }

        protected virtual string GetViewName()
        {
            if (BindingContext == null)
                return "unknown";

            var owner = BindingContext.Owner;
            if (owner == null)
                return "unknown";

            string typeName = owner.GetType().Name;
            string name = (owner is Behaviour) ? ((Behaviour)owner).name : "";
            return string.IsNullOrEmpty(name) ? typeName : $"{typeName}[{name}]";
        }

        protected override void OnDataContextChanged()
        {
            if (bindingDescription.Source.IsStatic)
                return;

            CreateSourceProxy(DataContext, bindingDescription.Source);
            UpdateDataOnBind();
        }

        protected BindingMode BindingMode
        {
            get
            {
                if (bindingMode != BindingMode.Default)
                    return bindingMode;

                bindingMode = bindingDescription.Mode;
                if (bindingMode == BindingMode.Default)
                    bindingMode = targetProxy.DefaultMode;

                if (bindingMode == BindingMode.Default)
                    Debug.Log("Not set the BindingMode!");

                return bindingMode;
            }
        }

        protected void UpdateDataOnBind()
        {
            try
            {
                if (UpdateTargetOnFirstBind(BindingMode) && sourceProxy != null)
                {
                    UpdateTargetFromSource();
                }

                if (UpdateSourceOnFirstBind(BindingMode) && targetProxy != null && targetProxy is IObtainable)
                {
                    UpdateSourceFromTarget();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(string.Format("An exception occurs in UpdateTargetOnBind.exception: {0}", e));
            }
        }

        protected void CreateSourceProxy(object source, SourceDescription description)
        {
            DisposeSourceProxy();

            sourceProxy = sourceProxyFactory.CreateProxy(description.IsStatic ? null : source, description);

            if (IsSubscribeSourceValueChanged(BindingMode) && sourceProxy is INotifiable)
            {
                sourceValueChangedHandler = (sender, args) => UpdateTargetFromSource();
                (sourceProxy as INotifiable).ValueChanged += sourceValueChangedHandler;
            }
        }

        protected void DisposeSourceProxy()
        {
            try
            {
                if (sourceProxy != null)
                {
                    if (sourceValueChangedHandler != null)
                    {
                        (sourceProxy as INotifiable).ValueChanged -= sourceValueChangedHandler;
                        sourceValueChangedHandler = null;
                    }

                    sourceProxy.Dispose();
                    sourceProxy = null;
                }
            }
            catch (Exception) { }
        }

        protected void CreateTargetProxy(object target, BindingDescription description)
        {
            DisposeTargetProxy();

            targetProxy = targetProxyFactory.CreateProxy(target, description);

            if (IsSubscribeTargetValueChanged(BindingMode) && targetProxy is INotifiable)
            {
                targetValueChangedHandler = (sender, args) => UpdateSourceFromTarget();
                (targetProxy as INotifiable).ValueChanged += targetValueChangedHandler;
            }
        }

        protected void DisposeTargetProxy()
        {
            try
            {
                if (targetProxy != null)
                {
                    if (targetValueChangedHandler != null)
                    {
                        (targetProxy as INotifiable).ValueChanged -= targetValueChangedHandler;
                        targetValueChangedHandler = null;
                    }
                    targetProxy.Dispose();
                    targetProxy = null;
                }
            }
            catch (Exception) { }
        }


        protected virtual void UpdateTargetFromSource()
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
                if (updateTargetAction == null)
                    Interlocked.CompareExchange(ref updateTargetAction, DoUpdateTargetFromSource, null);
#endif
                //Run on the main thread
                UISynchronizationContext.Post(updateTargetAction, null);
            }
        }

        protected void DoUpdateTargetFromSource(object state)
        {
            try
            {
                if (isUpdatingSource)
                    return;

                isUpdatingTarget = true;

                IObtainable obtainable = sourceProxy as IObtainable;
                if (obtainable == null)
                    return;

                IModifiable modifier = targetProxy as IModifiable;
                if (modifier == null)
                    return;

                TypeCode typeCode = sourceProxy.TypeCode;
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
                            Type valueType = sourceProxy.Type;
                            if (valueType.Equals(typeof(Vector2)))
                            {
                                var value = obtainable.GetValue<Vector2>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Vector3)))
                            {
                                var value = obtainable.GetValue<Vector3>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Vector4)))
                            {
                                var value = obtainable.GetValue<Vector4>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Color)))
                            {
                                var value = obtainable.GetValue<Color>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Rect)))
                            {
                                var value = obtainable.GetValue<Rect>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Quaternion)))
                            {
                                var value = obtainable.GetValue<Quaternion>();
                                SetTargetValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(TimeSpan)))
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
                Debug.LogWarning(string.Format("An exception occurs when the target property is updated.Please check the binding \"{0}{1}\" in the view \"{2}\".exception: {3}", targetTypeName, bindingDescription.ToString(), GetViewName(), e));
            }
            finally
            {
                isUpdatingTarget = false;
            }
        }

        protected virtual void UpdateSourceFromTarget()
        {
            try
            {
                if (isUpdatingTarget)
                    return;

                isUpdatingSource = true;


                IObtainable obtainable = targetProxy as IObtainable;
                if (obtainable == null)
                    return;

                IModifiable modifier = sourceProxy as IModifiable;
                if (modifier == null)
                    return;

                TypeCode typeCode = targetProxy.TypeCode;
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
                            Type valueType = targetProxy.Type;
                            if (valueType.Equals(typeof(Vector2)))
                            {
                                var value = obtainable.GetValue<Vector2>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Vector3)))
                            {
                                var value = obtainable.GetValue<Vector3>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Vector4)))
                            {
                                var value = obtainable.GetValue<Vector4>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Color)))
                            {
                                var value = obtainable.GetValue<Color>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Rect)))
                            {
                                var value = obtainable.GetValue<Rect>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(Quaternion)))
                            {
                                var value = obtainable.GetValue<Quaternion>();
                                SetSourceValue(modifier, value);
                            }
                            else if (valueType.Equals(typeof(TimeSpan)))
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
                Debug.LogWarning(string.Format("An exception occurs when the source property is updated.Please check the binding \"{0}{1}\" in the view \"{2}\".exception: {3}", targetTypeName, bindingDescription.ToString(), GetViewName(), e));
            }
            finally
            {
                isUpdatingSource = false;
            }
        }

        protected void SetTargetValue<T>(IModifiable modifier, T value)
        {
            if (converter == null && typeof(T).Equals(targetProxy.Type))
            {
                modifier.SetValue(value);
                return;
            }

            object safeValue = value;
            if (converter != null)
                safeValue = converter.Convert(value);

            if (!typeof(UnityEventBase).IsAssignableFrom(targetProxy.Type))
                safeValue = targetProxy.Type.ToSafe(safeValue);

            modifier.SetValue(safeValue);
        }

        private void SetSourceValue<T>(IModifiable modifier, T value)
        {
            if (converter == null && typeof(T).Equals(sourceProxy.Type))
            {
                modifier.SetValue(value);
                return;
            }

            object safeValue = value;
            if (converter != null)
                safeValue = converter.ConvertBack(safeValue);

            safeValue = sourceProxy.Type.ToSafe(safeValue);

            modifier.SetValue(safeValue);
        }

        protected bool IsSubscribeSourceValueChanged(BindingMode bindingMode)
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
                    throw new BindingException("Unexpected BindingMode");
            }
        }

        protected bool IsSubscribeTargetValueChanged(BindingMode bindingMode)
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
                    throw new BindingException("Unexpected BindingMode");
            }
        }

        protected bool UpdateTargetOnFirstBind(BindingMode bindingMode)
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
                    throw new BindingException("Unexpected BindingMode");
            }
        }

        protected bool UpdateSourceOnFirstBind(BindingMode bindingMode)
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
                    throw new BindingException("Unexpected BindingMode");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                DisposeSourceProxy();
                DisposeTargetProxy();
                bindingDescription = null;
                disposed = true;
                base.Dispose(disposing);
            }
        }
    }
}
