// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderProducerConfigurationTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenProducerConfigurationHasDefaultValues(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Producer.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenProducerConfigurationHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Producer.ShouldHaveDefaultValuesFromConfigLimit();
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentAndDisabledProducersContainsSingleValueThenItIsSet(SdkEnvironment environment)
    {
        List<int> disabledProducers = [1];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldHaveSingleItem();
        config.Producer.DisabledProducers.ShouldBeEquivalentTo(disabledProducers);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentAndDisabledProducersContainsSingleValueThenItIsSet()
    {
        List<int> disabledProducers = [1];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldHaveSingleItem();
        config.Producer.DisabledProducers.ShouldBeEquivalentTo(disabledProducers);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentAndDisabledProducersHasMultipleValuesThenAllAreSet(SdkEnvironment environment)
    {
        List<int> disabledProducers = [1, 3, 5];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldBeOfSize(3);
        config.Producer.DisabledProducers.ShouldBeEquivalentTo(disabledProducers);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentAndDisabledProducersHasMultipleValuesThenAllAreSet()
    {
        List<int> disabledProducers = [1, 3, 5];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldBeOfSize(3);
        config.Producer.DisabledProducers.ShouldBeEquivalentTo(disabledProducers);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentAndDisabledProducersContainsDuplicatesThenDuplicatesAreIgnored(SdkEnvironment environment)
    {
        List<int> disabledProducers = [1, 3, 5, 5, 3, 1, 1, 1, 3];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldBeOfSize(3);
        config.Producer.DisabledProducers[0].ShouldBe(disabledProducers[0]);
        config.Producer.DisabledProducers.ShouldContain(1);
        config.Producer.DisabledProducers.ShouldContain(3);
        config.Producer.DisabledProducers.ShouldContain(5);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentAndDisabledProducersContainsDuplicatesThenDuplicatesAreIgnored()
    {
        List<int> disabledProducers = [1, 3, 5, 5, 3, 1, 1, 1, 3];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDisabledProducers(disabledProducers)
                    .Build();

        config.Producer.DisabledProducers.ShouldBeOfSize(3);
        config.Producer.DisabledProducers[0].ShouldBe(disabledProducers[0]);
        config.Producer.DisabledProducers.ShouldContain(1);
        config.Producer.DisabledProducers.ShouldContain(3);
        config.Producer.DisabledProducers.ShouldContain(5);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MaxRecoveryTimeMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MaxRecoveryTimeMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MaxRecoveryTimeMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MaxRecoveryTimeMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MaxRecoveryTimeMax)]
    public void WhenBuiltForPredefinedEnvironmentAndMaxRecoveryTimeIsSetThenProducerConfigurationReflectsThat(SdkEnvironment environment, int maxRecoveryTime)
    {
        var expected = TimeSpan.FromSeconds(maxRecoveryTime);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMaxRecoveryTime(maxRecoveryTime)
                    .Build();

        config.Producer.MaxRecoveryTime.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(ConfigLimit.MaxRecoveryTimeMax)]
    public void WhenBuiltForCustomEnvironmentAndMaxRecoveryTimeIsSetThenProducerConfigurationReflectsThat(int maxRecoveryTime)
    {
        var expected = TimeSpan.FromSeconds(maxRecoveryTime);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMaxRecoveryTime(maxRecoveryTime)
                    .Build();

        config.Producer.MaxRecoveryTime.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    public void WhenBuiltForPredefinedEnvironmentAndMinIntervalBetweenRecoveryRequestsIsSetThenProducerConfigurationReflectsThat(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMinIntervalBetweenRecoveryRequests(timeoutSeconds)
                    .Build();

        config.Producer.MinIntervalBetweenRecoveryRequests.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    public void WhenBuiltForCustomEnvironmentAndMinIntervalBetweenRecoveryRequestsIsSetThenProducerConfigurationReflectsThat(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetMinIntervalBetweenRecoveryRequests(timeoutSeconds)
                    .Build();

        config.Producer.MinIntervalBetweenRecoveryRequests.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsMax)]
    public void WhenBuiltForPredefinedEnvironmentAndInactivitySecondsIsSetThenProducerConfigurationReflectsThat(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetInactivitySeconds(timeoutSeconds)
                    .Build();

        config.Producer.InactivitySeconds.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.InactivitySecondsMin)]
    [InlineData(ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(ConfigLimit.InactivitySecondsMax)]
    public void WhenBuiltForCustomEnvironmentAndInactivitySecondsIsSetThenProducerConfigurationReflectsThat(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetInactivitySeconds(timeoutSeconds)
                    .Build();

        config.Producer.InactivitySeconds.ShouldBe(expected);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(SdkEnvironment.Integration, ConfigLimit.InactivitySecondsPrematchMax)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(SdkEnvironment.Production, ConfigLimit.InactivitySecondsPrematchMax)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(SdkEnvironment.GlobalIntegration, ConfigLimit.InactivitySecondsPrematchMax)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(SdkEnvironment.GlobalProduction, ConfigLimit.InactivitySecondsPrematchMax)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(SdkEnvironment.Replay, ConfigLimit.InactivitySecondsPrematchMax)]
    public void WhenBuiltForPredefinedEnvironmentAndInactivitySecondsPrematchIsSetThenProducerConfigurationReflectsThat(SdkEnvironment environment, int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetInactivitySecondsPrematch(timeoutSeconds)
                    .Build();

        config.Producer.InactivitySecondsPrematch.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMax)]
    public void WhenBuiltForCustomEnvironmentAndInactivitySecondsPrematchIsSetThenProducerConfigurationReflectsThat(int timeoutSeconds)
    {
        var expected = TimeSpan.FromSeconds(timeoutSeconds);

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetInactivitySecondsPrematch(timeoutSeconds)
                    .Build();

        config.Producer.InactivitySecondsPrematch.ShouldBe(expected);
    }
}
