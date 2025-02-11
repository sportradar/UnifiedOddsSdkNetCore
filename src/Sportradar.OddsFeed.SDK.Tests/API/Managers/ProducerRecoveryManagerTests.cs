// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Moq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Managers;

public class ProducerRecoveryManagerTests
{
    private static readonly FakeTimeProvider TimeProvider = new FakeTimeProvider();

    private static readonly MessageInterest DefaultInterest = MessageInterest.AllMessages;

    private Producer _producer;

    private FeedMessageBuilder _feedMessageBuilder;

    private RecoveryOperation _recoveryOperation;

    private TimestampTracker _timestampTracker;

    private ProducerRecoveryManager _producerRecoveryManager;

    private readonly Mock<IRecoveryRequestIssuer> _recoveryRequestIssuerMock;

    public ProducerRecoveryManagerTests()
    {
        TimeProviderAccessor.SetTimeProvider(TimeProvider);

        TimeProvider.Now = DateTime.Now;
        _recoveryRequestIssuerMock = new Mock<IRecoveryRequestIssuer>();
        _recoveryRequestIssuerMock.Setup(arg => arg.RequestFullOddsRecoveryAsync(It.IsAny<IProducer>(), It.IsAny<int>())).ReturnsAsync(1);
        _recoveryRequestIssuerMock.Setup(arg => arg.RequestRecoveryAfterTimestampAsync(It.IsAny<IProducer>(), It.IsAny<DateTime>(), It.IsAny<int>())).ReturnsAsync(1);
    }

    private void CreateTestInstances()
    {
        _producer = new Producer(3, "Ctrl", "Betradar Ctrl", "https://api.betradar.com/v1/pre/", true, 20, 1800, "live", 4320);
        _feedMessageBuilder = new FeedMessageBuilder(_producer.Id);
        _timestampTracker = new TimestampTracker(_producer, new[] { DefaultInterest }, 20, 20);
        _recoveryOperation = new RecoveryOperation(_producer, _recoveryRequestIssuerMock.Object, new[] { DefaultInterest }, 0);
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, _recoveryOperation, _timestampTracker, 30);
    }

    [Fact]
    public void InitialStatusIsCorrect()
    {
        CreateTestInstances();
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
    }

    [Fact]
    public void BehavesCorrectlyWhenInNotStartedState()
    {
        CreateTestInstances();
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), MessageInterest.AllMessages);
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildBetStop(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        TimeProvider.AddSeconds(30);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

        //get everything to default state
        CreateTestInstances();
        // alive from wrong producer does nothing ...
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive(5));
        Assert.Equal(ProducerRecoveryStatus.NotStarted, _producerRecoveryManager.Status);
        // non-subscribed alive changes state
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive(null, null, false));
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
    }

    [Fact]
    public void BehavesCorrectlyWhenInStartedState()
    {
        CreateTestInstances();

        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), MessageInterest.AllMessages);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildBetStop(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

        // test status if recovery expires
        CreateTestInstances(); //get everything to default state
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        TimeProvider.AddSeconds((_producer.MaxRecoveryTime * 60) + 10);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), MessageInterest.AllMessages);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
    }

    [Fact]
    public void BehavesCorrectlyWhenInCompletedState()
    {
        CreateTestInstances();

        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

        // try 'everything ok'
        CreateTestInstances();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        TimeProvider.AddSeconds(10);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildBetStop(), DefaultInterest);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        TimeProvider.AddSeconds(15);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

        // try 'alive violation'
        TimeProvider.AddSeconds(21);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);

        // try an 'old' odds_change message
        CreateTestInstances();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(null, null, null, TimeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

        // try an 'old' bet_stop message
        CreateTestInstances();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildBetStop(null, null, null, TimeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

        // try 'no alive message' on user session
        CreateTestInstances();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        TimeProvider.AddSeconds(25);
        // without alive on system session we will get alive violation
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);

        // try 'alive violation' and 'old message'
        CreateTestInstances();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        TimeProvider.AddSeconds(25);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
    }

    [Fact]
    public void BehavesCorrectlyWhenInErrorState()
    {
        CreateTestInstances();

        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        TimeProvider.AddSeconds((_producer.MaxRecoveryTime * 60) + 10);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);

        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildBetStop(), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.CheckStatus();
        //simulate 'alive violation'
        TimeProvider.AddSeconds(30);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(null, null, null, TimeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
    }

    [Fact]
    public void BehavesCorrectlyWhenInDelayedState()
    {
        CreateTestInstances();

        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(null, null, null, TimeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        _producerRecoveryManager.CheckStatus();
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildOddsChange(null, null, null, TimeProvider.Now - TimeSpan.FromSeconds(30)), DefaultInterest);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Delayed, _producerRecoveryManager.Status);
        // try alive violation
        TimeProvider.AddSeconds(30);
        _producerRecoveryManager.CheckStatus();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
    }

    [Fact]
    public void RecoveryIsNotRestartedAfterConnectionIsShutdown()
    {
        CreateTestInstances();

        var recoveryOperationMock = new Mock<IRecoveryOperation>();
        recoveryOperationMock.Setup(x => x.Start()).Returns(true);
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker, 30);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        recoveryOperationMock.Verify(x => x.Start(), Times.Once);
        _producerRecoveryManager.ConnectionShutdown();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        recoveryOperationMock.Verify(x => x.Reset(), Times.Once);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        recoveryOperationMock.Verify(x => x.Start(), Times.Exactly(1));
    }

    [Fact]
    public void RecoveryIsRestartedAfterConnectionIsUp()
    {
        CreateTestInstances();

        var recoveryOperationMock = new Mock<IRecoveryOperation>();
        recoveryOperationMock.Setup(x => x.Start()).Returns(true);
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker, 30);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        recoveryOperationMock.Verify(x => x.Start(), Times.Once);

        _producerRecoveryManager.ConnectionShutdown();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        recoveryOperationMock.Verify(x => x.Reset(), Times.Once);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        recoveryOperationMock.Verify(x => x.Start(), Times.Exactly(1));

        _producerRecoveryManager.ConnectionRecovered();
        Assert.Equal(ProducerRecoveryStatus.Error, _producerRecoveryManager.Status);
        recoveryOperationMock.Verify(x => x.Reset(), Times.Once);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        recoveryOperationMock.Verify(x => x.Start(), Times.Exactly(2));
    }

    [Fact]
    public void RecoveryIsStartedAfterConnectionIsBack()
    {
        CreateTestInstances();

        TimeProvider.Now = new DateTime(2000, 1, 1, 12, 0, 0);
        var disconnectedTime = TimeProvider.Now - TimeSpan.FromHours(2);
        _producer.SetLastTimestampBeforeDisconnect(disconnectedTime);

        var recoveryOperation = new RecoveryOperation(_producer, _recoveryRequestIssuerMock.Object, new[] { DefaultInterest }, 0);
        var recoveryManager = new ProducerRecoveryManager(_producer, recoveryOperation, _timestampTracker, 30);

        recoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(_producer, disconnectedTime, 0), Times.Once);
        Assert.Equal(ProducerRecoveryStatus.Started, recoveryManager.Status);
        Assert.NotNull(recoveryOperation.RequestId);
        recoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(recoveryOperation.RequestId.Value), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, recoveryManager.Status);
        recoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(_producer.Id, TimeProvider.Now), DefaultInterest);

        recoveryManager.ConnectionShutdown();
        Assert.Equal(ProducerRecoveryStatus.Error, recoveryManager.Status);
        recoveryManager.ConnectionRecovered();
        var time = TimeProvider.Now;
        TimeProvider.AddSeconds(40);
        recoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, recoveryManager.Status);
        _recoveryRequestIssuerMock.Verify(x => x.RequestRecoveryAfterTimestampAsync(_producer, time, 0));
    }

    [Fact]
    public void UnsubscribeAliveStartsTheRecoveryWhenInCompletedState()
    {
        CreateTestInstances();

        //  get the recovery manager to completed state ...
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        TimeProvider.AddSeconds(10);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);

        //  feed him an unsubscribed alive ...
        TimeProvider.AddSeconds(40);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildAlive(), DefaultInterest);
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive(null, null, false));
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);

        //  try to get it back to completed state ...
        TimeProvider.AddSeconds(10);
        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Equal(ProducerRecoveryStatus.Completed, _producerRecoveryManager.Status);
    }

    [Fact]
    public void CorrectTimestampIsUsedWhenRecoveryIsInterruptedDoToAliveViolation()
    {
        CreateTestInstances();

        var recoveryOperationMock = new Mock<IRecoveryOperation>();
        recoveryOperationMock.Setup(x => x.Start()).Returns(true);
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker, 30);

        //start the recovery
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        //make sure that after the recoveryOperationMock IsRunning returns true
        recoveryOperationMock.Setup(x => x.IsRunning).Returns(true);

        //let's go over few normal cycles without user messages
        _producerRecoveryManager.CheckStatus();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        TimeProvider.AddSeconds(10);
        _producerRecoveryManager.CheckStatus();
        var lastAlive = _feedMessageBuilder.BuildAlive();
        _producerRecoveryManager.ProcessSystemMessage(lastAlive);

        // skip few alives
        TimeProvider.AddSeconds(30);
        _producerRecoveryManager.CheckStatus();
        recoveryOperationMock.Verify(x => x.Interrupt(SdkInfo.FromEpochTime(lastAlive.timestamp)), Times.Once);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
    }

    [Fact]
    public void CorrectTimestampIsUsedWhenRecoveryIsInterruptedDoToUnsubscribedAlive()
    {
        CreateTestInstances();

        var recoveryOperationMock = new Mock<IRecoveryOperation>();
        recoveryOperationMock.Setup(x => x.Start()).Returns(true);
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, recoveryOperationMock.Object, _timestampTracker, 30);

        //start the recovery
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
        //make sure that after the recoveryOperationMock IsRunning returns true
        recoveryOperationMock.Setup(x => x.IsRunning).Returns(true);

        //let's go over few normal cycles without user messages
        _producerRecoveryManager.CheckStatus();
        _producerRecoveryManager.ProcessSystemMessage(_feedMessageBuilder.BuildAlive());
        TimeProvider.AddSeconds(10);
        _producerRecoveryManager.CheckStatus();
        var lastAlive = _feedMessageBuilder.BuildAlive();
        _producerRecoveryManager.ProcessSystemMessage(lastAlive);

        // let's feed the recovery manager with unsubscribed alive
        TimeProvider.AddSeconds(10);
        var unsubscribedAlive = _feedMessageBuilder.BuildAlive();
        unsubscribedAlive.subscribed = 0;
        _producerRecoveryManager.ProcessSystemMessage(unsubscribedAlive);
        recoveryOperationMock.Verify(x => x.Interrupt(SdkInfo.FromEpochTime(lastAlive.timestamp)), Times.Once);
        Assert.Equal(ProducerRecoveryStatus.Started, _producerRecoveryManager.Status);
    }

    [Fact]
    public void ReceivedMessagesInvokeCorrectMethodOnTimestampTracker()
    {
        CreateTestInstances();

        var timestampTrackerMock = new Mock<ITimestampTracker>();
        _producerRecoveryManager = new ProducerRecoveryManager(_producer, new Mock<IRecoveryOperation>().Object, timestampTrackerMock.Object, 30);

        var userAlive = _feedMessageBuilder.BuildAlive();
        _producerRecoveryManager.ProcessUserMessage(userAlive, DefaultInterest);
        timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, userAlive), Times.Once);

        var systemAlive = _feedMessageBuilder.BuildAlive();
        _producerRecoveryManager.ProcessSystemMessage(systemAlive);
        timestampTrackerMock.Verify(x => x.ProcessSystemAlive(systemAlive), Times.Once);

        var betStop = _feedMessageBuilder.BuildBetStop();
        _producerRecoveryManager.ProcessUserMessage(betStop, DefaultInterest);
        timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, betStop), Times.Once);

        var oddsChange = _feedMessageBuilder.BuildOddsChange();
        _producerRecoveryManager.ProcessUserMessage(oddsChange, DefaultInterest);
        timestampTrackerMock.Verify(x => x.ProcessUserMessage(DefaultInterest, oddsChange), Times.Once);
    }

    [Fact]
    public void EventRecoveryCompletedIsCalledWithRightArgs()
    {
        CreateTestInstances();

        EventRecoveryCompletedEventArgs eventArgs = null;
        var eventId = Urn.Parse("sr:match:1");

        _producerRecoveryManager.EventRecoveryCompleted += (_, args) => eventArgs = args;
        _producer.EventRecoveries.TryAdd(9, eventId);

        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(9), DefaultInterest);
        Assert.NotNull(eventArgs);
        Assert.Equal(9, eventArgs.GetRequestId());
        Assert.Equal(eventId, eventArgs.GetEventId());

        Assert.True(_producer.EventRecoveries.IsEmpty);
    }

    [Fact]
    public void EventRecoveryCompletedIsNotCalledIfTheRequestIsMissing()
    {
        CreateTestInstances();

        EventRecoveryCompletedEventArgs eventArgs = null;
        var eventId = Urn.Parse("sr:match:1");

        _producerRecoveryManager.EventRecoveryCompleted += (_, args) => eventArgs = args;
        _producer.EventRecoveries.TryAdd(9, eventId);

        _producerRecoveryManager.ProcessUserMessage(_feedMessageBuilder.BuildSnapshotComplete(1), DefaultInterest);
        Assert.Null(eventArgs);

        Assert.False(_producer.EventRecoveries.IsEmpty);
    }
}
