using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongolianBarbecue
{
    public class ReceivedMessage : Message
    {
        readonly Func<Task> _ack;
        readonly Func<Task> _nack;

        public ReceivedMessage(byte[] body, Dictionary<string, string> headers, Func<Task> ack, Func<Task> nack) : base(body, headers)
        {
            _ack = ack ?? throw new ArgumentNullException(nameof(ack));
            _nack = nack ?? throw new ArgumentNullException(nameof(nack));
        }

        public Task Ack() => _ack();
        public Task Nack() => _nack();
    }
}