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
        //Arrange
        var provider = new ContainerBuilder()
            .AddLoggingInternal()
            .AddDbContextInternal()
            .AddUnitOfWorkInternal()
            .AddSyncEventBusInternal()
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
			.AddLoggingInternal()
			.AddDbContextInternal()
			.AddUnitOfWorkInternal()
			.AddSyncEventBusInternal()
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
