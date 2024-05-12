# Infra.Net

**Infra.Net** is an infrastructure framework for building Microservices with Event-Driven Architecture and Distributed Large-Scale Applications which have High Availability and Eventual Consistency.


 This framework contains implemented patterns like CQRS, Event-Sourcing and Domain-Driven Design (which are integrated with Event-Driven Architecture using **Kafka** and **Rabbitmq**), and you can easily configure it for your own solution.

## Installation

```nuget
NuGet\Install-Package Infra
```


## Configurations

### **Kafka** configuration:
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

### **Rabbitmq** configuration:
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
                        //NOTE: Choose dedicate name per microservice (will use as queue names prefiex)
                        c.ConsumerGroupId = AppDomain.CurrentDomain.FriendlyName;
                    });
            });
}
```

### **Command Queries + Event Bus**:
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

## Use Cases


### Integration Events 

#### Integration Event class sample:
```c#
[Topic(Name = "OrderItemStateChanged")] //for Kafka broker (optional attribute)
[Queue(QueueName = "OrderItemStateChanged", ExchangeName = "OrderItemStateChanged")] // for Rabbitmq broker (optional attribute)
public class OrderItemStateChanged : DomainEvent
{
    public string Value { get; set; }
    // ...

    // MustPropagate = false: Send an internal event (C# event) and call internal event handlers (in currenct microservice)
    // MustPropagate = true: Send an integration event (Broker event) and call all it's event handlers (in all microservices)
    public override bool MustPropagate { get; set; } = true;
}
```


#### Integration Event Handler class sample (in the same service or another microservice):
If you don't want to propagate event and produce and internal event (C# event), you can handle that event in the same microservice using this sample:
```c#
public class OrderItemStateChangedEventHandler : IEventHandler<OrderItemStateChanged>
{
    public Task Handle(OrderItemStateChanged @event, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}
```

If you want propagate an event through all microservices, you should configure any above brokers, then you can hanle event in any microservice which connected to the broker using this sample:
```c#
public class OrderItemStateChangedEventHandler : IMessageHandler<OrderItemStateChanged>
{
    public Task Handle(OrderItemStateChanged @event, Dictionary<string, string> headers)
    {
        return Task.CompletedTask;
    }
}
```

#### Produce an Integration Event sample:
```c#
public class OrderController : BaseController
{
    private readonly IEventBus _eventBus;

    //Inject IEventBus anywhere you want
    public OrderController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }


    [RequireValidModel]
    [HttpPost("[controller]/[action]")]
    public async Task<IActionResult> ProduceEvent(Guid id, OrderItemStates state)
        => ApiOk(_eventBus.Execute(new OrderItemStateChanged(id, state)));
}
```


### Domain-Driven Design samples:

#### Aggregate Root smaple
```c#
public class Order : AggregateRoot
{
    private Order()
    {
    }
    public long OrderId { get; private set; }
}
```

#### Entity sample
```c#
public class OrderNote : Entity
{
    private OrderNote()
    {
    }

    public long OrderNoteId { get; private set; }
    public Order Order { get; private set; }
    public long OrderId { get; private set; }
}
```

#### Domain Event class sample
```c#
[Topic(Name = OrderTopics.ORDER)] //Optional attribute
[Queue(QueueName = OrderTopics.ORDER, ExchangeName = OrderTopics.ORDER)] //Optional attribute
public class OrderCreated : DomainEvent
{
    public OrderCreated()
    {
    }

    public OrderCreated(
        Guid aggregateRootId,
        ProductChannel channel) : base(aggregateRootId)
    {
        AggregateRootId = aggregateRootId;
        Channel = channel;
        Timestamp = DateTime.Now;
    }

    public Guid AggregateRootId { get; set; }
    public ProductChannel Channel { get; set; }
    // MustPropagate = false: Send an internal event (C# event) and call internal event handlers (in currenct microservice)
    // MustPropagate = true: Send an integration event (Broker event) and call all it's event handlers (in all microservices)
    public override bool MustPropagate { get; set; } = true;
}
```

#### Produce a Domain Event sample
Put this two methods in the AggregateRoot for raising an event (for example OrderNoteCreated Event):
```c#
public class Order : AggregateRoot
{
    private Order()
    {
    }

    public long OrderId { get; private set; }

    public void AddNote(string text, int creatorId)
    {
        var @event = new OrderNoteCreated(this.Id, text, OrderId, creatorId);
        ApplyChange(@event);
    }

    private void Apply(OrderNoteCreated @event)
    {
        this.Notes ??= new List<OrderNote>();
        this.Notes.Add(new OrderNote(@event.Text, @event.CreatorId));
    }
}
```

