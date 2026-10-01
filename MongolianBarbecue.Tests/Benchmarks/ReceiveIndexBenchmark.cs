using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongolianBarbecue.Model;
using NUnit.Framework;

namespace MongolianBarbecue.Tests.Benchmarks;

[TestFixture]
public class ReceiveIndexBenchmark : FixtureBase
{
    const string QueueName = "destination-queue";
    const string OtherQueueName = "other-queue";
    const string CollectionName = "messages";
    const int OtherQueueMessageCount = 10000;

    IMongoCollection<BsonDocument> _collection;
    Config _config;

    protected override void SetUp()
    {
        var database = GetCleanTestDatabase();

        _config = new Config(database, CollectionName);

        // get rid of whatever the config created, so each test case can bring its own index
        _collection = database.GetCollection<BsonDocument>(CollectionName);
        _collection.Indexes.DropAll();
    }

    static IEnumerable<ReceiveIndexTestCase> GetTestCases()
    {
        var messageCounts = new[] { 1000, 5000 };
        var consumerCounts = new[] { 1, 10 };

        foreach (var messageCount in messageCounts)
        {
            foreach (var consumerCount in consumerCounts)
            {
                yield return new ReceiveIndexTestCase(messageCount, consumerCount, CreateOldIndex);
                yield return new ReceiveIndexTestCase(messageCount, consumerCount, CreateNewIndex);
            }
        }
    }

    static void CreateOldIndex(IMongoIndexManager<BsonDocument> indexes) => CreateIndex(indexes, "q", "rt", "n", "_id");

    static void CreateNewIndex(IMongoIndexManager<BsonDocument> indexes) => CreateIndex(indexes, "q", "_id", "rt", "n");

    static void CreateIndex(IMongoIndexManager<BsonDocument> indexes, params string[] fields)
    {
        var index = new BsonDocument(fields.Select(field => new BsonElement(field, 1)));

        indexes.CreateOne(new CreateIndexModel<BsonDocument>(new BsonDocumentIndexKeysDefinition<BsonDocument>(index)));
    }

    [TestCaseSource(nameof(GetTestCases))]
    public async Task ReceiveAllMessages(ReceiveIndexTestCase testCase)
    {
        var (messageCount, consumerCount, createIndex) = testCase;

        var stopwatch = new BetterStopwatch();

        createIndex(_collection.Indexes);

        var indexNames = (await _collection.Indexes.ListAsync()).ToList().Select(index => index["name"].AsString);

        stopwatch.RecordLap($"Creating index ({string.Join(", ", indexNames)})");

        var producer = _config.CreateProducer();

        // these are sent first and never received, so they sit in front of the benchmarked queue's messages in _id order
        await Task.WhenAll(Enumerable.Range(0, OtherQueueMessageCount)
            .Select(n => producer.SendAsync(OtherQueueName, new Message(Encoding.UTF8.GetBytes($"OTHER MESSAGE {n}")))));

        stopwatch.RecordLap("Sending messages to other queue", OtherQueueMessageCount);

        await Task.WhenAll(Enumerable.Range(0, messageCount)
            .Select(n => producer.SendAsync(QueueName, new Message(Encoding.UTF8.GetBytes($"MESSAGE {n}")))));

        stopwatch.RecordLap("Sending messages", messageCount);

        var receivedCount = 0;

        await Task.WhenAll(Enumerable.Range(0, consumerCount).Select(async _ =>
        {
            var consumer = _config.CreateConsumer(QueueName);

            while (true)
            {
                var message = await consumer.GetNextAsync();

                if (message == null) break;

                await message.AckAsync();

                Interlocked.Increment(ref receivedCount);
            }
        }));

        stopwatch.RecordLap($"Receiving messages ({consumerCount} consumers)", messageCount);

        PrintTable(stopwatch.Laps);

        Assert.That(receivedCount, Is.EqualTo(messageCount));
    }
}
