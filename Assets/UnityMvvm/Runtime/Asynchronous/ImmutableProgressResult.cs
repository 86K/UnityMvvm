

using System;

namespace Fusion.Mvvm
{
    public class ImmutableProgressResult<TProgress> : ProgressResult<TProgress>
    {
        public ImmutableProgressResult(TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetResult(null);
        }

        public ImmutableProgressResult(object result, TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetResult(result);
        }

        public ImmutableProgressResult(Exception exception, TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetException(exception);
        }
    }

    public class ImmutableProgressResult<TProgress, TResult> : ProgressResult<TProgress, TResult>
    {
        public ImmutableProgressResult(TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetResult(default(TResult));
        }

        public ImmutableProgressResult(TResult result, TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetResult(result);
        }

        public ImmutableProgressResult(Exception exception, TProgress progress) : base(false)
        {
            UpdateProgress(progress);
            SetException(exception);
        }
    }
}
