using Domain;
using Infra.Eevents;
using Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.EFCore
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly IEventBus _eventBus;
        private readonly SyncEventBus _syncEventBus;
        private readonly ILogger<EfUnitOfWork> _logger;

        public EfUnitOfWork()
        {
        }

        public EfUnitOfWork(
           DbContext context,
           SyncEventBus syncEventBus,
           ILogger<EfUnitOfWork> logger) : this(context, null, syncEventBus, logger)
        {
        }

        public EfUnitOfWork(
            DbContext context,
            IEventBus eventBus,
            SyncEventBus syncEventBus,
            ILogger<EfUnitOfWork> logger)
        {
            _logger = logger;
            _eventBus = eventBus;
            _syncEventBus = syncEventBus;
            Context = context;
        }

        public DbContext Context { get; }
        public IGenericRepository<T> GenericRepo<T>() where T : class => new EfGenericRepo<T>(Context);

        private async Task DispatchEvents(Event item)
        {
            if (_eventBus == null)
                return;

            if (!item.MustPropagate)
                return;

            try
            {
                await _eventBus.Execute(item, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task<int> Save() => Context.SaveChangesAsync();

        public async Task<int> Save<T>(AggregateRoot<T> root)
        {
            int rowCount;

            try
            {
                rowCount = await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            await DispatchEvents(root);

            return rowCount;
        }

        public async Task<int> Delete(AggregateRoot root)
        {
            Context.Remove(root);

            var affectedRows = await Context.SaveChangesAsync();
            await DispatchEvents(root);

            return affectedRows;
        }

        public async Task<int> Delete<T>(AggregateRoot<T> root)
        {
            Context.Remove(root);

            var affectedRows = await Context.SaveChangesAsync();
            await DispatchEvents(root);

            return affectedRows;
        }

        private async Task DispatchEvents<T>(AggregateRoot<T> root)
        {
            foreach (var item in root.UncommittedChanges)
            {
                if (item.MustPropagate)
                    await _syncEventBus.Execute(item, null, CancellationToken.None);
            }

            foreach (var item in root.UncommittedChanges)
            {
                await DispatchEvents(item);
            }
        }
    }
}
