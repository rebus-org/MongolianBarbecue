using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic;

[TestFixture]
public class AckNackTest : FixtureBase
{
    Producer _producer;
    Config _config;

    protected override void SetUp()
    {
        var database = GetCleanTestDatabase();

        _config = new Config(database, "messages");
        _producer = new Producer(_config);
    }

    [Test]
    [Ignore("should probably be like this, but it isn't right now")]
    public async Task CannotAckMessageFromAnotherConsumer()
    {
        await _producer.SendAsync("queue-a", NewMessage());
        var receivedMessage = await _config.CreateConsumer("queue-a").GetNextAsync();
        var messageId = receivedMessage.MessageId;

        var anotherConsumer = _config.CreateConsumer("queue-b");
            
        var invalidOperationException = Assert.ThrowsAsync<InvalidOperationException>(() => anotherConsumer.Ack(messageId));

        Console.WriteLine(invalidOperationException);
    }

    static Message NewMessage()
    {
        var bytes = new byte[] { 1, 2, 3 };
        return new Message(bytes);
    }
}