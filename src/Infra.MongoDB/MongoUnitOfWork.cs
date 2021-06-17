using Domain;
using Infra.Eevents;
using Infra.Events;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;
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

        public Task<IAsyncCursor<T>> FindCollection<T>() where T : class
        {
            return GetCollection<T>().AsQueryable().ToCursorAsync();
        }

        public Task<IAsyncCursor<T>> FindCollection<T>(Expression<Func<T, bool>> filter) where T : class
        {
            return GetCollection<T>().FindAsync(filter);
        }

        public async Task<T> FindOne<T>(Expression<Func<T, bool>> filter) where T : class
        {
            var response = await this.FindCollection(filter);

            return await response.FirstOrDefaultAsync();
        }

        public async Task<int> Save(AggregateRoot root)
        {
            var collectionName = root.GetType().Name;

            try
            {
                await Database
                    .GetCollection<AggregateRoot>(collectionName)
                    .ReplaceOneAsync(f => f.Id == root.Id, root, new ReplaceOptions
                    {
                        IsUpsert = true,
                        Collation = new Collation("en_US")
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            foreach (var item in root.UncommittedChanges)
            {
                if (item.MustPropagate)
                {
                    await _syncEventBus.Execute(item, null, CancellationToken.None);
                }
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
