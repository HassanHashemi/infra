# Infra.Net

**Infra.Net** is an infrastructure framework for building Microservices with Event-Driven Architecture and Distributed Large-Scale Applications which have High Availability and Eventual Consistency.


 This framework contains implemented patterns like CQRS, Event-Sourcing and Domain-Driven Design (which are integrated with Event-Driven Architecture using **Kafka** and **Rabbitmq**), and you can easily configure it for your own solution.


**Kafka** configuration:

If you want use Kafka as your broker you can configure it like this:

```c#
 public static class Program
 {
     public static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
             .UseServiceProviderFactory(new AutofacServiceProviderFactory())
             .ConfigureContainer<ContainerBuilder>(builder =>
             {
                 builder.AddKafka(p =>
                     {
                         p.BootstrapServers = "localhost:9092";
                     },
                     consumer =>
                     {
                         consumer.OffsetResetType = AutoOffsetReset.Earliest;
                         //NOTE: Choose dedicate name per microservice (consumer group)
                         consumer.GroupId = "UNIQUE-GROUP-ID (consumer group)";
                         consumer.BootstrappServers = "localhost:9092";
                         //NOTE: Don't add duplicate assemblies!
                         consumer.EventAssemblies = new[] { typeof(Program).Assembly };
                         consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
                     });
             }
 }
```

**Rabbitmq** configuration:

If you want use Rabbitmq as your broker you can configure it like this:

```c#
public static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.AddRabbitmqInternal(
                    p =>
                    {
                        p.Host = "localhost";
                        p.UserName = "guest";
                        p.Password = "guest";
                        p.VirtualHost = "/";
                    },
                    c =>
                    {
                        c.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
                        //NOTE: Don't add duplicate assemblies!
                        c.EventAssemblies = new[] { typeof(Program).Assembly };
                        //NOTE: Choose dedicate name per microservice (consumer group)
                        c.ConsumerGroupId = AppDomain.CurrentDomain.FriendlyName;
                    });
            });
}
```

**Command Queries** service extension:
```c#
public static ContainerBuilder AddCommandQuery(this ContainerBuilder builder)
{
    var scannedAssemblies = new[]
    {
        //NOTE: Don't add duplicate assemblies!
        typeof(Startup).Assembly
    };

    builder.Register<IUnitOfWork>(context =>
        {
            var db = context.Resolve<DbContext>();
            var logger = context.Resolve<ILogger<EfUnitOfWork>>();
            var syncEventBus = context.Resolve<SyncEventBus>();
            var broker = context.Resolve<IEventBus>(); //Kafka, Rabbitmq, etc

            return new EfUnitOfWork(db, broker, syncEventBus, logger);
        })
        .InstancePerLifetimeScope();

    builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);
    builder.AddSyncEventHandlers(scannedAssemblies);


    return builder;
}
```
