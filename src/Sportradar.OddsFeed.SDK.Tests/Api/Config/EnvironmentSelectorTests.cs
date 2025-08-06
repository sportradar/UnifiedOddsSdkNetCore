// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;
public class EnvironmentSelectorTests : ConfigurationBuilderSetup
{
    private readonly UofConfiguration _configuration = new UofConfiguration(new TestSectionProvider(null));
    private readonly IBookmakerDetailsProvider _bookmakerDetailsProvider = new TestBookmakerDetailsProvider();
    private readonly IProducersProvider _producersProvider = new TestProducersProvider();

    [Fact]
    public void EnvironmentSelectorConstructWithMissingConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new EnvironmentSelector(null, new TestSectionProvider(BaseSection), _bookmakerDetailsProvider, _producersProvider));
    }

    [Fact]
    public void EnvironmentSelectorConstructWithMissingSectionProvider()
    {
        Assert.Throws<ArgumentNullException>(() => new EnvironmentSelector(_configuration, null, _bookmakerDetailsProvider, _producersProvider));
    }

    [Fact]
    public void EnvironmentSelectorConstructWithMissingBookmakerDetailsProvider()
    {
        var selector = new EnvironmentSelector(_configuration, new TestSectionProvider(BaseSection), null, _producersProvider);

        Assert.NotNull(selector);
    }

    [Fact]
    public void EnvironmentSelectorConstructWithMissingProducersProvider()
    {
        var selector = new EnvironmentSelector(_configuration, new TestSectionProvider(BaseSection), _bookmakerDetailsProvider, null);

        Assert.NotNull(selector);
    }

    [Fact]
    public void SettingEnvironmentFromSectionWhenNonePresentThrows()
    {
        Assert.Throws<InvalidOperationException>(
            () => new EnvironmentSelector(_configuration, new TestSectionProvider(null), _bookmakerDetailsProvider, _producersProvider).SelectEnvironmentFromConfigFile());
    }

    [Fact]
    public void SelectIntegrationEnvironmentReturnTest()
    {
        var configurationBuilder = GetEnvironmentSelector().SelectEnvironment(SdkEnvironment.Integration);

        Assert.NotNull(configurationBuilder);
    }

    [Fact]
    public void SelectProductionEnvironmentReturnTest()
    {
        var configurationBuilder = GetEnvironmentSelector().SelectEnvironment(SdkEnvironment.Production);

        Assert.NotNull(configurationBuilder);
    }

    [Fact]
    public void SelectCustomEnvironmentThrows()
    {
        Assert.Throws<InvalidOperationException>(() => GetEnvironmentSelector().SelectEnvironment(SdkEnvironment.Custom));
    }

    [Fact]
    public void SelectReplayEnvironmentReturnTest()
    {
        var configurationBuilder = GetEnvironmentSelector().SelectReplay();

        Assert.NotNull(configurationBuilder);
    }

    [Fact]
    public void SelectCustomEnvironmentReturnTest()
    {
        var configurationBuilder = GetEnvironmentSelector().SelectCustom();

        Assert.NotNull(configurationBuilder);
        Assert.IsAssignableFrom<CustomConfigurationBuilder>(configurationBuilder);
    }

    [Fact]
    public void IntegrationEnvironmentResultValidation()
    {
        var cfg = GetEnvironmentSelector()
            .SelectEnvironment(SdkEnvironment.Integration)
            .SetDefaultLanguage(ScheduleData.CultureDe)
            .Build();

        Assert.NotNull(cfg);
        Assert.Equal(SdkEnvironment.Integration, cfg.Environment);
        VerifyConfig(cfg, SdkEnvironment.Integration, ScheduleData.CultureDe);
    }

    [Fact]
    public void ProductionEnvironmentResultValidation()
    {
        var cfg = GetEnvironmentSelector()
            .SelectEnvironment(SdkEnvironment.Production)
            .SetDefaultLanguage(ScheduleData.CultureDe)
            .Build();

        Assert.NotNull(cfg);
        Assert.Equal(SdkEnvironment.Production, cfg.Environment);
        VerifyConfig(cfg, SdkEnvironment.Production, ScheduleData.CultureDe);
    }

    [Fact]
    public void ReplayEnvironmentResultValidation()
    {
        var cfg = GetEnvironmentSelector()
            .SelectReplay()
            .SetDesiredLanguages(new List<CultureInfo> { ScheduleData.CultureDe })
            .Build();

        Assert.NotNull(cfg);
        Assert.Equal(SdkEnvironment.Replay, cfg.Environment);
        VerifyConfig(cfg, SdkEnvironment.Replay, ScheduleData.CultureDe);
    }

    [Fact]
    public void CustomEnvironmentDefaultResultValidation()
    {
        var cfg = GetEnvironmentSelector()
                .SelectCustom()
                .SetDesiredLanguages(new List<CultureInfo> { ScheduleData.CultureNl })
                .Build();

        Assert.NotNull(cfg);
        Assert.Equal(SdkEnvironment.Custom, cfg.Environment);
        VerifyConfig(cfg, SdkEnvironment.Integration, ScheduleData.CultureNl);
    }

    [Fact]
    public void ReplayShouldTargetIntegrationApiBecauseReplayButWeNeedAnExplanationWhy()
    {
        var cfg = GetEnvironmentSelector().SelectReplay().SetDefaultLanguage(ScheduleData.CultureEn).Build();

        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), cfg.Api.Host);
    }

    [Fact]
    public void ReplayShouldPointToNonGlobalMessagingHostAsItIsLongTermStrategy()
    {
        var cfg = GetEnvironmentSelector().SelectReplay().SetDefaultLanguage(ScheduleData.CultureEn).Build();

        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), cfg.Rabbit.Host);
    }

    [Fact]
    public void ReplayConfigurationShouldBeCreatedForReplayEnvironment()
    {
        var cfg = GetEnvironmentSelector().SelectReplay().SetDefaultLanguage(ScheduleData.CultureEn).Build();

        Assert.Equal(SdkEnvironment.Replay, cfg.Environment);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static void VerifyConfig(IUofConfiguration config, SdkEnvironment environment, CultureInfo baseLanguage)
    {
        Assert.Equal(TestAccessToken, config.AccessToken);
        Assert.Equal(baseLanguage, config.DefaultLanguage);
        Assert.Single(config.Languages);
        Assert.Equal(baseLanguage, config.Languages[0]);

        ValidateApiConfigForEnvironment(config.Api, environment);
        ValidateRabbitConfigForEnvironment(config, environment);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }
}
