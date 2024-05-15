namespace Infra.Events.Rabbitmq;

public enum ExchangeType
{
    Direct = 1,
    Fanout = 2,
    Topic = 3,
    Headers = 4
}