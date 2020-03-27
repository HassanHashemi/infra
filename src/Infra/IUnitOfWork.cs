using System.Threading.Tasks;
using Domain;

namespace Infra.Events
{
    public interface IUnitOfWork
    {
        Task SaveAsync(AggregateRoot root);
    }
}
