using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace MongolianBarbecue
{
    public class Producer
    {
        readonly Config _config;

        public Producer(Config config)
        {
            _config = config;
        }

        public async Task SendAsync(string destinationQueueName, Message message)
        {
            if (destinationQueueName == null) throw new ArgumentNullException(nameof(destinationQueueName));
            if (message == null) throw new ArgumentNullException(nameof(message));

            var headers = BsonArray.Create(message.Headers
                .Select(kvp => new BsonDocument { { Fields.Key, kvp.Key }, { Fields.Value, kvp.Value } }));

            await _config.Collection.InsertOneAsync(new BsonDocument
            {
                {Fields.DestinationQueueName, destinationQueueName},
                {Fields.SendTime, DateTime.UtcNow},
                {Fields.ReceiveTime, DateTime.MinValue },
                {Fields.Headers, headers},
                {Fields.Body, BsonBinaryData.Create(message.Body)}
            });
        }
    }
}