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
        private FakeTimeProvider _timeProvider;

        private readonly Producer _liveProducer = new Producer(1, "LO", "Live Odds", "https://api.betradar.com/v1/liveodds/", true, 20, 1800, "live", 600);
        private readonly Producer _premiumCricketProducer = new Producer(5, "PremiumCricket", "Premium Cricket", "https://api.betradar.com/v1/premium_cricket/", true, 20, 1800, "prematch|live", 4320);

        private Mock<IRecoveryRequestIssuer> _recoveryRequestIssuerMock;
        
        [TestInitialize]
        public void Setup()
        {
            _timeProvider = new FakeTimeProvider();
            TimeProviderAccessor.SetTimeProvider(_timeProvider);
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
        public void StartingAlreadyRunningOperationThrows()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            var result = operation.Start();
            Assert.IsTrue(result);

            result = operation.Start();
            Assert.IsFalse(result);

            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(RecoveryInitiationException))]
        public void StartingRecoveryWithAfterToFarInPastThrows()
        {
            var producer = _liveProducer;
            producer.SetLastTimestampBeforeDisconnect(_timeProvider.Now - (producer.MaxAfterAge() + TimeSpan.FromSeconds(30)));
            var operation = new RecoveryOperation(producer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
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
        public void StartingRecoveryWithAfterToFarInPastDoNotThrow()
        {
            var producer = _liveProducer;
            producer.SetLastTimestampBeforeDisconnect(_timeProvider.Now - (producer.MaxAfterAge() + TimeSpan.FromSeconds(30)));
            var operation = new RecoveryOperation(producer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, true);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Never);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            producer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
        }

        [TestMethod]
        public void PropertiesHaveCorrectValueAfterStart()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.IsRunning);
            Assert.AreEqual(1, operation.RequestId);
            Assert.IsFalse(operation.HasTimedOut());
        }

        [TestMethod]
        public void TimeOutCannotBeCompleteIfOperationHasNotTimedOut()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var result = operation.CompleteTimedOut();
            Assert.IsTrue(operation.IsRunning);
            Assert.IsFalse(operation.HasTimedOut());
            Assert.IsNull(result);
        }

        [TestMethod]
        public void OperationTimesOutAfterAllottedTime()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime - 1);
            Assert.IsFalse(operation.HasTimedOut(), $"TimeProvider={_timeProvider.Now}, StartTime={operation.LastStartTime}, MaxRecoveryTime={_liveProducer.MaxAfterAge().TotalSeconds}s");
            _timeProvider.AddSeconds(2);
            Assert.IsTrue(operation.HasTimedOut());
        }

        [TestMethod]
        public void ValuesAfterTimeOutAreCorrect()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime + 1);
            Assert.IsTrue(operation.HasTimedOut());
            var result = operation.CompleteTimedOut();
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.RequestId);
            Assert.IsNull(result.InterruptedAt);
            Assert.AreEqual(startTime, result.StartTime);
            Assert.IsTrue(result.TimedOut);
            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.RequestId);
        }

        [TestMethod]
        public void ValuesAfterCompletionAreCorrect()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(60);

            var completed = operation.TryComplete(MessageInterest.AllMessages, out var result);
            Assert.IsTrue(completed);
            Assert.IsNotNull(result);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.RequestId);
            Assert.AreEqual(startTime, result.StartTime);
            Assert.IsNull(result.InterruptedAt);
            Assert.IsFalse(result.TimedOut);

            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.RequestId);
        }

        [TestMethod]
        public void SettingInterruptOnNotRunningInstanceThrows()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Interrupt(_timeProvider.Now);
            Assert.IsFalse(operation.IsRunning);
            Assert.IsNull(operation.InterruptionTime);
        }

        [TestMethod]
        public void FirstInterruptTimeIsRecorded()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();

            _timeProvider.AddSeconds(60);
            var interruptTime = _timeProvider.Now;
            operation.Interrupt(interruptTime);


            _timeProvider.AddSeconds(60);
            operation.Interrupt(_timeProvider.Now);

            operation.TryComplete(MessageInterest.AllMessages, out var result);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.InterruptedAt, interruptTime);
        }

        [TestMethod]
        public void ResettingNotRunningOperationDoesNothing()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Reset();

            Assert.IsFalse(operation.IsRunning);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void OperationCanBeStartedAfterReset()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            Assert.IsTrue(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Once);
            Assert.IsTrue(operation.IsRunning);
            operation.Reset();
            Assert.IsFalse(operation.IsRunning);
            Assert.IsTrue(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WorksCorrectWithOnlyOneSession()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.AllMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.HighPriorityMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LowPriorityMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
            Assert.IsFalse(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));
            Assert.IsFalse(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));
            Assert.IsFalse(operation.TryComplete(MessageInterest.HighPriorityMessages, out _));
            Assert.IsFalse(operation.TryComplete(MessageInterest.LowPriorityMessages, out _));
            Assert.IsTrue(operation.TryComplete(MessageInterest.AllMessages, out _));
        }

        [TestMethod]
        public void WorksCorrectWithHiAndLowSession()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();

            Assert.IsFalse(operation.TryComplete(MessageInterest.LowPriorityMessages, out var result));
            Assert.IsNull(result);

            Assert.IsTrue(operation.TryComplete(MessageInterest.HighPriorityMessages, out result));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void WorksCorrectWhenUsingScopes()
        {
            var operation = new RecoveryOperation(_premiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
            Assert.IsTrue(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));

            operation = new RecoveryOperation(_premiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.IsFalse(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));
            Assert.IsFalse(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [TestMethod]
        public void OperationCanBeReused()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(60);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _timeProvider.AddSeconds(1800);
            operation.Start();
            _timeProvider.AddSeconds(120);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            operation.Start();
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime + 1);
            Assert.IsTrue(operation.HasTimedOut());
            Assert.IsNotNull(operation.CompleteTimedOut());

            _timeProvider.AddSeconds(10);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [TestMethod]
        public void CorrectRecoveryOperationIsInvoked()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));

            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);

            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [TestMethod]
        public void RecoveryIsCalledWithCorrectAdjustedAfter()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, true);

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(321));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(90));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [TestMethod]
        public void RecoveryIsCalledWithCorrectNodeId()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, true);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            // resetting timestamp
            var liveProducer2 = new Producer(1, "LO", "Live Odds", "https://api.betradar.com/v1/liveodds/", true, 20, 1800, "live", 600);
            operation = new RecoveryOperation(liveProducer2, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 7, false);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 7), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            liveProducer2.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 7), Times.Once);
            Assert.IsTrue(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }
    }
}
