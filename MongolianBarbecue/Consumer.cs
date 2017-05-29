using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongolianBarbecue
{
    public class Consumer
    {
        readonly Config _config;

        public string QueueName { get; }

        public Consumer(Config config, string queueName)
        {
            _config = config;
            QueueName = queueName;
        }

        public async Task<ReceivedMessage> GetNextAsync()
        {
            var receiveTimeCriteria = new BsonDocument { { "$lt", DateTime.UtcNow.Subtract(_config.DefaultMessageLeaseDuration) } };

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

            var message = new ReceivedMessage(body, headers, Delete, Abandon);

            return message;
        }
    }
}