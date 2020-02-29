using System.Threading.Tasks;
using Infra.Commands;

namespace Infra.Common.Decorators
{
    public class ValidationCommandHandlerDecorator<TCommand, TResult>
        : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand, TResult> _decoratedHandler;
        private readonly ICommandValidator<TCommand> _validator;

        public ValidationCommandHandlerDecorator(ICommandHandler<TCommand, TResult> decoratedHandler, ICommandValidator<TCommand> validator)
        {
            _decoratedHandler = decoratedHandler;
            _validator = validator;
        }

        public async Task<TResult> HandleAsync(TCommand command)
        {
            await _validator.ValidateAsync(command);
            return await _decoratedHandler.HandleAsync(command);
        }
    }
}