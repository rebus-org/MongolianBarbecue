using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongolianBarbecue.Model
{
    /// <summary>
    /// Represents a received message, which is just a <see cref="Message"/> with the added capabilities to be ACKed or NACKed
    /// </summary>
    public class ReceivedMessage : Message
    {
        readonly Func<Task> _ack;
        readonly Func<Task> _nack;

        /// <summary>
        /// Creates the message
        /// </summary>
        public ReceivedMessage(Dictionary<string, string> headers, byte[] body, Func<Task> ack, Func<Task> nack) : base(headers, body)
        {
            _ack = ack ?? throw new ArgumentNullException(nameof(ack));
            _nack = nack ?? throw new ArgumentNullException(nameof(nack));
        }

        /// <summary>
        /// ACKs the message (deleting it from the underlying storage)
        /// </summary>
        public Task Ack() => _ack();

        /// <summary>
        /// NACKs the message (making it visible again to other consumers)
        /// </summary>
        public Task Nack() => _nack();
    }
}