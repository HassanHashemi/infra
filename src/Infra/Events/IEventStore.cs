using Domain;
using System.Threading.Tasks;

namespace Infra.Events
{
    public interface IEventStore
    {
        Task Commit(AggregateRoot root);
    }
}
