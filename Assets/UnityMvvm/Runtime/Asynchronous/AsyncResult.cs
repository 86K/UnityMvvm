using System;
using System.Threading;

namespace Fusion.Mvvm
{
    public class AsyncResult : IAsyncResult, IPromise
    {
        private bool done;
        private object result;
        private Exception exception;

        private bool cancelled;
        protected bool cancelable;
        protected bool cancellationRequested;

        protected readonly object _lock = new object();

        private Synchronizable synchronizable;
        private Callbackable callbackable;

        public AsyncResult() : this(false)
        {
        }

        public AsyncResult(bool cancelable)
        {
            this.cancelable = cancelable;
        }

        /// <summary>
        /// Exception
        /// </summary>
        public virtual Exception Exception => exception;

        /// <summary>
        /// Returns  "true" if this task finished.
        /// </summary>
        public virtual bool IsDone => done;

        /// <summary>
        /// The execution result
        /// </summary>
        public virtual object Result => result;

        public virtual bool IsCancellationRequested => cancellationRequested;

        /// <summary>
        /// Returns "true" if this task was cancelled before it completed normally.
        /// </summary>
        public virtual bool IsCancelled => cancelled;

        public virtual void SetException(string error)
        {
            if (done)
                return;

            var exception = new Exception(string.IsNullOrEmpty(error) ? "unknown error!" : error);
            SetException(exception);
        }

        public virtual void SetException(Exception exception)
        {
            lock (_lock)
            {
                if (done)
                    return;

                this.exception = exception;
                done = true;
                Monitor.PulseAll(_lock);
            }

            RaiseOnCallback();
        }

        public virtual void SetResult(object result = null)
        {
            lock (_lock)
            {
                if (done)
                    return;

                this.result = result;
                done = true;
                Monitor.PulseAll(_lock);
            }

            RaiseOnCallback();
        }

        public virtual void SetCancelled()
        {
            lock (_lock)
            {
                if (!cancelable || done)
                    return;

                cancelled = true;
                exception = new OperationCanceledException();
                done = true;
                Monitor.PulseAll(_lock);
            }

            RaiseOnCallback();
        }

        /// <summary>
        /// Attempts to cancel execution of this task.  This attempt will 
        /// fail if the task has already completed, has already been cancelled,
        /// or could not be cancelled for some other reason.If successful,
        /// and this task has not started when "Cancel" is called,
        /// this task should never run. 
        /// </summary>
        /// <exception cref="NotSupportedException">If not supported, throw an exception.</exception>
        /// <returns></returns>
        public virtual bool Cancel()
        {
            if (!cancelable)
                throw new NotSupportedException();

            if (IsDone)
                return false;

            cancellationRequested = true;
            SetCancelled();
            return true;
        }

        protected virtual void RaiseOnCallback()
        {
            if (callbackable != null)
                callbackable.RaiseOnCallback();
        }

        public virtual ICallbackable Callbackable()
        {
            lock (_lock)
            {
                return callbackable ?? (callbackable = new Callbackable(this));
            }
        }

        public virtual ISynchronizable Synchronized()
        {
            lock (_lock)
            {
                return synchronizable ?? (synchronizable = new Synchronizable(this, _lock));
            }
        }

        /// <summary>
        /// Wait for the result,suspends the coroutine.
        /// eg:
        /// IAsyncResult result;
        /// yiled return result.WaitForDone();
        /// </summary>
        /// <returns></returns>
        public virtual object WaitForDone()
        {
            return Executors.WaitWhile(() => !IsDone);
        }
    }

    public class AsyncResult<TResult> : AsyncResult, IAsyncResult<TResult>, IPromise<TResult>
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(AsyncResult<TResult>));

        private Synchronizable<TResult> synchronizable;
        private Callbackable<TResult> callbackable;

        public AsyncResult() : this(false)
        {
        }

        public AsyncResult(bool cancelable) : base(cancelable)
        {
        }

        /// <summary>
        /// The execution result
        /// </summary>
        public virtual new TResult Result
        {
            get
            {
                var result = base.Result;
                return result != null ? (TResult)result : default;
            }
        }

        public virtual void SetResult(TResult result)
        {
            base.SetResult(result);
        }

        protected override void RaiseOnCallback()
        {
            base.RaiseOnCallback();
            if (callbackable != null)
                callbackable.RaiseOnCallback();
        }

        public new virtual ICallbackable<TResult> Callbackable()
        {
            lock (_lock)
            {
                return callbackable ?? (callbackable = new Callbackable<TResult>(this));
            }
        }

        public new virtual ISynchronizable<TResult> Synchronized()
        {
            lock (_lock)
            {
                return synchronizable ?? (synchronizable = new Synchronizable<TResult>(this, _lock));
            }
        }
    }
}
