using System;
using System.Threading.Tasks;

namespace JustSaying.Messaging.MessageProcessingStrategies
{
    public interface IMessageProcessingStrategy
    {
        void BeforeGettingMoreMessages();
        void ProcessMessage(Task action);
    }
}