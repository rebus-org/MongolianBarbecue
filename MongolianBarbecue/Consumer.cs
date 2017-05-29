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

        public async Task<Message> GetNextAsync()
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

            var document = await _config.Collection.FindOneAndUpdateAsync(filter, update, options);

            if (document == null) return null;

            return new Message(document[Fields.Body].AsByteArray, document[Fields.Headers].AsBsonArray
                .ToDictionary(value => value[Fields.Key].AsString, value => value[Fields.Value].AsString));
        }
    }
}