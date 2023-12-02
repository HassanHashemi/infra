using Autofac;
using Infra.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class CommandFuncDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand, TResult> _inner;
        private readonly ILogger<CommandLoggerDecorator<TCommand, TResult>> _logger;
        private readonly FuncDecoratorOptions _options;
        private readonly ILifetimeScope _scope;

        public CommandFuncDecorator(
            ICommandHandler<TCommand, TResult> inner,
            ILogger<CommandLoggerDecorator<TCommand, TResult>> logger,
            IOptions<FuncDecoratorOptions> options,
            ILifetimeScope scope)
        {
            _inner = inner;
            _logger = logger;
            _options = options.Value;
            _scope = scope;
        }

        public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            if (_options.Handler != null)
                await _options.Handler(command, _scope);

            return await _inner.HandleAsync(command, cancellationToken);
        }
    }
}
