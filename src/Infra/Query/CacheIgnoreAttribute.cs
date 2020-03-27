using System;

namespace Infra.Queries
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CacheKeyIgnoreAttribute : Attribute
    {
    }
}
