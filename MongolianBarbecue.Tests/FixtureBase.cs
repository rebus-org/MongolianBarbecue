using System;
using System.Collections;
using System.Collections.Concurrent;
using MongoDB.Driver;
using NUnit.Framework;
using Tababular;
using Testcontainers.MongoDb;

namespace MongolianBarbecue.Tests;

public abstract class FixtureBase
{
    const string DatabaseName = "mongobbq";

    static readonly Lazy<MongoDbContainer> Container = new(() =>
    {
        var container = new MongoDbBuilder("mongo:8.0").Build();

        container.StartAsync().GetAwaiter().GetResult();

        return container;
    });

    static readonly TableFormatter Formatter = new(new Hints { CollapseVerticallyWhenSingleLine = true });

    protected void PrintTable(IEnumerable objects)
    {
        Console.WriteLine(Formatter.FormatObjects(objects));
    }

    protected IMongoDatabase GetCleanTestDatabase()
    {
        var connectionString = Container.Value.GetConnectionString();

        Console.WriteLine($"Getting clean test database '{DatabaseName}' at '{connectionString}'");

        var mongoClient = new MongoClient(connectionString);

        mongoClient.DropDatabase(DatabaseName);

        var database = mongoClient.GetDatabase(DatabaseName);

        return database;
    }

    readonly ConcurrentStack<IDisposable> _disposables = new();

    protected void CleanUpDisposables()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }

    protected TDisposable Using<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
    {
        _disposables.Push(disposable);
        return disposable;
    }

    [SetUp]
    public void InternalSetUp()
    {
        SetUp();
    }

    protected virtual void SetUp()
    {
    }

    [TearDown]
    public void InternalTearDown()
    {
        CleanUpDisposables();
    }
}