

using System;
//using System.Collections.Concurrent;
using System.Threading;

namespace Fusion.Mvvm
{
    public abstract class SubjectBase
    {
        public abstract void Publish(object message);
    }

    public class Subject<T> : SubjectBase
    {
        private readonly ConcurrentDictionary<string, WeakReference<Subscription>> subscriptions = new ConcurrentDictionary<string, WeakReference<Subscription>>();
        public bool IsEmpty() { return subscriptions.Count <= 0; }

        public override void Publish(object message)
        {
            Publish((T)message);
        }

        public void Publish(T message)
        {
            if (subscriptions.Count <= 0)
                return;

            foreach (var kv in subscriptions)
            {
                kv.Value.TryGetTarget(out var subscription);
                if (subscription != null)
                    subscription.Publish(message);
                else
                    subscriptions.TryRemove(kv.Key, out _);
            }
        }

        public ISubscription<T> Subscribe(Action<T> action)
        {
            return new Subscription(this, action);
        }

        void Add(Subscription subscription)
        {
            var reference = new WeakReference<Subscription>(subscription, false);
            subscriptions.TryAdd(subscription.Key, reference);
        }

        void Remove(Subscription subscription)
        {
            subscriptions.TryRemove(subscription.Key, out _);
        }

        class Subscription : ISubscription<T>
        {
            private Subject<T> subject;
            private Action<T> action;
            private SynchronizationContext context;
            public string Key { get; private set; }

            public Subscription(Subject<T> subject, Action<T> action)
            {
                this.subject = subject;
                this.action = action;
                Key = Guid.NewGuid().ToString();
                this.subject.Add(this);
            }

            public void Publish(T message)
            {
                try
                {
                    if (context != null)
                        context.Post(state => action?.Invoke((T)state), message);
                    else
                        action?.Invoke(message);
                }
                catch (Exception e)
                {
#if DEBUG
                    throw;
#else
                    if (log.IsWarnEnabled)
                        log.Warn(e);
#endif
                }
            }

            public ISubscription<T> ObserveOn(SynchronizationContext context)
            {
                this.context = context ?? throw new ArgumentNullException("context");
                return this;
            }

            #region IDisposable Support
            private int disposed;

            protected virtual void Dispose(bool disposing)
            {
                try
                {
#if UNITY_WEBGL
                    if (this.disposed==1)
                        return;

                    disposed = 1;
                    if (subject != null)
                        subject.Remove(this);

                    context = null;
                    action = null;
                    subject = null;
#else
                    if (Interlocked.CompareExchange(ref disposed, 1, 0) == 0)
                    {
                        if (subject != null)
                            subject.Remove(this);

                        context = null;
                        action = null;
                        subject = null;
                    }
#endif
                }
                catch (Exception) { }
            }

            ~Subscription()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }
    }
}
