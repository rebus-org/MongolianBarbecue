using System.Collections.Generic;

namespace MongolianBarbecue
{
    public class Message
    {
        public Dictionary<string, string> Headers { get; }
        public byte[] Body { get; }

        public Message(byte[] body, Dictionary<string, string> headers = null)
        {
            Headers = headers ?? new Dictionary<string, string>();
            Body = body;
        }
    }
}