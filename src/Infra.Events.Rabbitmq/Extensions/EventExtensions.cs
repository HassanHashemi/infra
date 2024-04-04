using Domain;
using System.Reflection;

namespace Infra.Events.Rabbitmq.Extensions;

internal static class EventExtensions
{
    internal static RabbitMqExchange GetExchangeInfo<TEvent>(this TEvent @event) where TEvent : Event
    {
        var exchangeInfo = @event.GetType()
            .GetCustomAttribute<ExchangeAttribute>();

        if (exchangeInfo != null)
            return new RabbitMqExchange(exchangeInfo.Name, exchangeInfo.ExchangeType);

        return new RabbitMqExchange();
    }

    internal static RabbitMqQueue GetQueueInfo<TEvent>(this TEvent @event) where TEvent : Event
    {
        var queueInfo = @event.GetType()
            .GetCustomAttribute<QueueAttribute>();

        if (queueInfo != null)
            return new RabbitMqQueue(queueInfo.Name);

        return default;
    }
}