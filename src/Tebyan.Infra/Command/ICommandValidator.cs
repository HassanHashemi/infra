using System.Threading.Tasks;

namespace Infra.Commands
{
    public interface ICommandValidator<T> where T : ICommand
    {
        ValueTask ValidateAsync(T command);
    }
}