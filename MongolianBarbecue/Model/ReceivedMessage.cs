using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongolianBarbecue.Internals;

namespace MongolianBarbecue.Model
{
    /// <summary>
    /// Represents a received message, which is just a <see cref="Message"/> with the added capabilities to be ACKed or NACKed
    /// </summary>
    public class ReceivedMessage : Message
    {
        readonly Func<Task> _ack;
        readonly Func<Task> _nack;
        readonly Func<Task> _renew;

        /// <summary>
        /// Creates the message
        /// </summary>
        public ReceivedMessage(Dictionary<string, string> headers, byte[] body, Func<Task> ack, Func<Task> nack, Func<Task> renew) : base(headers, body)
        {
            _ack = ack ?? throw new ArgumentNullException(nameof(ack));
            _nack = nack ?? throw new ArgumentNullException(nameof(nack));
            _renew = renew ?? throw new ArgumentNullException(nameof(renew));

            if (!headers.TryGetValue(Fields.MessageId, out var messageId))
            {
                throw new ArgumentException($"ReceivedMessage model requires that the '{Fields.MessageId}' header is present");
            }

            MessageId = messageId;
        }

        /// <summary>
        /// Gets the message ID
        /// </summary>
        public string MessageId { get; }

        /// <summary>
        /// ACKs the message (deleting it from the underlying storage)
        /// </summary>
        public Task Ack() => _ack();

        /// <summary>
        /// NACKs the message (making it visible again to other consumers)
        /// </summary>
        public Task Nack() => _nack();

        /// <summary>
        /// Renews the lease for this message, prolonging the time it stays invisible to other consumers
        /// </summary>
        /// <returns></returns>
        public Task Renew() => _renew();
    }
}