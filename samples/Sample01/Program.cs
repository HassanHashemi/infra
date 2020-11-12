using Autofac;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
using Infra.Events.Kafka;
using Infra.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using News.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

    public static class Program
    {
        static async Task Main(string[] args)
        {
            var bus = new KafkaEventBus(new KafkaProducerConfig
            {
                BootstrapServers = "10.51.12.36:30029"
            });

            await bus.Execute(new UserCreated(Guid.NewGuid(), "Hasasn Hashemi"));

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
