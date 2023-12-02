using Infra.Commands;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class ValidationCommandHandlerDecorator<TCommand, TResult>
        : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand, TResult> _decoratedHandler;
        private readonly ICommandValidator<TCommand> _validator;

        public ValidationCommandHandlerDecorator(
            ICommandHandler<TCommand, TResult> decoratedHandler, 
            ICommandValidator<TCommand> validator = null)
        {
            _decoratedHandler = decoratedHandler;
            _validator = validator;
        }

        public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
        {
            if (_validator != null)
                await _validator.ValidateAsync(command);

            return await _decoratedHandler.HandleAsync(command, cancellationToken);
        }
    }
}