using System;
using System.Threading.Tasks;
using NServiceBus.Pipeline;

namespace NServiceBus.Routing.Partitioning
{
    class HardcodeReplyToAddressToLogicalAddress :
        IBehavior<IOutgoingPhysicalMessageContext, IOutgoingPhysicalMessageContext>
    {
        public HardcodeReplyToAddressToLogicalAddress(string instanceSpecificQueue)
        {
            this.instanceSpecificQueue = instanceSpecificQueue;
        }

        public Task Invoke(IOutgoingPhysicalMessageContext context, Func<IOutgoingPhysicalMessageContext, Task> next)
        {
            if (instanceSpecificQueue != null && !context.Extensions.TryGet(out NoReplyToAddressOverride _))
            {
                context.Headers[Headers.ReplyToAddress] = instanceSpecificQueue;
            }

            return next(context);
        }

        public class NoReplyToAddressOverride
        {
            public static NoReplyToAddressOverride Instance = new NoReplyToAddressOverride();

            NoReplyToAddressOverride()
            {
            }
        }

        string instanceSpecificQueue;
    }
}