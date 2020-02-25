using System.Threading.Tasks;

namespace Infra.Commands
{
    public class CommandResult<T> : CommandResult
    {
        public CommandResult(bool status, T result) : base(status)
        {
            this.Result = result;
        }

        public static Task<CommandResult<T>> SuccessTask(T item) => Task.FromResult(Success(item));
        public static CommandResult<T> Success(T item) => new CommandResult<T>(true, item);
        public static new CommandResult<T> Empty() => new CommandResult<T>(true, default);
        public T Result { get; }

        public static async Task<CommandResult<int>> FromDb(Task<int> saveResult)
        {
            var result = await saveResult;
            return new CommandResult<int>(result > 0, result);
        }
    }
}
