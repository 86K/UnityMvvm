

namespace Fusion.Mvvm
{
    public class ProgressResult<TProgress> : AsyncResult, IProgressResult<TProgress>, IProgressPromise<TProgress>
    {
        private ProgressCallbackable<TProgress> callbackable;
        protected TProgress _progress;

        public ProgressResult() : this(false)
        {
        }

        public ProgressResult(bool cancelable) : base(cancelable)
        {
        }

        /// <summary>
        /// The task's progress.
        /// </summary>
        public virtual TProgress Progress => _progress;

        protected override void RaiseOnCallback()
        {
            base.RaiseOnCallback();
            if (callbackable != null)
                callbackable.RaiseOnCallback();
        }

        protected virtual void RaiseOnProgressCallback(TProgress progress)
        {
            if (callbackable != null)
                callbackable.RaiseOnProgressCallback(progress);
        }
        public new virtual IProgressCallbackable<TProgress> Callbackable()
        {
            lock (_lock)
            {
                return callbackable ?? (callbackable = new ProgressCallbackable<TProgress>(this));
            }
        }

        public virtual void UpdateProgress(TProgress progress)
        {
            _progress = progress;
            RaiseOnProgressCallback(progress);
        }
    }

    public class ProgressResult<TProgress, TResult> : ProgressResult<TProgress>, IProgressResult<TProgress, TResult>, IProgressPromise<TProgress, TResult>
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof(ProgressResult<TProgress, TResult>));

        private Callbackable<TResult> callbackable;
        private ProgressCallbackable<TProgress, TResult> progressCallbackable;
        private Synchronizable<TResult> synchronizable;

        public ProgressResult() : this(false)
        {
        }

        public ProgressResult(bool cancelable) : base(cancelable)
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
                return result != null ? (TResult)result : default(TResult);
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
            if (progressCallbackable != null)
                progressCallbackable.RaiseOnCallback();
        }

        protected override void RaiseOnProgressCallback(TProgress progress)
        {
            base.RaiseOnProgressCallback(progress);
            if (progressCallbackable != null)
                progressCallbackable.RaiseOnProgressCallback(progress);
        }

        public new virtual IProgressCallbackable<TProgress, TResult> Callbackable()
        {
            lock (_lock)
            {
                return progressCallbackable ?? (progressCallbackable = new ProgressCallbackable<TProgress, TResult>(this));
            }
        }
        public new virtual ISynchronizable<TResult> Synchronized()
        {
            lock (_lock)
            {
                return synchronizable ?? (synchronizable = new Synchronizable<TResult>(this, _lock));
            }
        }

        ICallbackable<TResult> IAsyncResult<TResult>.Callbackable()
        {
            lock (_lock)
            {
                return callbackable ?? (callbackable = new Callbackable<TResult>(this));
            }
        }
    }
}
