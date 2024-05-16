

using System;

namespace Fusion.Mvvm
{
    public class MessageBase : EventArgs
    {
        public MessageBase(object sender)
        {
            Sender = sender;
        }

        /// <summary>
        /// Gets or sets the message's sender.
        /// </summary>
        public object Sender { get; protected set; }
    }
}
