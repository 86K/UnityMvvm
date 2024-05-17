using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class SourceProxyBase : BindingProxyBase, ISourceProxy
    {
        private TypeCode _typeCode = TypeCode.Empty;
        protected object Source { get; }

        protected SourceProxyBase(object source)
        {
            Source = source;
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
    }

    public abstract class NotifiableSourceProxyBase : SourceProxyBase, INotifiable
    {
        protected readonly object _lock = new object();
        private EventHandler _valueChanged;

        protected NotifiableSourceProxyBase(object source) : base(source)
        {
        }

        public virtual event EventHandler ValueChanged
        {
            add => _valueChanged += value;
            remove => _valueChanged -= value;
        }

        protected void RaiseValueChanged()
        {
            try
            {
                if (_valueChanged != null)
                    _valueChanged(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}