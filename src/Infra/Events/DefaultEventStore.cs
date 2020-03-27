using Domain;
using EventStore.ClientAPI;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Guard;

namespace Infra.Events
{
    public class DefaultEventStore : IEventStore
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private bool _connected;

        public DefaultEventStore(IOptions<EventStoreConfig> options)
        {
            _eventStoreConnection = EventStoreConnection.Create(new Uri(options.Value.ToString()));
        }

        public async Task<StreamEventsSlice> ReadStream(string stream)
        {
            await EnsureConnected();

            return await _eventStoreConnection.ReadStreamEventsForwardAsync(stream, 0, int.MaxValue, true);
        }

        public async Task Commit(AggregateRoot root)
        {
            await EnsureConnected();

            // aggregate version before uncommited changes have been applied
            var initialVersion = root.Version - root.UncommittedChanges.Count();
            if (initialVersion == 0)
            {
                initialVersion = ExpectedVersion.NoStream;
            }

            using (var transaction = await _eventStoreConnection.StartTransactionAsync(root.StreamName, initialVersion))
            {
                var events = root.UncommittedChanges.Select(e => GetEventData(e));
                await transaction.WriteAsync(events);
                await transaction.CommitAsync();
            }
        }

        public EventData GetEventData(Event @event)
        {
            NotNull(@event, nameof(@event));

            return new EventData(Guid.NewGuid(),
                @event.EventName,
                true,
                GetBytes(@event),
                GetBytes(@event.EventMetadata ?? new { }));

            static byte[] GetBytes(object input) => Encoding.UTF8.GetBytes(GetJson(input));
            static string GetJson(object input) => JsonConvert.SerializeObject(input, new JsonSerializerSettings
            {
                ContractResolver = new IgnorePropertiesResolver(nameof(Event.EventMetadata))
            });
        }

        public async Task Connect()
        {
            await EnsureConnected();
        }

        private async ValueTask EnsureConnected()
        {
            if (_connected)
            {
                return;
            }

            await _eventStoreConnection.ConnectAsync();
            _connected = true;
        }

        // Used to exclude metadata field from serialization
        private class IgnorePropertiesResolver : DefaultContractResolver
        {
            private readonly IEnumerable<string> _propsToIgnore;

            public IgnorePropertiesResolver(params string[] propNamesToIgnore) => _propsToIgnore = propNamesToIgnore;

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = _ => !_propsToIgnore.Contains(property.PropertyName);
                return property;
            }
        }
    }
}
