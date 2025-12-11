using System;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Sample01;

public class Test : AggregateRoot
{
    public Test(string title)
    {
        var @event = new TestCreatedEvent(Guid.NewGuid(), title);

        ApplyChange(@event);
    }

    public string Title { get; private set; }


    void Apply(TestCreatedEvent @event)
    {
        Title = @event.Title;
    }
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<Test> Tests { get; set; }  // Add DbSet<Test>
}
