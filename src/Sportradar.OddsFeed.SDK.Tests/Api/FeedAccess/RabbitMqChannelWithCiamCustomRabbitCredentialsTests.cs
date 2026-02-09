// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RabbitMQ.Client;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Feed.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitMqChannelWithCiamCustomRabbitCredentialsTests : IAsyncLifetime
{
    private const int HttpTimeoutInSeconds = 10;
    private const int MaxConnections = 10;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly XunitLoggerFactory _loggerFactory;
    private readonly ILogger<ConfiguredConnectionFactory> _loggerCcf;
    private readonly Deserializer<FeedMessage> _deserializer = new();
    private readonly RegexRoutingKeyParser _keyParser = new();

    private IUofConfiguration _uofConfig;
    private RabbitManagement _rabbitManagement;
    private ProjectConfiguration _projConfig;

    public RabbitMqChannelWithCiamCustomRabbitCredentialsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
        _loggerCcf = _loggerFactory.CreateLogger<ConfiguredConnectionFactory>();
    }

    public async Task InitializeAsync()
    {
        _projConfig = ProjectConfigurationBuilder.Create()
                                                    .UseTestRabbitConfiguration()
                                                    .UseRandomVirtualHost()
                                                    .Build();
        _rabbitManagement = new RabbitManagement(_testOutputHelper, _projConfig);

        await StartRabbitManagementWithRetries();
    }

    public async Task DisposeAsync()
    {
        await _rabbitManagement.DeleteVirtualHostAsync().ConfigureAwait(true);
        _rabbitManagement.SafeStop();
    }

    [Fact]
    public async Task MessageReceivedWhenExplicitRabbitUsernameAndPasswordAreSet()
    {
        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, "rabbit-user", "rabbit-password");

        var eventHandled = false;
        var throwingTokenCache = SetupMockAuthenticationTokenCacheToThrow();

        _rabbitManagement.CreateUser("rabbit-user", "rabbit-password", _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, throwingTokenCache, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.FeedMessageReceived += (_, _) => { eventHandled = true; };

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        SendAnyOddsChangeMessage();

        await WaitUpTo10SecThatEventIsHandled(eventHandled);

        messageReceiver.Close();
    }

    [Fact]
    public async Task WhenExplicitRabbitUsernameAndPasswordAreSetTheSameCredentialsAreUsedOnReconnect()
    {
        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, "rabbit-user", "rabbit-password");

        var eventHandled = false;
        var throwingTokenCache = SetupMockAuthenticationTokenCacheToThrow();

        _rabbitManagement.CreateUser("rabbit-user", "rabbit-password", _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, throwingTokenCache, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        await WaitForTheChannelToBeRecreatedDueToInactivity();

        await TestExecutionHelper.WaitToCompleteAsync(() => rabbitMqChannel.Channel.IsOpen);
        rabbitMqChannel.Channel.IsOpen.ShouldBeTrue();

        messageReceiver.Close();
    }

    [Fact]
    public async Task MessageReceivedWhenOnlyExplicitRabbitUsernameIsSetAndTokenObtainedFromTokenCache()
    {
        var eventHandled = false;
        const string feedToken = "ciam-token";
        var tokenCache = SetupAuthenticationTokenCacheToReturnFeedToken(feedToken);
        const string overridenUsername = "rabbit-user";

        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, overridenUsername);

        _rabbitManagement.CreateUser(overridenUsername, feedToken, _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, tokenCache, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.FeedMessageReceived += (_, _) => { eventHandled = true; };

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        SendAnyOddsChangeMessage();

        await WaitUpTo10SecThatEventIsHandled(eventHandled);

        messageReceiver.Close();
    }

    [Fact]
    public async Task WhenOnlyExplicitRabbitUsernameIsSetFreshTokenIsObtainedFromTokenCacheOnReconnect()
    {
        const string overridenUsername = "rabbit-user";
        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, overridenUsername);
        var firstToken = GenerateValidAuthenticationTokenWithGuid();
        firstToken.ExpiresIn = 3;
        var mockAuthenticationTokenCache = new Mock<IAuthenticationTokenCache>();
        mockAuthenticationTokenCache.Setup(c => c.GetTokenForFeed()).ReturnsAsync(firstToken);

        _rabbitManagement.CreateUser(overridenUsername, firstToken.AccessToken, _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, mockAuthenticationTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var secondToken = GenerateValidAuthenticationTokenWithGuid();
        mockAuthenticationTokenCache.Setup(c => c.GetTokenForFeed()).ReturnsAsync(secondToken);

        await WaitForTheChannelToBeRecreatedDueToInactivity();

        _rabbitManagement.CreateUser(overridenUsername, secondToken.AccessToken, _uofConfig.Rabbit.VirtualHost);

        await TestExecutionHelper.WaitToCompleteAsync(() => rabbitMqChannel.Channel.IsOpen);
        rabbitMqChannel.Channel.IsOpen.ShouldBeTrue();
        mockAuthenticationTokenCache.Verify(c => c.GetTokenForFeed(), Times.Exactly(2));

        messageReceiver.Close();
    }

    [Fact]
    public async Task MessageReceivedWhenOnlyExplicitRabbitPasswordIsSetAndUsernameIsBookmakerId()
    {

        var eventHandled = false;
        var throwingTokenCache = SetupMockAuthenticationTokenCacheToThrow();
        const string overridenPassword = "rabbit-password";

        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, password: overridenPassword);

        _rabbitManagement.CreateUser(_uofConfig.BookmakerDetails.BookmakerId.ToString(), overridenPassword, _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, throwingTokenCache, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.FeedMessageReceived += (_, _) => { eventHandled = true; };

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        SendAnyOddsChangeMessage();

        await WaitUpTo10SecThatEventIsHandled(eventHandled);

        messageReceiver.Close();
    }

    [Fact]
    public async Task WhenOnlyExplicitRabbitPasswordIsSetTheSameCredentialsAreUsedOnReconnectWithoutCredentialsProviderRefresh()
    {
        var throwingTokenCache = SetupMockAuthenticationTokenCacheToThrow();
        const string overridenPassword = "rabbit-password";

        _uofConfig = GetConfigUpdatedWithProjectConfigAndRabbitCredentials(_projConfig, password: overridenPassword);

        _rabbitManagement.CreateUser(_uofConfig.BookmakerDetails.BookmakerId.ToString(), overridenPassword, _uofConfig.Rabbit.VirtualHost);

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, throwingTokenCache, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);
        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        await WaitForTheChannelToBeRecreatedDueToInactivity();

        await TestExecutionHelper.WaitToCompleteAsync(() => rabbitMqChannel.Channel.IsOpen);
        rabbitMqChannel.Channel.IsOpen.ShouldBeTrue();

        messageReceiver.Close();
    }

    private async Task WaitForTheChannelToBeRecreatedDueToInactivity()
    {
        var logger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();

        while (InactivityLogIsNotYerPresent())
        {
            await Task.Delay(100);
        };
        return;

        bool InactivityLogIsNotYerPresent()
        {
            return !logger.InnerLogger.Messages.Any(a => a.Contains("There were no messages in more than"));
        }
    }

    private static IAuthenticationTokenCache SetupMockAuthenticationTokenCacheToThrow()
    {
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(c => c.GetTokenForFeed()).Throws(new InvalidOperationException("Should not be called"));
        cache.Setup(c => c.GetTokenForApi()).Throws(new InvalidOperationException("Should not be called"));

        return cache.Object;
    }

    private static List<string> GenerateDistinctRoutingKeys(MessageInterest messageInterest)
    {
        return FeedRoutingKeyBuilder.GenerateKeys([messageInterest]).SelectMany(x => x).Distinct().ToList();
    }

    private static async Task EnsureRabbitChannelIsCreated(RabbitMqChannel rabbitMqChannel)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => rabbitMqChannel.Channel != null, 50);
        rabbitMqChannel.Channel.ShouldNotBeNull();
    }

    private static async Task WaitUpTo10SecThatEventIsHandled(bool eventHandled)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => eventHandled);
    }

    private RabbitMqMessageReceiver ConstructMessageReceiver(ConnectionFactory rabbitConnectionFactory, IUofConfiguration config, out RabbitMqChannel rabbitMqChannel)
    {
        return ConstructMessageReceiver(rabbitConnectionFactory, config, TimeSpan.FromSeconds(5), out rabbitMqChannel);
    }

    private RabbitMqMessageReceiver ConstructMessageReceiver(ConnectionFactory rabbitConnectionFactory, IUofConfiguration config, TimeSpan maxTimeBetweenMessages, out RabbitMqChannel rabbitMqChannel)
    {
        ILogger<RabbitMqChannel> channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, config, connectionLogger);
        var channelFactory = new ChannelFactory(configuredConnectionFactory);
        var timer = new SdkTimer("TestTimer", TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(3));
        rabbitMqChannel = new RabbitMqChannel(channelFactory, timer, maxTimeBetweenMessages, config.AccessToken, channelLogger);
        var messageReceiver = new RabbitMqMessageReceiver(rabbitMqChannel, _deserializer, _keyParser, TestProducerManager.Create(), config, _loggerFactory);
        return messageReceiver;
    }

    private static IUofConfiguration GetConfigUpdatedWithProjectConfigAndRabbitCredentials(ProjectConfiguration projConfig, string username = null, string password = null)
    {
        var auth = new Mock<UofClientAuthentication.IPrivateKeyJwt>();
        auth.SetupGet(x => x.ClientId).Returns("client-id");
        auth.SetupGet(x => x.SigningKeyId).Returns("signing-key-id");
        auth.SetupGet(x => x.PrivateKey).Returns(new RsaSecurityKey(RSA.Create()));
        auth.SetupGet(x => x.Host).Returns("localhost");
        auth.SetupGet(x => x.Port).Returns(80);
        auth.SetupGet(x => x.UseSsl).Returns(false);

        var tenSeconds = TimeSpan.FromSeconds(HttpTimeoutInSeconds);
        var mockApiConfiguration = new Mock<IUofApiConfiguration>();
        mockApiConfiguration.SetupGet(x => x.Host).Returns("localhost");
        mockApiConfiguration.SetupGet(x => x.BaseUrl).Returns("http://localhost/api/");
        mockApiConfiguration.SetupGet(x => x.UseSsl).Returns(false);
        mockApiConfiguration.SetupGet(x => x.HttpClientTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientRecoveryTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.HttpClientFastFailingTimeout).Returns(tenSeconds);
        mockApiConfiguration.SetupGet(x => x.MaxConnectionsPerServer).Returns(MaxConnections);

        var mockBookmakerDetails = new Mock<IBookmakerDetails>();
        mockBookmakerDetails.SetupGet(b => b.BookmakerId).Returns(123);
        var uofConfig = new UofConfigurationBuilder()
                       .WithAuthenticationConfiguration(auth.Object)
                       .WithBookmakerDetails(mockBookmakerDetails.Object)
                       .WithRabbitConfiguration(rabbit =>
                                                {
                                                    var rabbitConfigBuilder = rabbit
                                                                             .WithHost(projConfig.RabbitHost)
                                                                             .WithPort(projConfig.RabbitPort)
                                                                             .WithVirtualHost(projConfig.VirtualHostName)
                                                                             .WithUseSsl(false)
                                                                             .WithConnectionTimeout(TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                                             .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault));
                                                    if (!string.IsNullOrEmpty(username))
                                                    {
                                                        rabbitConfigBuilder = rabbitConfigBuilder
                                                           .WithUsername(username);
                                                    }
                                                    if (!string.IsNullOrEmpty(password))
                                                    {
                                                        rabbitConfigBuilder = rabbitConfigBuilder
                                                           .WithPassword(password);
                                                    }

                                                    rabbitConfigBuilder
                                                       .Build();
                                                })
                       .WithApiConfiguration(mockApiConfiguration.Object)
                       .Build();

        return uofConfig;
    }

    private void SendAnyOddsChangeMessage()
    {
        var oddsChange = PreconfiguredFeedMessages.AnyOddsChange();
        var routingKey = FeedMessageBuilder.BuildRoutingKey(oddsChange);
        _rabbitManagement.Send(oddsChange, routingKey);
    }

    private static IAuthenticationTokenCache SetupAuthenticationTokenCacheToReturnFeedToken(string feedToken)
    {
        var mockAuthenticationTokenCache = new Mock<IAuthenticationTokenCache>();
        var authenticationTokenResponse = new AuthenticationTokenApiResponse
        {
            AccessToken = feedToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };
        mockAuthenticationTokenCache.Setup(c => c.GetTokenForFeed()).ReturnsAsync(new AuthenticationToken(authenticationTokenResponse));
        return mockAuthenticationTokenCache.Object;
    }

    private static AuthenticationToken GenerateValidAuthenticationTokenWithGuid()
    {
        var jwtFeedToken = Guid.NewGuid().ToString("N").Replace("-", string.Empty);
        return new AuthenticationToken(new AuthenticationTokenApiResponse
        {
            AccessToken = jwtFeedToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    private async Task StartRabbitManagementWithRetries()
    {
        // sometimes RabbitMQ management fails to start on the first try
        // because of cleaning up from the previous test,
        // so we try up to 3 times
        for (var i = 0; i < 3; i++)
        {
            try
            {
                _rabbitManagement.ResetAndStart();
                break;
            }
            catch (Exception e)
            {
                var testLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactoryWithConnectionTests>();
                testLogger.LogError(e, "An exception occurred starting RabbitMQ management. Attempt {Attempt}/3", i + 1);
                await Task.Delay(1000);
            }
        }
    }
}
