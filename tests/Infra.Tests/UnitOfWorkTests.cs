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

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(1));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.TestAggregateRootId == 1);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstAsync(x => x.TestAggregateRootId == 1);
            if (!string.IsNullOrWhiteSpace(testAggregateRoot.Title))
            {
                Assert.True(testAggregateRoot.Index == 1);
                break;
            }

            if (reTries <= 0)
            {
                Assert.Fail("Message was not consumed");
            }

            reTries--;
            await Task.Delay(TimeSpan.FromSeconds(0.1));
        }
    }

    [Fact]
    public async Task DomainEvent_WhenAggregateRootDeleted_ShouldDeleteAggregateRoot()
    {
        //Arrange
        var provider = new ContainerBuilder()
            .AddDbContextInternal()
            .AddEventBusInternal()
            .AddCommandQueryInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(2));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.TestAggregateRootId == 2);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Delete(aggregateRoot);

        //Assert
        var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstOrDefaultAsync(x => x.TestAggregateRootId == 2);
        Assert.True(testAggregateRoot == null);
    }
}

internal static class TestServiceExtension
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
        var internalServices = new ServiceCollection().AddLogging(x => x.AddConsole());
        builder.Populate(internalServices);

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
