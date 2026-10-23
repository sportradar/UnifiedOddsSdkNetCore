// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EasyNetQ.Management.Client;
using RabbitMQ.Client;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// Failures and recovery against the broker. A message is published only after the channel the assertion depends on exists.
/// </summary>
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitUnitWithConnectionErrorTests : RabbitFeedTestBase, IClassFixture<RabbitFeedFixture>
{
    private const int UnknownProducerId = 9999;

    public RabbitUnitWithConnectionErrorTests(RabbitFeedFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task WhenTheHostIsUnknownThenTheChannelRetriesAndTheReceiverGetsNothing()
    {
        var config = CreateConfiguration(host: "unknown_host", connectionTimeout: TimeSpan.FromSeconds(2));
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, waitForChannel: false);

        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber", 2);
        PublishOddsChange();
        var arrived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.Count >= 1, 100, 1000);

        arrived.ShouldBeFalse();
        feed.Channel.Channel.ShouldBeNull();
    }

    [Fact]
    public async Task WhenTheClientClosesTheConnectionThenShutdownLogsTheChannelNumberAndTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var channelNumber = feed.Channel.Channel.ChannelNumber;
        var started = feed.Channel.ChannelStarted;
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);

        feed.Connection.CloseConnection();
        await WaitUntilLogged(ChannelLogger, $"The channel is shutdown. ChannelNumber={channelNumber}");
        ChannelLogger.Messages.ShouldContain(message => message.Contains("Reason=Goodbye"));

        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitForChannelRestart(feed.Channel, started);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTheHeldConnectionIsNoLongerOpenThenTheFeedReplacesItAndTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);
        var held = feed.Connection.CreateConnection();
        var started = feed.Channel.ChannelStarted;

        held.Close();
        var dropped = await TestExecutionHelper.WaitToCompleteAsync(() => !feed.Connection.IsConnected(), 50, 5000);
        dropped.ShouldBeTrue();

        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitForChannelRestart(feed.Channel, started);

        feed.Connection.IsConnected().ShouldBeTrue();
        feed.Connection.ConnectionFactory.UserName.ShouldBe(Project.SdkRabbitUsername);
        feed.Connection.ConnectionFactory.Password.ShouldBe(Project.SdkRabbitPassword);
        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    [Fact]
    public async Task WhenTheBrokerClosesTheConnectionThenTheShutdownReasonIsLogged()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1);
        var name = await WaitForConnectionName(Project.SdkRabbitUsername);
        var connection = Fixture.Rabbit.ManagementClient.GetConnections().First(item => item.Name == name);

        Fixture.Rabbit.ManagementClient.CloseConnection(connection);
        await WaitUntilLogged(ChannelLogger, "is shutdown.");

        ChannelLogger.Messages.ShouldContain(message => message.Contains("Reason="));
    }

    [Fact]
    public async Task WhenTheChannelShutsDownWithTheAccessTokenThenTheLogDoesNotContainIt()
    {
        var config = CreateConfiguration();
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1);

        InvokeChannelShutdownEvent(feed.Channel, $"Channel closed. AccessToken: {config.AccessToken}");
        await WaitUntilLogged(ChannelLogger, "The channel is shutdown.");

        ChannelLogger.Messages.ShouldContain(message => message.Contains("Channel closed. AccessToken: "));
        ChannelLogger.Messages.ShouldNotContain(message => message.Contains(config.AccessToken, StringComparison.Ordinal));
    }

    [Fact]
    public async Task WhenTheConsumerShutsDownWithoutAReasonThenNoReasonIsLogged()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var invoked = InvokeBasicConsumerShutdownEvent(feed.Channel, null);
        await WaitUntilLogged(ChannelLogger, "The consumer is shutdown.");

        invoked.ShouldBeTrue();
        ChannelLogger.Messages.ShouldContain(message => message.EndsWith("Reason=", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task WhenTheChannelShutsDownWithoutAReasonThenNoReasonIsLogged()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        InvokeChannelShutdownEvent(feed.Channel, null);
        await WaitUntilLogged(ChannelLogger, "The channel is shutdown.");

        ChannelLogger.Messages.ShouldContain(message => message.EndsWith("Reason=", StringComparison.OrdinalIgnoreCase));
    }

    [RetryFact(3, 200)]
    public async Task WhenTheConnectionIsClosedBeforeSilenceElapsesThenNothingArrivesUntilTheChannelIsRecreated()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages, maxSilence: TimeSpan.FromSeconds(5));
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);
        var channel = feed.Channel.Channel;
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);
        var shutdownInvoked = false;
        feed.Connection.ConnectionShutdown += (_, _) => shutdownInvoked = true;

        feed.Connection.CloseConnection();
        PublishOddsChange(matchId: 2);
        await Task.Delay(400);

        shutdownInvoked.ShouldBeFalse();
        feed.Connection.IsConnected().ShouldBeFalse();
        feed.Channel.Channel.ShouldBeSameAs(channel);
        feed.ReceivedEvent("sr:match:2").ShouldBeFalse();

        var started = feed.Channel.ChannelStarted;
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitForChannelRestart(feed.Channel, started);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        feed.Connection.ConnectionFactory.UserName.ShouldBe(Project.SdkRabbitUsername);
        feed.Connection.ConnectionFactory.Password.ShouldBe(Project.SdkRabbitPassword);
        await PublishAndAwaitOddsChange(feed, 2, matchId: 3);

        var recoveredName = await WaitForConnectionName(Project.SdkRabbitUsername);
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(feed.Channel, feed.Channel.ChannelStarted);
        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldBe(recoveredName);
        await PublishAndAwaitOddsChange(feed, 3, matchId: 4);
    }

    [Fact]
    public async Task WhenSilenceElapsesOnAClosedConnectionThenTheNextMessageArrivesOnANewConnection()
    {
        await AssertClosedConnectionRecovers(nullTheChannel: false);
    }

    [Fact]
    public async Task WhenTheChannelIsMissingAndTheConnectionIsClosedThenTheNextMessageArrives()
    {
        await AssertClosedConnectionRecovers(nullTheChannel: true);
    }

    [Fact]
    public async Task WhenNoMessageArrivedSinceChannelStartAndTheConnectionIsClosedThenTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);
        feed.Connection.CloseConnection();
        feed.Channel.ChannelStarted = DateTime.Now.AddMinutes(-10);

        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitUntilChannelIsReady(feed.Channel);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTheChannelIsMissingSinceStartAndTheConnectionIsClosedThenTheNextMessageArrives()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        feed.Connection.CloseConnection();
        feed.Channel.ChannelStarted = DateTime.Now.AddMinutes(-10);
        feed.Channel.Channel = null;

        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitUntilChannelIsReady(feed.Channel);

        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTwoChannelsShareAClosedConnectionThenBothRecoverOnOneNewConnection()
    {
        var config = CreateConfiguration();
        using var first = await OpenFeedAsync(config, MessageInterest.AllMessages, maxSilence: TimeSpan.FromSeconds(1));
        using var second = await OpenFeedAsync(config, MessageInterest.AllMessages, maxSilence: TimeSpan.FromSeconds(1), connection: first.Connection, disposeConnection: false);
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);

        first.Connection.CloseConnection();
        first.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        second.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(first.Channel, first.Channel.ChannelStarted);
        await WaitForChannelRestart(second.Channel, second.Channel.ChannelStarted);

        var oneConnection = await TestExecutionHelper.WaitToCompleteAsync(() => CountConnections(Project.SdkRabbitUsername) == 1, 200, 10000);
        oneConnection.ShouldBeTrue();
        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(first, 1), sent);
        AssertSameOdds(await WaitForReceipt(second, 1), sent);
        first.Channel.Channel.ShouldNotBeSameAs(second.Channel.Channel);
    }

    [Fact]
    public async Task WhenTheClosedConnectionIsReplacedFromMultipleThreadsThenAllGetTheNewConnection()
    {
        using var feed = CreateFeed(CreateConfiguration());
        var first = feed.Connection.CreateConnection();
        feed.Connection.CloseConnection();
        var results = new IConnection[8];

        Parallel.For(0, results.Length, index => results[index] = feed.Connection.CreateConnection());

        foreach (var result in results)
        {
            result.ShouldBeSameAs(results[0]);
            result.ShouldNotBeSameAs(first);
            result.IsOpen.ShouldBeTrue();
        }

        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));
        await WaitUntilChannelIsReady(feed.Channel);
        await PublishAndAwaitOddsChange(feed, 1);
    }

    [Fact]
    public async Task WhenTheReceiverUnsubscribesThenTheChannelGetsTheMessageAndTheReceiverDoesNot()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var field = typeof(RabbitMqChannel).GetField("Received", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        field.SetValue(feed.Channel, null);

        PublishOddsChange();
        var channelReceived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.Channel.LastMessageReceived != DateTime.MinValue);

        channelReceived.ShouldBeTrue();
        feed.Count.ShouldBe(0);
    }

    [Fact]
    public async Task WhenTheReceiverIsClosedThenALaterMessageIsNotDelivered()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);

        feed.Receiver.Close();
        PublishOddsChange(matchId: 2);
        var arrived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.ReceivedEvent("sr:match:2"), 100, 1000);

        arrived.ShouldBeFalse();
    }

    [Fact]
    public async Task WhenTheBodyIsEmptyThenTheReceiverDoesNotDispatchIt()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);

        PublishRaw(Array.Empty<byte>(), "hi.-.live.odds_change.1.sr:match.1.-");
        await WaitUntilLogged(ReceiverLogger, "A message with empty body received.");

        feed.Count.ShouldBe(0);
        feed.FailedCount.ShouldBe(0);
    }

    [Fact]
    public async Task WhenTheBodyCannotBeParsedThenDeserializationFailedCarriesTheRawBytes()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var body = new byte[] { 1, 2, 3, 4, 100, 99 };

        PublishRaw(body, "hi.-.live.odds_change.1.sr:match.1.-");
        var failed = await TestExecutionHelper.WaitToCompleteAsync(() => feed.FailedCount >= 1);

        failed.ShouldBeTrue();
        feed.Count.ShouldBe(0);
        feed.LastFailedPayload().ShouldBe(body);
        ReceiverLogger.Messages.ShouldContain(message => message.Contains("Failed to parse message."));
    }

    [Fact]
    public async Task WhenTheRoutingKeyParserThrowsThenDeserializationFailedIsRaised()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages, parser: new ThrowingRoutingKeyParser());

        PublishOddsChange();
        var failed = await TestExecutionHelper.WaitToCompleteAsync(() => feed.FailedCount >= 1);

        failed.ShouldBeTrue();
        feed.Count.ShouldBe(0);
        ReceiverLogger.Messages.ShouldContain(message => message.Contains("Error consuming feed message."));
    }

    [Fact]
    public async Task WhenTheRoutingKeyParserThrowsAndNobodyHandlesDeserializationFailedThenTheErrorIsStillLogged()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages, parser: new ThrowingRoutingKeyParser(), trackFailures: false);

        PublishOddsChange();
        await WaitUntilLogged(ReceiverLogger, "Error consuming feed message.");

        feed.Count.ShouldBe(0);
        feed.FailedCount.ShouldBe(0);
    }

    [Fact]
    public async Task WhenTheProducerIsUnavailableThenTheMessageIsNotDispatched()
    {
        var product = TestProducerManager.Create().Producers.First(producer => !producer.IsAvailable).Id;
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);

        PublishOddsChange(product: product);
        await WaitUntilLogged(ReceiverLogger, "A message for producer which is disabled was received.");

        feed.Count.ShouldBe(0);
    }

    [Fact]
    public async Task WhenTheProducerIsUnknownThenTheMessageIsNotDispatched()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);

        PublishOddsChange(product: UnknownProducerId);
        await WaitUntilLogged(ReceiverLogger, "A message for producer which is not defined was received.");

        feed.Count.ShouldBe(0);
    }

    [Fact]
    public async Task WhenTheEnvironmentIsReplayThenAnUnavailableProducerIsStillDispatched()
    {
        var product = TestProducerManager.Create().Producers.First(producer => !producer.IsAvailable).Id;
        using var feed = await OpenFeedAsync(CreateConfiguration(environment: SdkEnvironment.Replay), MessageInterest.AllMessages);

        var sent = PublishOddsChange(product: product);
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
    }

    [Fact]
    public async Task WhenTheEnvironmentIsReplayThenAnUnknownProducerIsNotDispatched()
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(environment: SdkEnvironment.Replay), MessageInterest.AllMessages);

        PublishOddsChange(product: UnknownProducerId);
        await WaitUntilLogged(ReceiverLogger, "A message for producer which is not defined was received.");

        feed.Count.ShouldBe(0);
    }

    private async Task AssertClosedConnectionRecovers(bool nullTheChannel)
    {
        using var feed = await OpenFeedAsync(CreateConfiguration(), MessageInterest.AllMessages);
        var previousName = await WaitForConnectionName(Project.SdkRabbitUsername);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);
        feed.Connection.CloseConnection();
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        if (nullTheChannel)
        {
            feed.Channel.Channel = null;
        }

        await WaitUntilLogged(ChannelLogger, "There were no message in more than");
        await WaitUntilChannelIsReady(feed.Channel);

        (await WaitForConnectionName(Project.SdkRabbitUsername)).ShouldNotBe(previousName);
        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    private sealed class ThrowingRoutingKeyParser : IRoutingKeyParser
    {
        public Urn GetSportId(string routingKey, string messageTypeName)
        {
            throw new InvalidOperationException("routing key parser failed");
        }

        public bool TryGetSportId(string routingKey, string messageTypeName, out Urn sportId)
        {
            throw new InvalidOperationException("routing key parser failed");
        }
    }
}
