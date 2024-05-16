using System;

namespace Fusion.Mvvm
{
    public class ObservableLiteralSourceProxy : NotifiableSourceProxyBase, ISourceProxy, IObtainable
    {
        private readonly IObservableProperty observableProperty;

        public ObservableLiteralSourceProxy(IObservableProperty source) : base(source)
        {
            observableProperty = source ?? throw new ArgumentNullException("source");
            observableProperty.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs e)
        {
            RaiseValueChanged();
        }

        public override Type Type => observableProperty.Type;

        public virtual object GetValue()
        {
            return observableProperty.Value;
        }

        public virtual TValue GetValue<TValue>()
        {
            return (TValue)Convert.ChangeType(observableProperty.Value, typeof(TValue));
        }

        #region IDisposable Support    
        private bool disposedValue;

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (observableProperty != null)
                    observableProperty.ValueChanged -= OnValueChanged;

                disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
