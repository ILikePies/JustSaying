using System;
using JustBehave;
using JustSaying.Messaging;
using JustSaying.Models;
using JustSaying.TestingFramework;
using NSubstitute;
using NUnit.Framework;

namespace JustSaying.UnitTests.JustSayingBus
{
    public class WhenPublishingFailsAsync : GivenAServiceBus
    {
        private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();
        private bool _result;
        private const int PublishReAttempts = 2;

        protected override void Given()
        {
            base.Given();
            Config.PublishFailureReAttempts.Returns(PublishReAttempts);
            Config.PublishFailureBackoffMilliseconds.Returns(0);
            RecordAnyExceptionsThrown();
            _publisher.When(x => x.PublishAsync(Arg.Any<Message>())).Do(x => { throw new Exception(); });
        }

        protected override void When()
        {
            SystemUnderTest.AddMessagePublisher<GenericMessage>(_publisher, string.Empty);
            
            _result = SystemUnderTest.PublishAsync(new GenericMessage()).Result;
        }

        [Test]
        public void EventPublicationWasAttemptedTheConfiguredNumberOfTimes()
        {
            Patiently.VerifyExpectation(() => _publisher.Received(1).PublishAsync(Arg.Any<GenericMessage>()));
        }

        [Then]
        public void PublishFails()
        {
            Assert.False(_result);
        }
    }
}