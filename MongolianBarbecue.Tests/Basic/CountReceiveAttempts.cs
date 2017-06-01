using System.Text;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using MongolianBarbecue.Tests.Extensions;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class CountReceiveAttempts : FixtureBase
    {
        const string QueueName = "destination-queue";
        const int ClearlyCustomizedNumberOfDeliveryAttempts = 7;

        Producer _producer;
        Consumer _consumer;

        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();

            var config = new Config(database, "messages", maxDeliveryAttempts: ClearlyCustomizedNumberOfDeliveryAttempts);
            _producer = new Producer(config);
            _consumer = new Consumer(config, QueueName);
        }

        [Test]
        public async Task CannotReceiveMessageMoreThanMaxDeliveryAttemptsTimes()
        {
            await _producer.SendAsync(QueueName, new Message(Encoding.UTF8.GetBytes("hej du")));

            await ClearlyCustomizedNumberOfDeliveryAttempts.Times(async () =>
            {
                var message = await _consumer.GetNextAsync();

                if (message == null)
                {
                    throw new AssertionException("Did not expect to receive NULL this time!");
                }

                await message.Nack();
            });

            var receivedMessage = await _consumer.GetNextAsync();

            Assert.That(receivedMessage, Is.Null, $"Did not expect to receive the message after {ClearlyCustomizedNumberOfDeliveryAttempts} delivery attempts that were NACKed");
        }
    }
}