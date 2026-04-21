using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongolianBarbecue.Internals;
using MongolianBarbecue.Model;
using Nito.AsyncEx;

namespace MongolianBarbecue;

/// <summary>
/// Represents a message consumer for a specific queue
/// </summary>
public class Consumer
{
    readonly AsyncSemaphore _semaphore;
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
        _config = config ?? throw new ArgumentNullException(nameof(config));
        QueueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
        _semaphore = new AsyncSemaphore(_config.MaxParallelism);
    }

    /// <summary>
    /// Acknowledges having processed the message with the given <paramref name="messageId"/>.
    /// This will delete the message document from the underlying MongoDB collection.
    /// </summary>
    public async Task AckAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == null) throw new ArgumentNullException(nameof(messageId));

        var collection = _config.Collection;

        using var @lock = await _semaphore.LockAsync();

        _ = await collection.DeleteOneAsync(doc => doc["_id"] == messageId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Abandons the lease for the message with the given <paramref name="messageId"/>.
    /// This will set the <see cref="Fields.ReceiveTime"/> field of the message document to <see cref="DateTime.MinValue"/>.
    /// </summary>
    public async Task NackAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == null) throw new ArgumentNullException(nameof(messageId));

        var collection = _config.Collection;

        var abandonUpdate = new BsonDocument
        {
            {"$set", new BsonDocument {{Fields.ReceiveTime, DateTime.MinValue}}}
        };

        using var @lock = await _semaphore.LockAsync();

        try
        {
            var update = new BsonDocumentUpdateDefinition<BsonDocument>(abandonUpdate);

            await collection.UpdateOneAsync(doc => doc["_id"] == messageId, update, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // this one must pass
            throw;
        }
        catch
        {
            // lease will be released eventually
        }
    }

    /// <summary>
    /// Renews the lease for the message with the given <paramref name="messageId"/>.
    /// </summary>
    public async Task RenewAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == null) throw new ArgumentNullException(nameof(messageId));

        var collection = _config.Collection;

        var renewUpdate = new BsonDocument
        {
            {"$set", new BsonDocument {{Fields.ReceiveTime, DateTime.UtcNow}}}
        };

        using var @lock = await _semaphore.LockAsync();

        try
        {
            var update = new BsonDocumentUpdateDefinition<BsonDocument>(renewUpdate);

            await collection.UpdateOneAsync(doc => doc["_id"] == messageId, update, cancellationToken: cancellationToken);
        }
        catch
        {
            // lease will be released eventually
        }
    }

    /// <summary>
    /// Gets whether a message with the given ID exists
    /// </summary>
    public async Task<bool> ExistsAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == null) throw new ArgumentNullException(nameof(messageId));

        var collection = _config.Collection;

        var criteria = new BsonDocument
        {
            {"_id", messageId }
        };

        using var @lock = await _semaphore.LockAsync();

        var definition = new BsonDocumentFilterDefinition<BsonDocument>(criteria);

        return await collection.CountDocumentsAsync(definition, cancellationToken: cancellationToken) > 0;
    }

    /// <summary>
    /// Loads the message with the given <paramref name="messageId"/>, returning null if it doesn't exist.
    /// </summary>
    public async Task<ReceivedMessage> LoadAsync(string messageId, CancellationToken cancellationToken = default)
    {
        if (messageId == null) throw new ArgumentNullException(nameof(messageId));

        var collection = _config.Collection;

        using var @lock = await _semaphore.LockAsync();
        using var cursor = await collection.FindAsync(d => d["_id"] == messageId, cancellationToken: cancellationToken);

        var document = await cursor.FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (document == null) return null;

        return GetReceivedMessage(document);
    }

    /// <summary>
    /// Gets the next available message or immediately returns null if no message was available
    /// </summary>
    public async Task<ReceivedMessage> GetNextAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var receiveTimeCriteria = new BsonDocument { { "$lt", now.Subtract(_config.DefaultMessageLease) } };

        var filter = new BsonDocumentFilterDefinition<BsonDocument>(new BsonDocument
        {
            {Fields.DestinationQueueName, QueueName},
            {Fields.ReceiveTime, receiveTimeCriteria},
            {Fields.DeliveryAttempts, new BsonDocument {{"$lt", _config.MaxDeliveryAttempts}}},
        });

        var update = new BsonDocumentUpdateDefinition<BsonDocument>(new BsonDocument
        {
            {"$set", new BsonDocument {{Fields.ReceiveTime, now}}},
            {"$inc", new BsonDocument {{Fields.DeliveryAttempts, 1}}}
        });

        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            ReturnDocument = ReturnDocument.After
        };

        var collection = _config.Collection;

        using var @lock = await _semaphore.LockAsync();

        var document = await collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);

        if (document == null) return null;

        return GetReceivedMessage(document);
    }

    ReceivedMessage GetReceivedMessage(BsonValue document)
    {
        try
        {
            var body = document[Fields.Body].AsByteArray;

            var headers = document[Fields.Headers].AsBsonArray
                .ToDictionary(value => value[Fields.Key].AsString, value => value[Fields.Value].AsString);

            var id = document["_id"].AsString;

            var deliveryCount = document[Fields.DeliveryAttempts].AsInt32;

            var message = new ReceivedMessage(
                headers: headers, body: body,
                ack: cancellationToken => AckAsync(id, cancellationToken),
                nack: cancellationToken => NackAsync(id, cancellationToken),
                renew: cancellationToken => RenewAsync(id, cancellationToken),
                deliveryCount: deliveryCount
            );
            return message;
        }
        catch (Exception exception)
        {
            throw new FormatException($"Could not read received BSON document: {document}", exception);
        }
    }
}