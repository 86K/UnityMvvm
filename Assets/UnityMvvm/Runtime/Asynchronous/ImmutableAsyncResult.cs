

using System;

namespace Fusion.Mvvm
{
    public class ImmutableAsyncResult : AsyncResult
    {
        public ImmutableAsyncResult() : base(false)
        {
            SetResult(null);
        }

        public ImmutableAsyncResult(object result) : base(false)
        {
            SetResult(result);
        }

        public ImmutableAsyncResult(Exception exception) : base(false)
        {
            SetException(exception);
        }
    }

    public class ImmutableAsyncResult<TResult> : AsyncResult<TResult>
    {
        public ImmutableAsyncResult() : base(false)
        {
            SetResult(default);
        }

        public ImmutableAsyncResult(TResult result) : base(false)
        {
            SetResult(result);
        }

        public ImmutableAsyncResult(Exception exception) : base(false)
        {
            SetException(exception);
        }
    }
}
