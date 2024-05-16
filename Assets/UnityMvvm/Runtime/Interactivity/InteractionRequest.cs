

using System;

namespace Fusion.Mvvm
{
    /// <summary>
    /// Implementation of the <see cref="IInteractionRequest"/> interface.
    /// </summary>
    public class InteractionRequest : IInteractionRequest
    {
        private static readonly InteractionEventArgs emptyEventArgs = new InteractionEventArgs(null, null);

        private readonly object sender;

        public InteractionRequest() : this(null)
        {
        }

        public InteractionRequest(object sender)
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
        public void Raise()
        {
            Raise(null);
        }

        /// <summary>
        /// Fires the Raised event.
        /// </summary>
        /// <param name="callback">The callback to execute when the interaction is completed.</param>
        public void Raise(Action callback)
        {
            var handler = Raised;
            if (handler != null)
                handler(sender, callback == null ? emptyEventArgs : new InteractionEventArgs(null, () => { if (callback != null) callback(); }));
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IInteractionRequest"/> interface.
    /// </summary>
    public class InteractionRequest<T> : IInteractionRequest
    {
        private static readonly InteractionEventArgs emptyEventArgs = new InteractionEventArgs(null, null);

        private readonly object sender;
        public InteractionRequest() : this(null)
        {
        }

        public InteractionRequest(object sender)
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
        public void Raise(T context)
        {
            Raise(context, null);
        }

        /// <summary>
        /// Fires the Raised event.
        /// </summary>
        /// <param name="context">The context for the interaction request.</param>
        /// <param name="callback">The callback to execute when the interaction is completed.</param>
        public void Raise(T context, Action<T> callback)
        {
            var handler = Raised;
            if (handler != null)
                handler(sender, (context == null && callback == null) ? emptyEventArgs : new InteractionEventArgs(context, () => { if (callback != null) callback(context); }));
        }
    }
}
