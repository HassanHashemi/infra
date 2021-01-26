using Aota.SmppGateway.DataModel;
using Autofac;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
using Infra.Events.Kafka;
using Infra.Queries;
using Microsoft.Extensions.Hosting;
using News.Domain;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Aota.SmppGateway.DataModel
{
    public class SmppGatewayMessage : Event
    {
        public string Value { get; set; }
    }
}

namespace Sample01
{
    public class UserCreated : DomainEvent
    {
        public UserCreated(Guid userId, string fullName) : base(userId)
        {
            FullName = fullName;
            UserId = userId;
        }

        public string FullName { get; set; }
        public Guid UserId { get; set; }
    }

    public class TestHandler : MessageHandler
    {
        public TestHandler(KafkaListenerCallbacks callbacks) : base(callbacks)
        {
        }

        protected override Task MessageReceived(BusMessageReceivedArgs e)
        {
            Console.WriteLine(e);

            return Task.CompletedTask;
        }
    }

    public class LoggerService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(true)
            {
                await Task.Delay(3_000);
                Console.WriteLine("**************************************");
                Console.WriteLine("**************************************");
                Console.WriteLine("**************************************");
                Console.WriteLine("**************************************");
                Console.WriteLine("**************************************");
            }
        }
    }
    public static class Program
    {
        static async Task Main(string[] args)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(3_000);

                    var bus = new KafkaEventBus(new KafkaProducerConfig
                    {
                        BootstrapServers = "10.51.12.36:30029"
                    });

                    await bus.Execute(new UserCreated(Guid.NewGuid(), "Hassan"));
                }
            });

            Host.CreateDefaultBuilder()
              .ConfigureServices((context, services) =>
              {
                  services.AddMessageHandler<TestHandler>();
                  services.AddHostedService<LoggerService>();
                  services.AddKafka(
                    p =>
                    {
                        p.BootstrapServers = "10.51.12.36:30029";
                    },
                    r =>
                    {
                        r.GroupId = "Campaign.Core";
                        r.BootstrappServers = "10.51.12.36:30029";
                        r.Topics = new[] { typeof(UserCreated).FullName }; // File.ReadAllLines("/storage/scenario/topics.txt");
                    });
              })
              .Build()
              .Run();

          

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
