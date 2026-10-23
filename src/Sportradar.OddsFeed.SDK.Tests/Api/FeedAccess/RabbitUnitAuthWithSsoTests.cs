// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// SSO access token used as the RabbitMQ username and password. The broker rejects a blank password.
/// </summary>
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitUnitAuthWithSsoTests : RabbitFeedTestBase, IClassFixture<RabbitFeedFixture>
{
    public RabbitUnitAuthWithSsoTests(RabbitFeedFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task WhenUsingSsoTokenThenTheReceiverGetsTheOddsChange()
    {
        var accessToken = NewAccessToken();
        CreateRabbitUser(accessToken, accessToken);
        var config = CreateConfiguration(accessToken, accessToken, accessToken);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages);

        var sent = PublishOddsChange(sportId: 2, headerTimestamp: 1_700_000_000_123);
        var received = await WaitForReceipt(feed, 1);

        AssertSameOdds(received, sent);
        received.SportId.Id.ShouldBe(2);
        received.SentAt.ShouldBe(1_700_000_000_123);
        feed.Connection.ConnectionFactory.UserName.ShouldBe(accessToken);
        feed.Connection.ConnectionFactory.Password.ShouldBe(accessToken);
    }

    [Fact]
    public async Task WhenSsoTokenIsRejectedThenTheReceiverGetsNothingAndTheChannelRetries()
    {
        var config = CreateConfiguration(string.Empty, string.Empty, string.Empty);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, waitForChannel: false);

        await WaitUntilLogged(ChannelLogger, "Opening the channel ...", 2);
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber", 2);
        PublishOddsChange();
        var arrived = await TestExecutionHelper.WaitToCompleteAsync(() => feed.Count >= 1, 100, 1000);

        arrived.ShouldBeFalse();
        feed.Channel.Channel.ShouldBeNull();
    }

    [Fact]
    public async Task WhenSsoTokenIsNotAcceptedThenTheSameTokenIsUsedAfterTheUserIsCreated()
    {
        var accessToken = NewAccessToken();
        var config = CreateConfiguration(accessToken, accessToken, accessToken);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, waitForChannel: false);

        await WaitUntilLogged(ChannelLogger, "Opening the channel ...", 2);
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber", 2);
        feed.Channel.Channel.ShouldBeNull();
        feed.Connection.ConnectionFactory.UserName.ShouldBe(accessToken);
        feed.Connection.ConnectionFactory.Password.ShouldBe(accessToken);

        CreateRabbitUser(accessToken, accessToken);
        await WaitUntilChannelIsReady(feed.Channel);
        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
        feed.Connection.ConnectionFactory.UserName.ShouldBe(accessToken);
        feed.Connection.ConnectionFactory.Password.ShouldBe(accessToken);
    }

    [Fact]
    public async Task WhenShutdownReasonContainsTheAccessTokenThenTheLogDoesNot()
    {
        var accessToken = NewAccessToken();
        CreateRabbitUser(accessToken, accessToken);
        var config = CreateConfiguration(accessToken, accessToken, accessToken);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages);
        await PublishAndAwaitOddsChange(feed, 1);

        var invoked = InvokeBasicConsumerShutdownEvent(feed.Channel, $"Connection closed. AccessToken: {accessToken}");
        await WaitUntilLogged(ChannelLogger, "The consumer is shutdown.");

        invoked.ShouldBeTrue();
        ChannelLogger.Messages.ShouldContain(message => message.Contains("Connection closed. AccessToken: "));
        ChannelLogger.Messages.ShouldNotContain(message => message.Contains(accessToken, StringComparison.Ordinal));
        ChannelLogger.Messages.ShouldContain(message => message.Contains(SdkInfo.ClearSensitiveData(accessToken)));
    }

    [Fact]
    public async Task WhenConnectionIsResetThenTheSameSsoTokenIsUsedAndTheNextOddsChangeArrives()
    {
        var accessToken = NewAccessToken();
        CreateRabbitUser(accessToken, accessToken);
        var config = CreateConfiguration(accessToken, accessToken, accessToken);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, maxSilence: TimeSpan.FromSeconds(1));
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);

        var started = feed.Channel.ChannelStarted;
        var previousName = await WaitForConnectionName(accessToken);
        feed.Connection.CloseConnection();
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(feed.Channel, started);

        feed.Connection.ConnectionFactory.UserName.ShouldBe(accessToken);
        feed.Connection.ConnectionFactory.Password.ShouldBe(accessToken);
        var currentName = await WaitForConnectionName(accessToken);
        currentName.ShouldNotBe(previousName);
        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    private static string NewAccessToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
