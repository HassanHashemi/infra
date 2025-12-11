using System;
using Domain;

namespace Sample01;

public class TestCreatedEvent : DomainEvent
{
    public TestCreatedEvent(Guid newGuid, string title) : base(newGuid)
    {
        Title = title;
        Timestamp = DateTime.Now;
    }

    public string Title { get; }

    public override bool MustPropagate => false;
}