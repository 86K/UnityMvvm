using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Fusion.Mvvm
{
    public interface ITime
    {
        float Time { get; }
    }

    public class CoroutineScheduledExecutor : AbstractExecutor, IScheduledExecutor
    {
        private readonly ComparerImpl<IDelayTask> comparer = new ComparerImpl<IDelayTask>();
        private readonly List<IDelayTask> queue = new List<IDelayTask>();
        private bool running;
        public CoroutineScheduledExecutor() : this(false)
        {
        }

        public CoroutineScheduledExecutor(bool timeScaled)
        {
            Time = timeScaled ? new ScaledTime() : (ITime)new UnscaledTime();
        }

        public CoroutineScheduledExecutor(ITime time)
        {
            Time = time != null ? time : new UnscaledTime();
        }

        internal ITime Time { get; private set; }

        private void Add(IDelayTask task)
        {
            queue.Add(task);
            queue.Sort(comparer);
        }

        private bool Remove(IDelayTask task)
        {
            if (queue.Remove(task))
            {
                queue.Sort(comparer);
                return true;
            }
            return false;
        }

        public void Start()
        {
            if (running)
                return;

            running = true;

            InterceptableEnumerator ie = InterceptableEnumerator.Create(DoStart());
            ie.RegisterCatchBlock(e => { running = false; });
            Executors.RunOnCoroutineNoReturn(ie);
        }

        protected virtual IEnumerator DoStart()
        {
            while (running)
            {
                while (running && (queue.Count <= 0 || queue[0].Delay.Ticks > 0))
                {
                    yield return null;
                }

                if (!running)
                    yield break;

                IDelayTask task = queue[0];
                queue.RemoveAt(0);
                task.Run();
            }
        }

        public void Stop()
        {
            if (!running)
                return;

            running = false;
            List<IDelayTask> list = new List<IDelayTask>(queue);
            foreach (IDelayTask task in list)
            {
                if (task != null && !task.IsDone)
                    task.Cancel();
            }
            queue.Clear();
        }

        protected virtual void Check()
        {
            if (!running)
                throw new RejectedExecutionException("The ScheduledExecutor isn't started.");
        }

        public virtual IAsyncResult Schedule(Action command, long delay)
        {
            return Schedule(command, new TimeSpan(delay * TimeSpan.TicksPerMillisecond));
        }

        public virtual IAsyncResult Schedule(Action command, TimeSpan delay)
        {
            Check();
            return new OneTimeDelayTask(this, command, delay);
        }

        public virtual IAsyncResult<TResult> Schedule<TResult>(Func<TResult> command, long delay)
        {
            return Schedule(command, new TimeSpan(delay * TimeSpan.TicksPerMillisecond));
        }

        public virtual IAsyncResult<TResult> Schedule<TResult>(Func<TResult> command, TimeSpan delay)
        {
            Check();
            return new OneTimeDelayTask<TResult>(this, command, delay);
        }

        public virtual IAsyncResult ScheduleAtFixedRate(Action command, long initialDelay, long period)
        {
            return ScheduleAtFixedRate(command, new TimeSpan(initialDelay * TimeSpan.TicksPerMillisecond), new TimeSpan(period * TimeSpan.TicksPerMillisecond));
        }

        public virtual IAsyncResult ScheduleAtFixedRate(Action command, TimeSpan initialDelay, TimeSpan period)
        {
            Check();
            return new FixedRateDelayTask(this, command, initialDelay, period);
        }

        public virtual IAsyncResult ScheduleWithFixedDelay(Action command, long initialDelay, long delay)
        {
            return ScheduleWithFixedDelay(command, new TimeSpan(initialDelay * TimeSpan.TicksPerMillisecond), new TimeSpan(delay * TimeSpan.TicksPerMillisecond));
        }

        public virtual IAsyncResult ScheduleWithFixedDelay(Action command, TimeSpan initialDelay, TimeSpan delay)
        {
            Check();
            return new FixedDelayDelayTask(this, command, initialDelay, delay);
        }

        public virtual void Dispose()
        {
            Stop();
        }



        interface IDelayTask : IAsyncResult
        {
            TimeSpan Delay { get; }

            void Run();
        }

        class OneTimeDelayTask : AsyncResult, IDelayTask
        {
            private readonly long startTime;
            private TimeSpan delay;
            private readonly Action command;
            private readonly CoroutineScheduledExecutor executor;
            private readonly ITime time;
            public OneTimeDelayTask(CoroutineScheduledExecutor executor, Action command, TimeSpan delay)
            {
                time = executor.Time;
                startTime = (long)(time.Time * TimeSpan.TicksPerSecond);
                this.delay = delay;
                this.executor = executor;
                this.command = command;
                this.executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(startTime + delay.Ticks - (long)(time.Time * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (IsDone)
                    return false;

                if (!executor.Remove(this))
                    return false;

                cancellationRequested = true;
                SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (IsDone)
                        return;

                    if (IsCancellationRequested)
                    {
                        SetCancelled();
                    }
                    else
                    {
                        command();
                        SetResult();
                    }
                }
                catch (Exception e)
                {
                    SetException(e);
                    Debug.LogWarning(e);
                }
            }
        }

        class OneTimeDelayTask<TResult> : AsyncResult<TResult>, IDelayTask
        {
            private readonly long startTime;
            private TimeSpan delay;
            private readonly Func<TResult> command;
            private readonly CoroutineScheduledExecutor executor;
            private readonly ITime time;

            public OneTimeDelayTask(CoroutineScheduledExecutor executor, Func<TResult> command, TimeSpan delay)
            {
                time = executor.Time;
                startTime = (long)(time.Time * TimeSpan.TicksPerSecond);
                this.delay = delay;
                this.executor = executor;
                this.command = command;
                this.executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(startTime + delay.Ticks - (long)(time.Time * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (IsDone)
                    return false;

                if (!executor.Remove(this))
                    return false;

                cancellationRequested = true;
                SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (IsDone)
                        return;

                    if (IsCancellationRequested)
                    {
                        SetCancelled();
                    }
                    else
                    {
                        SetResult(command());
                    }
                }
                catch (Exception e)
                {
                    SetException(e);
                    Debug.LogWarning(e);
                }
            }
        }

        class FixedRateDelayTask : AsyncResult, IDelayTask
        {
            private readonly long startTime;
            private TimeSpan initialDelay;
            private TimeSpan period;
            private readonly CoroutineScheduledExecutor executor;
            private readonly Action command;
            private int count;
            private readonly ITime time;
            public FixedRateDelayTask(CoroutineScheduledExecutor executor, Action command, TimeSpan initialDelay, TimeSpan period) : base()
            {
                time = executor.Time;
                startTime = (long)(time.Time * TimeSpan.TicksPerSecond);
                this.initialDelay = initialDelay;
                this.period = period;
                this.executor = executor;
                this.command = command;
                this.executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(startTime + initialDelay.Ticks + period.Ticks * count - (long)(time.Time * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (IsDone)
                    return false;

                executor.Remove(this);
                cancellationRequested = true;
                SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (IsDone)
                        return;

                    if (IsCancellationRequested)
                    {
                        SetCancelled();
                    }
                    else
                    {
                        Interlocked.Increment(ref count);
                        executor.Add(this);
                        command();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }
        }

        class FixedDelayDelayTask : AsyncResult, IDelayTask
        {
            private TimeSpan delay;
            private long nextTime;
            private readonly CoroutineScheduledExecutor executor;
            private readonly Action command;
            private readonly ITime time;
            public FixedDelayDelayTask(CoroutineScheduledExecutor executor, Action command, TimeSpan initialDelay, TimeSpan delay) : base()
            {
                time = executor.Time;
                this.delay = delay;
                this.executor = executor;
                this.command = command;
                nextTime = (long)(time.Time * TimeSpan.TicksPerSecond + initialDelay.Ticks);
                this.executor.Add(this);
            }

            public virtual TimeSpan Delay => new TimeSpan(nextTime - (long)(time.Time * TimeSpan.TicksPerSecond));

            public override bool Cancel()
            {
                if (IsDone)
                    return false;

                executor.Remove(this);
                cancellationRequested = true;
                SetCancelled();
                return true;
            }

            public virtual void Run()
            {
                try
                {
                    if (IsDone)
                        return;

                    if (IsCancellationRequested)
                    {
                        SetCancelled();
                    }
                    else
                    {
                        command();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
                finally
                {
                    if (IsCancellationRequested)
                    {
                        SetCancelled();
                    }
                    else
                    {
                        nextTime = (long)(time.Time * TimeSpan.TicksPerSecond + delay.Ticks);
                        executor.Add(this);
                    }
                }
            }
        }

        class ComparerImpl<T> : IComparer<T> where T : IDelayTask
        {
            public int Compare(T x, T y)
            {
                if (x.Delay.Ticks == y.Delay.Ticks)
                    return 0;

                return x.Delay.Ticks > y.Delay.Ticks ? 1 : -1;
            }
        }

        public class ScaledTime : ITime
        {
            public float Time => UnityEngine.Time.time;
        }

        public class UnscaledTime : ITime
        {
            public float Time => UnityEngine.Time.unscaledTime;
        }
    }
}
