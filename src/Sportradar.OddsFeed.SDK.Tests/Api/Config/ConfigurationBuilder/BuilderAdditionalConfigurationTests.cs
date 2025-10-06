// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderAdditionalConfigurationTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenAdditionalConfigurationHasDefaultValues(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Additional.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenAdditionalConfigurationHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Additional.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.StatisticsIntervalMinutesMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.StatisticsIntervalMinutesMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.StatisticsIntervalMinutesMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.StatisticsIntervalMinutesMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.StatisticsIntervalMinutesMax)]
    public void WhenStatisticsIntervalSetForPredefinedEnvironmentThenAdditionalConfigurationHasExpectedInterval(SdkEnvironment environment, int timeoutMinutes)
    {
        var expected = TimeSpan.FromMinutes(timeoutMinutes);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetStatisticsInterval(timeoutMinutes)
                    .Build();

        config.Additional.StatisticsInterval.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMax)]
    public void WhenStatisticsIntervalSetForCustomEnvironmentThenAdditionalConfigurationHasExpectedInterval(int timeoutMinutes)
    {
        var expected = TimeSpan.FromMinutes(timeoutMinutes);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetStatisticsInterval(timeoutMinutes)
                    .Build();

        config.Additional.StatisticsInterval.ShouldBe(expected);
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
    public void WhenOmitMarketMappingsSetForPredefinedEnvironmentThenAdditionalConfigurationHasExpectedValue(SdkEnvironment environment, bool omitMarketMappings)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .OmitMarketMappings(omitMarketMappings)
                    .Build();

        config.Additional.OmitMarketMappings.ShouldBe(omitMarketMappings);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WhenOmitMarketMappingsSetForCustomEnvironmentThenAdditionalConfigurationHasExpectedValue(bool omitMarketMappings)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .OmitMarketMappings(omitMarketMappings)
                    .Build();

        config.Additional.OmitMarketMappings.ShouldBe(omitMarketMappings);
    }
}
