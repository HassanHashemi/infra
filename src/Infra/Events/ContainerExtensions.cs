using Autofac;
using System.Reflection;

namespace Infra.Events
{
    public static class ContainerExtensions
    {
        public static void AddSyncEventHandlers(this ContainerBuilder containerBuilder, params Assembly[] assemblies)
        {
            containerBuilder
                .RegisterType<SyncEventBus>()
                .InstancePerLifetimeScope();

            containerBuilder
               .RegisterAssemblyTypes(assemblies)
               .AsClosedTypesOf(typeof(IEventHandler<>), "1")
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
        }
    }
}
