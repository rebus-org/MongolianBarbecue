using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongolianBarbecue.Exceptions;
using MongolianBarbecue.Model;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Subtleties
{
    [TestFixture]
    public class TestDuplicateMessageIds : FixtureBase
    {
        const string QueueName = "destination-queue";

        Producer _producer;

        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();
            var config = new Config(database, "messages");
            
            _producer = new Producer(config);
        }

        [Test]
        public async Task CannotReceiveMessageMoreThanMaxDeliveryAttemptsTimes()
        {
            var headers = new Dictionary<string, string>
            {
                {"id", "known-id"}
            };
            var body = Encoding.UTF8.GetBytes("who cares");
            var message = new Message(headers, body);

            await _producer.SendAsync(QueueName, message);

            Assert.ThrowsAsync<UniqueMessageIdViolationException>(async () => await _producer.SendAsync(QueueName, message));
        }
    }
}