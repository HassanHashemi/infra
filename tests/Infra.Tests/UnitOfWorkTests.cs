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
        Guid primaryKey = GuidGenerator.NewGuid();
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
            .FirstAsync(x => x.Id == primaryKey);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots
                .AsNoTracking()
                .FirstAsync(x => x.Id == primaryKey);
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
        Guid primaryKey = GuidGenerator.NewGuid();
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
            .FirstAsync(x => x.Id == primaryKey);
        aggregateRoot.UpdateInfo("test", 1);
        await unitOfWork.Delete(aggregateRoot);

        //Assert
        var deletedAggregateRootExist = await db.TestAggregateRoots
            .AsNoTracking()
            .AnyAsync(a=>a.Id==primaryKey);
        Assert.False(deletedAggregateRootExist);
    }

    [Fact]
    public async Task KafkaDomainEventWithMustPropagate_WhenAggregateRootSaved_ShouldHandleIntegrationEvent()
    {
        Guid primaryKey = GuidGenerator.NewGuid();
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddKafkaEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(primaryKey));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.Id == primaryKey);
        aggregateRoot.UpdateInfoWithMustPropogate("test", 3);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstAsync(x => x.Id == primaryKey);
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
        Guid primaryKey = GuidGenerator.NewGuid();
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddRabbitmqEventBusInternal()
            .Build();

        var db = provider.Resolve<TestDbContext>();
        db.TestAggregateRoots.Add(new TestAggregateRoot(primaryKey));
        await db.SaveChangesAsync();

        //Act
        var unitOfWork = provider.Resolve<IUnitOfWork>();
        var aggregateRoot = await unitOfWork.Repo<TestAggregateRoot>().FirstAsync(x => x.Id == primaryKey);
        aggregateRoot.UpdateInfoWithMustPropogate("test", 4);
        await unitOfWork.Save(aggregateRoot);

        //Assert
        var reTries = 10;
        while (true)
        {
            var testAggregateRoot = await db.TestAggregateRoots.AsNoTracking().FirstAsync(x => x.Id == primaryKey);
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
