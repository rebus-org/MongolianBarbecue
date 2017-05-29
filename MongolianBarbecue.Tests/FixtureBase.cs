using System;
using System.Collections.Concurrent;
using MongoDB.Driver;
using NUnit.Framework;

namespace MongolianBarbecue.Tests
{
    public abstract class FixtureBase
    {
        protected IMongoDatabase GetCleanTestDatabase()
        {
            var mongoUrl = new MongoUrl("mongodb://localhost/mongolian-barbecue-test");

            var mongoClient = new MongoClient(mongoUrl);

            mongoClient.DropDatabase(mongoUrl.DatabaseName);

            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            
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
}