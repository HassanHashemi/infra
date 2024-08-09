using Autofac;
using Infra.EFCore;
using Infra.Tests.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infra.Tests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task DomainEvent_WhenAggregateRootSaved_ShouldUpdateAggregateRootWithDomainEvent()
    {
        int primaryKey = 1;
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddSyncEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(primaryKey));
        await db.SaveChangesAsync();
       
        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork
            .Repo<TestAggregateRoot>()
            .FirstAsync(x => x.TestAggregateRootId == primaryKey);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots
                .AsNoTracking()
                .FirstAsync(x => x.TestAggregateRootId == primaryKey);
            if (!string.IsNullOrWhiteSpace(testAggregateRoot.Title))
            {
                Assert.True(testAggregateRoot.Index == 1);
                return;
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
        int primaryKey = 2;
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddSyncEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(primaryKey));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork
            .Repo<TestAggregateRoot>()
            .FirstAsync(x => x.TestAggregateRootId == primaryKey);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Delete(aggregateRoot);

        //Assert
        var testAggregateRoot = await db.TestAggregateRoots
            .AsNoTracking()
            .FirstAsync(x => x.TestAggregateRootId == primaryKey);
        Assert.True(testAggregateRoot == null);
    }

    [Fact]
    public async Task KafkaDomainEventWithMustPropagate_WhenAggregateRootSaved_ShouldHandleIntegrationEvent()
    {
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddKafkaEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(1));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.TestAggregateRootId == 3);
        aggregateRoot.UpdateInfoWithMustPropogate("test", 3);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstAsync(x => x.TestAggregateRootId == 3);
            if (!string.IsNullOrWhiteSpace(testAggregateRoot.Title))
            {
                Assert.True(testAggregateRoot.Index == 3);
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
    public async Task RabbitmqDomainEventWithMustPropagate_WhenAggregateRootSaved_ShouldHandleIntegrationEvent()
    {
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddRabbitmqEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(1));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.TestAggregateRootId == 4);
        aggregateRoot.UpdateInfoWithMustPropogate("test", 4);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstAsync(x => x.TestAggregateRootId == 4);
            if (!string.IsNullOrWhiteSpace(testAggregateRoot.Title))
            {
                Assert.True(testAggregateRoot.Index == 4);
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
}
