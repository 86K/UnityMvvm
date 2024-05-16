

using UnityEngine;
using System.Collections.Generic;


namespace Fusion.Mvvm
{
    public class CoroutineResult : AsyncResult, ICoroutinePromise
    {
        protected List<Coroutine> coroutines = new List<Coroutine>();

        public CoroutineResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }
            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            coroutines.Add(coroutine);
        }
    }

    public class CoroutineResult<TResult> : AsyncResult<TResult>, ICoroutinePromise<TResult>
    {
        protected List<Coroutine> coroutines = new List<Coroutine>();

        public CoroutineResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }
            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            coroutines.Add(coroutine);
        }
    }

    public class CoroutineProgressResult<TProgress> : ProgressResult<TProgress>, ICoroutineProgressPromise<TProgress>
    {
        protected List<Coroutine> coroutines = new List<Coroutine>();

        public CoroutineProgressResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }
            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            coroutines.Add(coroutine);
        }
    }

    public class CoroutineProgressResult<TProgress, TResult> : ProgressResult<TProgress, TResult>, ICoroutineProgressPromise<TProgress, TResult>
    {
        protected List<Coroutine> coroutines = new List<Coroutine>();

        public CoroutineProgressResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }
            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            coroutines.Add(coroutine);
        }
    }
}
