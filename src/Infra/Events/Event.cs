using System;

namespace Infra.Events
{
    public abstract class Event
    {
        protected Event() => this.Timestamp = DateTime.Now;

        public DateTime Timestamp { get; }
        public virtual string EventName => this.GetType().FullName;
        public virtual bool MustPropagate { get; set; }
    }
}
