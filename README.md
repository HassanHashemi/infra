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
            var broker = context.Resolve<IEventBus>(); //SyncEventBus, Kafka, Rabbitmq, etc

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
//for Kafka broker (optional attribute, without this FullTypeName will use as Topic name)
[Topic(Name = "OrderItemStateChanged")]
//for Rabbitmq broker (optional attribute, without this FullTypeName will use as Exchange and Queue name)
[Queue(QueueName = "OrderItemStateChanged", ExchangeName = "OrderItemStateChanged")] 
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

For domain implementation, it's better to follow these rules:

**1-** Add appropriate Constructors in all Domain classes (using as a factory methods), in order to the object to be made as it should be. Don't let them create the objects however they want!

**2-** Add Default Constructor as **private** in order to EF can initialize the object.

**3-** Put strict validations in the constructors, for preventing invalid data creation. (this is one of validation layers)

4- Make All Properties as **{ get; private set; }** for preventing changes from outside of AggregateRoot, unless AggregateRoot expose a method to give this authority as it wants!

#### Aggregate Root smaple
```c#
public class Order : AggregateRoot
{
    private Order()
    {
    }

    public Order(
        ProductChannel channel,
        int customerId,
        decimal totalPrice,
        string description)
    {
        Guard.Positive(totalPrice, nameof(totalPrice));
        Guard.MaxLength(description, 200, nameof(description));

        var @event = new OrderCreated(
            channel,
            customerId,
            description);

        ApplyChange(@event);
    }

    public decimal TotalPrice { get; private set; }
    public ProductChannel Channel { get; private set; }
    public string Description { get; private set; }
    //...
}
```

#### Entity sample
```c#
public class OrderNote : Entity
{
    private OrderNote()
    {
    }

    public OrderNote(string text, int creatorId)
    {
        Guard.NotNullOrDefault(creatorId, nameof(creatorId));
        Guard.NotNullOrEmpty(text, nameof(text));

        this.Text = text;
        this.CreatorId = creatorId;
    }

    public long OrderNoteId { get; private set; }
    public string Text { get; private set; }
    public int CreatorId { get; set; }

    //Relation properties will useful for querying data (These also should be private setter)
    public Order Order { get; private set; }
    public long OrderId { get; private set; }
}
```

#### ValueObject sample
```c#
public class OrderContactInfo : ValueObject<OrderContactInfo>
{
    private OrderContactInfo()
    {
    }

    public OrderContactInfo(string email, string phoneNumber)
    {
        Guard.NotNullOrEmpty(email, nameof(email));
        Guard.NotNullOrEmpty(phoneNumber, nameof(phoneNumber));
        Guard.MaxLength(phoneNumber,15, nameof(phoneNumber));

        Email = email;
        PhoneNumber = phoneNumber;
    }

    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
}
```

#### Domain Event class sample
The **DomainEvent** class inherited from **Event** class, so you can handle DomainEvents just like Events everywhere you want.
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
For producing domain events, you need read the **AggregateRoot** from database with **IUnitOfWork** which track AggregateRoot changes with events and raise all DomainEvents on **Save()** method.
First you need to put this two methods in the AggregateRoot for raising an DomainEvent (for example OrderNoteCreated DomainEvent):
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

Then inject IUnitOfWork interface in somewhere you want excecute the add or update command (usually in a CommandHandler), make your changes on AggregateRoot (or it's Entities and ValueObjects) and call the method you implemented above, then Save changes using IUnitOfWork, the DomainEvent(s) will raise after database changes commited successfully.
```c#
public class ChangeOrderItemStateCommandHandler : ICommandHandler<ChangeOrderItemStateCommand, ChangeOrderItemStateCommandResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangeOrderItemStateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChangeOrderItemStateCommandResult> HandleAsync(ChangeOrderItemStateCommand command)
    {
        var orderItem = await _unitOfWork
            .Repo<OrderItem>()
            .FirstOrDefaultAsync(o => o.OrderItemId == command.OrderItemId);

        orderItem.ChangeState(command.Status, command.Description);

        var result = await _unitOfWork.Save(orderItem);

        return new ChangeOrderItemStateCommandResult(result);
    }
}
```
Note: Just AggregateRoots can expose methods and functionalities (Not Entities or ValueObjects), so AggregateRoots can raise DomainEvents (for noticing all handlers to it's internal changes).
