using Domain;

namespace Infra.Tests.Domain;

public class TestInfoUpdatedEvent : DomainEvent
{
    public TestInfoUpdatedEvent(Guid aggregateRootId, string title, int index) 
        : base(aggregateRootId)
    {
        Title = title;
        Index = index;
        Timestamp = DateTime.Now;
    }

    public string Title { get; set; }
    public int Index { get; set; }

    public override bool ForceAsync => true;
    public override bool MustPropagate => true;
}