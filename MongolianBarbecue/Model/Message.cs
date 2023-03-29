using System;
using System.Collections.Generic;

namespace MongolianBarbecue.Model;

/// <summary>
/// Represents a single message consisting of any number of string-string headers and a <code>byte[]</code> payload
/// </summary>
public class Message
{
    /// <summary>
    /// Gets the headers of the message
    /// </summary>
    public Dictionary<string, string> Headers { get; }

    /// <summary>
    /// Gets the payload of the message
    /// </summary>
    public byte[] Body { get; }

    /// <summary>
    /// Creates the message
    /// </summary>
    public Message(byte[] body) : this(new Dictionary<string, string>(), body)
    {
    }

    /// <summary>
    /// Creates the message
    /// </summary>
    public Message(Dictionary<string, string> headers, byte[] body)
    {
        Body = body ?? throw new ArgumentNullException(nameof(body));
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
    }
}