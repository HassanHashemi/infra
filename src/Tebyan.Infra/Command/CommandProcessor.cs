using Autofac;
using System.Threading.Tasks;

namespace Infra.Commands
{
    public sealed class CommandProcessor : ICommandProcessor
    {
        private readonly ILifetimeScope _container;

        public CommandProcessor(ILifetimeScope container)
        {
            _container = container;
        }

        public Task<CommandResult<TResult>> ExecuteAsync<TCommand, TResult>(TCommand command)
        {
            var handlerType = typeof(ICommandHandler<,>)
                .MakeGenericType(command.GetType(), typeof(TResult));

            dynamic handler = _container.ResolveKeyed("3", handlerType);

            return handler;
        }
    }
}