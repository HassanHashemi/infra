using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infra.MongoDB
{
    public class MongoGenericRepo<T>
    {
        private readonly IMongoCollection<T> _set;
        private readonly IQueryable<T> _querySet;

        public MongoGenericRepo(IMongoDatabase database)
        {
            _set = database.GetCollection<T>(typeof(T).Name);
            _querySet = _set.AsQueryable();
        }

        public IQueryable<T> Query => _querySet;
        public Type ElementType => _querySet.ElementType;
        public Expression Expression => _querySet.Expression;
        public IQueryProvider Provider => _querySet.Provider;
        public Task Add(T item) => _set.InsertOneAsync(item);
        public Task AddRange(T[] items) => _set.InsertManyAsync(items);
        public IEnumerator<T> GetEnumerator() => _querySet.GetEnumerator();
    }
}
