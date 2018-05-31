using System;
#if HAS_SERIALIZABLE_ATTRIBUTE
using System.Runtime.Serialization;
#endif

namespace MongolianBarbecue.Exceptions
{
    /// <summary>
    /// Exception thrown when a message with an explicitly set ID is sent, and a message already exists with that ID
    /// </summary>
#if HAS_SERIALIZABLE_ATTRIBUTE
    [Serializable]
#endif
    public class UniqueMessageIdViolationException : Exception
    {
        /// <summary>
        /// Gets the problematic ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Constructs the exception with the given ID, generating a sensible message at the same time
        /// </summary>
        public UniqueMessageIdViolationException(string id) : base($"Cannot send message with ID {id} because a message already exists with that ID")
        {
            Id = id;
        }

#if HAS_APPDOMAINS
        protected UniqueMessageIdViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
#endif
    }
}