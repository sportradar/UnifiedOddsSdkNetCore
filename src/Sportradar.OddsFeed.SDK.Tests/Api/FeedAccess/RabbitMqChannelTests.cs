// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
using xRetry;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

[SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class RabbitMqChannelTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly XunitLoggerFactory _loggerFactory;
    private readonly ILogger<ConfiguredConnectionFactory> _loggerCcf;
    private readonly Deserializer<FeedMessage> _deserializer = new Deserializer<FeedMessage>();
    private readonly RegexRoutingKeyParser _keyParser = new RegexRoutingKeyParser();
    private readonly Mock<IAuthenticationTokenCache> _authTokenCache = new Mock<IAuthenticationTokenCache>();

    private IUofConfiguration _uofConfig;
    private RabbitManagement _rabbitManagement;

    public RabbitMqChannelTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);
        _loggerCcf = _loggerFactory.CreateLogger<ConfiguredConnectionFactory>();
    }

    public async Task InitializeAsync()
    {
        var projConfig = ProjectConfigurationBuilder.Create()
                                                    .UseTestRabbitConfiguration()
                                                    .UseRandomVirtualHost()
                                                    .Build();
        _uofConfig = GetConfigUpdatedWithProjectConfig(projConfig);

        _rabbitManagement = new RabbitManagement(_testOutputHelper, projConfig);

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

    public async Task DisposeAsync()
    {
        await _rabbitManagement.DeleteVirtualHostAsync().ConfigureAwait(true);
        _rabbitManagement.SafeStop();
    }

    [Fact]
    public async Task MessageReceivedWhenReceivedOddsChangeThenConsume()
    {
        var eventHandled = false;
        FeedMessage receivedMessage = null;

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.FeedMessageReceived += (_, args) =>
                                               {
                                                   receivedMessage = args.Message;
                                                   eventHandled = true;
                                               };

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var oddsChange = PreconfiguredFeedMessages.AnyOddsChange();
        var routingKey = FeedMessageBuilder.BuildRoutingKey(oddsChange);
        _rabbitManagement.Send(oddsChange, routingKey);

        await WaitUpTo10SecThatEventIsHandled(eventHandled);

        receivedMessage.ShouldNotBeNull();
        receivedMessage.ShouldBeOfType<odds_change>();

        var oddsChangeResult = (odds_change)receivedMessage;
        oddsChangeResult.product.ShouldBe(oddsChange.product);
        oddsChangeResult.event_id.ShouldBe(oddsChange.event_id);

        messageReceiver.Close();
    }

    [Fact]
    public async Task MessageReceivedWhenReceivedOddsChangeAndChannelReceivedEventIsNotSubscribedThenMessageIsNotReceived()
    {
        var eventHandled = false;
        var eventHandlerRemoved = false;

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, TimeSpan.FromSeconds(10), out var rabbitMqChannel);

        messageReceiver.FeedMessageReceived += (_, _) => { eventHandled = true; };

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        // Remove all handlers from rabbitMqChannel.Received event using reflection
        var receivedEventField = typeof(RabbitMqChannel).GetField("Received", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (receivedEventField != null)
        {
            receivedEventField.SetValue(rabbitMqChannel, null);
            eventHandlerRemoved = true;
        }

        SendAnyOddsChangeMessage();

        await WaitUpTo10SecThatChannelReceiveMessage(rabbitMqChannel.LastMessageReceived);

        eventHandlerRemoved.ShouldBeTrue();
        eventHandled.ShouldBeFalse();
        rabbitMqChannel.LastMessageReceived.ShouldBeGreaterThan(DateTime.MinValue);

        messageReceiver.Close();
    }

    [Fact]
    public async Task RabbitMqChannelWhenNotSubscribedToReceivedThenMessageCanNotBeDispatched()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        // not subscribed to MessageReceived event

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        SendAnyOddsChangeMessage();

        var receiverLogger = _loggerFactory.GetOrCreateLogger(typeof(RabbitMqMessageReceiver));
        await receiverLogger.WaitUntilMessageArriveContaining("Message received.");

        receiverLogger.Messages.Any(a => a.Contains("Message received.")).ShouldBeTrue();

        messageReceiver.Close();
    }

    [Fact]
    public async Task WhenTryToConnectToUnknownHostThenLogWarning()
    {
        var projConfig = ProjectConfigurationBuilder.Create()
                                                    .UseTestRabbitConfiguration()
                                                    .UseRandomVirtualHost()
                                                    .Build();
        var mockBookmakerDetails = new Mock<IBookmakerDetails>();
        mockBookmakerDetails.SetupGet(b => b.BookmakerId).Returns(123);
        var uofConfig = new UofConfigurationBuilder()
                       .WithAccessToken("AnyAccessToken")
                       .WithBookmakerDetails(mockBookmakerDetails.Object)
                       .WithRabbitConfiguration(rabbit => rabbit
                                                         .WithHost("unknown_host")
                                                         .WithPort(projConfig.RabbitPort)
                                                         .WithUsername(projConfig.SdkRabbitUsername)
                                                         .WithPassword(projConfig.SdkRabbitPassword)
                                                         .WithVirtualHost(projConfig.VirtualHostName)
                                                         .WithUseSsl(false)
                                                         .WithConnectionTimeout(TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                         .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault))
                                                         .Build())
                       .Build();

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, uofConfig, out _);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));

        var rabbitChannelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();

        await rabbitChannelLogger.InnerLogger.WaitUntilMessageArriveContaining("Error checking connection for channelNumber", 30);

        rabbitChannelLogger.InnerLogger.Messages.Any(a => a.Contains("Error checking connection for channelNumber")).ShouldBeTrue();
    }

    [Fact]
    public async Task WhenChannelExistsAndErrorOccursThenLogWarningWithChannelNumber()
    {
        const int expectedChannelNumber = 123;
        var projConfig = ProjectConfigurationBuilder.Create()
                                                 .UseTestRabbitConfiguration()
                                                 .UseRandomVirtualHost()
                                                 .Build();
        var mockBookmakerDetails = new Mock<IBookmakerDetails>();
        mockBookmakerDetails.SetupGet(b => b.BookmakerId).Returns(123);
        var uofConfig = new UofConfigurationBuilder()
                       .WithAccessToken("AnyAccessToken")
                       .WithBookmakerDetails(mockBookmakerDetails.Object)
                       .WithRabbitConfiguration(rabbit => rabbit
                                                         .WithHost("unknown_host")
                                                         .WithPort(projConfig.RabbitPort)
                                                         .WithUsername(projConfig.SdkRabbitUsername)
                                                         .WithPassword(projConfig.SdkRabbitPassword)
                                                         .WithVirtualHost(projConfig.VirtualHostName)
                                                         .WithUseSsl(false)
                                                         .WithConnectionTimeout(TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                         .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault))
                                                         .Build())
                       .Build();

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, uofConfig, out var rabbitMqChannel);

        rabbitMqChannel.Channel = new StubChannel(expectedChannelNumber);
        rabbitMqChannel.LastMessageReceived = DateTime.Now.AddDays(-1);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));

        var rabbitChannelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();

        await rabbitChannelLogger.InnerLogger.WaitUntilMessageArriveContaining("Error checking connection for channelNumber", 30);

        rabbitChannelLogger.InnerLogger.Messages.Any(a => a.Contains($"Error checking connection for channelNumber: {expectedChannelNumber}")).ShouldBeTrue();
    }

    [Fact]
    public async Task RabbitMqChannelOpeningWithoutRoutingKeysThenItIsSystemChannel()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        var routingKeys = FeedRoutingKeyBuilder.GetStandardKeys().ToList();
        messageReceiver.Open(null, routingKeys);
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.BasicConsumer.ConsumerTags.First().ShouldContain("|system|");

        messageReceiver.Close();
    }

    [Fact]
    public async Task TryDoubleOpenThenThrowInvalidOperationException()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var exc = Should.Throw<InvalidOperationException>(() => messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages)));
        exc.Message.ShouldBe("The instance is already opened");

        messageReceiver.Close();
    }

    [Fact]
    public void TryOpenedWithoutRoutingKeyThenThrowArgumentNullException()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out _);

        var exc = Should.Throw<ArgumentNullException>(() => messageReceiver.Open(MessageInterest.AllMessages, Array.Empty<string>()));
        exc.Message.ShouldContain("Missing routing keys");
    }

    [Fact]
    public void WhenNotOpenedAndWantToCloseThenThrowInvalidOperationException()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out _);

        var exc = Should.Throw<InvalidOperationException>(() => messageReceiver.Close());
        exc.Message.ShouldBe("The instance is already closed");
    }

    [Fact]
    public async Task RabbitMqChannelWhenIfOpenedAndClosedTwiceThenThrowException()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        messageReceiver.Close();

        var exc = Should.Throw<InvalidOperationException>(() => messageReceiver.Close());
        exc.Message.ShouldBe("The instance is already closed");

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        channelLogger.InnerLogger.Messages.Any(a => a.Contains("this channel is already closed")).ShouldBeTrue();
    }

    [Fact]
    public async Task RabbitMqChannelWhenIfOpenedAndClosedTwiceThenLogMessageWithChannelNumber()
    {
        const int stubChannelNumber = 15;
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        messageReceiver.Close();

        rabbitMqChannel.Channel = new StubChannel(stubChannelNumber);

        var exc = Should.Throw<InvalidOperationException>(() => messageReceiver.Close());
        exc.Message.ShouldBe("The instance is already closed");

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        channelLogger.InnerLogger.Messages.Any(a => a.Contains("this channel is already closed")).ShouldBeTrue();
        channelLogger.InnerLogger.Messages.Any(a => a.Contains("Cannot close the channel on channelNumber: (null),")).ShouldBeFalse();
        channelLogger.InnerLogger.Messages.Any(a => a.Contains($"Cannot close the channel on channelNumber: {stubChannelNumber},")).ShouldBeTrue();
    }

    [Fact]
    public async Task IfConnectionIsNewerThenChannelThenRecreateChannel()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.LastMessageReceived = DateTime.Now;
        rabbitMqChannel.ChannelStarted = rabbitMqChannel.ChannelStarted.AddMinutes(-1);

        var rabbitChannelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await rabbitChannelLogger.InnerLogger.WaitUntilMessageArriveContaining("Recreating connection channel and attaching events");

        rabbitChannelLogger.InnerLogger.Messages.Any(a => a.Contains("Recreating connection channel and attaching events")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinInterval")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalWhenNoMessageArriveWithinIntervalAndConnectionIsOpen()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);

        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.LastMessageReceived = DateTime.UtcNow.AddMinutes(-1);

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no messages in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinInterval")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalWhenNoMessageArriveWithinIntervalAndConnectionIsClosed()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.LastMessageReceived = DateTime.UtcNow.AddMinutes(-1);

        rabbitMqChannel.ChannelFactory.ConnectionFactory.CloseConnection();

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await TestExecutionHelper.WaitToCompleteAsync(() => channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no message")), 50);

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no message in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinInterval")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalWhenNoMessageArriveWithinIntervalAndChannelIsNull()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.LastMessageReceived = DateTime.UtcNow.AddMinutes(-1);
        rabbitMqChannel.Channel = null;

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no messages in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinInterval")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalWhenNoMessageArriveWithinIntervalAndChannelIsNullAndConnectionIsClosed()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.LastMessageReceived = DateTime.UtcNow.AddMinutes(-1);
        rabbitMqChannel.ChannelFactory.ConnectionFactory.CloseConnection();
        rabbitMqChannel.Channel = null;

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no message in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStartWhenNoMessageArriveWithinIntervalAndConnectionIsOpen()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.ChannelStarted = DateTime.Now.AddMinutes(-10);

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no messages in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStartWhenNoMessageArriveWithinIntervalAndConnectionIsClosed()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.ChannelStarted = DateTime.Now.AddMinutes(-10);
        rabbitMqChannel.ChannelFactory.ConnectionFactory.CloseConnection();

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no message in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStartWhenNoMessageArriveWithinIntervalAndChannelIsNull()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.ChannelStarted = DateTime.Now.AddMinutes(-10);
        rabbitMqChannel.Channel = null;

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no messages in more than")).ShouldBeTrue();
    }

    [Fact]
    [Trait("RabbitMqChannel", "RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStart")]
    public async Task RecreateChannelIfNoMessagesArrivedWithinIntervalFromChannelStartWhenNoMessageArriveWithinIntervalAndChannelIsNullAndConnectionIsClosed()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        rabbitMqChannel.ChannelStarted = DateTime.Now.AddMinutes(-10);
        rabbitMqChannel.ChannelFactory.ConnectionFactory.CloseConnection();
        rabbitMqChannel.Channel = null;

        var channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await channelLogger.InnerLogger.WaitUntilMessageArriveContaining("There were no message");

        channelLogger.InnerLogger.Messages.Any(a => a.Contains("There were no message in more than")).ShouldBeTrue();
    }

    [Fact]
    public async Task BasicConsumerIsShutdownThenOnShutdownEventIsInvoked()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var eventInvoked = InvokeBasicConsumerShutdownEvent(rabbitMqChannel, $"Connection closed. AccessToken: {_uofConfig.AccessToken}");

        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining("is shutdown.");

        eventInvoked.ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("is shutdown.")).ShouldBeTrue();

        messageReceiver.Close();
    }

    [Fact]
    public async Task BasicConsumerIsShutdownAndReasonContainsSensitiveDataThenSensitiveDataIsCleared()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var eventInvoked = InvokeBasicConsumerShutdownEvent(rabbitMqChannel, $"Connection closed. AccessToken: {_uofConfig.AccessToken}");

        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining("is shutdown.");

        eventInvoked.ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("The consumer is shutdown.")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("Connection closed. AccessToken: ")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains(_uofConfig.AccessToken, StringComparison.Ordinal)).ShouldBeFalse();

        messageReceiver.Close();
    }

    [Fact]
    public async Task BasicConsumerIsShutdownWithoutReasonThenNoReasonIsLogged()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        var eventInvoked = InvokeBasicConsumerShutdownEvent(rabbitMqChannel, null);

        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining("is shutdown.");

        eventInvoked.ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("The consumer is shutdown.")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.EndsWith("Reason=", StringComparison.InvariantCultureIgnoreCase)).ShouldBeTrue();

        messageReceiver.Close();
    }

    [Fact]
    public async Task ChannelIsShutdownAndReasonContainsSensitiveDataThenSensitiveDataIsCleared()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        InvokeChannelShutdownEvent(rabbitMqChannel, $"Channel closed. AccessToken: {_uofConfig.AccessToken}");

        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining("is shutdown.");

        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("The channel is shutdown.")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("Channel closed. AccessToken: ")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.Contains(_uofConfig.AccessToken, StringComparison.Ordinal)).ShouldBeFalse();

        messageReceiver.Close();
    }

    [Fact]
    public async Task ChannelIsShutdownWithoutReasonThenNoReasonIsLogged()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureRabbitChannelIsCreated(rabbitMqChannel);

        InvokeChannelShutdownEvent(rabbitMqChannel, null);

        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining("is shutdown.");

        receiverLogger.InnerLogger.Messages.Any(a => a.Contains("The channel is shutdown.")).ShouldBeTrue();
        receiverLogger.InnerLogger.Messages.Any(a => a.EndsWith("Reason=", StringComparison.InvariantCultureIgnoreCase)).ShouldBeTrue();

        messageReceiver.Close();
    }

    [Fact]
    [Trait("ChannelFactory", "BeforeCreatedTheConnectionCreatedIsMinDate")]
    public void ChannelFactoryWhenNotYetCreatedThenConnectionCreatedIsMinDate()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        _ = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out var rabbitMqChannel);

        rabbitMqChannel.ChannelFactory.ConnectionCreated.ShouldBe(DateTime.MinValue);
    }

    [RetryFact(3, 5000)]
    public async Task WhenConnectionToRabbitIsInterruptedThenReasonForItShouldBeLogged()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out _);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureUserIsConnected(_uofConfig.Rabbit.Username);

        var existingConnections = _rabbitManagement.GetAllConnectionNames(_uofConfig.Rabbit.Username);
        existingConnections.ShouldNotBeEmpty($"User {_uofConfig.Rabbit.Username} should have at least one connection");

        const string closingReason = "any reason";
        await _rabbitManagement.CloseConnection(existingConnections[0], closingReason);
        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining(closingReason);

        receiverLogger.InnerLogger.Messages.ShouldContain((log) => log.Contains(closingReason, StringComparison.OrdinalIgnoreCase));
    }

    [RetryFact(3, 5000)]
    public async Task WhenConnectionToRabbitIsInterruptedAndReasonForItContainsAccessTokenThenAccessTokenShouldNotBeLogged()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig, _authTokenCache.Object, _loggerCcf);
        var messageReceiver = ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig, out _);

        messageReceiver.Open(MessageInterest.AllMessages, GenerateDistinctRoutingKeys(MessageInterest.AllMessages));
        await EnsureUserIsConnected(_uofConfig.Rabbit.Username);

        var existingConnections = _rabbitManagement.GetAllConnectionNames(_uofConfig.Rabbit.Username);
        existingConnections.ShouldNotBeEmpty($"User {_uofConfig.Rabbit.Username} should have at least one connection");

        const string reasonWithoutSensitiveData = "Reason with sensitive data";
        await _rabbitManagement.CloseConnection(existingConnections[0], $"{reasonWithoutSensitiveData}: {_uofConfig.AccessToken}");
        var receiverLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        await receiverLogger.InnerLogger.WaitUntilMessageArriveContaining(reasonWithoutSensitiveData);

        receiverLogger.InnerLogger.Messages.ShouldContain((log) => log.Contains(reasonWithoutSensitiveData, StringComparison.Ordinal));
        receiverLogger.InnerLogger.Messages.ShouldNotContain((log) => log.Contains(_uofConfig.AccessToken, StringComparison.Ordinal));
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

    private async Task EnsureUserIsConnected(string rabbitUser)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => _rabbitManagement.GetAllConnectionNames(rabbitUser).Any(), 50);
    }

    private static async Task WaitUpTo10SecThatEventIsHandled(bool eventHandled)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => eventHandled, 50);
    }

    private static async Task WaitUpTo10SecThatChannelReceiveMessage(DateTime lastMessageReceived)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => lastMessageReceived != DateTime.MinValue, 50);
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

    private static IUofConfiguration GetConfigUpdatedWithProjectConfig(ProjectConfiguration projConfig)
    {
        var mockBookmakerDetails = new Mock<IBookmakerDetails>();
        mockBookmakerDetails.SetupGet(b => b.BookmakerId).Returns(123);
        var config = new UofConfigurationBuilder()
                    .WithAccessToken("AnyAccessToken")
                    .WithBookmakerDetails(mockBookmakerDetails.Object)
                    .WithRabbitConfiguration(rabbit => rabbit
                                                      .WithHost(projConfig.RabbitHost)
                                                      .WithPort(projConfig.RabbitPort)
                                                      .WithUsername(projConfig.SdkRabbitUsername)
                                                      .WithPassword(projConfig.SdkRabbitPassword)
                                                      .WithVirtualHost(projConfig.VirtualHostName)
                                                      .WithUseSsl(false)
                                                      .WithConnectionTimeout(TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                      .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault))
                                                      .Build())
                    .Build();

        return config;
    }

    private void SendAnyOddsChangeMessage()
    {

        var oddsChange = PreconfiguredFeedMessages.AnyOddsChange();
        var routingKey = FeedMessageBuilder.BuildRoutingKey(oddsChange);
        _rabbitManagement.Send(oddsChange, routingKey);
    }

    private static bool InvokeBasicConsumerShutdownEvent(RabbitMqChannel rabbitMqChannel, string replyText)
    {
        var eventInvoked = false;
        var shutDownFieldInfo = typeof(EventingBasicConsumer).GetField("Shutdown", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (shutDownFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<ShutdownEventArgs>)shutDownFieldInfo.GetValue(rabbitMqChannel.BasicConsumer);
            receivedDelegate?.Invoke(rabbitMqChannel.BasicConsumer, new ShutdownEventArgs(ShutdownInitiator.Application, 1, replyText));
            eventInvoked = true;
        }
        return eventInvoked;
    }

    private static void InvokeChannelShutdownEvent(RabbitMqChannel rabbitMqChannel, string replyText)
    {
        // Trigger the ModelShutdown event from RabbitMQ library to test that event subscription works
        // In AutorecoveringModel, the event handlers are stored in _recordedShutdownEventHandlers field
        var channelType = rabbitMqChannel.Channel.GetType();
        var shutDownFieldInfo = channelType.GetField("_recordedShutdownEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (shutDownFieldInfo != null)
        {
            var eventDelegate = (EventHandler<ShutdownEventArgs>)shutDownFieldInfo.GetValue(rabbitMqChannel.Channel);
            eventDelegate?.Invoke(rabbitMqChannel.Channel, new ShutdownEventArgs(ShutdownInitiator.Application, 1, replyText));
        }
    }
}
