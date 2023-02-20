/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Net;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class RecoveryOperationTests
    {
        private readonly FakeTimeProvider _timeProvider = new FakeTimeProvider();
        private readonly Producer _liveProducer = new Producer(1, "LO", "Live Odds", "https://api.betradar.com/v1/liveodds/", true, 20, 1800, "live", 600);
        private readonly Producer _premiumCricketProducer = new Producer(5, "PremiumCricket", "Premium Cricket", "https://api.betradar.com/v1/premium_cricket/", true, 20, 1800, "prematch|live", 4320);
        private readonly Mock<IRecoveryRequestIssuer> _recoveryRequestIssuerMock;

        public RecoveryOperationTests()
        {
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

        [Fact]
        public void StartingAlreadyRunningOperationThrows()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            var result = operation.Start();
            Assert.True(result);

            result = operation.Start();
            Assert.False(result);

            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Never);
        }

        [Fact]
        public void StartingRecoveryWithAfterToFarInPastThrows()
        {
            var producer = _liveProducer;
            producer.SetLastTimestampBeforeDisconnect(_timeProvider.Now - (producer.MaxAfterAge() + TimeSpan.FromSeconds(30)));
            var operation = new RecoveryOperation(producer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            try
            {
                Assert.Throws<RecoveryInitiationException>(() => operation.Start());
            }
            catch (Exception)
            {
                _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Never);
                _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Never);
                producer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
                throw;
            }
        }

        [Fact]
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

        [Fact]
        public void PropertiesHaveCorrectValueAfterStart()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.True(operation.IsRunning);
            Assert.Equal(1, operation.RequestId);
            Assert.False(operation.HasTimedOut());
        }

        [Fact]
        public void TimeOutCannotBeCompleteIfOperationHasNotTimedOut()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var result = operation.CompleteTimedOut();
            Assert.True(operation.IsRunning);
            Assert.False(operation.HasTimedOut());
            Assert.Null(result);
        }

        [Fact]
        public void OperationTimesOutAfterAllottedTime()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime - 1);
            Assert.False(operation.HasTimedOut(), $"TimeProvider={_timeProvider.Now}, StartTime={operation.LastStartTime}, MaxRecoveryTime={_liveProducer.MaxAfterAge().TotalSeconds}s");
            _timeProvider.AddSeconds(2);
            Assert.True(operation.HasTimedOut());
        }

        [Fact]
        public void ValuesAfterTimeOutAreCorrect()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime + 1);
            Assert.True(operation.HasTimedOut());
            var result = operation.CompleteTimedOut();
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(1, result.RequestId);
            Assert.Null(result.InterruptedAt);
            Assert.Equal(startTime, result.StartTime);
            Assert.True(result.TimedOut);
            Assert.False(operation.IsRunning);
            Assert.Null(operation.RequestId);
        }

        [Fact]
        public void ValuesAfterCompletionAreCorrect()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            var startTime = _timeProvider.Now;
            _timeProvider.AddSeconds(60);

            var completed = operation.TryComplete(MessageInterest.AllMessages, out var result);
            Assert.True(completed);
            Assert.NotNull(result);

            Assert.True(result.Success);
            Assert.Equal(1, result.RequestId);
            Assert.Equal(startTime, result.StartTime);
            Assert.Null(result.InterruptedAt);
            Assert.False(result.TimedOut);

            Assert.False(operation.IsRunning);
            Assert.Null(operation.RequestId);
        }

        [Fact]
        public void SettingInterruptOnNotRunningInstanceThrows()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Interrupt(_timeProvider.Now);
            Assert.False(operation.IsRunning);
            Assert.Null(operation.InterruptionTime);
        }

        [Fact]
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
            Assert.NotNull(result);
            Assert.Equal(result.InterruptedAt, interruptTime);
        }

        [Fact]
        public void ResettingNotRunningOperationDoesNothing()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Reset();

            Assert.False(operation.IsRunning);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void OperationCanBeStartedAfterReset()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            Assert.True(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Once);
            Assert.True(operation.IsRunning);
            operation.Reset();
            Assert.False(operation.IsRunning);
            Assert.True(operation.Start());
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Fact]
        public void WorksCorrectWithOnlyOneSession()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.AllMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.HighPriorityMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.LowPriorityMessages, out _));

            operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.AllMessages }, 0, false);
            operation.Start();
            Assert.False(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
            Assert.False(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));
            Assert.False(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));
            Assert.False(operation.TryComplete(MessageInterest.HighPriorityMessages, out _));
            Assert.False(operation.TryComplete(MessageInterest.LowPriorityMessages, out _));
            Assert.True(operation.TryComplete(MessageInterest.AllMessages, out _));
        }

        [Fact]
        public void WorksCorrectWithHiAndLowSession()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages }, 0, false);
            operation.Start();

            Assert.False(operation.TryComplete(MessageInterest.LowPriorityMessages, out var result));
            Assert.Null(result);

            Assert.True(operation.TryComplete(MessageInterest.HighPriorityMessages, out result));
            Assert.NotNull(result);
        }

        [Fact]
        public void WorksCorrectWhenUsingScopes()
        {
            var operation = new RecoveryOperation(_premiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.False(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
            Assert.True(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));

            operation = new RecoveryOperation(_premiumCricketProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly, MessageInterest.VirtualSportMessages }, 0, false);
            operation.Start();
            Assert.False(operation.TryComplete(MessageInterest.PrematchMessagesOnly, out _));
            Assert.False(operation.TryComplete(MessageInterest.VirtualSportMessages, out _));
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [Fact]
        public void OperationCanBeReused()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            _timeProvider.AddSeconds(60);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _timeProvider.AddSeconds(1800);
            operation.Start();
            _timeProvider.AddSeconds(120);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            operation.Start();
            _timeProvider.AddSeconds(_liveProducer.MaxRecoveryTime + 1);
            Assert.True(operation.HasTimedOut());
            Assert.NotNull(operation.CompleteTimedOut());

            _timeProvider.AddSeconds(10);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [Fact]
        public void CorrectRecoveryOperationIsInvoked()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, false);
            operation.Start();
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));

            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);

            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [Fact]
        public void RecoveryIsCalledWithCorrectAdjustedAfter()
        {
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, true);

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(321));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(90));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(10));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), _liveProducer.LastTimestampBeforeDisconnect, 0), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [Fact]
        public void RecoveryIsCalledWithCorrectNodeId()
        {
            _liveProducer.SetLastTimestampBeforeDisconnect(DateTime.MinValue);
            var operation = new RecoveryOperation(_liveProducer, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 0, true);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 0), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 0), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            // resetting timestamp
            var liveProducer2 = new Producer(1, "LO", "Live Odds", "https://api.betradar.com/v1/liveodds/", true, 20, 1800, "live", 600);
            operation = new RecoveryOperation(liveProducer2, _recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, 7, false);
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), 7), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));

            liveProducer2.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromHours(1));
            operation.Start();
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), 7), Times.Once);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }

        [Fact]
        public void RecoveryIsCalledWithCorrectAdjustedTimestampAfterInitialIsForbidden()
        {
            var nodeId = 10;
            var recoveryRequestIssuerMock = new Mock<IRecoveryRequestIssuer>();
            recoveryRequestIssuerMock.SetupSequence(arg => arg.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                                       .ThrowsAsync(new CommunicationException("request is forbidden", "some url", HttpStatusCode.Forbidden, null))
                                       .ReturnsAsync(2);
            var operation = new RecoveryOperation(_liveProducer, recoveryRequestIssuerMock.Object, new[] { MessageInterest.LiveMessagesOnly }, nodeId, false);

            //initial fails with Forbidden status code
            _liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(320));
            operation.Start();
            recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once);
            Assert.False(operation.IsRunning);

            var liveProducer = new Producer(_liveProducer.Id,
                                         _liveProducer.Name,
                                         _liveProducer.Description,
                                         _liveProducer.ApiUrl,
                                         _liveProducer.IsAvailable,
                                         _liveProducer.MaxInactivitySeconds,
                                         _liveProducer.MaxRecoveryTime,
                                         "live",
                                         _liveProducer.StatefulRecoveryWindow);
            // now it should allow to adjust timestamp
            liveProducer.SetLastTimestampBeforeDisconnect(TimeProviderAccessor.Current.Now - TimeSpan.FromMinutes(3200));
            operation.Start();
            recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Exactly(2));
            Assert.True(operation.IsRunning);
            Assert.True(operation.TryComplete(MessageInterest.LiveMessagesOnly, out _));
        }
    }
}
