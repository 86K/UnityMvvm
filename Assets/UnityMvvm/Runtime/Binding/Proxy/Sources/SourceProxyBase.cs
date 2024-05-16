

using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public abstract class SourceProxyBase : BindingProxyBase, ISourceProxy
    {
        protected TypeCode typeCode = TypeCode.Empty;
        protected readonly object source;
        public SourceProxyBase(object source)
        {
            this.source = source;
        }

        public abstract Type Type { get; }

        public virtual TypeCode TypeCode
        {
            get
            {
                if (typeCode == TypeCode.Empty)
                {
#if NETFX_CORE
                    typeCode = WinRTLegacy.TypeExtensions.GetTypeCode(Type);
#else
                    typeCode = Type.GetTypeCode(Type);
#endif
                }
                return typeCode;
            }
        }

        public virtual object Source => source;
    }

    public abstract class NotifiableSourceProxyBase : SourceProxyBase, INotifiable
    {
        protected readonly object _lock = new object();
        protected EventHandler valueChanged;

        public NotifiableSourceProxyBase(object source) : base(source)
        {
        }

        public virtual event EventHandler ValueChanged
        {
            add
            {
                lock (_lock) { valueChanged += value; }
            }

            remove
            {
                lock (_lock) { valueChanged -= value; }
            }
        }

        protected virtual void RaiseValueChanged()
        {
            try
            {
                if (valueChanged != null)
                    valueChanged(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}
