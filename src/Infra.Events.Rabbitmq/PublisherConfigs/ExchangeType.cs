namespace Infra.Events.Rabbitmq;

public enum ExchangeType
{
    Direct = 1,
    Fanout = 2,
    Headers = 3,
    Topic = 4
}