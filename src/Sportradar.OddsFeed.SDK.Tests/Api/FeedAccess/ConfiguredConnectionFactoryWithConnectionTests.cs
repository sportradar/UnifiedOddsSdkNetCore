// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

[Collection(NonParallelCollectionFixture.NonParallelTestCollection)]
public class ConfiguredConnectionFactoryWithConnectionTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly XunitLoggerFactory _loggerFactory;

    private readonly Deserializer<FeedMessage> _deserializer = new Deserializer<FeedMessage>();
    private readonly RegexRoutingKeyParser _keyParser = new RegexRoutingKeyParser();

    private IUofConfiguration _uofConfig;
    private RabbitManagement _rabbitManagement;

    public ConfiguredConnectionFactoryWithConnectionTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _loggerFactory = new XunitLoggerFactory(testOutputHelper);

        var projConfig = ProjectConfiguration.CreateWithRandomVirtualHost();
        _uofConfig = GetConfigUpdatedWithProjectConfig(projConfig);
    }

    public async Task InitializeAsync()
    {
        var projConfig = ProjectConfiguration.CreateWithRandomVirtualHost();
        _uofConfig = GetConfigUpdatedWithProjectConfig(projConfig);

        _rabbitManagement = new RabbitManagement(_testOutputHelper, projConfig);

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
        await _rabbitManagement.DeleteVirtualHostAsync().ConfigureAwait(false);
        _rabbitManagement.Stop();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [Trait("ConfiguredConnectionFactory", "MissingClientProvidedName")]
    public void ConnectionFactoryWhenMissingClientProvidedNameThenThrows(string clientProvidedName)
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        rabbitConnectionFactory.ClientProvidedName = clientProvidedName;

        Should.Throw<ArgumentNullException>(() => ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig));
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "EmptyClientProperties")]
    public void ConnectionFactoryWhenClientPropertiesIsEmptyThenThrows()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        rabbitConnectionFactory.ClientProperties = new Dictionary<string, object>();

        Should.Throw<ArgumentNullException>(() => ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig));
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "NullClientProperties")]
    public void ConnectionFactoryWhenClientPropertiesIsNullThenThrows()
    {
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        rabbitConnectionFactory.ClientProperties = null;

        Should.Throw<ArgumentNullException>(() => ConstructMessageReceiver(rabbitConnectionFactory, _uofConfig));
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "ClientPropertiesMissingBookmakerDetails")]
    public void ConnectionFactoryWhenConfigMissingBookmakerDetailsThenConnectWithoutBookmakerId()
    {
        var projConfig = ProjectConfiguration.CreateWithRandomVirtualHost();
        var uofConfig = new UofConfigurationBuilder()
                       .WithAccessToken("AnyAccessToken")
                       .WithRabbitConfiguration(rabbit => rabbit
                                                         .WithHost(projConfig.GetRabbitIp())
                                                         .WithPort(projConfig.DefaultRabbitPort)
                                                         .WithUsername(projConfig.SdkRabbitUsername)
                                                         .WithPassword(projConfig.SdkRabbitPassword)
                                                         .WithVirtualHost(projConfig.VirtualHostName)
                                                         .WithUseSsl(false)
                                                         .Build())
                       .Build();

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(uofConfig);

        rabbitConnectionFactory.ClientProperties.ShouldContainKey("SrUfSdkBId");
        rabbitConnectionFactory.ClientProperties["SrUfSdkBId"].ShouldBeNull();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "UsingSsl")]
    public void ConnectionFactoryWhenConfigUseSslThenAcceptPolicyIsApplied()
    {
        var projConfig = ProjectConfiguration.CreateWithRandomVirtualHost();
        var uofConfig = new UofConfigurationBuilder()
                       .WithAccessToken("AnyAccessToken")
                       .WithRabbitConfiguration(rabbit => rabbit
                                                         .WithHost(projConfig.GetRabbitIp())
                                                         .WithPort(projConfig.DefaultRabbitPort)
                                                         .WithUsername(projConfig.SdkRabbitUsername)
                                                         .WithPassword(projConfig.SdkRabbitPassword)
                                                         .WithVirtualHost(projConfig.VirtualHostName)
                                                         .WithUseSsl(true)
                                                         .Build())
                       .Build();

        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(uofConfig);

        rabbitConnectionFactory.Ssl.AcceptablePolicyErrors.ShouldBe(SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable);
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeSubscribedConnectionBlockedEventHandler")]
    public void ConnectionFactoryWhenConnectionBlockEventIsInvokedThenCallsHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;
        var eventHandled = false;

        configuredConnectionFactory.ConnectionBlocked += (_, _) => { eventHandled = true; };

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedBlockedEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<ConnectionBlockedEventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, new ConnectionBlockedEventArgs("Connection blocked for testing"));
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
        eventHandled.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeNotSubscribedConnectionBlockedEventHandler")]
    public void ConnectionFactoryWhenConnectionBlockEventIsNotSubscribedToAndIsInvokedThenThereIsNoHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;

        // not subscribed to ConnectionBlocked event

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedBlockedEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<ConnectionBlockedEventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, new ConnectionBlockedEventArgs("Connection blocked for testing"));
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeSubscribedConnectionUnblockedEventHandler")]
    public void ConnectionFactoryWhenConnectionUnblockEventIsInvokedThenCallsHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;
        var eventHandled = false;
        configuredConnectionFactory.ConnectionUnblocked += (_, _) => { eventHandled = true; };

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedUnblockedEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<EventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, EventArgs.Empty);
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
        eventHandled.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeNotSubscribedConnectionUnblockedEventHandler")]
    public void ConnectionFactoryWhenConnectionUnblockEventIsNotSubscribedToAndIsInvokedThenThereIsNoHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;

        // not subscribed to ConnectionUnblocked event

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedUnblockedEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<EventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, EventArgs.Empty);
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeSubscribedConnectionShutdownEventHandler")]
    public void ConnectionFactoryWhenConnectionShutdownEventIsInvokedThenCallsHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;
        var eventHandled = false;
        configuredConnectionFactory.ConnectionShutdown += (_, _) => { eventHandled = true; };

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedShutdownEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<ShutdownEventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 1, "Connection shutdown for testing"));
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
        eventHandled.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "CloseConnection")]
    public void ConnectionWhenConnectionClosedThenCannotUseConnection()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var connection = configuredConnectionFactory.CreateConnection();
        configuredConnectionFactory.CloseConnection();

        connection.IsOpen.ShouldBeFalse();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeNotSubscribedConnectionShutdownEventHandler")]
    public void ConnectionFactoryWhenConnectionShutdownEventIsNotSubscribedToAndIsInvokedThenThereIsNoHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;

        // not subscribed to ConnectionUnblocked event

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        var eventFieldInfo = connection.GetType().GetField("_recordedShutdownEventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventFieldInfo != null)
        {
            var receivedDelegate = (EventHandler<ShutdownEventArgs>)eventFieldInfo.GetValue(connection);
            receivedDelegate?.Invoke(connection, new ShutdownEventArgs(ShutdownInitiator.Application, 1, "Connection shutdown for testing"));
            eventInvoked = true;
        }

        eventInvoked.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeSubscribedCallbackExceptionEventHandler")]
    public void ConnectionFactoryWhenCallbackExceptionEventIsInvokedThenCallsHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;
        var eventHandled = false;
        configuredConnectionFactory.CallbackException += (_, _) => { eventHandled = true; };

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        // CallbackException event is on the delegate connection, not recorded like other events
        var delegateFieldInfo = connection.GetType().GetField("_delegate", BindingFlags.Instance | BindingFlags.NonPublic);
        if (delegateFieldInfo != null)
        {
            var delegateConnection = delegateFieldInfo.GetValue(connection);
            if (delegateConnection != null)
            {
                var eventFieldInfo = delegateConnection.GetType().GetField("CallbackException", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (eventFieldInfo != null)
                {
                    var receivedDelegate = (EventHandler<CallbackExceptionEventArgs>)eventFieldInfo.GetValue(delegateConnection);
                    receivedDelegate?.Invoke(delegateConnection, new CallbackExceptionEventArgs(null));
                    eventInvoked = true;
                }
            }
        }

        eventInvoked.ShouldBeTrue();
        eventHandled.ShouldBeTrue();
    }

    [Fact]
    [Trait("ConfiguredConnectionFactory", "InvokeNotSubscribedCallbackExceptionEventHandler")]
    public void ConnectionFactoryWhenCallbackExceptionEventIsNotSubscribedToAndIsInvokedThenThereIsNoHandler()
    {
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var rabbitConnectionFactory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(_uofConfig);
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, _uofConfig, connectionLogger);

        var eventInvoked = false;

        // not subscribed to CallbackException event

        var connection = configuredConnectionFactory.CreateConnection();
        connection.ShouldNotBeNull();

        // CallbackException event is on the delegate connection, not recorded like other events
        var delegateFieldInfo = connection.GetType().GetField("_delegate", BindingFlags.Instance | BindingFlags.NonPublic);
        if (delegateFieldInfo != null)
        {
            var delegateConnection = delegateFieldInfo.GetValue(connection);
            if (delegateConnection != null)
            {
                var eventFieldInfo = delegateConnection.GetType().GetField("CallbackException", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (eventFieldInfo != null)
                {
                    var receivedDelegate = (EventHandler<CallbackExceptionEventArgs>)eventFieldInfo.GetValue(delegateConnection);
                    receivedDelegate?.Invoke(delegateConnection, new CallbackExceptionEventArgs(null));
                    eventInvoked = true;
                }
            }
        }

        eventInvoked.ShouldBeTrue();
    }

    private RabbitMqMessageReceiver ConstructMessageReceiver(ConnectionFactory rabbitConnectionFactory, IUofConfiguration config)
    {
        ILogger<RabbitMqChannel> channelLogger = _loggerFactory.GetOrCreateLogger<RabbitMqChannel>();
        ILogger<ConfiguredConnectionFactory> connectionLogger = _loggerFactory.GetOrCreateLogger<ConfiguredConnectionFactory>();
        var configuredConnectionFactory = new ConfiguredConnectionFactory(rabbitConnectionFactory, config, connectionLogger);
        var channelFactory = new ChannelFactory(configuredConnectionFactory);
        var timer = new SdkTimer("TestTimer", TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(3));
        var rabbitMqChannel = new RabbitMqChannel(channelFactory, timer, TimeSpan.FromSeconds(5), config.AccessToken, channelLogger);
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
                                                      .WithHost(projConfig.GetRabbitIp())
                                                      .WithPort(projConfig.DefaultRabbitPort)
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
}
