using Infra.Queries;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public class QueryLoggerDecorator<TQuery, TResult>
        : IQueryHandler<TQuery, TResult> where TQuery : IQueryResult<TResult>
    {
        private readonly ILogger<QueryLoggerDecorator<TQuery, TResult>> _logger;
        private readonly IQueryHandler<TQuery, TResult> _inner;

        public QueryLoggerDecorator(ILogger<QueryLoggerDecorator<TQuery, TResult>> logger, IQueryHandler<TQuery, TResult> inner)
        {
            _logger = logger;
            _inner = inner;
        }

        public async Task<TResult> HandleAsync(TQuery parameters, CancellationToken cts)
        {
            var timer = new Stopwatch();

            timer.Start();
            var result = await _inner.HandleAsync(parameters, cts);
            timer.Stop();

            Log(parameters, timer);

            return result;
        }

        private void Log(TQuery parameters, Stopwatch timer)
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
