using System.Threading.Tasks;

namespace Infra
{
    public interface IExecutionContext<TUser> where TUser : class
    {
        ValueTask<TUser> User();
    }
}
