using System.Net;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using JustSaying.Messaging;
using JustSaying.Messaging.MessageSerialisation;
using Newtonsoft.Json;
using Message = JustSaying.Models.Message;

namespace JustSaying.AwsTools
{
    public class SqsPublisher : SqsQueueByName, IMessagePublisher
    {
        private readonly IAmazonSQS _client;
        private readonly IMessageSerialisationRegister _serialisationRegister;

        public SqsPublisher(string queueName, IAmazonSQS client, int retryCountBeforeSendingToErrorQueue, IMessageSerialisationRegister serialisationRegister)
            : base(queueName, client, retryCountBeforeSendingToErrorQueue)
        {
            _client = client;
            _serialisationRegister = serialisationRegister;
        }

        public void Publish(Message message)
        {
            _client.SendMessage(new SendMessageRequest
            {
                MessageBody = GetMessageInContext(message),
                //MessageBody = _serialisationRegister.GetSerialiser(message.GetType()).Serialise(message),
                QueueUrl = Url
            });
        }

        public async Task<bool> PublishAsync(Message message)
        {
            var response = await Task.Factory.FromAsync<SendMessageRequest, SendMessageResult>(Client.BeginSendMessage, Client.EndSendMessage,
                new SendMessageRequest
                {
                    MessageBody = GetMessageInContext(message),
                    QueueUrl = Url
                }, null);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        private static string GetMessageInContext(Message message)
        {
            // ToDo: No no mr JsonConvert.
            var context = new { Subject = message.GetType().Name, Message = message };
            return JsonConvert.SerializeObject(context);
        }
    }
}