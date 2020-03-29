using System.Threading.Tasks;
using Domain;

namespace Infra.Events
{
    public interface IUnitOfWork
    {
        Task<int> Save(AggregateRoot root);
    }
}
