using Autofac;
using Infra.Queries;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Infra.Commands
{
    public sealed class CommandProcessor : ICommandProcessor
    {
        private readonly ILifetimeScope _container;
        private readonly CommandProcessorOptions _options;

        public CommandProcessor(ILifetimeScope container, IOptions<CommandProcessorOptions> options)
        {
            if (options.Value is null)
                throw new ArgumentException("CommandProcessorOptions must be set");

            _container = container;
            _options = options.Value;
        }

        public Task<TResult> ExecuteAsync<TCommand, TResult>(TCommand command)
        {
            var handlerType = typeof(ICommandHandler<,>)
                .MakeGenericType(command.GetType(), typeof(TResult));

            dynamic handler = _container.ResolveKeyed(_options.EndServiceKey, handlerType);

            return handler.HandleAsync(command);
        }
    }
}