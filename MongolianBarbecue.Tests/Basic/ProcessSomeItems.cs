using System.Threading.Tasks;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class ProcessSomeItems : FixtureBase
    {
        Config _config;

        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();

            _config = new Config(database, "messages");
        }

        [Test]
        public async Task CanSendSingleItem()
        {
            const string queueName = "destination-queue";

            var producer = new Producer(_config);
            await producer.SendAsync(queueName, new Message(new byte[] { 1, 2, 3 }));

            var consumer = new Consumer(_config, queueName);
            var message = await consumer.GetNextAsync();

            Assert.That(message, Is.Not.Null);
            Assert.That(message.Body, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }
    }
}