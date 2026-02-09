// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderAuthenticationConfigurationTests : ConfigurationBuilderSetup
{
    private const string AnyAccessToken = "test-x-access-token";
    private const string TestSigningKeyId = "test-key-id";
    private const string TestClientId = "test-client-id";
    private const string AuthenticationHost = "localhost";
    private const string IncorrectAuthenticationHostWithHttp = "http://localhost";
    private const string IncorrectAuthenticationHostWithHttps = "https://localhost";
    private const string IncorrectAuthenticationHostWithCustomPort = "localhost:89";
    private const string IncorrectAuthenticationHostWithDefaultHttpPort = "localhost:80";
    private const string IncorrectAuthenticationHostWithDefaultHttpsPort = "localhost:443";
    private const int AuthenticationPort = 80;
    private const SdkEnvironment AnyEnvironment = SdkEnvironment.Production;
    private const int AnyNodeId = 1;

    private static readonly AsymmetricSecurityKey TestPrivateKey = new RsaSecurityKey(RSA.Create(2056));

    private static readonly UofClientAuthentication.IPrivateKeyJwtData DefaultPrivateKeyJwt = UofClientAuthentication
                                                                                             .PrivateKeyJwt()
                                                                                             .SetSigningKeyId(TestSigningKeyId)
                                                                                             .SetClientId(TestClientId)
                                                                                             .SetPrivateKey(TestPrivateKey)
                                                                                             .Build();

    public static IEnumerable<object[]> NonRsaKeys()
    {
        yield return [new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256))];
        yield return [new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))];
        yield return [new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521))];
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenBuiltForPredefinedEnvironmentThenAuthenticationCredentialsAreSet(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.SigningKeyId.ShouldBe(TestSigningKeyId);
        config.Authentication.PrivateKey.ShouldBe(TestPrivateKey);
        config.Authentication.ClientId.ShouldBe(TestClientId);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenBuiltForPredefinedEnvironmentThenAuthenticationHostIsSet(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.Host.ShouldBe(EnvironmentManager.GetAuthenticationHost(environment));
        config.Authentication.Port.ShouldBeGreaterThan(0);
        config.Authentication.UseSsl.ShouldBeTrue();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenBuiltForPredefinedEnvironmentAndSigningKeyIdIsNotProvidedThenThrowsException(SdkEnvironment environment)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .SetClientId(TestClientId)
                                                 .Build())
                         .SelectEnvironment(environment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenBuiltForPredefinedEnvironmentAndClientIdIsNotProvidedThenThrowsException(SdkEnvironment environment)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .Build())
                         .SelectEnvironment(environment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenBuiltForPredefinedEnvironmentAndPrivateKeyIsNotProvidedThenThrowsException(SdkEnvironment environment)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetClientId(TestClientId)
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .Build())
                         .SelectEnvironment(environment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenBuiltForAnyEnvironmentThenThrowIfSigningKeyIdIsEmptyOrWhitespace(string invalidSigningKeyId)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(invalidSigningKeyId)
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .SetClientId(TestClientId)
                                                 .Build())
                         .SelectEnvironment(AnyEnvironment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentException>(() => build());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenBuiltForAnyEnvironmentThenThrowIfClientIdIsEmptyOrWhitespace(string invalidClientId)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .SetClientId(invalidClientId)
                                                 .Build())
                         .SelectEnvironment(AnyEnvironment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentException>(() => build());
    }

    [Fact]
    public void WhenBuiltForAnyEnvironmentThenThrowIfPrivateKeyIsNull()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .SetPrivateKey(null)
                                                 .SetClientId(TestClientId)
                                                 .Build())
                         .SelectEnvironment(AnyEnvironment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentException>(() => build());
    }

    [Fact]
    public void WhenBuiltForCustomAndSigningKeyIdIsNotProvidedThenThrowsException()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .SetClientId(TestClientId)
                                                 .Build())
                         .SelectCustom()
                         .SetClientAuthenticationHost(AuthenticationHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentAndClientIdIsNotProvidedThenThrowsException()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .SetPrivateKey(TestPrivateKey)
                                                 .Build())
                         .SelectCustom()
                         .SetClientAuthenticationHost(AuthenticationHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentAndPrivateKeyIsNotProvidedThenThrowsException()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetClientId(TestClientId)
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .Build())
                         .SelectCustom()
                         .SetClientAuthenticationHost(AuthenticationHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Theory]
    [InlineData(IncorrectAuthenticationHostWithHttp)]
    [InlineData(IncorrectAuthenticationHostWithHttps)]
    [InlineData(IncorrectAuthenticationHostWithCustomPort)]
    [InlineData(IncorrectAuthenticationHostWithDefaultHttpPort)]
    [InlineData(IncorrectAuthenticationHostWithDefaultHttpsPort)]
    public void WhenBuiltForCustomEnvironmentAndAuthenticationHostIsIncorrectThenThrowsException(string incorrectHost)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .SetClientAuthenticationHost(incorrectHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenAuthenticationEndpointIncludesHostPortAndSsl()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .SetClientAuthenticationHost(AuthenticationHost)
                    .SetClientAuthenticationPort(AuthenticationPort)
                    .SetClientAuthenticationUseSsl(true)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.Host.ShouldBe(AuthenticationHost);
        config.Authentication.Port.ShouldBe(AuthenticationPort);
        config.Authentication.UseSsl.ShouldBeTrue();
        config.Authentication.SigningKeyId.ShouldBe(TestSigningKeyId);
        config.Authentication.PrivateKey.ShouldBe(TestPrivateKey);
        config.Authentication.ClientId.ShouldBe(TestClientId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void WhenBuiltForCustomEnvironmentThenThrowIfAuthenticationPortIsNonPositive(int invalidPort)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .SetClientAuthenticationHost(AuthenticationHost)
                         .SetClientAuthenticationPort(invalidPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentOutOfRangeException>(() => build());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenBuiltForCustomEnvironmentThenThrowIfAuthenticationHostIsEmptyOrWhitespace(string invalidHost)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .SetClientAuthenticationHost(invalidHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentException>(() => build());
    }

    [Fact]
    public void WhenClientAuthenticationIsSetToNullThenExceptionIsThrown()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(null)
                         .SelectCustom()
                         .SetClientAuthenticationHost(AuthenticationHost)
                         .SetClientAuthenticationPort(AuthenticationPort)
                         .SetClientAuthenticationUseSsl(true)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentNullException>(() => build());
    }

    [Fact]
    public void WhenBuiltForReplayEnvironmentWithSelectReplayThenThrow()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectReplay()
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Fact]
    public void WhenBuiltForReplayEnvironmentWithSelectEnvironmentThenThrow()
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectEnvironment(SdkEnvironment.Replay)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<InvalidOperationException>(() => build());
    }

    [Fact]
    public void WhenClientAuthenticationIsNotConfiguredForCustomEnvironmentThenAuthenticationConfigurationIsNull()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.ShouldBeNull();
        config.AccessToken.ShouldBe(AnyAccessToken);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenClientAuthenticationIsNotConfiguredForPredefinedEnvironmentThenAuthenticationConfigurationIsNull(SdkEnvironment sdkEnvironment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(AnyAccessToken)
                    .SelectEnvironment(sdkEnvironment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.ShouldBeNull();
        config.AccessToken.ShouldBe(AnyAccessToken);
    }

    [Theory]
    [MemberData(nameof(NonRsaKeys))]
    public void WhenPrivateKeyIsNotRsaThenThrowsArgumentException(AsymmetricSecurityKey nonRsaKey)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(UofClientAuthentication
                                                 .PrivateKeyJwt()
                                                 .SetSigningKeyId(TestSigningKeyId)
                                                 .SetPrivateKey(nonRsaKey)
                                                 .SetClientId(TestClientId)
                                                 .Build())
                         .SelectEnvironment(AnyEnvironment)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        Should.Throw<ArgumentException>(build, "The provided private key does not support RSA algorithms. Only RSA keys are supported.");
    }

    [Fact]
    public void WhenCustomEnvironmentAndClientAuthIsNotConfiguredSettingAuthHostThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
                                        {
                                            new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                                               .SetAccessToken(AnyAccessToken)
                                               .SelectCustom()
                                               .SetClientAuthenticationHost(AuthenticationHost)
                                               .SetDefaultLanguage(TestConsts.CultureEn)
                                               .Build();
                                        }).Message.ShouldBe("Cannot set authentication host when client authentication is not configured");
    }

    [Fact]
    public void WhenCustomEnvironmentAndClientAuthIsNotConfiguredSettingAuthPortThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
                                        {
                                            new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                                               .SetAccessToken(AnyAccessToken)
                                               .SelectCustom()
                                               .SetClientAuthenticationPort(AuthenticationPort)
                                               .SetDefaultLanguage(TestConsts.CultureEn)
                                               .Build();
                                        }).Message.ShouldBe("Cannot set authentication port when client authentication is not configured");
    }

    [Fact]
    public void WhenCustomEnvironmentAndClientAuthIsNotConfiguredSettingAuthUseSslThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() =>
                                        {
                                            new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                                               .SetAccessToken(AnyAccessToken)
                                               .SelectCustom()
                                               .SetClientAuthenticationUseSsl(true)
                                               .SetDefaultLanguage(TestConsts.CultureEn)
                                               .Build();
                                        }).Message.ShouldBe("Cannot set authentication SSL settings when client authentication is not configured");
    }

    [Fact]
    public void WhenClientAuthenticationIsConfiguredForCustomEnvironmentThenDefaultAuthenticationHostIsIntegration()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        var configForIntegration = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                                  .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectEnvironment(SdkEnvironment.Integration)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.Host.ShouldBe(configForIntegration.Authentication.Host);
        config.Authentication.Port.ShouldBe(configForIntegration.Authentication.Port);
        config.Authentication.UseSsl.ShouldBe(configForIntegration.Authentication.UseSsl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void WhenClientAuthenticationIsConfiguredForCustomEnvironmentAndTenantIsEmptyOrWhitespaceThenThrows(string invalidTenant)
    {
        var build = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                         .SetClientAuthentication(DefaultPrivateKeyJwt)
                         .SelectCustom()
                         .SetClientAuthenticationTenant(invalidTenant)
                         .SetDefaultLanguage(TestConsts.CultureEn)
                         .Build();

        build.ShouldThrow<ArgumentException>().Message.ShouldContain(nameof(UofClientAuthentication.IPrivateKeyJwt.Tenant));
    }

    [Fact]
    public void WhenClientAuthenticationIsConfiguredForCustomEnvironmentAndTenantIsConfiguredThenTenantUsesConfiguredValue()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .SetClientAuthenticationTenant("custom-tenant")
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.Tenant.ShouldBe("custom-tenant");
    }

    [Fact]
    public void WhenClientAuthenticationIsConfiguredForCustomEnvironmentAndTenantIsNotConfiguredThenTenantIsNull()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Authentication.Tenant.ShouldBeNull();
    }

    [Fact]
    public void WhenClientAuthenticationIsNotConfiguredForCustomEnvironmentAndTenantIsConfiguredThenExceptionIsThrown()
    {
        var act = () => new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(AnyAccessToken)
                    .SelectCustom()
                    .SetClientAuthenticationTenant("anyTenantValue")
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        act.ShouldThrow<ArgumentException>().Message.ShouldContain(nameof(UofClientAuthentication.IPrivateKeyJwt.Tenant));
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void WhenClientAuthenticationIsConfiguredAndRestIsLoadedFromConfigFileForNonCustomAndNonReplayEnvironmentThenAuthenticationShouldBeConfigured(SdkEnvironment nonCustomNonReplayEnvironment)
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.Environment).Returns(nonCustomNonReplayEnvironment);
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);
        configurationSectionMock.Setup(sectionMock => sectionMock.DefaultLanguage).Returns(TestConsts.CultureEn.TwoLetterISOLanguageName);
        configurationSectionMock.Setup(sectionMock => sectionMock.NodeId).Returns(AnyNodeId);

        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection())
                                           .Returns(configurationSectionMock.Object);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectEnvironment(nonCustomNonReplayEnvironment)
                    .LoadFromConfigFile()
                    .Build();

        config.Authentication.ShouldNotBeNull();
        config.Authentication.SigningKeyId.ShouldBe(TestSigningKeyId);
        config.Authentication.ClientId.ShouldBe(TestClientId);
        config.Authentication.PrivateKey.ShouldBe(TestPrivateKey);
    }

    [Fact]
    public void WhenClientAuthenticationIsConfiguredAndRestIsLoadedFromConfigFileForCustomEnvironmentThenAuthenticationShouldBeConfigured()
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.Environment).Returns(SdkEnvironment.Custom);
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);
        configurationSectionMock.Setup(sectionMock => sectionMock.DefaultLanguage).Returns(TestConsts.CultureEn.TwoLetterISOLanguageName);
        configurationSectionMock.Setup(sectionMock => sectionMock.NodeId).Returns(AnyNodeId);

        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection())
                                           .Returns(configurationSectionMock.Object);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetClientAuthentication(DefaultPrivateKeyJwt)
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .Build();

        config.Authentication.ShouldNotBeNull();
        config.Authentication.SigningKeyId.ShouldBe(TestSigningKeyId);
        config.Authentication.ClientId.ShouldBe(TestClientId);
        config.Authentication.PrivateKey.ShouldBe(TestPrivateKey);
    }

    [Fact]
    public void ForbidsSettingAccessTokenWhenClientAuthenticationIsConfigured()
    {
        var build = () =>
                    {
                        var tokenSetter = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider);
                        tokenSetter
                           .SetClientAuthentication(DefaultPrivateKeyJwt);
                        tokenSetter.SetAccessToken(AnyAccessToken)
                                          .SelectEnvironment(SdkEnvironment.Integration)
                                          .SetDefaultLanguage(TestConsts.CultureEn)
                                          .SetNodeId(AnyNodeId)
                                          .Build();
                    };

        Should.Throw<ArgumentException>(() => build()).Message.ShouldBe("Cannot set access token when client authentication is configured");
    }

    [Fact]
    public void ForbidsSettingAccessTokenFromConfigFileWhenClientAuthenticationIsConfigured()
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);

        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection())
                                           .Returns(configurationSectionMock.Object);

        var build = () =>
                    {
                        var tokenSetter = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider);
                        tokenSetter
                           .SetClientAuthentication(DefaultPrivateKeyJwt);
                        tokenSetter.SetAccessTokenFromConfigFile()
                                         .SelectEnvironment(SdkEnvironment.Integration)
                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                         .SetNodeId(AnyNodeId)
                                         .Build();
                    };

        Should.Throw<ArgumentException>(() => build()).Message.ShouldBe("Cannot set access token when client authentication is configured");
    }

    [Fact]
    public void ForbidsSettingClientAuthenticationWhenAccessTokenIsConfigured()
    {
        var build = () =>
                    {
                        var tokenSetter = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider);

                        tokenSetter.SetAccessToken(AnyAccessToken);
                        tokenSetter
                           .SetClientAuthentication(DefaultPrivateKeyJwt)
                                         .SelectEnvironment(SdkEnvironment.Integration)
                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                         .SetNodeId(AnyNodeId)
                                         .Build();
                    };

        Should.Throw<ArgumentException>(() => build()).Message.ShouldBe("Cannot set client authentication when access token is configured");
    }

    [Fact]
    public void ForbidsSettingClientAuthenticationWhenAccessTokenIsConfiguredFromConfigFile()
    {
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionMock.Setup(sectionMock => sectionMock.AccessToken).Returns(AnyAccessToken);

        UofConfigurationSectionProviderMock.Setup(sectionProvider => sectionProvider.GetSection())
                                           .Returns(configurationSectionMock.Object);

        var build = () =>
                    {
                        var tokenSetter = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider);

                        tokenSetter.SetAccessTokenFromConfigFile();
                        tokenSetter
                           .SetClientAuthentication(DefaultPrivateKeyJwt)
                           .SelectEnvironment(SdkEnvironment.Integration)
                           .SetDefaultLanguage(TestConsts.CultureEn)
                           .SetNodeId(AnyNodeId)
                           .Build();
                    };

        Should.Throw<ArgumentException>(() => build()).Message.ShouldBe("Cannot set client authentication when access token is configured");
    }
}
