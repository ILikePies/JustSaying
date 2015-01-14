using System;
using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageProcessingStrategies
{
    public class MaximumThroughput : IMessageProcessingStrategy
    {
        public void BeforeGettingMoreMessages()
        {
        }

        public void ProcessMessage(Task action)
        {
            action.Start();
        }
    }
}