namespace Infra.Events.Rabbitmq;

public class QueueAttribute : Attribute
{
	public QueueAttribute()
	{
	}

    public QueueAttribute(string queueName)
    {
        QueueName = queueName;
    }

    public QueueAttribute(string queueName, string exchangeName, string routingKey = default)
    {
        Guard.NotNullOrEmpty(queueName, nameof(queueName));
        Guard.NotNullOrEmpty(exchangeName, nameof(exchangeName));

        QueueName = queueName;
        ExchangeName = exchangeName;

        if (!string.IsNullOrWhiteSpace(routingKey))
        {
            RoutingKey = routingKey;
            ExchangeType = ExchangeType.Direct;
        }
    }

    public QueueAttribute(string queueName, string exchangeName, ExchangeType exchangeType = default)
    {
        Guard.NotNullOrEmpty(queueName, nameof(queueName));
        Guard.NotNullOrEmpty(exchangeName, nameof(exchangeName));

        QueueName = queueName;
        ExchangeName = exchangeName;
        ExchangeType = exchangeType;
    }

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
    public ExchangeType ExchangeType { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
}
