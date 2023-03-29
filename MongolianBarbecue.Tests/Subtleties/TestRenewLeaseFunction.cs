using System;
using System.Text;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral

namespace MongolianBarbecue.Tests.Subtleties;

[TestFixture]
public class TestRenewLeaseFunction : FixtureBase
{
    Producer _producer;
    Consumer _consumer;

    protected override void SetUp()
    {
        var database = GetCleanTestDatabase();

        var config = new Config(
            database,
            "messages",
            defaultMessageLeaseSeconds: 2
        );

        _producer = config.CreateProducer();
        _consumer = config.CreateConsumer("queue-a");
    }

    [Test]
    public async Task LeaseExpiresWithoutRenewal()
    {
        const string payload = "This is the shit";
        var encoding = Encoding.UTF8;
        var message = new Message(encoding.GetBytes(payload));
            
        await _producer.SendAsync("queue-a", message);

        var receivedMessage = await _consumer.GetNextAsync();

        Assert.That(receivedMessage, Is.Not.Null);
        Assert.That(encoding.GetString(receivedMessage.Body), Is.EqualTo(payload));

        var messageId = receivedMessage.MessageId;

        // just for kicks: check that the queue is empty now
        Assert.That(await _consumer.GetNextAsync(), Is.Null);

        // wait until lease expires
        await Task.Delay(TimeSpan.FromSeconds(2.5));

        // check that we now receive the same message again
        var receivedMessageCopy = await _consumer.GetNextAsync();

        Assert.That(receivedMessageCopy, Is.Not.Null);
        Assert.That(encoding.GetString(receivedMessageCopy.Body), Is.EqualTo(payload));
        Assert.That(receivedMessageCopy.MessageId, Is.EqualTo(messageId));
    }

    [Test]
    public async Task LeaseDoesNotExpireWhenPeriodicallyRenewed()
    {
        const string payload = "This is the shit";
        var encoding = Encoding.UTF8;
        var message = new Message(encoding.GetBytes(payload));
            
        await _producer.SendAsync("queue-a", message);

        var receivedMessage = await _consumer.GetNextAsync();

        Assert.That(receivedMessage, Is.Not.Null);
        Assert.That(encoding.GetString(receivedMessage.Body), Is.EqualTo(payload));

        // renew it for a while
        await Task.Delay(TimeSpan.FromSeconds(1));
        await receivedMessage.Renew();
        await Task.Delay(TimeSpan.FromSeconds(1));
        await receivedMessage.Renew();
        await Task.Delay(TimeSpan.FromSeconds(1));
        await receivedMessage.Renew();
        await Task.Delay(TimeSpan.FromSeconds(1));

        // now we have clearly gone beyond the 2.5 s default lease time - check that the message is still invisible
        Assert.That(await _consumer.GetNextAsync(), Is.Null);
    }
}