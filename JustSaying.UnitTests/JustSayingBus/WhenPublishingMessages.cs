using System.Linq.Expressions;
using System.Threading.Tasks;
using JustBehave;
using JustSaying.Messaging;
using JustSaying.Models;
using JustSaying.TestingFramework;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Routing.Handlers;
using NUnit.Framework;

namespace JustSaying.UnitTests.JustSayingBus
{
    public class WhenPublishingMessages : GivenAServiceBus
    {
        private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
        
        protected override void When()
        {
            SystemUnderTest.AddMessagePublisher<GenericMessage>(_publisher, string.Empty);

            SystemUnderTest.Publish(new GenericMessage());
        }

        [Then]
        public void PublisherIsCalledToPublish()
        {
            Patiently.VerifyExpectation(() => _publisher.Received().Publish(Arg.Any<GenericMessage>()));
        }

        [Then]
        public void PublishMessageTimeStatsSent()
        {
            Patiently.VerifyExpectation(() => Monitor.Received(1).PublishMessageTime(Arg.Any<long>()), 10.Seconds());
        }
    }

    public class WhenPublishingMessagesAsync : GivenAServiceBus
    {
        private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
        private bool _result;

        protected override void When()
        {
            _publisher.PublishAsync(null).ReturnsForAnyArgs(TaskEx.FromResult(true));
            SystemUnderTest.AddMessagePublisher<GenericMessage>(_publisher, string.Empty);

            _result = SystemUnderTest.PublishAsync(new GenericMessage()).Result;
        }

        [Then]
        public void PublisherIsCalledToPublish()
        {
            Patiently.VerifyExpectation(() => _publisher.Received().PublishAsync(Arg.Any<GenericMessage>()));
        }

        [Then]
        public void PublishMessageTimeStatsSent()
        {
            Patiently.VerifyExpectation(() => Monitor.Received(1).PublishMessageTime(Arg.Any<long>()), 10.Seconds());
        }

        [Test]
        public void PublishSucceeds()
        {
            Assert.True(_result);
        }
    }
}