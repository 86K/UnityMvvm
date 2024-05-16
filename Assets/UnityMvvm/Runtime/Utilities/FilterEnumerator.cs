

using System;
using System.Collections;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    [Obsolete("This type will be removed in version 3.0")]
    public class FilterEnumerator : IEnumerator
    {
        private readonly IEnumerator enumerator;
        private readonly Predicate<object> match;
        public FilterEnumerator(IEnumerator enumerator, Predicate<object> match)
        {
            this.enumerator = enumerator;
            this.match = match;
        }

        public object Current { get; private set; }

        public bool MoveNext()
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!match(current))
                    continue;

                Current = current;
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
    public class FilterEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> enumerator;
        private Predicate<T> match;
        public FilterEnumerator(IEnumerator<T> enumerator, Predicate<T> match)
        {
            Current = default(T);
            this.enumerator = enumerator;
            this.match = match;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (!match(current))
                    continue;

                Current = current;
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
                match = null;
                disposedValue = true;
            }
        }

        ~FilterEnumerator()
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
