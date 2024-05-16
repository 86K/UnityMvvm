

using System;

namespace Fusion.Mvvm
{
    public interface IThreadExecutor
    {
        IAsyncResult Execute(Action action);

        IAsyncResult<TResult> Execute<TResult>(Func<TResult> func);

        IAsyncResult Execute(Action<IPromise> action);

        IProgressResult<TProgress> Execute<TProgress>(Action<IProgressPromise<TProgress>> action);

        IAsyncResult<TResult> Execute<TResult>(Action<IPromise<TResult>> action);

        IProgressResult<TProgress, TResult> Execute<TProgress, TResult>(Action<IProgressPromise<TProgress, TResult>> action);
    }
}
