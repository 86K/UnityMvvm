using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fusion.Mvvm
{
    public abstract class TargetProxyBase : BindingProxyBase, ITargetProxy
    {
        private readonly WeakReference _target;
        private TypeCode _typeCode = TypeCode.Empty;
        protected readonly string _targetName;

        protected TargetProxyBase(object target)
        {
            if (target != null)
            {
                _target = new WeakReference(target, false);
                _targetName = target.ToString();
            }
        }

        public abstract Type Type { get; }

        public virtual TypeCode TypeCode
        {
            get
            {
                if (_typeCode == TypeCode.Empty)
                {
                    _typeCode = Type.GetTypeCode(Type);
                }

                return _typeCode;
            }
        }

        public virtual object Target
        {
            get
            {
                var target = _target?.Target;
                return IsAlive(target) ? target : null;
            }
        }

        private bool IsAlive(object target)
        {
            try
            {
                if (target is UIBehaviour behaviour)
                {
                    if (behaviour.IsDestroyed())
                        return false;
                    return true;
                }

                if (target is UnityEngine.Object o)
                {
                    //Check if the object is valid because it may have been destroyed.
                    //Unmanaged objects,the weak caches do not accurately track the validity of objects.
                    var name = o.name;
                    return true;
                }

                return target != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public virtual BindingMode DefaultMode => BindingMode.OneWay;
    }

    public abstract class ValueTargetProxyBase : TargetProxyBase, IModifiable, IObtainable, INotifiable
    {
        private bool _disposed;
        private bool _subscribed;

        private readonly object _lock = new object();
        private EventHandler _valueChanged;

        protected ValueTargetProxyBase(object target) : base(target)
        {
        }

        public event EventHandler ValueChanged
        {
            add
            {
                lock (_lock)
                {
                    _valueChanged += value;

                    if (_valueChanged != null && !_subscribed)
                        Subscribe();
                }
            }

            remove
            {
                lock (_lock)
                {
                    _valueChanged -= value;

                    if (_valueChanged == null && _subscribed)
                        Unsubscribe();
                }
            }
        }

        private void Subscribe()
        {
            try
            {
                if (_subscribed)
                    return;

                var target = Target;
                if (target == null)
                    return;

                _subscribed = true;
                DoSubscribeForValueChange(target);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{_targetName} Subscribe Exception:{e}");
            }
        }

        protected virtual void DoSubscribeForValueChange(object target)
        {
        }

        private void Unsubscribe()
        {
            try
            {
                if (!_subscribed)
                    return;

                var target = Target;
                if (target == null)
                    return;

                _subscribed = false;
                DoUnsubscribeForValueChange(target);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{_targetName} Unsubscribe Exception:{e}");
            }
        }

        protected virtual void DoUnsubscribeForValueChange(object target)
        {
        }

        public abstract object GetValue();

        public abstract TValue GetValue<TValue>();

        public abstract void SetValue<TValue>(TValue value);

        public abstract void SetValue(object value);

        protected void RaiseValueChanged()
        {
            try
            {
                var handler = _valueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{e}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                lock (_lock)
                {
                    Unsubscribe();
                }

                base.Dispose(disposing);
            }
        }
    }

    public abstract class EventTargetProxyBase : TargetProxyBase, IModifiable
    {
        protected EventTargetProxyBase(object target) : base(target)
        {
        }

        public abstract void SetValue(object value);

        public abstract void SetValue<TValue>(TValue value);
    }
}