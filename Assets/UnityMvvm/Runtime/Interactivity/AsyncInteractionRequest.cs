

using System;
using System.Threading.Tasks;

namespace Fusion.Mvvm
{
    /// <summary>
    /// Implementation of the <see cref="IInteractionRequest"/> interface.
    /// </summary>
    public class AsyncInteractionRequest : IInteractionRequest
    {
        private readonly object sender;

        public AsyncInteractionRequest() : this(null)
        {
        }

        public AsyncInteractionRequest(object sender)
        {
            this.sender = sender != null ? sender : this;
        }

        /// <summary>
        /// Fired when interaction is needed.
        /// </summary>
        public event EventHandler<InteractionEventArgs> Raised;

        /// <summary>
        /// Fires the Raised event.
        /// </summary>
        public Task Raise()
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            Raised?.Invoke(sender, new AsyncInteractionEventArgs(source, null));
            return source.Task;
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IInteractionRequest"/> interface.
    /// </summary>
    public class AsyncInteractionRequest<T> : IInteractionRequest
    {
        private readonly object sender;
        public AsyncInteractionRequest() : this(null)
        {
        }

        public AsyncInteractionRequest(object sender)
        {
            this.sender = sender != null ? sender : this;
        }

        /// <summary>
        /// Fired when interaction is needed.
        /// </summary>
        public event EventHandler<InteractionEventArgs> Raised;

        /// <summary>
        /// Fires the Raised event.
        /// </summary>
        /// <param name="context">The context for the interaction request.</param>
        public async Task<T> Raise(T context)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            Raised?.Invoke(sender, new AsyncInteractionEventArgs(source, context));
            await source.Task;
            return context;
        }
    }
}
