using System;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using MongolianBarbecue.Tests.Extensions;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic;

[TestFixture]
public class SeparateQueues : FixtureBase
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
    public async Task TheQueuesAreSeparate()
    {
        await _producer.SendAsync("queue-a", new Message(new byte[] { 1, 1, 1 }));
        await _producer.SendAsync("queue-b", new Message(new byte[] { 2, 2, 2 }));
        await _producer.SendAsync("queue-c", new Message(new byte[] { 3, 3, 3 }));

        var messages = Tuple.Create(
            await _config.GetAllMessagesFrom("queue-a"),
            await _config.GetAllMessagesFrom("queue-b"),
            await _config.GetAllMessagesFrom("queue-c")
        );

        Assert.That(messages.Item1.Count, Is.EqualTo(1));
        Assert.That(messages.Item2.Count, Is.EqualTo(1));
        Assert.That(messages.Item3.Count, Is.EqualTo(1));

        Assert.That(messages.Item1[0].Body, Is.EqualTo(new byte[] { 1, 1, 1 }));
        Assert.That(messages.Item2[0].Body, Is.EqualTo(new byte[] { 2, 2, 2 }));
        Assert.That(messages.Item3[0].Body, Is.EqualTo(new byte[] { 3, 3, 3 }));
    }

}