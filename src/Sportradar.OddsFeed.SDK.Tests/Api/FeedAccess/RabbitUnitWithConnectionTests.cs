// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Feed.Messages;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// Normal feed operation against the broker. Assertions are on the message receiver.
/// </summary>
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitUnitWithConnectionTests : RabbitFeedTestBase, IClassFixture<RabbitFeedFixture>
{
    public RabbitUnitWithConnectionTests(RabbitFeedFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task WhenTheConnectionCheckTicksBeforeOpenAndAfterCloseThenNothingExtraIsDelivered()
    {
        var timer = new ManualConnectionCheckTimer();
        using var feed = CreateFeed(CreateConfiguration(), timer: timer);

        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Connection check skipped because the channel is not opened");
        feed.Channel.Channel.ShouldBeNull();

        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));
        timer.Elapse();
        await WaitUntilChannelIsReady(feed.Channel);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);

        feed.Receiver.Close();
        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Connection check skipped because the channel is closed");

        PublishOddsChange(matchId: 2);
        var arrived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.ReceivedEvent("sr:match:2"), 50, 500);
        arrived.ShouldBeFalse();
    }

    [Fact]
    public async Task WhenOddsChangeIsPublishedThenTheReceiverGetsItWithSportIdEventUrnAndSentAt()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);

        var sent = PublishOddsChange(matchId: 42, sportId: 2, headerTimestamp: 1_700_000_000_123);
        var received = await WaitForReceipt(feed, 1);

        AssertSameOdds(received, sent);
        received.SportId.ShouldBe(Urn.Parse("sr:sport:2"));
        received.EventUrn.ShouldBe(Urn.Parse("sr:match:42"));
        received.IsEventRelated.ShouldBeTrue();
        received.GeneratedAt.ShouldBe(sent.GeneratedAt);
        received.SentAt.ShouldBe(1_700_000_000_123);
        feed.Connection.CreateConnection().ClientProperties["SrUfSdkBId"].ToString().ShouldBe("123");
        feed.Channel.ChannelFactory.ConnectionCreated.ShouldNotBe(DateTime.MinValue);
    }

    [Fact]
    public async Task WhenAliveIsPublishedThenTheReceiverGetsItWithoutAnEventUrn()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var alive = FeedMessageBuilder.Create(1).BuildAlive(1);

        Publish(alive);
        var received = await WaitForReceipt(feed, 1);

        received.ShouldBeOfType<alive>();
        received.EventUrn.ShouldBeNull();
        received.IsEventRelated.ShouldBeFalse();
    }

    [Fact]
    public async Task WhenASystemMessageIsPublishedThenTheReceiverGetsItOnTheSystemSession()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), interest: null);
        var sent = FeedMessageBuilder.Create(1).BuildSnapshotComplete(7);

        Publish(sent);
        var received = await WaitForReceipt(feed, 1);

        received.ShouldBeOfType<snapshot_complete>();
        ((snapshot_complete)received).request_id.ShouldBe(sent.request_id);
        received.EventUrn.ShouldBeNull();
        feed.Channel.BasicConsumer.ConsumerTags.ShouldContain(tag => tag.Contains("|system|"));
        TrafficLogger.Messages.ShouldContain(message => message.Contains("<~> system <~>"));
    }

    [Fact]
    public async Task WhenRoutingKeyCannotBeParsedThenTheMessageIsStillReceived()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var sent = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000).Build();

        Publish(sent, "hi.-.live.odds_change.nope.sr:match.1.-");
        var received = await WaitForReceipt(feed, 1);

        received.SportId.ShouldBeNull();
        ReceiverLogger.Messages.ShouldContain(message => message.Contains("Failed to parse the SportId"));
    }

    [Fact]
    public async Task WhenEventIdIsEmptyThenEventUrnIsNotSet()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var sent = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000).Build();
        sent.event_id = string.Empty;

        Publish(sent, "hi.-.live.odds_change.1.sr:match.1.-");
        var received = await WaitForReceipt(feed, 1);

        received.EventUrn.ShouldBeNull();
        received.IsEventRelated.ShouldBeTrue();
    }

    [Fact]
    public async Task WhenHeaderUsesTheWrongKeyThenSentAtFallsBack()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var sent = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000)
                                    .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5)).Build();

        Publish(sent, headers: new Dictionary<string, object> { { "time_in_ms", 1_700_000_000_999L } });
        var received = await WaitForReceipt(feed, 1);

        received.SentAt.ShouldBe(received.GeneratedAt + 1);
    }

    [Fact]
    public async Task WhenHeaderTimestampIsNotANumberThenSentAtFallsBack()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var sent = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000)
                                    .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5)).Build();

        Publish(sent, headers: new Dictionary<string, object> { { "timestamp_in_ms", "not-a-timestamp" } });
        var received = await WaitForReceipt(feed, 1);

        received.SentAt.ShouldBe(received.GeneratedAt + 1);
    }

    [Fact]
    public async Task WhenRawHandlerThrowsThenTheReceiverStillGetsTheMessage()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var rawInvoked = false;
        feed.Receiver.RawFeedMessageReceived += (_, _) =>
                                                {
                                                    rawInvoked = true;
                                                    throw new InvalidOperationException("raw handler failed");
                                                };

        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
        rawInvoked.ShouldBeTrue();
        ReceiverLogger.Messages.ShouldContain(message => message.Contains("Error dispatching raw message for"));
    }

    [Fact]
    public async Task WhenFeedMessageReceivedIsNotSubscribedThenTheMessageIsStillProcessed()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages, trackMessages: false);
        var rawInvoked = false;
        feed.Receiver.RawFeedMessageReceived += (_, _) => rawInvoked = true;

        PublishOddsChange();
        await WaitUntilLogged(ReceiverLogger, "Message received.");

        rawInvoked.ShouldBeTrue();
        feed.Count.ShouldBe(0);
    }

    [Fact]
    public async Task WhenReceiverIsClosedAndReopenedThenTheNextMessageArrives()
    {
        var config = CreateConfiguration();
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);

        feed.Receiver.Close();
        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));
        await WaitUntilChannelIsReady(feed.Channel);

        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    [Fact]
    public async Task WhenOpenedTwiceThenThrows()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);

        var exception = Should.Throw<InvalidOperationException>(() => feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages)));

        exception.Message.ShouldBe("The instance is already opened");
    }

    [Fact]
    public void WhenOpenedWithoutRoutingKeysThenThrows()
    {
        using var feed = CreateFeed(CreateConfiguration());

        var exception = Should.Throw<ArgumentNullException>(() => feed.Receiver.Open(MessageInterest.AllMessages, Array.Empty<string>()));

        exception.Message.ShouldContain("Missing routing keys");
    }

    [Fact]
    public void WhenClosedBeforeOpenThenThrows()
    {
        using var feed = CreateFeed(CreateConfiguration());

        var exception = Should.Throw<InvalidOperationException>(() => feed.Receiver.Close());

        exception.Message.ShouldBe("The instance is already closed");
    }

    [Fact]
    public async Task WhenClosedTwiceThenThrowsAndLogsTheChannelNumber()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var channelNumber = feed.Channel.Channel.ChannelNumber;

        feed.Receiver.Close();
        var exception = Should.Throw<InvalidOperationException>(() => feed.Receiver.Close());

        exception.GetType().ShouldBe(typeof(InvalidOperationException));
        exception.Message.ShouldBe("The instance is already closed");
        ChannelLogger.Messages.ShouldContain(message => message.Contains($"Closing the channel with channelNumber: {channelNumber}"));
        ChannelLogger.Messages.ShouldContain(message => message.Contains("this channel is already closed"));
    }

    [Fact]
    public async Task WhenClosedAgainAfterTheModelIsDisposedThenItStillReportsAlreadyClosed()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var model = feed.Channel.Channel;

        feed.Receiver.Close();
        feed.Channel.Channel = model;
        var exception = Should.Throw<InvalidOperationException>(() => feed.Receiver.Close());

        exception.GetType().ShouldBe(typeof(InvalidOperationException));
        exception.Message.ShouldBe("The instance is already closed");
        ChannelLogger.Messages.ShouldContain(message => message.Contains("this channel is already closed"));
    }

    [Fact]
    public async Task WhenTwoChannelsShareTheFactoryThenBothReceiversGetMessagesOnOneConnection()
    {
        var config = CreateConfiguration();
        using var first = await OpenFeedAsync(config, MessageInterest.AllMessages);
        using var second = await OpenFeedAsync(config, MessageInterest.AllMessages, connection: first.Connection, disposeConnection: false);

        var oneConnection = await TestExecutionHelper.WaitToCompleteAsync(() => CountConnections(Project.SdkRabbitUsername) == 1, 200, 10000);
        oneConnection.ShouldBeTrue($"Expected one SDK connection, found {CountConnections(Project.SdkRabbitUsername)}.");
        var sent = PublishOddsChange();

        AssertSameOdds(await WaitForReceipt(first, 1), sent);
        AssertSameOdds(await WaitForReceipt(second, 1), sent);
        first.Channel.Channel.ShouldNotBeSameAs(second.Channel.Channel);
    }

    [Fact]
    public async Task WhenCreateConnectionIsCalledTwiceWhileOpenThenTheSameConnectionIsUsed()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var first = feed.Connection.CreateConnection();
        var second = feed.Connection.CreateConnection();

        second.ShouldBeSameAs(first);
        feed.Connection.IsConnected().ShouldBeTrue();
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenCreateConnectionIsCalledFromMultipleThreadsThenOneConnectionIsCreated()
    {
        using var feed = CreateFeed(CreateConfiguration());
        var results = new IConnection[8];

        Parallel.For(0, results.Length, index => results[index] = feed.Connection.CreateConnection());

        foreach (var result in results)
        {
            result.ShouldBeSameAs(results[0]);
            result.IsOpen.ShouldBeTrue();
        }

        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));
        await WaitUntilChannelIsReady(feed.Channel);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public void ChannelFactoryReportsMinValueBeforeTheConnectionIsOpened()
    {
        using var feed = CreateFeed(CreateConfiguration());

        feed.Channel.ChannelFactory.ConnectionCreated.ShouldBe(DateTime.MinValue);
        feed.Connection.IsConnected().ShouldBeFalse();
    }

    [Fact]
    public async Task WhenBookmakerDetailsAreMissingThenTheReceiverStillGetsTheMessage()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(withBookmaker: false), MessageInterest.AllMessages);

        await PublishAndAwaitOddsChange(feed, 1);
        feed.Connection.ConnectionFactory.ClientProperties["SrUfSdkBId"].ShouldBeNull();
        feed.Connection.ConnectionFactory.UserName.ShouldBe(Project.SdkRabbitUsername);
    }

    [Fact]
    public async Task WhenTheConnectionIsNewerThanTheChannelThenTheChannelIsRecreatedAndTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var connectionName = await WaitForConnectionName(Project.SdkRabbitUsername);
        var started = feed.Channel.ChannelStarted;
        feed.Channel.LastMessageReceived = DateTime.Now;
        var backdated = started.AddMinutes(-1);
        feed.Channel.ChannelStarted = backdated;

        await WaitUntilLogged(ChannelLogger, "Recreating connection channel and attaching events");
        await WaitForChannelRestart(feed.Channel, backdated);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldBe(connectionName);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenSilenceElapsesOnAnOpenConnectionThenTheChannelIsRecreatedAndTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var connectionName = await WaitForConnectionName(Project.SdkRabbitUsername);
        var started = feed.Channel.ChannelStarted;
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);

        await WaitUntilLogged(ChannelLogger, "There were no messages in more than");
        await WaitForChannelRestart(feed.Channel, started);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldBe(connectionName);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenNoMessageArrivesAfterChannelStartThenTheOpenChannelIsRecreated()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var backdated = DateTime.Now.AddMinutes(-10);
        feed.Channel.ChannelStarted = backdated;

        await WaitUntilLogged(ChannelLogger, "There were no messages in more than");
        await WaitForChannelRestart(feed.Channel, backdated);

        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTheChannelIsMissingSinceStartThenItIsCreatedAndTheMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var connectionName = await WaitForConnectionName(Project.SdkRabbitUsername);
        feed.Channel.Channel = null;
        feed.Channel.ChannelStarted = DateTime.Now.AddMinutes(-10);

        await WaitUntilLogged(ChannelLogger, "There were no messages in more than");
        await WaitUntilChannelIsReady(feed.Channel);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldBe(connectionName);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTheChannelIsMissingAndSilenceElapsesThenItIsCreatedAndTheMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        feed.Channel.Channel = null;

        await WaitUntilLogged(ChannelLogger, "There were no messages in more than");
        await WaitUntilChannelIsReady(feed.Channel);

        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenOddsChangeProcessingIsSlowThenTheLogIncludesMarketCounts()
    {
        await AssertSlowLog(OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000)
                                             .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5).WithOutcome("2", 2.5)).Build(),
                            "Markets=1",
                            "Outcomes=2");
    }

    [Fact]
    public async Task WhenOddsChangeHasNoMarketsAndProcessingIsSlowThenTheLogShowsZero()
    {
        var message = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000)
                                       .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5)).Build();
        message.odds.market = null;
        await AssertSlowLog(message, "Markets=0", "Outcomes=0");
    }

    [Fact]
    public async Task WhenOddsChangeHasNoOddsAndProcessingIsSlowThenTheLogShowsZero()
    {
        var message = OddsChangeBuilder.Create().WithProduct(1).WithMatchId(1).WithTimestamp(1_700_000_000_000)
                                       .AddMarket(market => market.WithMarketId(1).WithOutcome("1", 1.5)).Build();
        message.odds = null;
        await AssertSlowLog(message, "Markets=0", "Outcomes=0");
    }

    [Fact]
    public async Task WhenBetStopProcessingIsSlowThenTheLogShowsZeroMarkets()
    {
        await AssertSlowLog(FeedMessageBuilder.Create(1).BuildBetStop(1), "Markets=0", "Outcomes=0");
    }

    [Fact]
    public async Task WhenBetSettlementProcessingIsSlowThenTheLogIncludesOutcomeCounts()
    {
        var message = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                          .WithSportId(UrnCreate.SportId(1))
                                          .WithProducerId(1)
                                          .WithTimestamp(1_700_000_000_000)
                                          .AddMarket(1, "type=correct_score")
                                          .AddOutcome(1, 1)
                                          .AddOutcome(2, 2)
                                          .AddOutcome(3, 5)
                                          .Build();
        await AssertSlowLog(message, "Markets=1", "Outcomes=3");
    }

    [Fact]
    public async Task WhenBetSettlementHasNoMarketsAndProcessingIsSlowThenTheLogShowsZero()
    {
        var message = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                          .WithSportId(UrnCreate.SportId(1))
                                          .WithProducerId(1)
                                          .WithTimestamp(1_700_000_000_000)
                                          .Build();
        await AssertSlowLog(message, "Markets=0", "Outcomes=0");
    }

    [Fact]
    public async Task WhenBetSettlementOutcomesAreNullAndProcessingIsSlowThenTheLogShowsZero()
    {
        var message = BetSettlementBuilder.CreateForSportEvent("sr:match:12345")
                                          .WithSportId(UrnCreate.SportId(1))
                                          .WithProducerId(1)
                                          .WithTimestamp(1_700_000_000_000)
                                          .Build();
        message.outcomes = null;
        await AssertSlowLog(message, "Markets=0", "Outcomes=0");
    }

    [Fact]
    public async Task WhenConnectionBlockedIsRaisedThenTheFactoryHandlerIsCalled()
    {
        await AssertConnectionEvent("_recordedBlockedEventHandlers", new ConnectionBlockedEventArgs("Connection blocked for testing"), subscribed: true);
    }

    [Fact]
    public async Task WhenConnectionBlockedIsRaisedWithoutAFactorySubscriberThenNothingIsCalled()
    {
        await AssertConnectionEvent("_recordedBlockedEventHandlers", new ConnectionBlockedEventArgs("Connection blocked for testing"), subscribed: false);
    }

    [Fact]
    public async Task WhenConnectionUnblockedIsRaisedThenTheFactoryHandlerIsCalled()
    {
        await AssertConnectionEvent("_recordedUnblockedEventHandlers", EventArgs.Empty, subscribed: true);
    }

    [Fact]
    public async Task WhenConnectionUnblockedIsRaisedWithoutAFactorySubscriberThenNothingIsCalled()
    {
        await AssertConnectionEvent("_recordedUnblockedEventHandlers", EventArgs.Empty, subscribed: false);
    }

    [Fact]
    public async Task WhenConnectionShutdownIsRaisedThenTheFactoryHandlerIsCalled()
    {
        await AssertConnectionEvent("_recordedShutdownEventHandlers", new ShutdownEventArgs(ShutdownInitiator.Application, 1, "Connection shutdown for testing"), subscribed: true);
    }

    [Fact]
    public async Task WhenConnectionShutdownIsRaisedWithoutAFactorySubscriberThenNothingIsCalled()
    {
        await AssertConnectionEvent("_recordedShutdownEventHandlers", new ShutdownEventArgs(ShutdownInitiator.Application, 1, "Connection shutdown for testing"), subscribed: false);
    }

    [Fact]
    public async Task WhenCallbackExceptionIsRaisedThenTheFactoryHandlerIsCalled()
    {
        await AssertCallbackException(subscribed: true);
    }

    [Fact]
    public async Task WhenCallbackExceptionIsRaisedWithoutAFactorySubscriberThenNothingIsCalled()
    {
        await AssertCallbackException(subscribed: false);
    }

    [Fact]
    public async Task DisposeClosesTheConnectionAndASecondDisposeIsSafe()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1);

        feed.Connection.Dispose();
        feed.Connection.Dispose();

        feed.Connection.IsConnected().ShouldBeFalse();
        feed.Connection.ConnectionCreated.ShouldBe(DateTime.MinValue);
    }

    private async Task AssertSlowLog(FeedMessage message, string markets, string outcomes)
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages, parser: new DelayingRoutingKeyParser());
        Publish(message);

        var received = await WaitForReceipt(feed, 1);
        received.ShouldNotBeNull();
        ReceiverLogger.Messages.ShouldContain(entry => entry.Contains("Deserialization of") && entry.Contains(markets) && entry.Contains(outcomes));
    }

    private async Task AssertConnectionEvent(string fieldName, EventArgs args, bool subscribed)
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var handled = false;
        if (subscribed)
        {
            switch (args)
            {
                case ConnectionBlockedEventArgs:
                    feed.Connection.ConnectionBlocked += (_, _) => handled = true;
                    break;
                case ShutdownEventArgs:
                    feed.Connection.ConnectionShutdown += (_, _) => handled = true;
                    break;
                default:
                    feed.Connection.ConnectionUnblocked += (_, _) => handled = true;
                    break;
            }
        }

        var connection = feed.Connection.CreateConnection();
        var invoked = TryInvokeRecordedHandler(connection, fieldName, args);

        invoked.ShouldBeTrue();
        handled.ShouldBe(subscribed);
    }

    private async Task AssertCallbackException(bool subscribed)
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var handled = false;
        if (subscribed)
        {
            feed.Connection.CallbackException += (_, _) => handled = true;
        }

        var connection = feed.Connection.CreateConnection();
        var invoked = TryInvokeCallbackException(connection);

        invoked.ShouldBeTrue();
        handled.ShouldBe(subscribed);
    }

    private static bool TryInvokeRecordedHandler(IConnection connection, string fieldName, EventArgs args)
    {
        var field = connection.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field?.GetValue(connection) is not Delegate handler)
        {
            return false;
        }

        handler.DynamicInvoke(connection, args);
        return true;
    }

    private static bool TryInvokeCallbackException(IConnection connection)
    {
        var delegateField = connection.GetType().GetField("_delegate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var inner = delegateField?.GetValue(connection);
        if (inner == null)
        {
            return false;
        }

        var eventField = inner.GetType().GetField("CallbackException", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (eventField?.GetValue(inner) is not Delegate handler)
        {
            return false;
        }

        handler.DynamicInvoke(inner, new CallbackExceptionEventArgs(null));
        return true;
    }

    private sealed class DelayingRoutingKeyParser : IRoutingKeyParser
    {
        private readonly RegexRoutingKeyParser _inner = new RegexRoutingKeyParser();

        public Urn GetSportId(string routingKey, string messageTypeName)
        {
            return _inner.GetSportId(routingKey, messageTypeName);
        }

        public bool TryGetSportId(string routingKey, string messageTypeName, out Urn sportId)
        {
            Thread.Sleep(350);
            return _inner.TryGetSportId(routingKey, messageTypeName, out sportId);
        }
    }

    /// <summary>
    /// Raises <see cref="ISdkTimer.Elapsed"/> only when the test asks. The channel already treats this timer as its connection check.
    /// </summary>
    private sealed class ManualConnectionCheckTimer : ISdkTimer
    {
        public string TimerName => "ConnectionCheckTimer";

        public event EventHandler Elapsed;

        public void Start()
        {
        }

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
        }

        public void FireOnce(TimeSpan dueTime)
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }

        public void Elapse()
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}
