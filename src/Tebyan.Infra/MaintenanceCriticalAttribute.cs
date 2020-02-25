using System;

namespace Infra
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MaintenanceCriticalAttribute : Attribute
    {
        public MaintenanceCriticalAttribute(Criticiality criticiality)
        {
            Criticiality = criticiality;
        }

        public Criticiality Criticiality { get; }
        public string Description { get; set; }
    }
}
