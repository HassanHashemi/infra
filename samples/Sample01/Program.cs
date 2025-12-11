using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
using Infra.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Infra.Eevents;
using Infra.Events.Rabbitmq;

namespace Sample01;

public static class Program
{
    public static async Task Main(string[] args)
    {
        //await CreateHostBuilder(args).RunConsoleAsync();
        //return;

        //var bus = new KafkaEventBus(new KafkaProducerConfig
        //{
        //    BootstrapServers = "95.17.134.21:3249"
        //});
        //await bus.Execute(new FlightOrderItemStateChanged() { Value = "123" });
        //return;
        //var dict = new Dictionary<string, string>()
        //{
        //    { "name", "Pear" }
        //};

        //await bus.Execute(new SmppGatewayMessage { Value = "akbar", Inner = new Inner("pear") }, dict);

        //var options = Options.Create(new EventStoreConfig()
        //{
        //    Host = "localhost",
        //    UserName = "admin",
        //    Password = "changeit",
        //    Port = 1113
        //});

        //var store = new DefaultEventStore(options);
        //await store.Connect();

        ////for (int i = 0; i < 25; i++)
        ////{
        ////    await Load(store);
        ////}

        //var user = new User("Hassan");
        //user.ChangeName("Hossein");
        //user.ChangeDecription("There are some text over here..");
        //user.ChangeName("Sajjad");

        //await store.Commit(user);
        var services = new ServiceCollection().AddLogging(x => x.AddConsole());

        services.Configure<FuncDecoratorOptions>(o =>
        {
            o.Handler = (e, c) => Task.FromResult(1);
        });

        services.Configure<QueryProcessorOptions>(o => o.EndServiceKey = "4");
        services.Configure<CommandProcessorOptions>(o => o.JsonSerializer = new TestSerializer());
        services.AddDistributedMemoryCache();
        services.AddMemoryCache();
        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.AddCommandQueryInternal(typeof(Program).Assembly);



        builder.AddRabbitmqInternal(
            p =>
            {
                p.Host = "localhost";
                p.UserName = "rabbitmq";
                p.Password = "rabbitmq";
                p.VirtualHost = "/";
            },
            c =>
            {
                c.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
                c.EventAssemblies = new[] { typeof(Program).Assembly };
            });

        var provider = builder.Build();
        var processor = provider.Resolve<ICommandProcessor>();
        var result = processor.ExecuteAsync<TestCommand, string>(new TestCommand()).Result;
        var queryProcessor = provider.Resolve<IQueryProcessor>();
        var cts = new CancellationTokenSource(1);
        var r = await queryProcessor.ExecuteAsync(new TestQuery(), cts.Token);
        var bus = provider.Resolve<IEventBus>();
        await bus.Execute(new SampleEvent
        {
            Value = "Test"
        }, new Dictionary<string, string>());


    }

    public static ContainerBuilder AddCommandQueryInternal(this ContainerBuilder builder, params Assembly[] scannedAssemblies)
    {
        builder.AddSyncEventHandlers(scannedAssemblies);
        builder.AddCommandQuery(commandProcessorOptions: new CommandProcessorOptions
        {
            JsonSerializer = new TestSerializer()
        }, scannedAssemblies: scannedAssemblies);

        // builder.RegisterType<SyncEventBus>()
        //     .InstancePerLifetimeScope();

        // builder
        //     .RegisterAssemblyTypes(scannedAssemblies)
        //     .AsClosedTypesOf(typeof(IEventHandler<>), "1")
        //         .AsImplementedInterfaces()
        //         .InstancePerLifetimeScope();

        // builder.RegisterType<QueryProcessor>().As<IQueryProcessor>()
        //     .InstancePerLifetimeScope();

        // builder
        //     .RegisterAssemblyTypes(scannedAssemblies)
        //     .AsClosedTypesOf(typeof(IQueryHandler<,>), "1")
        //         .AsImplementedInterfaces()
        //         .InstancePerLifetimeScope();

        // builder
        //    .RegisterGenericDecorator(
        //        typeof(CacheDecorator<,>),
        //        typeof(IQueryHandler<,>),
        //    fromKey: "1",
        //    toKey: "2")
        //    .InstancePerLifetimeScope();

        // builder
        //    .RegisterGenericDecorator(
        //        typeof(QueryFuncDecorator<,>),
        //        typeof(IQueryHandler<,>),
        //    fromKey: "2",
        //    toKey: "3")
        //    .InstancePerLifetimeScope();

        // builder
        //     .RegisterGenericDecorator(
        //         typeof(QueryLoggerDecorator<,>),
        //         typeof(IQueryHandler<,>),
        //     fromKey: "3",
        //     toKey: "4")
        //     .InstancePerLifetimeScope();

        // builder
        //     .RegisterType<CommandProcessor>()
        //     .As<ICommandProcessor>()
        //     .InstancePerLifetimeScope();

        // builder
        //     .RegisterAssemblyTypes(scannedAssemblies)
        //     .AsClosedTypesOf(typeof(ICommandHandler<,>), "1")
        //         .AsImplementedInterfaces()
        //         .InstancePerLifetimeScope();

        // builder
        //     .RegisterAssemblyTypes(scannedAssemblies)
        //     .AsClosedTypesOf(typeof(ICommandValidator<>))
        //         .AsImplementedInterfaces()
        //         .InstancePerLifetimeScope();

        // builder
        //     .RegisterGenericDecorator(
        //         typeof(ValidationCommandHandlerDecorator<,>),
        //         typeof(ICommandHandler<,>),
        //             fromKey: "1",
        //             toKey: "2")
        //             .InstancePerLifetimeScope();

        // builder
        //    .RegisterGenericDecorator(
        //        typeof(CommandFuncDecorator<,>),
        //        typeof(ICommandHandler<,>),
        //            fromKey: "2",
        //            toKey: "3")
        //            .InstancePerLifetimeScope();

        // builder
        //.RegisterGenericDecorator(
        //    typeof(CommandLoggerDecorator<,>),
        //    typeof(ICommandHandler<,>),
        //        fromKey: "3",
        //        toKey: "4")
        //        .InstancePerLifetimeScope();

        return builder;
    }
}