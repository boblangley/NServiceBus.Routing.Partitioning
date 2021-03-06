namespace NServiceBus.Routing.Partitioning
{
    using System;
    using System.Linq;
    using NServiceBus;
    using Routing;

    public class PartitionAwareDistributionStrategy :
        DistributionStrategy
    {
        public PartitionAwareDistributionStrategy(string endpoint, Func<object, string> mapper,
            DistributionStrategyScope scope) : base(endpoint, scope)
        {
            this.mapper = mapper;
        }

        public override string SelectReceiver(string[] receiverAddresses)
        {
            throw new NotSupportedException();
        }

        public override string SelectDestination(DistributionContext context)
        {
            var discriminator = mapper(context.Message.Instance);

            context.Headers[PartitionHeaders.PartitionKey] = discriminator;

            var remoteAddress = context.ToTransportAddress(new EndpointInstance(Endpoint, discriminator));
            return context.ReceiverAddresses.Single(a => a == remoteAddress);
        }

        Func<object, string> mapper;
    }
}