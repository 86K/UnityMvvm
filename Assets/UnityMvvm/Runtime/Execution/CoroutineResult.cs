using UnityEngine;
using System.Collections.Generic;

namespace Fusion.Mvvm
{
    public class CoroutineResult : AsyncResult, ICoroutinePromise
    {
        private readonly List<Coroutine> _coroutines = new List<Coroutine>();

        public CoroutineResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in _coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }

            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            _coroutines.Add(coroutine);
        }
    }

    public class CoroutineResult<TResult> : AsyncResult<TResult>, ICoroutinePromise<TResult>
    {
        private readonly List<Coroutine> _coroutines = new List<Coroutine>();

        public CoroutineResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in _coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }

            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            _coroutines.Add(coroutine);
        }
    }

    public class CoroutineProgressResult<TProgress> : ProgressResult<TProgress>, ICoroutineProgressPromise<TProgress>
    {
        private readonly List<Coroutine> _coroutines = new List<Coroutine>();

        public CoroutineProgressResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in _coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }

            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            _coroutines.Add(coroutine);
        }
    }

    public class CoroutineProgressResult<TProgress, TResult> : ProgressResult<TProgress, TResult>, ICoroutineProgressPromise<TProgress, TResult>
    {
        private readonly List<Coroutine> _coroutines = new List<Coroutine>();

        public CoroutineProgressResult() : base(true)
        {
        }

        public override bool Cancel()
        {
            if (IsDone)
                return false;

            cancellationRequested = true;
            foreach (Coroutine coroutine in _coroutines)
            {
                Executors.StopCoroutine(coroutine);
            }

            SetCancelled();
            return true;
        }

        public void AddCoroutine(Coroutine coroutine)
        {
            _coroutines.Add(coroutine);
        }
    }
}