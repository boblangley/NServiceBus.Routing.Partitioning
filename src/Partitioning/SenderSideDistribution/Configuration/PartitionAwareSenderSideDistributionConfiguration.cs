namespace NServiceBus.Routing.Partitioning
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus;
    using Configuration.AdvanceExtensibility;

    public class PartitionAwareSenderSideDistributionConfiguration :
        ExposeSettings
    {
        public PartitionAwareSenderSideDistributionConfiguration(RoutingSettings routingSettings, string endpointName,
            string[] partitions)
            : base(routingSettings.GetSettings())
        {
            this.routingSettings = routingSettings;
            this.endpointName = endpointName;
            this.partitions = partitions;
        }

        public void AddPartitionMappingForMessageType<T>(Func<T, string> mapMessageToPartitionKey)
        {
            routingSettings.RouteToEndpoint(typeof(T), endpointName);
            messageTypeMappers[typeof(T)] = message => mapMessageToPartitionKey((T) message);
        }

        internal string MapMessageToPartitionKey(object message)
        {
            var messageType = message.GetType();

            if (!messageTypeMappers.TryGetValue(messageType, out var mapper))
            {
                throw new Exception($"No partition mapping is found for message type '{messageType}'.");
            }

            var partition = mapper(message);

            if (partitions.Contains(partition))
            {
                return partition;
            }

            throw new Exception(
                $"Partition '{partition}' returned by partition mapping of '{messageType}' did not match any of the registered destination partitions '{string.Join(",", partitions)}'.");
        }

        RoutingSettings routingSettings;
        string endpointName;
        string[] partitions;
        Dictionary<Type, Func<object, string>> messageTypeMappers = new Dictionary<Type, Func<object, string>>();
    }
}