// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

public class ConfiguredConnectionFactoryTests
{
    private readonly IUofConfiguration _defaultConfig = UofConfigurations.SingleLanguage;

    [Fact]
    public void ConstructorWithoutConnectionFactoryThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(null, _defaultConfig, new NullLogger<ConfiguredConnectionFactory>()));
    }

    [Fact]
    public void ConstructorWithoutConfigurationThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), null, new NullLogger<ConfiguredConnectionFactory>()));
    }

    [Fact]
    public void ConstructorWithoutLoggerThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, null));
    }

    [Fact]
    public void ConstructorCreatesConnectionFactory()
    {
        if (_defaultConfig.Rabbit is UofRabbitConfiguration rabbitConfig)
        {
            rabbitConfig.ConnectionTimeout = TimeSpan.FromSeconds(27);
            rabbitConfig.Heartbeat = TimeSpan.FromSeconds(17);
        }
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());

        Assert.NotNull(configuredConnectionFactory);
        Assert.NotNull(configuredConnectionFactory.ConnectionFactory);
        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());

        ValidateBaseConfigurationFactory(configuredConnectionFactory.ConnectionFactory);
        ValidateConfigurationFactoryWithUofConfiguration(configuredConnectionFactory.ConnectionFactory, _defaultConfig);
    }

    [Fact]
    public void CreatingConnectionSucceeds()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        Assert.NotNull(stubConnection);
        Assert.NotEqual(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.True(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void CreatingConnectionMultipleTimesReturnsFirstInstance()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        var stubConnection1 = GetEmptyStubConnection(configuredConnectionFactory);
        var stubConnection2 = GetEmptyStubConnection(configuredConnectionFactory);

        Assert.NotNull(stubConnection1);
        Assert.NotNull(stubConnection2);
        Assert.Same(stubConnection1, stubConnection2);
        Assert.True(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void CreatingConnectionAttachesTheBlockEvent()
    {
        var isConnectionBlocked = false;
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        configuredConnectionFactory.ConnectionBlocked += (_, _) => isConnectionBlocked = true;
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        stubConnection.HandleConnectionBlocked("some-reason");

        Assert.True(isConnectionBlocked);
    }

    [Fact]
    public void CreatingConnectionAttachesTheUnblockEvent()
    {
        var isConnectionUnblocked = false;
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        configuredConnectionFactory.ConnectionUnblocked += (_, _) => isConnectionUnblocked = true;
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        stubConnection.HandleConnectionUnblocked();

        Assert.True(isConnectionUnblocked);
    }

    [Fact]
    public void CreatingConnectionAttachesTheShutdownEvent()
    {
        var isConnectionShutdown = false;
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        configuredConnectionFactory.ConnectionShutdown += (_, _) => isConnectionShutdown = true;
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        stubConnection.Close(123, "some-reason");

        Assert.True(isConnectionShutdown);
    }

    [Fact]
    public void CreatingConnectionAttachesTheCallbackEvent()
    {
        var callbackExceptionInvoked = false;
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        configuredConnectionFactory.CallbackException += (_, _) => callbackExceptionInvoked = true;
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        stubConnection.Abort();

        Assert.True(callbackExceptionInvoked);
    }

    [Fact]
    public void ClosingExistingConnectionSucceeds()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        GetEmptyStubConnection(configuredConnectionFactory);

        Assert.True(configuredConnectionFactory.IsConnected());

        configuredConnectionFactory.CloseConnection();

        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void ClosingNonExistingConnectionSucceeds()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());

        Assert.False(configuredConnectionFactory.IsConnected());

        configuredConnectionFactory.CloseConnection();

        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void DisposeClosesConnection()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        GetEmptyStubConnection(configuredConnectionFactory);

        Assert.True(configuredConnectionFactory.IsConnected());

        configuredConnectionFactory.Dispose();

        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void DisposeMultipleTimesDisposesJustOnce()
    {
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        GetEmptyStubConnection(configuredConnectionFactory);

        Assert.True(configuredConnectionFactory.IsConnected());

        configuredConnectionFactory.Dispose();
        configuredConnectionFactory.Dispose();

        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());
    }

    [Fact]
    public void ExceptionDuringReleasingConnectionIsHandled()
    {
        var shutdownInvoked = false;
        var configuredConnectionFactory = new ConfiguredConnectionFactory(SetupMockConnectionFactoryWith(_defaultConfig), _defaultConfig, new NullLogger<ConfiguredConnectionFactory>());
        configuredConnectionFactory.ConnectionShutdown += (_, _) => shutdownInvoked = true;
        var stubConnection = GetEmptyStubConnection(configuredConnectionFactory);

        stubConnection.PrepareForThrowOnClose();
        configuredConnectionFactory.CloseConnection();

        Assert.False(shutdownInvoked);
        Assert.Equal(DateTime.MinValue, configuredConnectionFactory.ConnectionCreated);
        Assert.False(configuredConnectionFactory.IsConnected());
    }

    private static StubConnection GetEmptyStubConnection(ConfiguredConnectionFactory configuredConnectionFactory)
    {
        configuredConnectionFactory.ConnectionFactory = new StubConnectionFactory();
        return (StubConnection)configuredConnectionFactory.CreateConnection();
    }

    private static void ValidateBaseConfigurationFactory(IConnectionFactory connectionFactory)
    {
        Assert.NotNull(connectionFactory);
        Assert.NotNull(connectionFactory.ClientProperties);
        Assert.NotEmpty(connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkType", connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkVersion", connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkInit", connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkConnName", connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkType", connectionFactory.ClientProperties);
        Assert.Contains("SrUfSdkBId", connectionFactory.ClientProperties);
    }

    private static void ValidateConfigurationFactoryWithUofConfiguration(IConnectionFactory connectionFactory, IUofConfiguration configuration)
    {
        Assert.NotNull(connectionFactory);
        Assert.NotNull(configuration);
        Assert.Equal($"UofSdk / {SdkInfo.SdkType}", connectionFactory.ClientProvidedName);
        Assert.Equal(configuration.Rabbit.Username, connectionFactory.UserName);
        if (configuration.Rabbit.Password == null)
        {
            Assert.Equal(string.Empty, connectionFactory.Password);
        }
        else
        {
            Assert.Equal(configuration.Rabbit.Password, connectionFactory.Password);
        }
        Assert.Equal(configuration.Rabbit.VirtualHost, connectionFactory.VirtualHost);
        Assert.Equal(configuration.Rabbit.Heartbeat, connectionFactory.RequestedHeartbeat);
    }

    private static IConnectionFactory SetupMockConnectionFactoryWith(IUofConfiguration uofConfig)
    {
        var mockConnectionFactory = new Mock<IConnectionFactory>();
        mockConnectionFactory.SetupGet(g => g.ClientProperties).Returns(ConfiguredConnectionFactory.GetClientPropertiesWithSdkInfo(uofConfig));
        mockConnectionFactory.SetupGet(g => g.ClientProvidedName).Returns($"UofSdk / {SdkInfo.SdkType}");
        mockConnectionFactory.SetupGet(g => g.UserName).Returns(uofConfig.Rabbit.Username);
        mockConnectionFactory.SetupGet(g => g.Password).Returns(uofConfig.Rabbit.Password ?? string.Empty);
        mockConnectionFactory.SetupGet(g => g.VirtualHost).Returns(uofConfig.Rabbit.VirtualHost);
        mockConnectionFactory.SetupGet(g => g.RequestedHeartbeat).Returns(uofConfig.Rabbit.Heartbeat);

        return mockConnectionFactory.Object;
    }
}
