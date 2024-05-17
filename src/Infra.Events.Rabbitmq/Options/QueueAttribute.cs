namespace Infra.Events.Rabbitmq;

public class QueueAttribute : Attribute
{
    public QueueAttribute()
    {
        ExchangeType = ExchangeType.Fanout;
    }

    public QueueAttribute(string queueName)
    {
        QueueName = queueName;
        ExchangeType = ExchangeType.Fanout;
    }

    public QueueAttribute(string queueName, string exchangeName, ExchangeType exchangeType = ExchangeType.Fanout)
    {
        Guard.NotNullOrEmpty(queueName, nameof(queueName));
        Guard.NotNullOrEmpty(exchangeName, nameof(exchangeName));

        QueueName = queueName;
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
    }

    /// <summary>
    /// Warning: if you want set
    /// RoutingKey, set proper ExchangeType !
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="exchangeName"></param>
    /// <param name="routingKey"></param>
    /// <param name="exchangeType"></param>
    public QueueAttribute(string queueName, string exchangeName, string routingKey, ExchangeType exchangeType)
    {
        Guard.NotNullOrEmpty(queueName, nameof(queueName));
        Guard.NotNullOrEmpty(exchangeName, nameof(exchangeName));

        QueueName = queueName;
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
        RoutingKey = routingKey;
    }

    /// <summary>
    /// Default is FullTypeName
    /// </summary>
    public string QueueName { get; set; }
    /// <summary>
    /// Default is FullTypeName
    /// </summary>
    public string ExchangeName { get; set; }
    /// <summary>
    /// Default is Fanout
    /// </summary>
    public ExchangeType ExchangeType { get; set; }
    /// <summary>
    /// Default is string.Empty
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
}
