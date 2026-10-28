// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// Common IAM feed token used as the RabbitMQ password. The username is the bookmaker id unless rabbit credentials are set.
/// </summary>
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitUnitAuthWithCiamTests : RabbitFeedTestBase, IClassFixture<RabbitFeedFixture>
{
    public RabbitUnitAuthWithCiamTests(RabbitFeedFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task WhenUsingCiamTokenThenTheReceiverGetsTheOddsChange()
    {
        var token = NewToken();
        var cache = CacheReturning(token);
        CreateRabbitUser(BookmakerId.ToString(), token.AccessToken);
        var config = CreateCiamConfiguration();
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, tokenCache: cache.Object);

        var sent = PublishOddsChange(sportId: 2, headerTimestamp: 1_700_000_000_123);
        var received = await WaitForReceipt(feed, 1);

        AssertSameOdds(received, sent);
        received.SportId.Id.ShouldBe(2);
        received.SentAt.ShouldBe(1_700_000_000_123);
        var credentials = feed.Connection.ConnectionFactory.CredentialsProvider;
        credentials.ShouldBeAssignableTo<CredentialProviderCommonIam>();
        credentials.Name.ShouldBe("CommonIAM");
        credentials.UserName.ShouldBe(BookmakerId.ToString());
        credentials.ValidUntil.ShouldBe(TimeSpan.FromSeconds(token.ExpiresIn));
        ConnectionLogger.Messages.ShouldContain(message => message.Contains("CommonIam credentials refreshed."));
        ConnectionLogger.Messages.ShouldNotContain(message => message.Contains(token.AccessToken, StringComparison.Ordinal));
        ConnectionLogger.Messages.ShouldContain(message => message.Contains(SdkInfo.ClearSensitiveData(token.AccessToken)));
    }

    [Fact]
    public async Task WhenCiamTokenIsMissingThenTheChannelRetriesAndTheLogHasNoPassword()
    {
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(item => item.GetTokenForFeed()).ReturnsAsync((AuthenticationToken)null);
        var timer = new ManualConnectionCheckTimer();
        using var feed = CreateFeed(CreateCiamConfiguration(), tokenCache: cache.Object, timer: timer);
        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));

        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber");
        await WaitUntilLogged(ConnectionLogger, "Password: (null)");
        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Opening the channel ...", 2);
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber", 2);

        feed.Channel.Channel.ShouldBeNull();
        cache.Verify(item => item.GetTokenForFeed(), Times.Exactly(2));
        ConnectionLogger.Messages.First(message => message.Contains("CommonIam credentials refreshed."))
                        .ShouldContain($"Username: {BookmakerId},");
    }

    [Fact]
    public async Task WhenCiamTokenIsRejectedThenTheChannelRetriesWithThatToken()
    {
        var token = NewToken();
        var cache = CacheReturning(token);
        var timer = new ManualConnectionCheckTimer();
        using var feed = CreateFeed(CreateCiamConfiguration(), tokenCache: cache.Object, timer: timer);
        feed.Receiver.Open(MessageInterest.AllMessages, RoutingKeysFor(MessageInterest.AllMessages));

        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber");
        timer.Elapse();
        await WaitUntilLogged(ChannelLogger, "Opening the channel ...", 2);
        await WaitUntilLogged(ChannelLogger, "Error checking connection for channelNumber", 2);

        feed.Channel.Channel.ShouldBeNull();
        cache.Verify(item => item.GetTokenForFeed(), Times.Exactly(2));
        feed.Connection.ConnectionFactory.CredentialsProvider.UserName.ShouldBe(BookmakerId.ToString());
    }

    [Fact]
    public async Task WhenTheCiamTokenExpiresThenTheOpenConnectionStaysUp()
    {
        var token = NewToken(expiresIn: 1);
        var cache = CacheReturning(token);
        CreateRabbitUser(BookmakerId.ToString(), token.AccessToken);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(), MessageInterest.AllMessages, maxSilence: TimeSpan.FromMinutes(2), tokenCache: cache.Object);

        await Task.Delay(1500);

        feed.Channel.Channel.IsOpen.ShouldBeTrue();
        cache.Verify(item => item.GetTokenForFeed(), Times.Once);
    }

    [Fact]
    public async Task WhenTheCiamTokenExpiresAndTheConnectionIsReplacedThenTheNewTokenIsUsed()
    {
        var first = NewToken(expiresIn: 1);
        var cache = CacheReturning(first);
        CreateRabbitUser(BookmakerId.ToString(), first.AccessToken);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(), MessageInterest.AllMessages, maxSilence: TimeSpan.FromMinutes(2), tokenCache: cache.Object);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);
        await Task.Delay(1500);
        cache.Verify(item => item.GetTokenForFeed(), Times.Once);

        var second = NewToken();
        cache.Setup(item => item.GetTokenForFeed()).ReturnsAsync(second);
        CreateRabbitUser(BookmakerId.ToString(), second.AccessToken);
        var started = feed.Channel.ChannelStarted;
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-3);
        await WaitForChannelRestart(feed.Channel, started);

        cache.Verify(item => item.GetTokenForFeed(), Times.Exactly(2));
        var refreshes = ConnectionLogger.Messages.Where(message => message.Contains("CommonIam credentials refreshed.")).ToList();
        refreshes.Count.ShouldBe(2);
        refreshes.ShouldContain(message => message.Contains(SdkInfo.ClearSensitiveData(first.AccessToken)));
        refreshes.ShouldContain(message => message.Contains(SdkInfo.ClearSensitiveData(second.AccessToken)));
        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    [Fact]
    public async Task WhenRabbitUsernameAndPasswordAreSetThenTheReceiverGetsTheOddsChange()
    {
        const string username = "rabbit-user";
        const string password = "rabbit-password";
        var cache = CacheThatThrows();
        CreateRabbitUser(username, password);
        var config = CreateCiamConfiguration(username, password);
        using var feed = await OpenFeedAsync(config, MessageInterest.AllMessages, tokenCache: cache.Object);

        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
        var credentials = feed.Connection.ConnectionFactory.CredentialsProvider;
        credentials.UserName.ShouldBe(username);
        credentials.Password.ShouldBe(password);
        cache.Verify(item => item.GetTokenForFeed(), Times.Never);
    }

    [Fact]
    public async Task WhenRabbitUsernameAndPasswordAreSetThenReconnectKeepsThoseCredentials()
    {
        const string username = "rabbit-user";
        const string password = "rabbit-password";
        var cache = CacheThatThrows();
        CreateRabbitUser(username, password);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(username, password),
                                              MessageInterest.AllMessages,
                                              maxSilence: TimeSpan.FromSeconds(1),
                                              tokenCache: cache.Object);
        var started = feed.Channel.ChannelStarted;

        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(feed.Channel, started);

        feed.Channel.Channel.IsOpen.ShouldBeTrue();
        feed.Connection.ConnectionFactory.CredentialsProvider.UserName.ShouldBe(username);
        feed.Connection.ConnectionFactory.CredentialsProvider.Password.ShouldBe(password);
        cache.Verify(item => item.GetTokenForFeed(), Times.Never);
    }

    [Fact]
    public async Task WhenOnlyRabbitUsernameIsSetThenTheFeedTokenIsThePassword()
    {
        const string username = "rabbit-user";
        var token = NewToken();
        var cache = CacheReturning(token);
        CreateRabbitUser(username, token.AccessToken);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(rabbitUsername: username), MessageInterest.AllMessages, tokenCache: cache.Object);

        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
        feed.Connection.ConnectionFactory.CredentialsProvider.UserName.ShouldBe(username);
        cache.Verify(item => item.GetTokenForFeed(), Times.Once);
    }

    [Fact]
    public async Task WhenOnlyRabbitUsernameIsSetThenReconnectUsesAFreshToken()
    {
        const string username = "rabbit-user";
        var first = NewToken();
        var cache = CacheReturning(first);
        CreateRabbitUser(username, first.AccessToken);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(rabbitUsername: username),
                                              MessageInterest.AllMessages,
                                              maxSilence: TimeSpan.FromSeconds(1),
                                              tokenCache: cache.Object);
        await PublishAndAwaitOddsChange(feed, 1, matchId: 1);

        var second = NewToken();
        cache.Setup(item => item.GetTokenForFeed()).ReturnsAsync(second);
        CreateRabbitUser(username, second.AccessToken);
        var started = feed.Channel.ChannelStarted;
        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(feed.Channel, started);

        cache.Verify(item => item.GetTokenForFeed(), Times.Exactly(2));
        await PublishAndAwaitOddsChange(feed, 2, matchId: 2);
    }

    [Fact]
    public async Task WhenOnlyRabbitPasswordIsSetThenTheUsernameIsTheBookmakerId()
    {
        const string password = "rabbit-password";
        var cache = CacheThatThrows();
        CreateRabbitUser(BookmakerId.ToString(), password);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(rabbitPassword: password), MessageInterest.AllMessages, tokenCache: cache.Object);

        var sent = PublishOddsChange();
        AssertSameOdds(await WaitForReceipt(feed, 1), sent);
        var credentials = feed.Connection.ConnectionFactory.CredentialsProvider;
        credentials.UserName.ShouldBe(BookmakerId.ToString());
        credentials.Password.ShouldBe(password);
        cache.Verify(item => item.GetTokenForFeed(), Times.Never);
    }

    [Fact]
    public async Task WhenOnlyRabbitPasswordIsSetThenReconnectDoesNotReadTheTokenCache()
    {
        const string password = "rabbit-password";
        var cache = CacheThatThrows();
        CreateRabbitUser(BookmakerId.ToString(), password);
        using var feed = await OpenFeedAsync(CreateCiamConfiguration(rabbitPassword: password),
                                              MessageInterest.AllMessages,
                                              maxSilence: TimeSpan.FromSeconds(1),
                                              tokenCache: cache.Object);
        var started = feed.Channel.ChannelStarted;

        feed.Channel.LastMessageReceived = DateTime.Now.AddMinutes(-1);
        await WaitForChannelRestart(feed.Channel, started);

        feed.Channel.Channel.IsOpen.ShouldBeTrue();
        feed.Connection.ConnectionFactory.CredentialsProvider.UserName.ShouldBe(BookmakerId.ToString());
        cache.Verify(item => item.GetTokenForFeed(), Times.Never);
    }

    private static AuthenticationToken NewToken(int expiresIn = 3600)
    {
        return new AuthenticationToken(new AuthenticationTokenApiResponse
        {
            AccessToken = Guid.NewGuid().ToString("N"),
            ExpiresIn = expiresIn,
            TokenType = "Bearer"
        });
    }

    private static Mock<IAuthenticationTokenCache> CacheReturning(AuthenticationToken token)
    {
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(item => item.GetTokenForFeed()).ReturnsAsync(token);
        return cache;
    }

    private static Mock<IAuthenticationTokenCache> CacheThatThrows()
    {
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(item => item.GetTokenForFeed()).Throws(new InvalidOperationException("Token cache should not be used"));
        return cache;
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
