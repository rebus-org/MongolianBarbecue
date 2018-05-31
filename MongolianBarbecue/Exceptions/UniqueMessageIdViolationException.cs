using System;

namespace MongolianBarbecue.Exceptions
{
    public class UniqueMessageIdViolationException : Exception
    {
        public string Id { get; }

        public UniqueMessageIdViolationException(string id) : base($"Cannot send message with ID {id} because a message already exists with that ID")
        {
            Id = id;
        }
    }
}