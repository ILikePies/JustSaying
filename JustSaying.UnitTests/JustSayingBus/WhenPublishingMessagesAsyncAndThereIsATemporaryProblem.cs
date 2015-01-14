using System;
using System.Threading.Tasks;
using JustBehave;
using JustSaying.Messaging;
using JustSaying.TestingFramework;
using NSubstitute;
using NUnit.Framework;

namespace JustSaying.UnitTests.JustSayingBus
{
    public class WhenPublishingMessagesAsyncAndThereIsATemporaryProblem : GivenAServiceBus
    {
        private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
        private bool _result;

        protected override void When()
        {
            Config.PublishFailureReAttempts = 1;

            var results = new Results<Func<Task<bool>>>(() => {throw new Exception(); })
                .Then(() => TaskEx.FromResult(true));

            _publisher.PublishAsync(null).ReturnsForAnyArgs((x) => results.Next()());
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

        [Then]
        public void PublishSucceeds()
        {
            Assert.True(_result);
        }
    }
}