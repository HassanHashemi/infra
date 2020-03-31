using Domain;
using Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Infra.EFCore
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly SyncEventBus _localEventBus;
        private readonly ILogger<EfUnitOfWork> _logger;

        public EfUnitOfWork(
            DbContext context,
            SyncEventBus localBus,
            ILogger<EfUnitOfWork> logger)
        {
            _logger = logger;
            _localEventBus = localBus;

            Context = context;
        }

        public DbContext Context { get; }
        public IGenericRepository<T> Repo<T>() where T : class => new EfGenericRepo<T>(Context);

        public async Task<int> Save(AggregateRoot root)
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

            foreach (var item in root.UncommittedChanges)
            {
                await TryExecuteLocal(item);
            }

            return rowCount;
        }

        private async Task TryExecuteLocal(Event item)
        {
            if (this._localEventBus == null)
            {
                return;
            }

            try
            {
                await this._localEventBus.Execute(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public Task<int> Save() => Context.SaveChangesAsync();
    }
}
