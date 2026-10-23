// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Net.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.FeedAccess;

/// <summary>
/// Connection-factory guards. These fail before a socket is opened, so they do not use the broker.
/// </summary>
public class RabbitUnitConnectionFactoryTests
{
    private readonly ProjectConfiguration _project = ProjectConfiguration.CreateWithRandomVirtualHost();

    [Fact]
    public void ConstructorWithoutConnectionFactoryThrows()
    {
        var config = Configuration();
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(null, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void ConstructorWithoutConfigurationThrows()
    {
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(Configuration());
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, null, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void ConstructorWithoutLoggerThrows()
    {
        var config = Configuration();
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config);
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingClientProvidedNameThrows(string clientProvidedName)
    {
        var config = Configuration();
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config);
        factory.ClientProvidedName = clientProvidedName;

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void EmptyClientPropertiesThrows()
    {
        var config = Configuration();
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config);
        factory.ClientProperties = new Dictionary<string, object>();

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void NullClientPropertiesThrows()
    {
        var config = Configuration();
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config);
        factory.ClientProperties = null;

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void ConstructorCopiesProjectConfigurationAndLeavesTheConnectionClosed()
    {
        var config = Configuration();
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config);
        var configured = new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance);

        Assert.Equal(DateTime.MinValue, configured.ConnectionCreated);
        Assert.False(configured.IsConnected());
        Assert.Equal($"UofSdk / {SdkInfo.SdkType}", factory.ClientProvidedName);
        Assert.Equal(_project.SdkRabbitUsername, factory.UserName);
        Assert.Equal(_project.SdkRabbitPassword, factory.Password);
        Assert.Equal(_project.VirtualHostName, factory.VirtualHost);
        Assert.Equal(_project.GetRabbitIp(), factory.HostName);
        Assert.Equal(_project.DefaultRabbitPort, factory.Port);
        Assert.Equal(config.Rabbit.Heartbeat, factory.RequestedHeartbeat);
        Assert.Equal(config.Rabbit.ConnectionTimeout, factory.RequestedConnectionTimeout);
        Assert.Equal("123", factory.ClientProperties["SrUfSdkBId"]);
        Assert.Contains("SrUfSdkType", factory.ClientProperties.Keys);
        Assert.Contains("SrUfSdkVersion", factory.ClientProperties.Keys);
        Assert.Contains("SrUfSdkInit", factory.ClientProperties.Keys);
        Assert.Contains("SrUfSdkConnName", factory.ClientProperties.Keys);
    }

    [Fact]
    public void MissingBookmakerDetailsLeavesBookmakerIdUnset()
    {
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(Configuration(withBookmaker: false));

        Assert.Null(factory.ClientProperties["SrUfSdkBId"]);
    }

    [Fact]
    public void UseSslAppliesTheAcceptPolicy()
    {
        var factory = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(Configuration(useSsl: true));

        Assert.True(factory.Ssl.Enabled);
        Assert.Equal(SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable,
                     factory.Ssl.AcceptablePolicyErrors);
    }

    [Fact]
    public void CloseWhenNoConnectionExistsLeavesItDisconnected()
    {
        var config = Configuration();
        var configured = new ConfiguredConnectionFactory(ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config),
                                                         config,
                                                         NullLogger<ConfiguredConnectionFactory>.Instance);

        configured.CloseConnection();

        Assert.False(configured.IsConnected());
        Assert.Equal(DateTime.MinValue, configured.ConnectionCreated);
    }

    [Fact]
    public void ExceptionDuringCloseIsHandledAndTheConnectionIsReleased()
    {
        var config = Configuration();
        var configured = new ConfiguredConnectionFactory(ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config),
                                                         config,
                                                         NullLogger<ConfiguredConnectionFactory>.Instance);
        configured.ConnectionFactory = new StubConnectionFactory();
        var shutdownInvoked = false;
        configured.ConnectionShutdown += (_, _) => shutdownInvoked = true;
        var connection = (StubConnection)configured.CreateConnection();
        connection.PrepareForThrowOnClose();

        configured.CloseConnection();

        Assert.False(shutdownInvoked);
        Assert.False(configured.IsConnected());
        Assert.Equal(DateTime.MinValue, configured.ConnectionCreated);
    }

    private IUofConfiguration Configuration(bool withBookmaker = true, bool useSsl = false)
    {
        var builder = new Tests.Common.Builders.UofConfigurationBuilder()
                     .WithAccessToken("AnyAccessToken")
                     .WithRabbitConfiguration(rabbit => rabbit.WithHost(_project.GetRabbitIp())
                                                             .WithPort(_project.DefaultRabbitPort)
                                                             .WithUsername(_project.SdkRabbitUsername)
                                                             .WithPassword(_project.SdkRabbitPassword)
                                                             .WithVirtualHost(_project.VirtualHostName)
                                                             .WithUseSsl(useSsl)
                                                             .WithConnectionTimeout(TimeSpan.FromSeconds(ConfigLimit.RabbitConnectionTimeoutDefault))
                                                             .WithHeartbeat(TimeSpan.FromSeconds(ConfigLimit.RabbitHeartbeatDefault))
                                                             .Build());
        if (withBookmaker)
        {
            var bookmaker = new Moq.Mock<Sportradar.OddsFeed.SDK.Entities.Rest.IBookmakerDetails>();
            bookmaker.SetupGet(details => details.BookmakerId).Returns(123);
            builder.WithBookmakerDetails(bookmaker.Object);
        }

        return builder.Build();
    }
}
