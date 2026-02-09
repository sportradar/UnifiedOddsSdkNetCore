// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderRabbitConfigurationTests : ConfigurationBuilderSetup
{
    private const string AnyAccessToken = "any-access-token";
    private const string TestSigningKeyId = "test-key-id";
    private const string TestClientId = "test-client-id";
    private static readonly AsymmetricSecurityKey TestPrivateKey = new RsaSecurityKey(RSA.Create(2056));

    private static readonly UofClientAuthentication.IPrivateKeyJwtData DefaultPrivateKeyJwt = UofClientAuthentication
                                                                                             .PrivateKeyJwt()
                                                                                             .SetSigningKeyId(TestSigningKeyId)
                                                                                             .SetClientId(TestClientId)
                                                                                             .SetPrivateKey(TestPrivateKey)
                                                                                             .Build();

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenRabbitHasDefaultValuesAndVirtualHostMatchesBookmaker(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.ShouldMatchRabbitDefaultsForNonCustomEnvironment(environment);
        config.Rabbit.VirtualHost.ShouldBe(config.BookmakerDetails.VirtualHost);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenRabbitHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Rabbit.ShouldMatchRabbitDefaultsForCustomEnvironment();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.RabbitConnectionTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.RabbitConnectionTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.RabbitConnectionTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.RabbitConnectionTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.RabbitConnectionTimeoutMax)]
    public void WhenRabbitConnectionTimeoutSetForPredefinedEnvironmentThenRabbitHasExpectedConnectionTimeout(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetRabbitConnectionTimeout(timeoutSeconds)
                    .Build();

        config.Rabbit.ConnectionTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMax)]
    public void WhenRabbitConnectionTimeoutSetForCustomEnvironmentThenRabbitHasExpectedConnectionTimeout(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetRabbitConnectionTimeout(timeoutSeconds)
                    .Build();

        config.Rabbit.ConnectionTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.RabbitHeartbeatMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.RabbitHeartbeatMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.RabbitHeartbeatMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.RabbitHeartbeatMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.RabbitHeartbeatMax)]
    public void WhenRabbitHeartbeatSetForPredefinedEnvironmentThenRabbitHasExpectedHeartbeat(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetRabbitHeartbeat(timeoutSeconds)
                    .Build();

        config.Rabbit.Heartbeat.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(ConfigLimit.RabbitHeartbeatMax)]
    public void WhenRabbitHeartbeatSetForCustomEnvironmentThenRabbitHasExpectedHeartbeat(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetRabbitHeartbeat(timeoutSeconds)
                    .Build();

        config.Rabbit.Heartbeat.ShouldBe(expected);
    }

    [Fact]
    public void WhenRabbitUsernameSetForCustomEnvironmentWithClientAuthenticationThenRabbitHasExpectedValue()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMessagingCredentials("any-username", "any-password")
                         .Build();

        config.Rabbit.Username.ShouldBe("any-username");
    }

    [Fact]
    public void WhenRabbitUsernameSetForCustomEnvironmentWithClientAuthenticationFromSectionThenRabbitHasExpectedValue()
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);
        configurationSectionMock.Setup(sectionMock => sectionMock.DefaultLanguage).Returns(TestConsts.CultureEn.TwoLetterISOLanguageName);
        configurationSectionMock.Setup(sectionMock => sectionMock.RabbitUsername).Returns("any-username");
        configurationSectionMock.Setup(sectionMock => sectionMock.RabbitPassword).Returns("any-password");
        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection()).Returns(configurationSectionMock.Object);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .LoadFromConfigFile()
                         .Build();

        config.Rabbit.Username.ShouldBe("any-username");
    }

    [Fact]
    public void WhenRabbitPasswordSetForCustomEnvironmentWithClientAuthenticationThenRabbitHasExpectedValue()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMessagingCredentials("any-username", "any-password")
                    .Build();

        config.Rabbit.Password.ShouldBe("any-password");
    }

    [Fact]
    public void WhenRabbitPasswordSetForCustomEnvironmentWithClientAuthenticationFromSectionThenRabbitHasExpectedValue()
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);
        configurationSectionMock.Setup(sectionMock => sectionMock.DefaultLanguage).Returns(TestConsts.CultureEn.TwoLetterISOLanguageName);
        configurationSectionMock.Setup(sectionMock => sectionMock.RabbitUsername).Returns("any-username");
        configurationSectionMock.Setup(sectionMock => sectionMock.RabbitPassword).Returns("any-password");
        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection()).Returns(configurationSectionMock.Object);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .Build();

        config.Rabbit.Password.ShouldBe("any-password");
    }
}
