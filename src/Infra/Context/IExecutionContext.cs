using System.Threading.Tasks;

namespace Infra
{
    public interface IExecutionContext
    {
        ValueTask<IUser> User();
    }
}
