// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class TokenSetterBuilderTests : ConfigurationBuilderSetup
{
    [Fact]
    public void DirectSettingMinimalProperties()
    {
        Assert.Equal(TestAccessToken, GetTokenSetter().SetAccessToken(TestAccessToken).SelectEnvironment(SdkEnvironment.Integration).SetDefaultLanguage(TestData.Culture).Build().AccessToken);
    }

    [Fact]
    public void DirectSettingMinimalPropertiesWithoutAccessTokenFails()
    {
        Assert.Throws<InvalidOperationException>(() => GetTokenSetter().SetAccessTokenFromConfigFile().SelectEnvironment(SdkEnvironment.Integration).Build());
    }

    [Fact]
    public void DirectSettingMinimalPropertiesWithoutLanguageFails()
    {
        Assert.Throws<InvalidOperationException>(() => GetTokenSetter().SetAccessToken(TestAccessToken).SelectEnvironment(SdkEnvironment.Integration).Build());
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
        Assert.Equal(SdkEnvironment.Integration, BaseSection.Environment);
        var config = GetTokenSetter(BaseSection).BuildFromConfigFile();
        Assert.Equal(BaseSection.AccessToken, config.AccessToken);
        Assert.Equal(BaseSection.Environment, config.Environment);

        ValidateDefaultConfig(config, BaseSection.Environment);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, BaseSection.Environment);
        ValidateRabbitConfigForEnvironment(config, BaseSection.Environment);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }

    [Fact]
    public void EnvironmentFromConfigurationSectionCanBeOverridden()
    {
        Assert.Equal(SdkEnvironment.Integration, BaseSection.Environment);
        var config = GetTokenSetter(BaseSection).SetAccessTokenFromConfigFile().SelectEnvironment(SdkEnvironment.GlobalProduction).LoadFromConfigFile().Build();
        Assert.Equal(BaseSection.AccessToken, config.AccessToken);
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
        var config = GetTokenSetter(BaseSection).SetAccessTokenFromConfigFile().SelectEnvironmentFromConfigFile().LoadFromConfigFile().Build();
        Assert.Equal(BaseSection.AccessToken, config.AccessToken);

        ValidateDefaultConfig(config, BaseSection.Environment);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, BaseSection.Environment);
        ValidateRabbitConfigForEnvironment(config, BaseSection.Environment);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }
}
