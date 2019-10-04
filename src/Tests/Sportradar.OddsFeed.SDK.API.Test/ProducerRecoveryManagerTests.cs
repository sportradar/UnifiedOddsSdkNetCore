/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class ProducerRecoveryManagerTests
    {
        private static FakeTimeProvider _timeProvider;

        private static readonly MessageInterest DefaultInterest = MessageInterest.AllMessages;

        private Producer _producer;

        private FeedMessageBuilder _messageBuilder;

        private RecoveryOperation _recoveryOperation;

        private TimestampTracker _timestampTracker;

        private ProducerRecoveryManager _producerRecoveryManager;

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
            _recoveryRequestIssuerMock.Setup(arg => arg.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>())).ReturnsAsync(1);
            _recoveryRequestIssuerMock.Setup(arg => arg.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(1);

            CreateTestInstances();
        }

        private void CreateTestInstances()
        {
            _producer = new Producer(3, "Ctrl", "Betradar Ctrl", "https://api.betradar.com/v1/pre/", true, 20, 1800, "live");
            _messageBuilder = new FeedMessageBuilder(_producer);
            _timestampTracker = new TimestampTracker(_producer, new [] {DefaultInterest}, 20, 20);
            _recoveryOperation = new RecoveryOperation(_producer, _recoveryRequestIssuerMock.Object, new[] {DefaultInterest}, 0, false);
            _producerRecoveryManager = new ProducerRecoveryManager(_producer, _recoveryOperation, _timestampTracker);
        }

        [TestMethod]
        public void initial_status_is_correct()
        {
            Assert.IsTrue(_producerRecoveryManager.Status == ProducerRecoveryStatus.NotStarted);
        }

        [TestMethod]
        public void behaves_correctly_when_in_not_started_state()
        {
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(_producerRecoveryManager.Status, ProducerRecoveryStatus.NotStarted);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), MessageInterest.AllMessages);
            Assert.AreEqual(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildBetStop(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _timeProvider.AddSeconds(30);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

            //get everything to default state
            CreateTestInstances();
            // alive from wrong producer does nothing ...
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive(5));
            Assert.AreEqual(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
            // non-subscribed alive changes state
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive(null, null, false));
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void behaves_correctly_when_in_started_state()
        {
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), MessageInterest.AllMessages);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildBetStop(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

            // test status if recovery expires
            CreateTestInstances(); //get everything to default state
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _timeProvider.AddSeconds(_producer.MaxRecoveryTime + 10);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), MessageInterest.AllMessages);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void behaves_correctly_when_in_completed_state()
        {
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

            // lets try 'everything ok'
            CreateTestInstances();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _timeProvider.AddSeconds(10);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildBetStop(), DefaultInterest);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _timeProvider.AddSeconds(15);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

            //lets try 'alive violation'
            _timeProvider.AddSeconds(21);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);

            //lets try an 'old' odds_change message
            CreateTestInstances();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

            //lets try an 'old' bet_stop message
            CreateTestInstances();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildBetStop(null, _timeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

            // lets try 'no alive message' on user session
            CreateTestInstances();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), DefaultInterest);
            _timeProvider.AddSeconds(25);
            // without alive on system session we will get alive violation
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

            // lets try 'alive violation' and 'old message'
            CreateTestInstances();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), DefaultInterest);
            _timeProvider.AddSeconds(25);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void behaves_correctly_when_in_error_state()
        {
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _timeProvider.AddSeconds(_producer.MaxRecoveryTime + 10);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);

            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildBetStop(), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.CheckStatus();
            //simulate 'alive violation'
            _timeProvider.AddSeconds(30);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void behaves_correctly_when_in_delayed_state()
        {
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(), DefaultInterest);
            _producerRecoveryManager.CheckStatus();
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);
            // try alive violation
            _timeProvider.AddSeconds(30);
            _producerRecoveryManager.CheckStatus();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void recovery_is_restarted_after_connection_is_shutdown()
        {
            var recoveryOperationMock = new Mock<IRecoveryOperation>();
            recoveryOperationMock.Setup(x => x.Start()).Returns(true);
            _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            recoveryOperationMock.Verify(x => x.Start(), Times.Once);
            _producerRecoveryManager.ConnectionShutdown();
            Assert.AreEqual(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
            recoveryOperationMock.Verify(x => x.Reset(), Times.Once);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            recoveryOperationMock.Verify(x => x.Start(), Times.Exactly(2));
        }

        [TestMethod]
        public void recovery_is_started_after_connection_is_shutdown()
        {
            _timeProvider.Now = new DateTime(2000, 1, 1, 12, 0, 0);
            var disconnectedTime = _timeProvider.Now - TimeSpan.FromHours(2);
            _producer.SetLastTimestampBeforeDisconnect(disconnectedTime);

            var recoveryOperation = new RecoveryOperation(_producer, _recoveryRequestIssuerMock.Object, new[] {DefaultInterest}, 0, false);
            var recoveryManager = new ProducerRecoveryManager(_producer, recoveryOperation, _timestampTracker);

            recoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(_producer, disconnectedTime, 0), Times.Once);
            Assert.AreEqual(ProducerRecoveryStatus.Started, recoveryManager.Status);
            Debug.Assert(recoveryOperation.RequestId != null, "recoveryOperation.RequestId != null");
            recoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(recoveryOperation.RequestId.Value), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, recoveryManager.Status);
            recoveryManager.ProcessUserMessage(_messageBuilder.BuildAlive(_producer.Id, _timeProvider.Now), DefaultInterest);

            recoveryManager.ConnectionShutdown();
            Assert.AreEqual(ProducerRecoveryStatus.Error, recoveryManager.Status);
            recoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, recoveryManager.Status);
            _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(_producer, _timeProvider.Now, 0));
        }

        [TestMethod]
        public void unsubscribe_alive_starts_the_recovery_when_in_completed_state()
        {
            // lets get the recovery manager to completed state ...
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
            _timeProvider.AddSeconds(10);
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

            // lets feed him a unsubscribed alive ...
            _timeProvider.AddSeconds(5);
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive(null, null, false));
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

            // lets try to get it back to completed state ...
            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.AreEqual(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void correct_timestamp_is_used_when_recovery_is_interrupted_do_to_alive_violation()
        {
            var recoveryOperationMock = new Mock<IRecoveryOperation>();
            recoveryOperationMock.Setup(x => x.Start()).Returns(true);
            _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker);

            //start the recovery
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(_producerRecoveryManager.Status, ProducerRecoveryStatus.Started);
            //make sure that after the the recoveryOperationMock IsRunning returns true
            recoveryOperationMock.Setup(x => x.IsRunning).Returns(true);

            //let's go over few normal cycles without user messages
            _producerRecoveryManager.CheckStatus();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _timeProvider.AddSeconds(10);
            _producerRecoveryManager.CheckStatus();
            var lastAlive = _messageBuilder.BuildAlive();
            _producerRecoveryManager.ProcessSystemMessage(lastAlive);

            // skip few alives
            _timeProvider.AddSeconds(30);
            _producerRecoveryManager.CheckStatus();
            recoveryOperationMock.Verify(x => x.Interrupt(SdkInfo.FromEpochTime(lastAlive.timestamp)), Times.Once);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void correct_timestamp_is_used_when_recovery_is_interrupted_do_to_unsubscribed_alive()
        {
            var recoveryOperationMock = new Mock<IRecoveryOperation>();
            recoveryOperationMock.Setup(x => x.Start()).Returns(true);
            _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker);

            //start the recovery
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            Assert.AreEqual(_producerRecoveryManager.Status, ProducerRecoveryStatus.Started);
            //make sure that after the the recoveryOperationMock IsRunning returns true
            recoveryOperationMock.Setup(x => x.IsRunning).Returns(true);

            //let's go over few normal cycles without user messages
            _producerRecoveryManager.CheckStatus();
            _producerRecoveryManager.ProcessSystemMessage(_messageBuilder.BuildAlive());
            _timeProvider.AddSeconds(10);
            _producerRecoveryManager.CheckStatus();
            var lastAlive = _messageBuilder.BuildAlive();
            _producerRecoveryManager.ProcessSystemMessage(lastAlive);

            // let's feed the recovery manager with unsubscribed alive
            _timeProvider.AddSeconds(10);
            var unsubscribedAlive = _messageBuilder.BuildAlive();
            unsubscribedAlive.subscribed = 0;
            _producerRecoveryManager.ProcessSystemMessage(unsubscribedAlive);
            recoveryOperationMock.Verify(x => x.Interrupt(SdkInfo.FromEpochTime(lastAlive.timestamp)), Times.Once);
            Assert.AreEqual(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        }

        [TestMethod]
        public void received_messages_invoke_correct_method_on_timestamp_tracker()
        {
            var timestampTrackerMock = new Mock<ITimestampTracker>();
            _producerRecoveryManager = new ProducerRecoveryManager(_producer, new Mock<IRecoveryOperation>().Object, timestampTrackerMock.Object);

            var userAlive = _messageBuilder.BuildAlive();
            _producerRecoveryManager.ProcessUserMessage(userAlive, DefaultInterest);
            timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, userAlive), Times.Once);

            var systemAlive = _messageBuilder.BuildAlive();
            _producerRecoveryManager.ProcessSystemMessage(systemAlive);
            timestampTrackerMock.Verify(x => x.ProcessSystemAlive(systemAlive), Times.Once);

            var betStop = _messageBuilder.BuildBetStop();
            _producerRecoveryManager.ProcessUserMessage(betStop, DefaultInterest);
            timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, betStop), Times.Once);

            var oddsChange = _messageBuilder.BuildOddsChange();
            _producerRecoveryManager.ProcessUserMessage(oddsChange, DefaultInterest);
            timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, oddsChange), Times.Once);
        }

        [TestMethod]
        public void event_recovery_completed_is_called_with_right_args()
        {
            EventRecoveryCompletedEventArgs eventArgs = null;
            var eventId = URN.Parse("sr:match:1");

            _producerRecoveryManager.EventRecoveryCompleted += (sender, args) => eventArgs = args;
            _producer.EventRecoveries.TryAdd(9, eventId);

            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(9), DefaultInterest);
            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(9, eventArgs.GetRequestId());
            Assert.AreEqual(eventId, eventArgs.GetEventId());

            Assert.IsTrue(_producer.EventRecoveries.IsEmpty);
        }

        [TestMethod]
        public void event_recovery_completed_is_not_called_if_the_request_is_missing()
        {
            EventRecoveryCompletedEventArgs eventArgs = null;
            var eventId = URN.Parse("sr:match:1");

            _producerRecoveryManager.EventRecoveryCompleted += (sender, args) => eventArgs = args;
            _producer.EventRecoveries.TryAdd(9, eventId);

            _producerRecoveryManager.ProcessUserMessage(_messageBuilder.BuildSnapshotComplete(1), DefaultInterest);
            Assert.IsNull(eventArgs);

            Assert.IsFalse(_producer.EventRecoveries.IsEmpty);
        }
    }
}