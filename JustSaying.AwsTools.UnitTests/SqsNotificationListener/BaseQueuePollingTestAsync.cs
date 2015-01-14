using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using JustBehave;
using JustSaying.AwsTools;
using JustSaying.Messaging.MessageHandling;
using JustSaying.Messaging.MessageSerialisation;
using JustSaying.Messaging.Monitoring;
using JustSaying.TestingFramework;
using NSubstitute;
using NUnit.Framework;

namespace AwsTools.UnitTests.SqsNotificationListener
{
    public class WhenMessageHandlingSucceedsAsync : BaseQueuePollingTestAsync
    {
        protected override void Given()
        {
            base.Given();
            Handler.HandleAsync(null).ReturnsForAnyArgs(TaskEx.FromResult(true));
        }

        [Test]
        public void MessagesGetDeserialisedByCorrectHandler()
        {
            Patiently.VerifyExpectation(() => Serialiser.Received().Deserialise(MessageBody));
        }

        [Then]
        public void ProcessingIsPassedToTheHandlerForCorrectMessage()
        {
            Patiently.VerifyExpectation(() => Handler.Received().HandleAsync(DeserialisedMessage));
        }

        [Then]
        public void AllMessagesAreClearedFromQueue()
        {
            Patiently.VerifyExpectation(() => Sqs.Received(2).DeleteMessage(Arg.Any<DeleteMessageRequest>()));
        }

        [Then]
        public void ReceiveMessageTimeStatsSent()
        {
            Patiently.VerifyExpectation(() => Monitor.Received().ReceiveMessageTime(Arg.Any<long>()));
        }
    }

    public class BaseQueuePollingTestAsync : BehaviourTest<JustSaying.AwsTools.SqsNotificationListener>
    {
        protected const string QueueUrl = "url";
        protected IAmazonSQS Sqs;
        protected IMessageSerialiser<GenericMessage> Serialiser;
        protected GenericMessage DeserialisedMessage;
        protected const string MessageBody = "object";
        protected IAsyncHandler<GenericMessage> Handler;
        protected IMessageMonitor Monitor;
        protected IMessageSerialisationRegister SerialisationRegister;
        protected IMessageLock MessageLock;
        private readonly string _messageTypeString = typeof(GenericMessage).ToString();

        protected override JustSaying.AwsTools.SqsNotificationListener CreateSystemUnderTest()
        {

            return new JustSaying.AwsTools.SqsNotificationListener(new SqsQueueByUrl(QueueUrl, Sqs), SerialisationRegister, Monitor, null, MessageLock);
        }

        protected override void Given()
        {
            Sqs = Substitute.For<IAmazonSQS>();
            Serialiser = Substitute.For<IMessageSerialiser<GenericMessage>>();
            SerialisationRegister = Substitute.For<IMessageSerialisationRegister>();
            Monitor = Substitute.For<IMessageMonitor>();
            Handler = Substitute.For<IAsyncHandler<GenericMessage>>();
            var response = GenerateResponseMessage(_messageTypeString, Guid.NewGuid());
            Sqs.ReceiveMessage(Arg.Any<ReceiveMessageRequest>()).Returns(x => response, x => new ReceiveMessageResponse());

            SerialisationRegister.GetSerialiser(_messageTypeString).Returns(Serialiser);
            DeserialisedMessage = new GenericMessage { RaisingComponent = "Component" };
            Serialiser.Deserialise(Arg.Any<string>()).Returns(x => DeserialisedMessage);
        }

        protected override void When()
        {
            SystemUnderTest.AddMessageHandler(Handler);
            SystemUnderTest.Listen();
        }

        protected ReceiveMessageResponse GenerateResponseMessage(string messageType, Guid messageId)
        {
            return new ReceiveMessageResponse
            {
                Messages = new List<Message> {       
                    new Message
                    {   
                        MessageId = messageId.ToString(),
                        Body = "{\"Subject\":\"" + messageType + "\"," + "\"Message\":\"" + MessageBody + "\"}"
                    },
                    new Message
                    {
                        MessageId = messageId.ToString(),
                        Body = "{\"Subject\":\"SOME_UNKNOWN_MESSAGE\"," + "\"Message\":\"SOME_RANDOM_MESSAGE\"}"
                    }}
            };
        }

        public override void PostAssertTeardown()
        {
            SystemUnderTest.StopListening();
        }
    }
}