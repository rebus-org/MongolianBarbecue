using System;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using NUnit.Framework;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class LeaseTimeouts : FixtureBase
    {
        const string QueueName = "destination-queue";
        const int DefaultMessageLeaseSeconds = 3;
        const int ExtraDelay = 1;

        Producer _producer;
        Consumer _consumer;

        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();

            var config = new Config(database, "messages", defaultMessageLeaseSeconds: DefaultMessageLeaseSeconds);
            _producer = new Producer(config);
            _consumer = new Consumer(config, QueueName);
        }

        [Test]
        public async Task GetsTheSameMessageAgainWhenLeaseExpires()
        {
            var valuablePayload = new byte[] { 0xBA, 0xDA, 0x55, 0xC0, 0xFF, 0x33 };
            await _producer.SendAsync(QueueName, new Message(valuablePayload));

            var messageReceivedFirstTime = await _consumer.GetNextAsync();

            await Task.Delay(TimeSpan.FromSeconds(DefaultMessageLeaseSeconds + ExtraDelay));

            var messageReceivedSecondTime = await _consumer.GetNextAsync();

            Assert.That(messageReceivedFirstTime.Body, Is.EqualTo(valuablePayload));
            Assert.That(messageReceivedSecondTime.Body, Is.EqualTo(valuablePayload));
        }

        [Test]
        public async Task DoesNotGetTheSameMessageAgainWhenLeaseExpiresIfTheMessageWasAcked()
        {
            var valuablePayload = new byte[] { 0xBA, 0xDA, 0x55, 0xC0, 0xFF, 0x33 };
            await _producer.SendAsync(QueueName, new Message(valuablePayload));

            var messageReceivedFirstTime = await _consumer.GetNextAsync();
            await messageReceivedFirstTime.Ack();

            await Task.Delay(TimeSpan.FromSeconds(DefaultMessageLeaseSeconds + ExtraDelay));

            var messageReceivedSecondTime = await _consumer.GetNextAsync();

            Assert.That(messageReceivedFirstTime.Body, Is.EqualTo(valuablePayload));
            Assert.That(messageReceivedSecondTime, Is.Null);
        }

        [Test]
        public async Task GetsTheSameMessageAgainBeforeLeaseExpiresIfTheMessageWasNacked()
        {
            var valuablePayload = new byte[] { 0xBA, 0xDA, 0x55, 0xC0, 0xFF, 0x33 };
            await _producer.SendAsync(QueueName, new Message(valuablePayload));

            var messageReceivedFirstTime = await _consumer.GetNextAsync();
            await messageReceivedFirstTime.Nack();

            var messageReceivedSecondTime = await _consumer.GetNextAsync();

            Assert.That(messageReceivedFirstTime.Body, Is.EqualTo(valuablePayload));
            Assert.That(messageReceivedSecondTime.Body, Is.EqualTo(valuablePayload));
        }

    }
}