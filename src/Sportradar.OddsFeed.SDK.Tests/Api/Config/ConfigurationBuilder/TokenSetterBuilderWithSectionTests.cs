// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class TokenSetterBuilderWithSectionTests : ConfigurationBuilderWithSectionSetup
{
    [Fact]
    public void DirectSettingOnlyRequiredProperties()
    {
        const string accessToken = "any-access-token";

        var config = GetTokenSetter()
                    .SetAccessToken(accessToken)
                    .SelectEnvironment(SdkEnvironment.Integration)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        Assert.Equal(accessToken, config.AccessToken);
        Assert.Equal(SdkEnvironment.Integration, config.Environment);
        Assert.Equal(TestConsts.CultureEn, config.DefaultLanguage);
    }

    [Fact]
    public void DirectSettingMinimalPropertiesWithoutAccessTokenFails()
    {
        Assert.Throws<InvalidOperationException>(() => GetTokenSetter().SetAccessTokenFromConfigFile());
    }

    [Fact]
    public void DirectSettingMinimalPropertiesWithoutLanguageFails()
    {
        Assert.Throws<InvalidOperationException>(() => GetTokenSetter().SetAccessToken(TestConsts.AnyAccessToken).SelectEnvironment(SdkEnvironment.Integration).Build());
    }

    [Fact]
    public void SettingNullAccessTokenThrows()
    {
        Assert.Throws<ArgumentException>(() => GetTokenSetter().SetAccessToken(null));
    }

    [Fact]
    public void SettingEmptyAccessTokenThrows()
    {
        Assert.Throws<ArgumentException>(() => GetTokenSetter().SetAccessToken(string.Empty));
    }

    [Fact]
    public void TokenFromConfigurationSectionIsConfigured()
    {
        var section = UofConfigurationSections.OnlyRequiredFields();

        var config = GetTokenSetter(section).BuildFromConfigFile();

        Assert.Equal(section.AccessToken, config.AccessToken);
        Assert.Equal(section.Environment, config.Environment);
        ValidateDefaultConfig(config, section.Environment);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, section.Environment);
        ValidateRabbitConfigForEnvironment(config, section.Environment);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }

    [Fact]
    public void EnvironmentFromConfigurationSectionCanBeOverridden()
    {
        var section = UofConfigurationSections.OnlyRequiredFields();

        var config = GetTokenSetter(section).SetAccessTokenFromConfigFile().SelectEnvironment(SdkEnvironment.GlobalProduction).LoadFromConfigFile().Build();

        Assert.Equal(section.AccessToken, config.AccessToken);
        Assert.Equal(SdkEnvironment.GlobalProduction, config.Environment);
        ValidateDefaultConfig(config, SdkEnvironment.GlobalProduction);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, SdkEnvironment.GlobalProduction);
        ValidateRabbitConfigForEnvironment(config, SdkEnvironment.GlobalProduction);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }

    [Fact]
    public void TokenFromConfigurationSectionCanNotBeOverridden()
    {
        var section = UofConfigurationSections.OnlyRequiredFields();

        var config = GetTokenSetter(section).SetAccessTokenFromConfigFile().SelectEnvironmentFromConfigFile().LoadFromConfigFile().Build();

        Assert.Equal(section.AccessToken, config.AccessToken);
        ValidateDefaultConfig(config, section.Environment);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, section.Environment);
        ValidateRabbitConfigForEnvironment(config, section.Environment);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }
}
