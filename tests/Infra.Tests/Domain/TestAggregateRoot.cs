using System.ComponentModel.DataAnnotations;
using Domain;

namespace Infra.Tests.Domain;

public class TestAggregateRoot : AggregateRoot
{
    private TestAggregateRoot()
    {
    }
    
    public TestAggregateRoot(Guid id)
    {
        this.Id = id;
        this.CreateDate = DateTime.Now;
    }

    [MaxLength(500)]
    public string Title { get; private set; }
    
    public int Index { get; private set; }

    public void UpdateInfo(string title, int index)
    {
        Guard.NotNullOrEmpty(title, nameof(title));

        var @event = new TestAggregateRootInfoUpdatedDomainEvent(this.Id, title, index);

        ApplyChange(@event);
    }
    
    public void UpdateInfoWithMustPropogate(string title, int index)
    {
        Guard.NotNullOrEmpty(title, nameof(title));

        var @event = new TestAggregateRootInfoUpdatedDomainEvent(this.Id, title, index)
        {
            MustPropagate = true
        };

        ApplyChange(@event);
    }

    private void Apply(TestAggregateRootInfoUpdatedDomainEvent domainEvent)
    {
        this.Index += domainEvent.Index;
        this.Title = domainEvent.Title;
    }
}