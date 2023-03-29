using System.Collections.Generic;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic;

[TestFixture]
public class Processing : FixtureBase
{
    const string QueueName = "nanomsg";

    Producer _producer;
    Consumer _consumer;

    protected override void SetUp()
    {
        var database = GetCleanTestDatabase();

        var config = new Config(database, "messages");
        _producer = new Producer(config);
        _consumer = new Consumer(config, QueueName);
    }

    [Test]
    public async Task CanGetWhetherMessageExists()
    {
        var headers = new Dictionary<string, string> { { "id", "exists" } };
        var message = new Message(headers, new byte[] {1, 2, 3});

        await _producer.SendAsync(QueueName,message);

        var doesNotExistsExists = await _consumer.Exists("does-not-exist");
        var existsExists = await _consumer.Exists("exists");

        Assert.That(doesNotExistsExists, Is.False);
        Assert.That(existsExists, Is.True);
    }
}