using Autofac;
using Infra.Commands;
using Infra.Queries;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace Infra.Common.Decorators
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddCommandQuery(
            this ContainerBuilder builder,
            CommandProcessorOptions commandProcessorOptions = null,
            QueryProcessorOptions queryProcessorOptions = null,
            FuncDecoratorOptions funcDecoratorOptions = null,
            params Assembly[] scannedAssemblies)
        {
            builder
                .RegisterInstance(Options.Create(commandProcessorOptions ?? new CommandProcessorOptions
                {
                    EndServiceKey = "4"
                }))
                .As<IOptions<CommandProcessorOptions>>();

            builder
                .RegisterInstance(Options.Create(queryProcessorOptions ?? new QueryProcessorOptions
                {
                    EndServiceKey = "4"
                }))
                .As<IOptions<QueryProcessorOptions>>();

            builder
                .RegisterInstance(Options.Create(funcDecoratorOptions ?? new FuncDecoratorOptions
                {
                    Handler = (e, c) => Task.CompletedTask
                }))
                .As<IOptions<FuncDecoratorOptions>>();

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