using Autofac;
using Infra.Eevents;
using Infra.HostedServices;
using System.Reflection;

namespace Infra.Events;

public static class ContainerExtensions
{
    /// <summary>
    /// Instead of Kafka or Rabbitmq, you can register SyncEventBus as IEventBus (using C# internal events)  
    /// </summary>
    /// <param name="containerBuilder"></param>
    public static ContainerBuilder AddSyncEventBus(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<SyncEventBus>()
            .As<IEventBus>()
            .SingleInstance();

        containerBuilder
            .RegisterType<BackgroundTaskInvoker>()
            .As<IBackgroundTaskInvoker>()
            .SingleInstance();

        return containerBuilder;
    }

    public static ContainerBuilder AddSyncEventHandlers(this ContainerBuilder containerBuilder, params Assembly[] assemblies)
    {
        containerBuilder
            .RegisterType<SyncEventBus>()
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(IEventHandler<>), "1")
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        containerBuilder
            .RegisterType<BackgroundTaskInvoker>()
            .As<IBackgroundTaskInvoker>()
            .SingleInstance();

        return containerBuilder;
    }
}