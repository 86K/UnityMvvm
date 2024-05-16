

#if NET_4_6 || NET_STANDARD_2_0
using System;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class Progress<T> : IProgress<T>
    {
        private readonly bool runOnMainThread;
        private readonly Action<T> handler;

        public Progress() : this(null, true)
        {
        }

        public Progress(Action<T> handler) : this(handler, true)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
        }

        public Progress(Action<T> handler, bool runOnMainThread)
        {
            this.handler = handler;
            this.runOnMainThread = runOnMainThread;
        }

        public event EventHandler<T> ProgressChanged;

        protected virtual void OnReport(T value)
        {
            try
            {
                Action<T> handler = this.handler;
                EventHandler<T> changedEvent = ProgressChanged;
                if (handler != null || changedEvent != null)
                {
                    if (runOnMainThread)
                        Executors.RunOnMainThread(() => { RaiseProgressChanged(value); });
                    else
                        RaiseProgressChanged(value);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        void IProgress<T>.Report(T value)
        {
            OnReport(value);
        }

        private void RaiseProgressChanged(T value)
        {
            Action<T> handler = this.handler;
            EventHandler<T> progressChanged = ProgressChanged;

            handler?.Invoke(value);
            progressChanged?.Invoke(this, value);
        }
    }
}
#endif
