using Domain;
using System.Threading.Tasks;

namespace Infra
{
    public interface IUnitOfWork
    {
        Task<int> Save<T>(AggregateRoot<T> root);
        Task<int> Delete(AggregateRoot root);
        Task<int> Delete<T>(AggregateRoot<T> root);
    }
}
