using Domain;
using Infra.Serialization.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Infra.Events.Rabbitmq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infra.Events.Kafka
{
	public class RabbitMqService : BackgroundService
	{
		private readonly RabbitmqConsumerConfig _config;
		private readonly HandlerInvoker _handlerFactory;
		private readonly IJsonSerializer _serializer;
		private readonly ILogger<RabbitMqService> _logger;

		private IConnection _connection;
		private EventingBasicConsumer _consumer;

		public RabbitMqService(
			ILogger<RabbitMqService> logger,
			HandlerInvoker handlerFactory,
			IOptions<RabbitmqConsumerConfig> subscriberConfig) :
			this(logger, handlerFactory, subscriberConfig.Value, null)
		{
		}

		public RabbitMqService(
			ILogger<RabbitMqService> logger,
			HandlerInvoker handlerFactory,
			RabbitmqConsumerConfig subscriberConfig,
			RabbitmqOptions options)
		{
			if (!subscriberConfig.IsValid)
			{
				throw new ArgumentException(nameof(subscriberConfig));
			}

			this._logger = logger;
			this._config = subscriberConfig;
			this._handlerFactory = handlerFactory;

			this._serializer = options.Serializer ?? new DefaultNewtonSoftJsonSerializer();

			//if (subscriberConfig.Topics == null || !subscriberConfig.Topics.Any())
			//{
			//	_logger.LogWarning("No topics found to subscribe");
			//}
			//else
			//{
			//	_logger.LogInformation($"subscribing to {_serializer.Serialize(subscriberConfig.Topics)}");
			//}
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
			};

			this._connection = factory.CreateConnection();

			try
			{
				foreach (var assembly in _config.Transports)
				{
					using (_connection)
					{
						using (var channel = _connection.CreateModel())
						{

							channel.ExchangeDeclare(exchange: assembly.exchange, type: ExchangeType.Fanout);

							channel.QueueBind(queue: assembly.queueName,
								exchange: assembly.exchange,
								routingKey: "");

							Console.WriteLine(" [*] Waiting for messages.");

							this._consumer = new EventingBasicConsumer(channel);

							this._consumer.Received += (model, eventArgs) =>
							{
								Receive(eventArgs.Body);
							};

							channel.BasicConsume(queue: assembly.queueName,
								autoAck: true,
								consumer: _consumer);
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.ToString());
				throw;
			}

			return Task.CompletedTask;
		}

		private async void Receive(ReadOnlyMemory<byte> eventArgs)
		{
			var payloadString = Encoding.UTF8.GetString(eventArgs.ToArray());

			var @event = _serializer.Deserialize<Event>(payloadString);

			await _handlerFactory.Invoke(@event.EventName, payloadString, null);
		}
	}
}