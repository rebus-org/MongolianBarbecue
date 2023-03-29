using System;
using System.Collections;
using System.Collections.Concurrent;
using MongoDB.Driver;
using NUnit.Framework;
using Tababular;

namespace MongolianBarbecue.Tests;

public abstract class FixtureBase
{
    static readonly MongoUrl MongoUrl;

    static FixtureBase()
    {
        var connectionString = $"mongodb://localhost/mongobbq-{DateTime.Now.GetHashCode()%10000}";
            
        MongoUrl = new MongoUrl(connectionString);
    }

    static readonly TableFormatter Formatter = new TableFormatter(new Hints { CollapseVerticallyWhenSingleLine = true });

    protected void PrintTable(IEnumerable objects)
    {
        Console.WriteLine(Formatter.FormatObjects(objects));
    }

    protected IMongoDatabase GetCleanTestDatabase()
    {
        Console.WriteLine($"Getting clean test database at '{MongoUrl}'");

        var mongoClient = new MongoClient(MongoUrl);

        mongoClient.DropDatabase(MongoUrl.DatabaseName);

        var database = mongoClient.GetDatabase(MongoUrl.DatabaseName);

        return database;
    }

    readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

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