using Autofac;
using Domain;
using Infra.Commands;
using Infra.Common.Decorators;
using Infra.Events;
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
        public UserCreated()
        {
        }

        public UserCreated(Guid userId, string fullName) : base(userId)
        {
            FullName = fullName;
            UserId = userId;
        }

        public string FullName { get; set; }
        public Guid UserId { get; set; }
    }

    public class NameChanged : DomainEvent
    {
        public NameChanged()
        {
        }

        public NameChanged(Guid userId, string fullName) : base(userId)
        {
            this.UserId = userId;
            this.FullName = fullName;
        }

        public Guid UserId { get; set; }
        public string FullName { get; set; }
    }

    public class User : AggregateRoot
    {
        public User()
        {
        }

        public User(string fullName)
        {
            Id = Guid.NewGuid();

            ApplyChange(new UserCreated(Id, fullName));
        }

        public string FullName { get; set; }
        public string Description { get; set; }

        public void ChangeName(string fullName)
        {
            ApplyChange(new NameChanged(this.Id, fullName));
        }

        public void ChangeDecription(string description)
        {
            ApplyChange(new DescriptionChanged(Id, description));
        }

        private void Apply(NameChanged @event)
        {
            FullName = @event.FullName;
        }

        private void Apply(DescriptionChanged @event)
        {
            Description = @event.Description;
        }

        private void Apply(UserCreated @event)
        {
            Id = @event.UserId;
            FullName = @event.FullName;
        }
    }

    public class TestValidator : ICommandValidator<TestCommand>
    {
        public ValueTask ValidateAsync(TestCommand command)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestCommand : ICommand
    {
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<string> HandleAsync(TestCommand command)
        {
            throw new System.NotImplementedException();
        }
    }

    public static class Program
    {
        static async Task Main(string[] args)
        {
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
            var builder = new ContainerBuilder();
            
            AddCommandQuery(builder, typeof(Program).Assembly);
            var provider = builder.Build();
            var processor = provider.Resolve<ICommandProcessor>();
            var result = processor.ExecuteAsync<TestCommand, string>(new TestCommand()).Result;
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

        private static async Task Load(DefaultEventStore store)
        {
            var watch = new Stopwatch();
            watch.Start();

            var all = await store.ReadStream("Sample01.User-a4609b96-235e-4aaf-9ca3-a220f3015a78");
            var user = new User();
            var events = new List<DomainEvent>();

            foreach (var item in all.Events)
            {
                if (item.Event.EventType == typeof(UserCreated).FullName)
                {
                    var e = JsonConvert.DeserializeObject<UserCreated>(Encoding.UTF8.GetString(item.Event.Data));
                    events.Add(e);
                }
                else if (item.Event.EventType == typeof(NameChanged).FullName)
                {
                    var e = JsonConvert.DeserializeObject<NameChanged>(Encoding.UTF8.GetString(item.Event.Data));
                    events.Add(e);
                }
                else if (item.Event.EventType == typeof(DescriptionChanged).FullName)
                {
                    var e = JsonConvert.DeserializeObject<DescriptionChanged>(Encoding.UTF8.GetString(item.Event.Data));
                    events.Add(e);
                }
            }

            user.LoadsFromHistory(events);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}
