using Domain;
using System;
using Domain;

namespace Sample01
{
    public class DescriptionChanged : DomainEvent
    {
        public DescriptionChanged(Guid id, string description)
        {
            this.Id = id;
            this.Description = description;
        }

        public Guid Id { get; set; }
        public string Description { get; set; }
    }
}