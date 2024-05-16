

using System;

namespace Fusion.Mvvm
{
    /// <summary>
    /// Event args for the <see cref="IInteractionRequest.Raised"/> event.
    /// </summary>
    public class InteractionEventArgs : EventArgs
    {
        private readonly object context;

        private readonly Action callback;

        /// <summary>
        /// Constructs a new instance of <see cref="InteractionEventArgs"/>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callback"></param>
        public InteractionEventArgs(object context, Action callback)
        {
            this.context = context;
            this.callback = callback;
        }

        /// <summary>
        /// Gets the context for a requested interaction.
        /// </summary>
        public object Context => context;

        /// <summary>
        /// Gets the callback to execute when an interaction is completed.
        /// </summary>
        public Action Callback => callback;
    }
}
