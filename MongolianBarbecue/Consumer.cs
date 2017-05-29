using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongolianBarbecue.Internals;
using MongolianBarbecue.Model;

namespace MongolianBarbecue
{
    /// <summary>
    /// Represents a message consumer for a specific queue
    /// </summary>
    public class Consumer
    {
        readonly Config _config;

        /// <summary>
        /// Gets the name of the queue
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Creates the consumer from the given configuration and queue name
        /// </summary>
        public Consumer(Config config, string queueName)
        {
            _config = config;
            QueueName = queueName;
        }

        /// <summary>
        /// Gets the next available message or immediately returns null if no message was available
        /// </summary>
        public async Task<ReceivedMessage> GetNextAsync()
        {
            var receiveTimeCriteria = new BsonDocument { { "$lt", DateTime.UtcNow.Subtract(_config.DefaultMessageLease) } };

            var filter = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
            {
                {Fields.DestinationQueueName, QueueName},
                {Fields.ReceiveTime, receiveTimeCriteria}
            });

            var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument
            {
                {"$set", new BsonDocument {{Fields.ReceiveTime, DateTime.UtcNow}}}
            });

            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After
            };

            var collection = _config.Collection;

            var document = await collection.FindOneAndUpdateAsync(filter, update, options);

            if (document == null) return null;

            var body = document[Fields.Body].AsByteArray;
            var headers = document[Fields.Headers].AsBsonArray
                .ToDictionary(value => value[Fields.Key].AsString, value => value[Fields.Value].AsString);

            var id = document["_id"].AsString;

            Task Delete() => collection.DeleteOneAsync(doc => doc["_id"] == id);

            var abandonUpdate = new BsonDocument
            {
                {"$set", new BsonDocument {{Fields.ReceiveTime, DateTime.MinValue}}}
            };

            Task Abandon() => collection.UpdateOneAsync(doc => doc["_id"] == id,
                new BsonDocumentUpdateDefinition<BsonDocument>(abandonUpdate));

            var message = new ReceivedMessage(headers, body, Delete, Abandon);

            return message;
        }
    }
}