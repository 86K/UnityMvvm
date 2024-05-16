

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace Fusion.Mvvm
{
    public class CoroutineAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        protected object _lock = new object();
        protected bool done = false;
        protected Exception exception;
        protected Action continuation;

        public bool IsCompleted => done;

        public void GetResult()
        {
            lock (_lock)
            {
                if (!done)
                    throw new Exception("The task is not finished yet");
            }

            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();
        }

        public void SetResult(Exception exception)
        {
            lock (_lock)
            {
                if (done)
                    return;

                this.exception = exception;
                done = true;
                try
                {
                    if (continuation != null)
                        continuation();
                }
                catch (Exception) { }
                finally
                {
                    continuation = null;
                }
            }
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            lock (_lock)
            {
                if (done)
                {
                    continuation();
                }
                else
                {
                    this.continuation += continuation;
                }
            }
        }
    }

    public class CoroutineAwaiter<T> : CoroutineAwaiter, IAwaiter<T>, ICriticalNotifyCompletion
    {
        protected T result;

        public CoroutineAwaiter()
        {
        }

        public new T GetResult()
        {
            lock (_lock)
            {
                if (!done)
                    throw new Exception("The task is not finished yet");
            }

            if (exception != null)
                ExceptionDispatchInfo.Capture(exception).Throw();

            return result;
        }

        public void SetResult(T result, Exception exception)
        {
            lock (_lock)
            {
                if (done)
                    return;

                this.result = result;
                this.exception = exception;
                done = true;
                try
                {
                    if (continuation != null)
                        continuation();
                }
                catch (Exception) { }
                finally
                {
                    continuation = null;
                }
            }
        }
    }

    public struct AsyncOperationAwaiter : IAwaiter, ICriticalNotifyCompletion
    {
        private readonly AsyncOperation asyncOperation;
        private Action<AsyncOperation> continuationAction;

        public AsyncOperationAwaiter(AsyncOperation asyncOperation)
        {
            this.asyncOperation = asyncOperation;
            continuationAction = null;
        }

        public bool IsCompleted => asyncOperation.isDone;

        public void GetResult()
        {
            if (!IsCompleted)
                throw new Exception("The task is not finished yet");

            if (continuationAction != null)
                asyncOperation.completed -= continuationAction;
            continuationAction = null;
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            if (asyncOperation.isDone)
            {
                continuation();
            }
            else
            {
                continuationAction = (ao) => { continuation(); };
                asyncOperation.completed += continuationAction;
            }
        }
    }

    public struct AsyncOperationAwaiter<T, TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion where T : AsyncOperation
    {
        private readonly T asyncOperation;
        private readonly Func<T, TResult> getter;
        private Action<AsyncOperation> continuationAction;

        public AsyncOperationAwaiter(T asyncOperation, Func<T, TResult> getter)
        {
            this.asyncOperation = asyncOperation ?? throw new ArgumentNullException("asyncOperation");
            this.getter = getter ?? throw new ArgumentNullException("getter");
            continuationAction = null;
        }

        public bool IsCompleted => asyncOperation.isDone;

        public TResult GetResult()
        {
            if (!IsCompleted)
                throw new Exception("The task is not finished yet");

            if (continuationAction != null)
            {
                asyncOperation.completed -= continuationAction;
                continuationAction = null;
            }
            return getter(asyncOperation);
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            if (asyncOperation.isDone)
            {
                continuation();
            }
            else
            {
                continuationAction = (ao) => { continuation(); };
                asyncOperation.completed += continuationAction;
            }
        }
    }

    public struct AsyncResultAwaiter<T> : IAwaiter<object>, ICriticalNotifyCompletion where T : IAsyncResult
    {
        private T asyncResult;

        public AsyncResultAwaiter(T asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            this.asyncResult = asyncResult;
        }

        public bool IsCompleted => asyncResult.IsDone;

        public object GetResult()
        {
            if (!IsCompleted)
                throw new Exception("The task is not finished yet");

            if (asyncResult.Exception != null)
                ExceptionDispatchInfo.Capture(asyncResult.Exception).Throw();

            return asyncResult.Result;
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");
            asyncResult.Callbackable().OnCallback((ar) => { continuation(); });
        }
    }

    public struct AsyncResultAwaiter<T, TResult> : IAwaiter<TResult>, ICriticalNotifyCompletion where T : IAsyncResult<TResult>
    {
        private T asyncResult;

        public AsyncResultAwaiter(T asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");
            this.asyncResult = asyncResult;
        }

        public bool IsCompleted => asyncResult.IsDone;

        public TResult GetResult()
        {
            if (!IsCompleted)
                throw new Exception("The task is not finished yet");

            if (asyncResult.Exception != null)
                ExceptionDispatchInfo.Capture(asyncResult.Exception).Throw();

            return asyncResult.Result;
        }

        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");
            asyncResult.Callbackable().OnCallback((ar) => { continuation(); });
        }
    }
}