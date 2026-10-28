// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Net.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
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
        var factory = ImmutableConnectionFactory(Configuration());
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, null, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void ConstructorWithoutLoggerThrows()
    {
        var config = Configuration();
        var factory = ImmutableConnectionFactory(config);
        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingClientProvidedNameThrows(string clientProvidedName)
    {
        var config = Configuration();
        var factory = ImmutableConnectionFactory(config);
        factory.ClientProvidedName = clientProvidedName;

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void EmptyClientPropertiesThrows()
    {
        var config = Configuration();
        var factory = ImmutableConnectionFactory(config);
        factory.ClientProperties = new Dictionary<string, object>();

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void NullClientPropertiesThrows()
    {
        var config = Configuration();
        var factory = ImmutableConnectionFactory(config);
        factory.ClientProperties = null;

        Assert.Throws<ArgumentNullException>(() => new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance));
    }

    [Fact]
    public void ConstructorCopiesProjectConfigurationAndLeavesTheConnectionClosed()
    {
        var config = Configuration();
        var factory = ImmutableConnectionFactory(config);
        var configured = new ConfiguredConnectionFactory(factory, config, NullLogger<ConfiguredConnectionFactory>.Instance);

        Assert.Equal(DateTime.MinValue, configured.ConnectionCreated);
        Assert.False(configured.IsConnected());
        Assert.Equal($"UofSdk / {SdkInfo.SdkType}", factory.ClientProvidedName);
        AssertSsoCredentials(factory, _project.SdkRabbitUsername, _project.SdkRabbitPassword);
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
        var factory = ImmutableConnectionFactory(Configuration(withBookmaker: false));

        Assert.Null(factory.ClientProperties["SrUfSdkBId"]);
    }

    [Fact]
    public void WhenBookmakerDetailsAreMissingThenSsoValidUntilIsUnset()
    {
        var validUntil = ImmutableConnectionFactory(Configuration(withBookmaker: false)).CredentialsProvider.ValidUntil;

        Assert.Null(validUntil);
    }

    [Fact]
    public void WhenBookmakerExpireAtIsSetThenSsoValidUntilMatchesIt()
    {
        var expireAt = DateTime.Now.AddHours(10);
        var before = DateTime.Now;

        var validUntil = ImmutableConnectionFactory(Configuration(expireAt: expireAt)).CredentialsProvider.ValidUntil;

        Assert.NotNull(validUntil);
        Assert.InRange(expireAt - validUntil.Value, before.AddMilliseconds(-50), DateTime.Now.AddMilliseconds(50));
    }

    [Fact]
    public void UseSslAppliesTheAcceptPolicy()
    {
        var factory = ImmutableConnectionFactory(Configuration(useSsl: true));

        Assert.True(factory.Ssl.Enabled);
        Assert.Equal(SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable,
                     factory.Ssl.AcceptablePolicyErrors);
    }

    [Fact]
    public void CloseWhenNoConnectionExistsLeavesItDisconnected()
    {
        var config = Configuration();
        var configured = new ConfiguredConnectionFactory(ImmutableConnectionFactory(config),
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
        var configured = new ConfiguredConnectionFactory(ImmutableConnectionFactory(config),
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

    [Fact]
    public void RefreshWhenProviderIsSsoThenCredentialsStayTheSame()
    {
        var config = Configuration();
        var credentials = ImmutableConnectionFactory(config).CredentialsProvider;
        var validUntil = credentials.ValidUntil;

        credentials.Refresh();

        Assert.IsType<CredentialProviderSso>(credentials);
        Assert.Equal("Sso", credentials.Name);
        Assert.Equal(config.Rabbit.Username, credentials.UserName);
        Assert.Equal(config.Rabbit.Password, credentials.Password);
        Assert.Equal(validUntil, credentials.ValidUntil);
    }

    [Fact]
    public void CreateConnectionFactoryWhenAuthenticationIsConfiguredThenUsesCommonIamCredentials()
    {
        var token = new AuthenticationToken(new AuthenticationTokenApiResponse
        {
            AccessToken = "feed-token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(item => item.GetTokenForFeed()).ReturnsAsync(token);
        var config = CiamConfiguration();

        var credentials = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config, cache.Object, NullLogger<ConfiguredConnectionFactory>.Instance)
                                                     .CredentialsProvider;

        Assert.IsType<CredentialProviderCommonIam>(credentials);
        Assert.Equal("CommonIAM", credentials.Name);
        Assert.Equal("123", credentials.UserName);
        Assert.Equal("feed-token", credentials.Password);
        Assert.Equal(TimeSpan.FromSeconds(3600), credentials.ValidUntil);
        cache.Verify(item => item.GetTokenForFeed(), Times.Once);
    }

    [Fact]
    public void CreateConnectionFactoryWhenRabbitPasswordIsSetThenCommonIamDoesNotReadTheTokenCache()
    {
        var cache = new Mock<IAuthenticationTokenCache>();
        cache.Setup(item => item.GetTokenForFeed()).Throws(new InvalidOperationException("Token cache should not be used"));
        var config = CiamConfiguration(rabbitPassword: "rabbit-password");

        var credentials = ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config, cache.Object, NullLogger<ConfiguredConnectionFactory>.Instance)
                                                     .CredentialsProvider;

        Assert.IsType<CredentialProviderCommonIam>(credentials);
        Assert.Equal("123", credentials.UserName);
        Assert.Equal("rabbit-password", credentials.Password);
        cache.Verify(item => item.GetTokenForFeed(), Times.Never);
    }

    private ConnectionFactory ImmutableConnectionFactory(IUofConfiguration config)
    {
        return ConfiguredConnectionFactory.CreateConnectionFactoryWithImmutableFields(config,
                                                                                      new Mock<IAuthenticationTokenCache>().Object,
                                                                                      NullLogger<ConfiguredConnectionFactory>.Instance);
    }

    private static void AssertSsoCredentials(IConnectionFactory factory, string username, string password)
    {
        var credentials = factory.CredentialsProvider;
        Assert.IsType<CredentialProviderSso>(credentials);
        Assert.Equal("Sso", credentials.Name);
        Assert.Equal(username, credentials.UserName);
        Assert.Equal(password, credentials.Password);
    }

    private IUofConfiguration CiamConfiguration(string rabbitPassword = null)
    {
        var builder = new UofConfigurationBuilder()
                     .WithAuthenticationConfiguration(auth => auth.WithClientId("client-id")
                                                                  .WithSigningKeyId("signing-key-id")
                                                                  .WithHost("localhost")
                                                                  .WithPort(80)
                                                                  .WithSsl(false)
                                                                  .WithAnyPrivateKey())
                     .WithBookmakerDetails(Bookmaker())
                     .WithRabbitConfiguration(rabbit =>
                                              {
                                                  var rabbitBuilder = rabbit.WithHost(_project.GetRabbitIp())
                                                                            .WithPort(_project.DefaultRabbitPort)
                                                                            .WithVirtualHost(_project.VirtualHostName)
                                                                            .WithUseSsl(false);
                                                  if (rabbitPassword != null)
                                                  {
                                                      rabbitBuilder.WithPassword(rabbitPassword);
                                                  }

                                                  rabbitBuilder.Build();
                                              });
        return builder.Build();
    }

    private static IBookmakerDetails Bookmaker()
    {
        var bookmaker = new Mock<IBookmakerDetails>();
        bookmaker.SetupGet(details => details.BookmakerId).Returns(123);
        return bookmaker.Object;
    }

    private static IBookmakerDetails BookmakerExpiringAt(DateTime expireAt)
    {
        var details = new bookmaker_details
        {
            bookmaker_id = 123,
            expire_at = expireAt,
            expire_atSpecified = true
        };
        return new BookmakerDetails(new BookmakerDetailsDto(details, TimeSpan.Zero));
    }

    private IUofConfiguration Configuration(bool withBookmaker = true, bool useSsl = false, DateTime? expireAt = null)
    {
        var builder = new UofConfigurationBuilder()
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
            builder.WithBookmakerDetails(expireAt.HasValue ? BookmakerExpiringAt(expireAt.Value) : Bookmaker());
        }

        return builder.Build();
    }
}
