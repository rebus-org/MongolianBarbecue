using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongolianBarbecue
{
    /// <summary>
    /// Represents a Mongolian Barbecue configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Indicates the default message lease timeout in seconds
        /// </summary>
        public const int DefaultMessageLeaseSeconds = 60;

        /// <summary>
        /// Indicates the default max level of parallelism allowed (i.e. allowed number of concurrent async <see cref="Task"/>-based 
        /// operations, constrained within one <see cref="Producer"/> or <see cref="Consumer"/> instance)
        /// </summary>
        const int DefaultMaxParallelism = 20;

        /// <summary>
        /// Creates the configuration using the given MongoDB URL (which must contain a database name) and <paramref name="collectionName"/>.
        /// Optionally specifies the default lease time in seconds by setting <paramref name="defaultMessageLeaseSeconds"/> (default is <see cref="DefaultMessageLeaseSeconds"/>).
        /// Optionally specifies the default max parallelism by setting <paramref name="maxParallelism"/> (default is <see cref="DefaultMaxParallelism"/>).
        /// </summary>
        public Config(MongoUrl mongoUrl, string collectionName, int defaultMessageLeaseSeconds = DefaultMessageLeaseSeconds, int maxParallelism = DefaultMaxParallelism)
        {
            if (mongoUrl == null) throw new ArgumentNullException(nameof(mongoUrl));

            if (string.IsNullOrWhiteSpace(mongoUrl.DatabaseName))
            {
                throw new ArgumentException("The MongoDB URL does not contain a database name");
            }

            if (defaultMessageLeaseSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultMessageLeaseSeconds), defaultMessageLeaseSeconds, "Please specify a positive number of seconds for the lease duration");
            }

            var mongoDatabase = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);

            Collection = mongoDatabase.GetCollection<BsonDocument>(collectionName);
            MaxParallelism = maxParallelism;
            DefaultMessageLease = TimeSpan.FromSeconds(defaultMessageLeaseSeconds);
        }

        /// <summary>
        /// Creates the configuration using the given MongoDB database and <paramref name="collectionName"/>.
        /// Optionally specifies the default lease time in seconds by setting <paramref name="defaultMessageLeaseSeconds"/> (default is <see cref="DefaultMessageLeaseSeconds"/>).
        /// Optionally specifies the default max parallelism by setting <paramref name="maxParallelism"/> (default is <see cref="DefaultMaxParallelism"/>).
        /// </summary>
        public Config(IMongoDatabase database, string collectionName, int defaultMessageLeaseSeconds = DefaultMessageLeaseSeconds, int maxParallelism = DefaultMaxParallelism)
        {
            if (defaultMessageLeaseSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(defaultMessageLeaseSeconds), defaultMessageLeaseSeconds, "Please specify a positive number of seconds for the lease duration");
            }

            MaxParallelism = maxParallelism;
            Collection = database?.GetCollection<BsonDocument>(collectionName) ?? throw new ArgumentNullException(nameof(database));
            DefaultMessageLease = TimeSpan.FromSeconds(defaultMessageLeaseSeconds);
        }

        /// <summary>
        /// Creates a producer using the current configuration
        /// </summary>
        public Producer CreateProducer() => new Producer(this);

        /// <summary>
        /// Creates a consumer using the current configuration and the given <paramref name="queueName"/>
        /// </summary>
        public Consumer CreateConsumer(string queueName) => new Consumer(this, queueName);

        internal IMongoCollection<BsonDocument> Collection { get; }

        internal int MaxParallelism { get; }

        internal TimeSpan DefaultMessageLease { get; }
    }
}