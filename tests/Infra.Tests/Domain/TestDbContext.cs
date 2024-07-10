using Microsoft.EntityFrameworkCore;

namespace Infra.Tests.Domain;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestAggregateRoot> TestAggregateRoots { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestAggregateRoot>().Ignore(p => p.UncommittedChanges);
        modelBuilder.Entity<TestAggregateRoot>().Ignore(p => p.Version);
        modelBuilder.Entity<TestAggregateRoot>().HasKey(p => p.TestAggregateRootId);
        modelBuilder.Entity<TestAggregateRoot>().HasIndex(p => p.TestAggregateRootId).IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}