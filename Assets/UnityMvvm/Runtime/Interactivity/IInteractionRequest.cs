

using System;

namespace Fusion.Mvvm
{
    /// <summary>
    /// Represents a request from user interaction.
    /// </summary>
    public interface IInteractionRequest
    {
        /// <summary>
        /// Fired when the interaction is needed.
        /// </summary>
        event EventHandler<InteractionEventArgs> Raised;
    }
}
