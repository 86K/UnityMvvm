

using System;
using System.Collections;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    [Obsolete("This type will be removed in version 3.0")]
    public class TransformEnumerator : IEnumerator
    {
        private readonly IEnumerator enumerator;
        private readonly Converter<object, object> converter;
        public TransformEnumerator(IEnumerator enumerator, Converter<object, object> converter)
        {
            this.enumerator = enumerator;
            this.converter = converter;
        }

        public object Current { get; private set; }

        public bool MoveNext()
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                Current = converter(current);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }

    [Obsolete("This type will be removed in version 3.0")]
    public class TransformEnumerator<TInput, TOutput> : IEnumerator<TOutput>
    {
        private IEnumerator<TInput> enumerator;
        private Converter<TInput, TOutput> converter;
        public TransformEnumerator(IEnumerator<TInput> enumerator, Converter<TInput, TOutput> converter)
        {
            this.enumerator = enumerator;
            this.converter = converter;
        }

        public TOutput Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                Current = converter(current);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Reset();
                enumerator = null;
                converter = null;
                disposedValue = true;
            }
        }

        ~TransformEnumerator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
