﻿using System;
using JustBehave;
using NUnit.Framework;
using JustSaying.TestingFramework;

namespace AwsTools.UnitTests.SqsNotificationListener.HandlingExceptions
{
    public class WhenErrorHandlingActionIsProvided : BaseQueuePollingTest
    {
        private Action<Exception, Amazon.SQS.Model.Message> _globalErrorHandler;
        private bool _handledException;

        protected override void When()
        {
            _globalErrorHandler = (ex, m) => { _handledException = true; };

            var listener = new JustSaying.AwsTools.SqsNotificationListener(null, null, null,
                                                       onError: _globalErrorHandler);

            listener.HandleMessage(null);
        }

        [Then]
        public void NoExceptionIsThrown()
        {
            Assert.That(ThrownException, Is.Null);
        }

        [Then]
        public void CustomExceptionHandlingIsCalled()
        {
            Patiently.AssertThat(() => _handledException);
        }

        protected override void Given()
        {
        }
    }
}