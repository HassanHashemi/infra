using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Common.Decorators;
using Infra.Eevents;
using Infra.EFCore;
using Infra.Events;
using Infra.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Infra.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task DomainEvent_WhenAggregateRootSaved_ShouldUpdateAggregateRootWithDomainEvent()
    {
        //Arrange
        var provider = new ContainerBuilder()
            .AddDbContextInternal()
            .AddEventBusInternal()
            .AddCommandQueryInternal()
            .Build();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync();
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Save(aggregateRoot);

        //AssertS
        var dbContext = provider.Resolve<TestDbContext>();
        while (true)
        {
            var testAggregateRoot = await dbContext.TestAggregateRoots.FirstAsync();
            if (!string.IsNullOrWhiteSpace(testAggregateRoot.Title))
            {
                Assert.True(testAggregateRoot.Index == 1);
                break;
            }
        }
    }
}

internal static class ServiceExtension
{
    internal static IConfiguration Configuration { get; private set; }

    internal static ContainerBuilder AddDbContextInternal(this ContainerBuilder builder)
    {
        var services = new ServiceCollection();

        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "TestDb"));
        
        services.AddScoped<DbContext, TestDbContext>();
        
        builder.Populate(services);

        return builder;
    }

    internal static ContainerBuilder AddEventBusInternal(this ContainerBuilder builder)
    {
        builder.AddSyncEventBus();

        return builder;
    }

    internal static ContainerBuilder AddCommandQueryInternal(this ContainerBuilder builder)
    {
        var scannedAssemblies = new[]
        {
            typeof(TestAggregateRootInfoUpdatedDomainEvent).Assembly
        };

        builder.Register<IUnitOfWork>(context =>
            {
                var db = context.Resolve<DbContext>();
                var logger = context.Resolve<ILogger<EfUnitOfWork>>();
                var syncEventBus = context.Resolve<SyncEventBus>();
                var eventBus = context.Resolve<IEventBus>();

                return new EfUnitOfWork(db, eventBus, syncEventBus, logger);
            })
            .InstancePerLifetimeScope();

        builder.AddSyncEventHandlers(scannedAssemblies);
        builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);

        return builder;
    }
}
