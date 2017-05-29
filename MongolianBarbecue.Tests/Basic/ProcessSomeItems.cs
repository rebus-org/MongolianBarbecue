using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class ProcessSomeItems : FixtureBase
    {
        const string QueueName = "destination-queue";

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
        public async Task QueueStartsOutEmpty()
        {
            Assert.That(await _consumer.GetNextAsync(), Is.Null);
        }

        [Test]
        public async Task CanSendSingleItem()
        {
            await _producer.SendAsync(QueueName, new Message(new byte[] { 1, 2, 3 }));

            var message = await _consumer.GetNextAsync();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.Body, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }

        [Test]
        public async Task DoesNotReceiveSameItemAgain()
        {
            await _producer.SendAsync(QueueName, new Message(new byte[] { 1, 2, 3 }));

            await _consumer.GetNextAsync();

            var message = await _consumer.GetNextAsync();

            Assert.That(message, Is.Null);
        }


        [TestCase(3000)]
        public async Task MoreMessages(int count)
        {
            var strings = Enumerable.Range(0, count)
                .Select(i => $"This is message {i:0000000}")
                .ToList();

            await Task.WhenAll(strings
                .Select(str => _producer.SendAsync(QueueName, new Message(Encoding.UTF8.GetBytes(str)))));

            var receivedStrings = new List<string>();

            while (true)
            {
                var nextMessage = await _consumer.GetNextAsync();

                if (nextMessage == null) break;

                receivedStrings.Add(Encoding.UTF8.GetString(nextMessage.Body));
            }

            strings.Sort();
            receivedStrings.Sort();

            Assert.That(receivedStrings, Is.EqualTo(strings));
        }
    }
}