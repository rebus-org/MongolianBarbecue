using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongolianBarbecue.Tests.Benchmarks;

public record ReceiveIndexTestCase(int MessageCount, int ConcumerCount, Action<IMongoIndexManager<BsonDocument>> CreateIndex);