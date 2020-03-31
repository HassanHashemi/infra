using System.Threading.Tasks;
using Domain;

namespace Infra
{
    public interface IUnitOfWork
    {
        Task<int> Save(AggregateRoot root);
    }
}
