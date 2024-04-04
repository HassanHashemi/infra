using System.Net.Sockets;
using Infra.Serialization.Json;
using RabbitMQ.Client;

namespace Infra.Events.Rabbitmq;

public class RabbitmqOptions
{
    private const string DefaultPass = "guest";
    private const string DefaultUser = "guest";
    private const string DefaultVHost = "/";
    private const uint DefaultFrameMax = 0;
    private const uint DefaultMaxMessageSize = 0;
    private const ushort DefaultChannelMax = 2047;
    private static readonly TimeSpan DefaultHeartbeat = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(30);

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = AmqpTcpEndpoint.UseDefaultPort;
    public string UserName { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; } = DefaultVHost;
    public string ClientProvidedName { get; set; }
    public IJsonSerializer Serializer { get; set; }

    #region Optionals
    public static AddressFamily DefaultAddressFamily { get; set; }
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public bool DispatchConsumersAsync { get; set; } = false;
    public int ConsumerDispatchConcurrency { get; set; } = 1;
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan HandshakeContinuationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan ContinuationTimeout { get; set; } = TimeSpan.FromSeconds(20);
    public TimeSpan RequestedConnectionTimeout { get; set; } = DefaultConnectionTimeout;
    public TimeSpan SocketReadTimeout { get; set; } = DefaultConnectionTimeout;
    public TimeSpan SocketWriteTimeout { get; set; } = DefaultConnectionTimeout;
    public bool TopologyRecoveryEnabled { get; set; } = true;
    public IDictionary<string, object> ClientProperties { get; set; }
    public ushort RequestedChannelMax { get; set; } = DefaultChannelMax;
    public uint RequestedFrameMax { get; set; } = DefaultFrameMax;
    public TimeSpan RequestedHeartbeat { get; set; }
    public uint MaxMessageSize { get; set; } = DefaultMaxMessageSize;
    #endregion
}