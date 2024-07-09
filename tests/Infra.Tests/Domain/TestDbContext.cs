using Microsoft.EntityFrameworkCore;

namespace Infra.Tests.Domain;

public class TestDbContext : DbContext
{
    public DbSet<TestAggregateRoot> TestAggregateRoots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestAggregateRoot>().HasData(new TestAggregateRoot());
        base.OnModelCreating(modelBuilder);
    }
}