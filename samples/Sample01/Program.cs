using Aota.SmppGateway.DataModel;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
using Infra.Events.Kafka;
using Infra.Queries;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Aota.SmppGateway.DataModel
{
    [Topic(Name = "smppgw")]
    public class SmppGatewayMessage : Event
    {
        public SmppGatewayMessage()
        {

        }

        public string Value { get; set; }
    }
}

namespace Sample01
{
    public class TestHandler : IMessageHandler<SmppGatewayMessage>
    {
        public Task Handle(SmppGatewayMessage @event, Dictionary<string, string> headers)
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
                    p.BootstrapServers = "192.168.203.4:30044";
                },
                consumer =>
                {
                    consumer.OffsetResetType = AutoOffsetReset.Earliest;
                    consumer.GroupId = "gw-test";
                    //consumer.Topics = new[] { typeof(SmppGatewayMessage).FullName };
                    consumer.BootstrappServers = "192.168.203.4:30044";
                    consumer.EventAssemblies = new[] { typeof(Program).Assembly };
                });
            });

        public static async Task Main(string[] args)
        {
            //await CreateHostBuilder(args).RunConsoleAsync();
            //return;
            
            var bus = new KafkaEventBus(new KafkaProducerConfig
            {
                BootstrapServers = "192.168.203.4:30044"
            });

            var dict = new Dictionary<string, string>()
            {
                { "name", "Pear" }
            };

            await bus.Execute(new SmppGatewayMessage { Value = "akbar" }, dict);

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
            //var services = new ServiceCollection().AddLogging(x => x.AddConsole());
            //var builder = new ContainerBuilder();

            //AddCommandQuery(builder, typeof(Program).Assembly);
            //var provider = builder.Build();
            //var processor = provider.Resolve<ICommandProcessor>();
            //var result = processor.ExecuteAsync<TestCommand, string>(new TestCommand()).Result;
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
                   typeof(QueryLoggerDecorator<,>),
                   typeof(IQueryHandler<,>),
               fromKey: "2",
               toKey: "3")
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
                   typeof(CommandLoggerDecorator<,>),
                   typeof(ICommandHandler<,>),
                       fromKey: "2",
                       toKey: "3")
                       .InstancePerLifetimeScope();

            return builder;
        }
    }
}
