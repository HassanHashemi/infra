using System.Linq;

namespace Infra.EFCore
{
    public interface IGenericRepository<T> : IQueryable<T>
    {
        void Add(T item);
        void AddRange(T[] items);
    }
}
