// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderCacheConfigurationTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenCacheConfigurationHasDefaultValues(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Cache.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenCacheConfigurationHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Cache.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, true)]
    [InlineData(SdkEnvironment.Integration, false)]
    [InlineData(SdkEnvironment.Production, true)]
    [InlineData(SdkEnvironment.Production, false)]
    [InlineData(SdkEnvironment.GlobalIntegration, true)]
    [InlineData(SdkEnvironment.GlobalIntegration, false)]
    [InlineData(SdkEnvironment.GlobalProduction, true)]
    [InlineData(SdkEnvironment.GlobalProduction, false)]
    [InlineData(SdkEnvironment.Replay, true)]
    [InlineData(SdkEnvironment.Replay, false)]
    public void WhenIgnoreBetPalTimelineSportEventStatusSetForPredefinedEnvironmentThenCacheConfigurationReflectsFlag(SdkEnvironment environment, bool ignoreValue)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetIgnoreBetPalTimelineSportEventStatus(ignoreValue)
                    .Build();

        config.Cache.IgnoreBetPalTimelineSportEventStatus.ShouldBe(ignoreValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WhenIgnoreBetPalTimelineSportEventStatusSetForCustomEnvironmentThenCacheConfigurationReflectsFlag(bool ignoreValue)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetIgnoreBetPalTimelineSportEventStatus(ignoreValue)
                    .Build();

        config.Cache.IgnoreBetPalTimelineSportEventStatus.ShouldBe(ignoreValue);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    public void WhenIgnoreBetPalTimelineSportEventStatusCacheTimeoutSetForPredefinedEnvironmentThenCacheConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout.ShouldBe(timeout);
    }

    [Theory]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    public void WhenIgnoreBetPalTimelineSportEventStatusCacheTimeoutSetForCustomEnvironmentThenCacheConfigurationHasExpectedTimeout(int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout.ShouldBe(timeout);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.ProfileCacheTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.ProfileCacheTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.ProfileCacheTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.ProfileCacheTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.ProfileCacheTimeoutMax)]
    public void WhenProfileCacheTimeoutSetForPredefinedEnvironmentThenCacheConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetProfileCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.ProfileCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMax)]
    public void WhenProfileCacheTimeoutSetForCustomEnvironmentThenCacheConfigurationHasExpectedTimeout(int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetProfileCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.ProfileCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SingleVariantMarketTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SingleVariantMarketTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SingleVariantMarketTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SingleVariantMarketTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SingleVariantMarketTimeoutMax)]
    public void WhenVariantMarketDescriptionCacheTimeoutSetForPredefinedEnvironmentThenCacheConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetVariantMarketDescriptionCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.VariantMarketDescriptionCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMax)]
    public void WhenVariantMarketDescriptionCacheTimeoutSetForCustomEnvironmentThenCacheConfigurationHasExpectedTimeout(int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetVariantMarketDescriptionCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.VariantMarketDescriptionCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventCacheTimeoutMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventCacheTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventCacheTimeoutMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventCacheTimeoutMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventCacheTimeoutMax)]
    public void WhenSportEventCacheTimeoutSetForPredefinedEnvironmentThenCacheConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetSportEventCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.SportEventCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMax)]
    public void WhenSportEventCacheTimeoutSetForCustomEnvironmentThenCacheConfigurationHasExpectedTimeout(int timeoutHours)
    {
        var expected = TimeSpan.FromHours(timeoutHours);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetSportEventCacheTimeout(timeoutHours)
                    .Build();

        config.Cache.SportEventCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    public void WhenSportEventStatusCacheTimeoutSetForPredefinedEnvironmentThenCacheConfigurationHasExpectedTimeout(SdkEnvironment environment, int timeoutMinutes)
    {
        var expected = TimeSpan.FromMinutes(timeoutMinutes);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetSportEventStatusCacheTimeout(timeoutMinutes)
                    .Build();

        config.Cache.SportEventStatusCacheTimeout.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    public void WhenSportEventStatusCacheTimeoutSetForCustomEnvironmentThenCacheConfigurationHasExpectedTimeout(int timeoutMinutes)
    {
        var expected = TimeSpan.FromMinutes(timeoutMinutes);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetSportEventStatusCacheTimeout(timeoutMinutes)
                    .Build();

        config.Cache.SportEventStatusCacheTimeout.ShouldBe(expected);
    }
}
