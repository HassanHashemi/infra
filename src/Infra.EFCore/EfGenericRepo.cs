using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Infra.EFCore
{
    public class EfGenericRepo<T> : IGenericRepository<T> where T : class
    {
        private readonly DbSet<T> _set;
        private readonly IQueryable<T> _querySet;

        public EfGenericRepo(DbContext context)
        {
            _set = context.Set<T>();
            _querySet = _set.AsQueryable();
        }

        public IQueryable<T> Query => _set;
        public Type ElementType => _querySet.ElementType;
        public Expression Expression => _querySet.Expression;
        public IQueryProvider Provider => _querySet.Provider;
        public void Add(T item) => _set.Add(item);
        public void AddRange(T[] items) => _set.AddRange(items);
        public IEnumerator<T> GetEnumerator() => _querySet.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
