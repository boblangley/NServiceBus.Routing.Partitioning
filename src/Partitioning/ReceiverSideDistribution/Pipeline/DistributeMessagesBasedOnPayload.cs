namespace NServiceBus.Routing.Partitioning
{
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Pipeline;

    class DistributeMessagesBasedOnPayload :
        IBehavior<IIncomingLogicalMessageContext, IIncomingLogicalMessageContext>
    {
        public DistributeMessagesBasedOnPayload(string localPartitionKey, Forwarder forwarder, Func<object, string> mapper)
        {
            this.localPartitionKey = localPartitionKey;
            this.forwarder = forwarder;
            this.mapper = mapper;
        }

        public Task Invoke(IIncomingLogicalMessageContext context, Func<IIncomingLogicalMessageContext, Task> next)
        {
            var intent = GetMessageIntent(context);
            var isSubscriptionMessage = intent == MessageIntentEnum.Subscribe || intent == MessageIntentEnum.Unsubscribe;
            var isReply = intent == MessageIntentEnum.Reply;

            if (isSubscriptionMessage || isReply)
            {
                return next(context);
            }

            // If the header is set we assume it's validity was checked by DistributeMessagesBasedOnHeader
            if (context.MessageHeaders.ContainsKey(PartitionHeaders.PartitionKey))
            {
                return next(context);
            }

            var messagePartitionKey = mapper(context.Message.Instance);

            if (string.IsNullOrWhiteSpace(messagePartitionKey))
            {
                throw new PartitionMappingFailedException($"Could not map a partition key for message of type {context.Headers[Headers.EnclosedMessageTypes]}");
            }

            log.Debug($"##### Received message: {context.Headers[Headers.EnclosedMessageTypes]} with Mapped PartitionKey={messagePartitionKey} on partition {localPartitionKey}");

            if (messagePartitionKey == localPartitionKey)
            {
                return next(context);
            }

            context.Headers[PartitionHeaders.PartitionKey] = messagePartitionKey;

            return forwarder.Forward(context, messagePartitionKey);
        }

        static MessageIntentEnum? GetMessageIntent(IMessageProcessingContext context)
        {
            if (context.MessageHeaders.TryGetValue(Headers.MessageIntent, out var intentStr))
            {
                return (MessageIntentEnum)Enum.Parse(typeof(MessageIntentEnum), intentStr);
            }

            return null;
        }

        string localPartitionKey;
        Forwarder forwarder;
        Func<object, string> mapper;
        static ILog log = LogManager.GetLogger<DistributeMessagesBasedOnPayload>();
    }
}