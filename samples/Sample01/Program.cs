﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Confluent.Kafka;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
using Infra.Events.Kafka;
using Infra.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using News.Domain;
using Orders.Domain.Events;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Orders.Domain.Events
{
    public class Inner
    {
        private Inner()
        {
        }

        public Inner(string value)
        {
            Item = value;
        }

        public string Item { get; set; }
    }

    [Topic(Name = "Ota.FlightOrderItem1")]
    public class FlightOrderItemStateChanged : DomainEvent
    {
        public string Value { get; set; }
        //public SmppGatewayMessage()
        //{

        //}

        ////public SmppGatewayMessage()
        ////{

        ////}

        //public string Value { get; set; }
        //public Inner Inner { get; set; }
    }
}

public class TestCommand : ICommand
{

}

public class TestCommandHandler : ICommandHandler<TestCommand, string>
{
    public Task<string> HandleAsync(TestCommand command)
    {
        return Task.FromResult(1.ToString());
    }
}

public class TestQuery : IQueryResult<string>
{

}

public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public Task<string> HandleAsync(TestQuery parameters)
    {
        return Task.FromResult(1.ToString());
    }
}

namespace Sample01
{
    public class TestHandler : IMessageHandler<FlightOrderItemStateChanged>
    {
        public Task Handle(FlightOrderItemStateChanged @event, Dictionary<string, string> headers)
        {
            return Task.CompletedTask;
        }
    }

    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.AddKafka(p =>
                {
                    p.BootstrapServers = "91.107.239.221:30049";
                },
                consumer =>
                {
                    consumer.OffsetResetType = AutoOffsetReset.Earliest;
                    consumer.GroupId = "gw-test37";
                    //consumer.Topics = new[] { typeof(SmppGatewayMessage).FullName };
                    consumer.BootstrappServers = "91.107.239.221:30049";
                    consumer.EventAssemblies = new[] { typeof(Program).Assembly };
                    consumer.MaxPollIntervalMs = 50_000;
                    consumer.SessionTimeoutMs = 50_000;
                    consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
                    ///consumer.AutoOffsetCommit = false;,
                });
            });

        public static async Task Main(string[] args)
        {
            //await CreateHostBuilder(args).RunConsoleAsync();
            //return;

            //var bus = new KafkaEventBus(new KafkaProducerConfig
            //{
            //    BootstrapServers = "91.107.239.221:30049"
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
            services.Configure<CommandProcessorOptions>(o => o.EndServiceKey = "4");
            services.AddDistributedMemoryCache();
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.AddCommandQuery(typeof(Program).Assembly);
            var provider = builder.Build();
            var processor = provider.Resolve<ICommandProcessor>();
            //var result = processor.ExecuteAsync<TestCommand, string>(new TestCommand()).Result;
            var queryProcessor = provider.Resolve<IQueryProcessor>();
            var r = await queryProcessor.ExecuteAsync(new TestQuery());
        }

        public static ContainerBuilder AddCommandQuery(this ContainerBuilder builder, params Assembly[] scannedAssemblies)
        {
            builder.RegisterType<SyncEventBus>()
                .InstancePerLifetimeScope();

            builder
                .RegisterAssemblyTypes(scannedAssemblies)
                .AsClosedTypesOf(typeof(IEventHandler<>), "1")
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();

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
