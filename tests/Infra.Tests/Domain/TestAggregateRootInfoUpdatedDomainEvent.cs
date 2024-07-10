using News.Domain;

namespace Infra.Tests.Domain;

public class TestAggregateRootInfoUpdatedDomainEvent : DomainEvent
{
    public TestAggregateRootInfoUpdatedDomainEvent(Guid aggregateRootId, string title, int index) : base(aggregateRootId)
    {
        Title = title;
        Index = index;
        Timestamp = DateTime.Now;
    }

    public string Title { get; set; }
    public int Index { get; set; }
}