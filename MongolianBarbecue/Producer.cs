using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongolianBarbecue.Exceptions;
using MongolianBarbecue.Internals;
using MongolianBarbecue.Model;

namespace MongolianBarbecue
{
    /// <summary>
    /// Represents a message producer
    /// </summary>
    public class Producer
    {
        readonly Config _config;
        readonly SemaphoreSlim _semaphore;

        /// <summary>
        /// Creates the producer from the given configuration
        /// </summary>
        public Producer(Config config)
        {
            _config = config;
            _semaphore = new SemaphoreSlim(_config.MaxParallelism, _config.MaxParallelism);
        }

        /// <summary>
        /// Sends the given message to the specified queue
        /// </summary>
        public async Task SendAsync(string destinationQueueName, Message message)
        {
            if (destinationQueueName == null) throw new ArgumentNullException(nameof(destinationQueueName));
            if (message == null) throw new ArgumentNullException(nameof(message));

            if (!message.Headers.TryGetValue(Fields.MessageId, out var id))
            {
                id = Guid.NewGuid().ToString();
                message.Headers[Fields.MessageId] = id;
            }

            var headers = BsonArray.Create(message.Headers
                .Select(kvp => new BsonDocument { { Fields.Key, kvp.Key }, { Fields.Value, kvp.Value } }));

            await _semaphore.WaitAsync();

            try
            {
                await _config.Collection.InsertOneAsync(new BsonDocument
                {
                    {"_id", id},
                    {Fields.DestinationQueueName, destinationQueueName},
                    {Fields.SendTime, DateTime.UtcNow},
                    {Fields.DeliveryAttempts, 0},
                    {Fields.ReceiveTime, DateTime.MinValue},
                    {Fields.Headers, headers},
                    {Fields.Body, BsonBinaryData.Create(message.Body)}
                });
            }
            catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new UniqueMessageIdViolationException(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}