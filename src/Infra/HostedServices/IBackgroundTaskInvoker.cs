using System;
using System.Threading.Tasks;
using Autofac;

namespace Infra.HostedServices;

public interface IBackgroundTaskInvoker
{
    void Execute(Func<ILifetimeScope, Task> func);
}