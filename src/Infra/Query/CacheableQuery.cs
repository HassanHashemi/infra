using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infra.Queries
{
    public abstract class CacheableQuery<TQuery, TResult> : IQueryResult<TResult>
    {
        /// <summary>
        /// Living time of the cache, Default is one day.
        /// </summary>
        public virtual DateTimeOffset? AbsoluteExpiration => DateTime.Now + TimeSpan.FromDays(1);

        /// <summary>
        /// SlidingExpiration defaults to 1 day.
        /// </summary>
        public virtual TimeSpan? SlidingExpiration => TimeSpan.FromDays(1);

        public virtual string GetKey() => $"{this.GetType().FullName}.{this.GetKeyInternal()}";

        private string GetKeyInternal()
        {
            var fields = GetFields();

            if (!fields.Any())
            {
                return this.GetType().FullName;
            }

            const int startValue = 17, multiplier = 59;
            int hashCode = startValue;

            foreach (var field in fields)
            {
                object value = field.GetValue(this);

                if (value != null)
                {
                    hashCode = ( hashCode * multiplier ) + value.GetHashCode();
                }
            }

            return hashCode.ToString();
        }

        private IEnumerable<FieldInfo> GetFields()
        {
            var t = GetType();
            var fields = new List<FieldInfo>();

            while (t != typeof(object))
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                t = t.BaseType;
            }

            return fields;
        }
    }
}
