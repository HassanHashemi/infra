using System;
using Autofac;
using Infra.Common.Decorators;
using Infra.Eevents;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Infra.Events.Rabbitmq;
using Microsoft.Extensions.Logging;
using Sample;

namespace PublisherSample;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection().AddLogging(x => x.AddConsole());

        services.Configure<FuncDecoratorOptions>(o =>
        {
            o.Handler = (e, c) => Task.FromResult(1);
        });

        services.AddDistributedMemoryCache();
        var builder = new ContainerBuilder();
        builder.Populate(services);

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
        var bus = provider.Resolve<IEventBus>();

        while (true)
        {
            await bus.Execute(new FlightOrderItemStateChanged
            {
                Value = "Test"
            }, new Dictionary<string, string>());

            Console.Write("Press any key to publish new message");

            Console.ReadLine();
        }
    }
}