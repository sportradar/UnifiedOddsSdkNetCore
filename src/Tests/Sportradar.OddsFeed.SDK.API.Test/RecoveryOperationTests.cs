/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class RecoveryOperationTests
    {
        private static FakeTimeProvider _timeProvider;

        private static readonly Producer LiveProducer = new Producer(1, "LO", "Live Odds", "https://api.betradar.com/v1/liveodds/", true, 20, 1800, "live");
        private static readonly Producer PremiumCricketProducer = new Producer(5, "PremiumCricket", "Premium Cricket", "https://api.betradar.com/v1/premium_cricket/", true, 20, 1800, "prematch|live");

        private Mock<IRecoveryRequestIssuer> _recoveryRequestIssuerMock;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _timeProvider = new FakeTimeProvider();
            TimeProviderAccessor.SetTimeProvider(_timeProvider);
        }

        [TestInitialize]
        public void Setup()
        {
            _timeProvider.Now = DateTime.Now;
            _recoveryRequestIssuerMock = new Mock<IRecoveryRequestIssuer>();
            _recoveryRequestIssuerMock.SetupSequence(arg => arg.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()))
                .ReturnsAsync(1)
                .ReturnsAsync(2)
                .ReturnsAsync(3)
                .ReturnsAsync(4)
                .ReturnsAsync(5)
                .ReturnsAsync(6)
                .ReturnsAsync(7)
                .ReturnsAsync(8);
            _recoveryRequestIssuerMock.SetupSequence(arg => arg.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .ReturnsAsync(1)
                .ReturnsAsync(2)
                .ReturnsAsync(3)
                .ReturnsAsync(4)
                .ReturnsAsync(5)
                .ReturnsAsync(6)
                .ReturnsAsync(7)
                .ReturnsAsync(8);
        }

        [TestMethod]
        public void starting_already_running_operation_throws()
        {
            LiveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            var result = operation.Start();
            Assert.IsTrue(result);

            result = operation.Start();
            Assert.IsFalse(result);

            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(RecoveryInitiationException))]
        public void starting_recovery_with_after_to_far_in_past_throws()
        {
            var producer = LiveProducer;
            producer.SetLastTimestampBeforeDisconnect(_timeProvider.Now - (producer.MaxAfterAge() + TimeSpan.FromSeconds(30)));
            var operation = new RecoveryOperation(producer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, true);
            try
            {
                operation.Start();
            }
            catch(Exception)
            {
                _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Never);
                _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Never);
                producer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
                throw;
            }
        }

        [TestMethod]
        public void starting_recovery_with_after_to_far_in_past_dont_throw()
        {
            var producer = LiveProducer;
            producer.SetLastTimestampBeforeDisconnect(_timeProvider.Now - (producer.MaxAfterAge() + TimeSpan.FromSeconds(30)));
            var operation = new RecoveryOperation(producer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Never);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            producer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
        }

        [TestMethod]
        public void properties_have_correct_value_after_start()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.IsRunning);
            Assert.AreEqual(operation.RequestId, 1);
            Assert.IsTrue(operation.HasTimedOut() == false);
        }

        [TestMethod]
        public void time_out_cannot_be_complete_if_operation_has_not_timed_out()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var result = operation.CompleteTimedOut();
            Assert.IsTrue(operation.IsRunning);
            Assert.IsFalse(operation.HasTimedOut());
            Assert.IsNull(result);
        }

        [TestMethod]
        public void operation_times_out_after_allotted_time()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(LiveProducer.MaxRecoveryTime * 60 - 1);
            Assert.IsFalse(operation.HasTimedOut());
            _timeProvider.AddSeconds(2);
            Assert.IsTrue(operation.HasTimedOut());
        }

        [TestMethod]
        public void values_after_time_out_are_correct()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(LiveProducer.MaxRecoveryTime * 60 + 1);
            Assert.IsTrue(operation.HasTimedOut());
            var result = operation.CompleteTimedOut();
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(result.RequestId, 1);
            Assert.AreEqual(result.InterruptedAt, null);
            Assert.AreEqual(result.StartTime, startTime);
            Assert.IsTrue(result.TimedOut);
            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.RequestId);
        }

        [TestMethod]
        public void values_after_completion_are_correct()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(60);

            RecoveryResult result;
            var completed = operation.TryComplete(MessageInterest.AllMessages, out result);
            Assert.IsTrue(completed);
            Assert.IsNotNull(result);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(result.RequestId, 1);
            Assert.AreEqual(result.StartTime, startTime);
            Assert.IsNull(result.InterruptedAt);
            Assert.IsFalse(result.TimedOut);

            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.RequestId);
        }

        [TestMethod]
        public void setting_interrupt_on_not_running_instance_throws()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Interrupt(_timeProvider.Now);
            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.InterruptionTime);
        }

        [TestMethod]
        public void first_interrupt_time_is_recorded()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();

            _timeProvider.AddSeconds(60);
            var interruptTime = _timeProvider.Now;
            operation.Interrupt(interruptTime);


            _timeProvider.AddSeconds(60);
            operation.Interrupt(_timeProvider.Now);

            RecoveryResult result;
            operation.TryComplete(MessageInterest.AllMessages, out result);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.InterruptedAt, interruptTime);
        }

        [TestMethod]
        public void resetting_not_running_operation_does_nothing()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Reset();

            Assert.IsFalse(operation.IsRunning);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void operation_can_be_started_after_reset()
        {
            LiveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            Assert.IsTrue(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Once);
            Assert.IsTrue(operation.IsRunning);
            operation.Reset();
            Assert.IsFalse(operation.IsRunning);
            Assert.IsTrue(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [TestMethod]
        public void works_correct_with_only_one_session()
        {
            RecoveryResult result;
            RecoveryOperation operation;

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.AllMessages, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.VirtualSportMessages, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.HighPriorityMessages, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LowPriorityMessages, out result));

            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
            Assert.IsFalse(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out result));
            Assert.IsFalse(operation.TryComplete(MessageInterest.VirtualSportMessages, out result));
            Assert.IsFalse(operation.TryComplete(MessageInterest.HighPriorityMessages, out result));
            Assert.IsFalse(operation.TryComplete(MessageInterest.LowPriorityMessages, out result));
            Assert.IsTrue(operation.TryComplete(MessageInterest.AllMessages, out result));
        }

        [TestMethod]
        public void works_ok_with_hi_and_low_interests()
        {
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();

            RecoveryResult result;
            Assert.IsFalse(operation.TryComplete(MessageInterest.LowPriorityMessages, out result));
            Assert.IsNull(result);

            Assert.IsTrue(operation.TryComplete(MessageInterest.HighPriorityMessages, out result));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void works_correct_when_using_scopes()
        {
            RecoveryResult result;
            RecoveryOperation operation;

            operation = new RecoveryOperation(PremiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
            Assert.IsTrue(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out result));

            operation = new RecoveryOperation(PremiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out result));
            Assert.IsFalse(operation.TryComplete(MessageInterest.VirtualSportMessages, out result));
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
        }

        [TestMethod]
        public void operation_can_be_reused()
        {
            RecoveryResult result;
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(60);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            _timeProvider.AddSeconds(1800);
            operation.Start();
            _timeProvider.AddSeconds(120);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            operation.Start();
            _timeProvider.AddSeconds(LiveProducer.MaxRecoveryTime * 60 + 1);
            Assert.IsTrue(operation.HasTimedOut());
            Assert.IsNotNull(operation.CompleteTimedOut());

            _timeProvider.AddSeconds(10);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
        }

        [TestMethod]
        public void correct_recovery_operation_is_invoked()
        {
            RecoveryResult result;
            LiveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));

            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);

            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
        }

        [TestMethod]
        public void recovery_is_called_with_correct_after()
        {
            RecoveryResult result;
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), LiveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(90));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), LiveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(321));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), LiveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
        }

        [TestMethod]
        public void recovery_is_called_with_correct_node_id()
        {
            RecoveryResult result;
            LiveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            operation = new RecoveryOperation(LiveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 7, false);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 7), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));

            LiveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 7), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out result));
        }
    }
}
