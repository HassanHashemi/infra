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

    public void UpdateInfo(string title, int index, bool mustPropagate = false)
    {
        Guard.NotNullOrEmpty(title, nameof(title));

        var @event = new TestInfoUpdatedEvent(this.Id, title, index)
        {
            MustPropagate = mustPropagate,
            Timestamp = DateTime.Now
        };

        ApplyChange(@event);
    }

    private void Apply(TestInfoUpdatedEvent @event)
    {
        this.Index += @event.Index;
        this.Title = @event.Title;
    }
}