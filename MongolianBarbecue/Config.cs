using MongoDB.Bson;
using MongoDB.Driver;

namespace MongolianBarbecue
{
    public class Config
    {
        readonly IMongoCollection<BsonDocument> _collection;

        public Config(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<BsonDocument>(collectionName);
        }

        internal IMongoCollection<BsonDocument> Collection => _collection;
    }
}