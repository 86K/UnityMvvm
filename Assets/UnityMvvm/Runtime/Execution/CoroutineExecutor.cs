using System;
using System.Collections;

namespace Fusion.Mvvm
{
    public class CoroutineExecutor : ExecutorBase, ICoroutineExecutor
    {
        public void RunOnCoroutineNoReturn(IEnumerator routine)
        {
            Executors.RunOnCoroutineNoReturn(routine);
        }

        public IAsyncResult RunOnCoroutine(IEnumerator routine)
        {
            return Executors.RunOnCoroutine(routine);
        }

        public IAsyncResult RunOnCoroutine(Func<IPromise, IEnumerator> func)
        {
            return Executors.RunOnCoroutine(func);
        }

        public IAsyncResult<TResult> RunOnCoroutine<TResult>(Func<IPromise<TResult>, IEnumerator> func)
        {
            return Executors.RunOnCoroutine(func);
        }

        public IProgressResult<TProgress> RunOnCoroutine<TProgress>(Func<IProgressPromise<TProgress>, IEnumerator> func)
        {
            return Executors.RunOnCoroutine(func);
        }

        public IProgressResult<TProgress, TResult> RunOnCoroutine<TProgress, TResult>(
            Func<IProgressPromise<TProgress, TResult>, IEnumerator> func)
        {
            return Executors.RunOnCoroutine(func);
        }
    }
}