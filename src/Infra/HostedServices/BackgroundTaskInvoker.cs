using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;

namespace Infra.HostedServices;

public class BackgroundTaskInvoker : IBackgroundTaskInvoker
{
    private readonly ILifetimeScope _scope;
    private readonly ILogger<BackgroundTaskInvoker> _logger;

    public BackgroundTaskInvoker(ILifetimeScope scope, ILogger<BackgroundTaskInvoker> logger)
    {
        this._scope = scope;
        this._logger = logger;
    }

    public async void Execute(Func<ILifetimeScope, Task> func)
    {
        try
        {
            await func(this._scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}