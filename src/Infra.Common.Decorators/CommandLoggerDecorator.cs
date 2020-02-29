using Infra.Commands;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class CommandLoggerDecorator<TCommand, TResult> : ICommandHandler<TCommand, TResult> where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand, TResult> _inner;
        private readonly ILogger<CommandLoggerDecorator<TCommand, TResult>> _logger;

        public CommandLoggerDecorator(
            ICommandHandler<TCommand, TResult> inner,
            ILogger<CommandLoggerDecorator<TCommand, TResult>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<TResult> HandleAsync(TCommand command)
        {
            var timer = new Stopwatch();

            timer.Start();
            var result = await _inner.HandleAsync(command);
            timer.Stop();

            Log(command, timer);

            return result;
        }

        private void Log(TCommand parameters, Stopwatch timer)
        {
            if (timer.Elapsed >= TimeSpan.FromMilliseconds(500))
            {
                _logger.LogWarning($"Finished {parameters} and took {timer.Elapsed.TotalMilliseconds}");
            }
            else
            {
                _logger.LogInformation($"Finished {parameters} and took {timer.Elapsed.TotalMilliseconds}");
            }
        }
    }
}
