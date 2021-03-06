using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Features;
using NServiceBus.Routing.Partitioning;

namespace NServiceBus
{
    public static class ReceiverSideDistributionExtensions
    {
        public static PartitionAwareReceiverSideDistributionConfiguration EnableReceiverSideDistribution(
            this RoutingSettings routingSettings, string[] discriminators)
        {
            var settings = routingSettings.GetSettings();

            if (settings.TryGet(out PartitionAwareReceiverSideDistributionConfiguration config))
            {
                return config;
            }

            config = new PartitionAwareReceiverSideDistributionConfiguration(routingSettings, discriminators);
            settings.Set<PartitionAwareReceiverSideDistributionConfiguration>(config);
            settings.Set(typeof(ReceiverSideDistribution).FullName, FeatureState.Enabled);

            return config;
        }
    }
}