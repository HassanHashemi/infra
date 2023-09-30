using Autofac;
using Infra.Commands;
using Infra.Queries;
using System.Reflection;

namespace Infra.Common.Decorators
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddCommandQuery(this ContainerBuilder builder, params Assembly[] scannedAssemblies)
        {
            builder.RegisterType<QueryProcessor>().As<IQueryProcessor>()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(scannedAssemblies)
                .AsClosedTypesOf(typeof(IQueryHandler<,>), "1")
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

            builder
               .RegisterGenericDecorator(
                   typeof(CacheDecorator<,>),
                   typeof(IQueryHandler<,>),
               fromKey: "1",
               toKey: "2")
               .InstancePerLifetimeScope();

            builder
               .RegisterGenericDecorator(
                   typeof(QueryFuncDecorator<,>),
                   typeof(IQueryHandler<,>),
               fromKey: "2",
               toKey: "3")
               .InstancePerLifetimeScope();

            builder
                .RegisterGenericDecorator(
                    typeof(QueryLoggerDecorator<,>),
                    typeof(IQueryHandler<,>),
                fromKey: "3",
                toKey: "4")
                .InstancePerLifetimeScope();

            builder
                .RegisterType<CommandProcessor>()
                .As<ICommandProcessor>()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(scannedAssemblies)
                .AsClosedTypesOf(typeof(ICommandHandler<,>), "1")
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(scannedAssemblies)
                .AsClosedTypesOf(typeof(ICommandValidator<>))
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

            builder
                .RegisterGenericDecorator(
                    typeof(ValidationCommandHandlerDecorator<,>),
                    typeof(ICommandHandler<,>),
                        fromKey: "1",
                        toKey: "2")
                        .InstancePerLifetimeScope();

            builder
               .RegisterGenericDecorator(
                   typeof(CommandFuncDecorator<,>),
                   typeof(ICommandHandler<,>),
                       fromKey: "2",
                       toKey: "3")
                       .InstancePerLifetimeScope();

            builder
           .RegisterGenericDecorator(
               typeof(CommandLoggerDecorator<,>),
               typeof(ICommandHandler<,>),
                   fromKey: "3",
                   toKey: "4")
                   .InstancePerLifetimeScope();

            return builder;
        }
    }
}