using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Infra.Events.Rabbitmq;
using Microsoft.Extensions.Hosting;

namespace ConsumerSample;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder =>
            {
                //builder.AddKafka(p =>
                //{
                //    p.BootstrapServers = "91.107.239.221:30049";
                //},
                //consumer =>
                //{
                //    consumer.OffsetResetType = AutoOffsetReset.Earliest;
                //    consumer.GroupId = "gw-test37";
                //    //consumer.Topics = new[] { typeof(SmppGatewayMessage).FullName };
                //    consumer.BootstrappServers = "91.107.239.221:30049";
                //    consumer.EventAssemblies = new[] { typeof(Program).Assembly };
                //    consumer.MaxPollIntervalMs = 50_000;
                //    consumer.SessionTimeoutMs = 50_000;
                //    consumer.PreMessageHandlingHandler = (provider, @event, headers) => ValueTask.CompletedTask;
                //    ///consumer.AutoOffsetCommit = false;,
                //});

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
            });
}