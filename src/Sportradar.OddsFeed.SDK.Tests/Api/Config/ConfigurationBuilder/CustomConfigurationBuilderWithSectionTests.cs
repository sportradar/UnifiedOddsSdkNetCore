// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

// Ignore Spelling: Ssl
// ReSharper disable TooManyChainedReferences

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class CustomConfigurationBuilderWithSectionTests : ConfigurationBuilderWithSectionSetup
{
    private const string MessagingUsername = "AnyMessagingUsername";
    private const string MessagingPassword = "AnyMessagingPassword";

    [Fact]
    public void ApiHostHasCorrectValue()
    {
        var config = BuildCustomConfig().SetApiHost(TestConsts.AnyApiHost).Build();

        Assert.Equal(TestConsts.AnyApiHost, config.Api.Host);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApiUseSslHasCorrectValue(bool useSsl)
    {
        var config = BuildCustomConfig().UseApiSsl(useSsl).Build();

        Assert.Equal(useSsl, config.Api.UseSsl);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MessagingUseSslHasCorrectValue(bool useSsl)
    {
        var config = BuildCustomConfig().UseMessagingSsl(useSsl).Build();

        Assert.Equal(useSsl, config.Rabbit.UseSsl);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingHostHasCorrectValue()
    {
        var config = BuildCustomConfig().SetMessagingHost(TestConsts.AnyRabbitHost).Build();

        Assert.Equal(TestConsts.AnyRabbitHost, config.Rabbit.Host);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingCredentialsHasCorrectValue()
    {
        var config = BuildCustomConfig().SetMessagingCredentials(MessagingUsername, MessagingPassword).Build();

        config.Rabbit.Username.ShouldBe(MessagingUsername);
        config.Rabbit.Password.ShouldBe(MessagingPassword);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingPortHasCorrectValue()
    {
        const int port = 12345;
        var config = BuildCustomConfig().SetMessagingPort(port).Build();

        Assert.Equal(port, config.Rabbit.Port);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void BuilderSetAccessToken()
    {
        var config = GetTokenSetterWithoutSection()
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(TestConsts.AnyAccessToken);
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void SectionWithAccessToken()
    {
        var section = UofConfigurationSections.OnlyRequiredFields();

        var config = GetTokenSetterWithSection(section)
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.AccessToken.ShouldBe(section.AccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(section.AccessToken);
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndCustomUserNameAndPassword()
    {
        var config = GetTokenSetterWithoutSection()
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMessagingCredentials(MessagingUsername, MessagingPassword)
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(MessagingUsername);
        config.Rabbit.Password.ShouldBe(MessagingPassword);
    }

    [Fact]
    public void SectionWithAccessTokenAndBuilderSetsCustomUserNameAndPassword()
    {
        var section = UofConfigurationSections.OnlyRequiredFields();

        var config = GetTokenSetterWithSection(section)
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMessagingCredentials(MessagingUsername, MessagingPassword)
                    .Build();

        config.AccessToken.ShouldBe(section.AccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(MessagingUsername);
        config.Rabbit.Password.ShouldBe(MessagingPassword);
    }

    [Fact]
    public void BuilderSetAuthentication()
    {
        var config = GetTokenSetterWithoutSection()
                    .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.AccessToken.ShouldBeNull();
        config.Authentication.ShouldNotBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBeNull();
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void BuilderSetAuthenticationAndBuilderSetsValidUserNameAndPassword()
    {
        var config = GetTokenSetterWithoutSection()
                    .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMessagingCredentials(MessagingUsername, MessagingPassword)
                    .Build();

        config.AccessToken.ShouldBeNull();
        config.Authentication.ShouldNotBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(MessagingUsername);
        config.Rabbit.Password.ShouldBe(MessagingPassword);
    }

    [Fact]
    public void BuilderSetAuthenticationWithNullUserNameThenThrow()
    {
        var error = Should.Throw<ArgumentNullException>(() => GetTokenSetterWithoutSection()
                                                             .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                             .SelectCustom()
                                                             .SetDefaultLanguage(TestConsts.CultureEn)
                                                             .SetMessagingCredentials(null, MessagingPassword));

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderSetAuthenticationWithEmptyUserNameThenThrow()
    {
        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithoutSection()
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .SetMessagingCredentials(string.Empty, MessagingPassword));

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderSetAuthenticationWithNullPasswordThenThrow()
    {
        var error = Should.Throw<ArgumentNullException>(() => GetTokenSetterWithoutSection()
                                                             .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                             .SelectCustom()
                                                             .SetDefaultLanguage(TestConsts.CultureEn)
                                                             .SetMessagingCredentials(MessagingUsername, null));

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderSetAuthenticationWithEmptyPasswordThenThrow()
    {
        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithoutSection()
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .SetMessagingCredentials(MessagingUsername, string.Empty));

        error.ShouldNotBeNull();
    }

    [Fact]
    public void SectionWithAccessTokenAndCustomUserNameAndPassword()
    {
        var section = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                              .WithRabbitUsername(MessagingUsername)
                                              .WithRabbitPassword(MessagingPassword)
                                              .Build();

        var config = GetTokenSetterWithSection(section)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(section.RabbitUsername);
        config.Rabbit.Password.ShouldBe(section.RabbitPassword);
    }

    [Fact]
    public void BuilderSetAuthenticationAndCustomUserNameAndPasswordInSection()
    {
        var section = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                              .WithRabbitUsername(MessagingUsername)
                                              .WithRabbitPassword(MessagingPassword)
                                              .Build();

        var config = GetTokenSetterWithSection(section)
                    .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBeNull();
        config.Authentication.ShouldNotBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(section.RabbitUsername);
        config.Rabbit.Password.ShouldBe(section.RabbitPassword);
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithNullUserNameAndValidPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(null)
                                                    .WithRabbitPassword(MessagingPassword)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetAccessToken(TestConsts.AnyAccessToken)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithEmptyUserNameAndValidPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(string.Empty)
                                                    .WithRabbitPassword(MessagingPassword)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetAccessToken(TestConsts.AnyAccessToken)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithValidUserNameAndNullPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(MessagingUsername)
                                                    .WithRabbitPassword(null)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetAccessToken(TestConsts.AnyAccessToken)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithValidUserNameAndEmptyPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(MessagingUsername)
                                                    .WithRabbitPassword(string.Empty)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetAccessToken(TestConsts.AnyAccessToken)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithNullUserNameAndValidPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(null)
                                                    .WithRabbitPassword(MessagingPassword)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithEmptyUserNameAndValidPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(string.Empty)
                                                    .WithRabbitPassword(MessagingPassword)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithValidUserNameAndNullPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(MessagingUsername)
                                                    .WithRabbitPassword(null)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithValidUserNameAndEmptyPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(MessagingUsername)
                                                    .WithRabbitPassword(string.Empty)
                                                    .Build();

        var error = Should.Throw<ArgumentException>(() => GetTokenSetterWithSection(customSection)
                                                         .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                                                         .SelectCustom()
                                                         .SetDefaultLanguage(TestConsts.CultureEn)
                                                         .LoadFromConfigFile());

        error.ShouldNotBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithEmptyUserNameAndNullPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(string.Empty)
                                                    .WithRabbitPassword(null)
                                                    .Build();

        var config = GetTokenSetterWithSection(customSection)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(TestConsts.AnyAccessToken); // questionable
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void BuilderWithAccessTokenAndSectionWithNullUserNameAndEmptyPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(null)
                                                    .WithRabbitPassword(string.Empty)
                                                    .Build();

        var config = GetTokenSetterWithSection(customSection)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBe(TestConsts.AnyAccessToken);
        config.Authentication.ShouldBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBe(TestConsts.AnyAccessToken); // questionable
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithEmptyUserNameAndNullPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(string.Empty)
                                                    .WithRabbitPassword(null)
                                                    .Build();

        var config = GetTokenSetterWithSection(customSection)
                    .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBeNull();
        config.Authentication.ShouldNotBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBeNull();
        config.Rabbit.Password.ShouldBeNull();
    }

    [Fact]
    public void BuilderWithAuthenticationAndSectionWithNullUserNameAndEmptyPassword()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithRabbitUsername(null)
                                                    .WithRabbitPassword(string.Empty)
                                                    .Build();

        var config = GetTokenSetterWithSection(customSection)
                    .SetClientAuthentication(TestConsts.AnyPrivateKeyJwt)
                    .SelectCustom()
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .LoadFromConfigFile()
                    .Build();

        config.AccessToken.ShouldBeNull();
        config.Authentication.ShouldNotBeNull();
        config.Rabbit.ShouldNotBeNull();
        config.Rabbit.Username.ShouldBeNull();
        config.Rabbit.Password.ShouldBeNull();
    }

    private static TokenSetter GetTokenSetterWithoutSection()
    {
        return new TokenSetter(UofConfigurationSectionProviderBuilder.CreateWithoutSection(), new TestBookmakerDetailsProvider(), new TestProducersProvider());
    }

    private static TokenSetter GetTokenSetterWithSection(IUofConfigurationSection section)
    {
        return new TokenSetter(UofConfigurationSectionProviderBuilder.CreateWith(section), new TestBookmakerDetailsProvider(), new TestProducersProvider());
    }
}
