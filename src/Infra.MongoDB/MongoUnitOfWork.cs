using Domain;
using Infra.Eevents;
using Infra.Events;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.MongoDB
{
    public sealed class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IEventBus _eventBus;
        private readonly SyncEventBus _syncEventBus;
        private readonly ILogger<MongoUnitOfWork> _logger;

        public MongoUnitOfWork()
        {
        }

        public MongoUnitOfWork(
           IMongoDatabase database,
           SyncEventBus syncEventBus,
           ILogger<MongoUnitOfWork> logger) : this(database, null, syncEventBus, logger)
        {
        }

        public MongoUnitOfWork(
            IMongoDatabase database,
            IEventBus eventBus,
            SyncEventBus syncEventBus,
            ILogger<MongoUnitOfWork> logger)
        {
            _logger = logger;
            _eventBus = eventBus;
            _syncEventBus = syncEventBus;
            Database = database;
        }

        public IMongoDatabase Database { get; private set; }
        public MongoGenericRepo<T> GenericRepo<T>() where T : class => new(Database);
        public IMongoCollection<TDoc> GetCollection<TDoc>(MongoCollectionSettings settings = null) where TDoc : class
        {
            return Database.GetCollection<TDoc>(typeof(TDoc).Name, settings);
        }

        public async Task<int> Save(AggregateRoot root)
        {
            var collectionName = root.GetType().Name;

            try
            {
                await Database.GetCollection<AggregateRoot>(collectionName)
                    .ReplaceOneAsync(f => f.Id == root.Id, root, new ReplaceOptions
                    {
                        IsUpsert = true
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            foreach (var item in root.UncommittedChanges)
            {
                await _syncEventBus.Execute(item, null, CancellationToken.None);
            }

            foreach (var item in root.UncommittedChanges)
            {
                await DispatchEvents(item);
            }

            return 1;
        }

        private async Task DispatchEvents(Event item)
        {
            if (_eventBus == null)
            {
                return;
            }

            try
            {
                await _eventBus.Execute(item, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
