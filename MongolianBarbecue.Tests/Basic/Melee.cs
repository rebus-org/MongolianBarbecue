using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongolianBarbecue.Model;
using MongolianBarbecue.Tests.Extensions;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Basic
{
    [TestFixture]
    public class Melee : FixtureBase
    {
        Producer _producer;
        Config _config;

        protected override void SetUp()
        {
            var database = GetCleanTestDatabase();

            _config = new Config(database, "messages");
            _producer = new Producer(_config);
        }

        [TestCase(10000, 10)]
        public async Task WreckHavoc(int count, int queues)
        {
            var stopwatch = new BetterStopwatch();

            var strings = Enumerable.Range(0, count)
                .Select(n => $"MESSAGE {n}")
                .ToList();

            var queueNames = Enumerable.Range(0, queues).Select(n => $"queue-{n}").ToList();

            stopwatch.RecordLap("Setting up data");

            await Task.WhenAll(strings.Select(str =>
            {
                var destinationQueueName = queueNames.RandomItem();
                var message = new Message(Encoding.UTF8.GetBytes(str));
                return _producer.SendAsync(destinationQueueName, message);
            }));

            stopwatch.RecordLap("Sending messages", count);

            var tasks = queueNames.Select(_config.GetAllMessagesFrom).ToArray();

            await Task.WhenAll(tasks);

            stopwatch.RecordLap("Receiving messages", count);

            var allReceivedStrings = tasks
                .SelectMany(t => t.Result.Select(m => Encoding.UTF8.GetString(m.Body)))
                .ToList();

            strings.Sort();
            allReceivedStrings.Sort();

            PrintTable(stopwatch.Laps);

            Assert.That(allReceivedStrings, Is.EqualTo(strings));
        }
    }
}