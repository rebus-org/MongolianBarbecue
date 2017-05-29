using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongolianBarbecue
{
    public class Config
    {
        public Config(IMongoDatabase database, string collectionName, int defaultMessageLeaseSeconds = 60, int maxParallelism = 20)
        {
            if (defaultMessageLeaseSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultMessageLeaseSeconds), defaultMessageLeaseSeconds, "Please specify a positive number of seconds for the lease duration");
            }

            MaxParallelism = maxParallelism;
            Collection = database.GetCollection<BsonDocument>(collectionName);
            DefaultMessageLeaseDuration = TimeSpan.FromSeconds(defaultMessageLeaseSeconds);
        }

        internal IMongoCollection<BsonDocument> Collection { get; }

        internal int MaxParallelism { get; }

        internal TimeSpan DefaultMessageLeaseDuration { get; }
    }
}