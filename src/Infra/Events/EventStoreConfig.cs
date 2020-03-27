namespace Infra.Events
{
    public class EventStoreConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; } = 1113;

        public override string ToString() => $"tcp://{UserName}:{Password}@{Host}:{Port}";
    }
}
