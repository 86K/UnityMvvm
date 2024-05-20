

using System;
using System.Threading;

namespace Fusion.Mvvm
{
    public interface ISubscription<T> : IDisposable
    {
        /// <summary>
        /// Changes the thread of message consumption.
        /// For example, sending a message to the UI thread for execution.
        /// </summary>
        /// <example>
        /// <code>
        /// messenger.Subscribe<Message>(m=>{}).ObserveOn(SynchronizationContext.Current);
        /// </code>
        /// </example>
        /// <param name="context"></param>
        /// <returns></returns>
        ISubscription<T> ObserveOn(SynchronizationContext context);
    }
}
