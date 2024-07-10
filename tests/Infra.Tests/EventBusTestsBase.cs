using Microsoft.Extensions.Configuration;

namespace Infra.Tests;

public abstract class EventBusTestsBase
{
    protected IConfiguration InitConfiguration() => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

    //public abstract IContainer InitEventBus(ServiceCollection externalServices = null);
    //public abstract Task EventTest_WhenSendEvent_ShouldCallEventHandlerAsync();
    //public abstract Task EventTest_WhenSendEvent_ShouldCallCustomFuncBeforeEventHandlerAsync();
}