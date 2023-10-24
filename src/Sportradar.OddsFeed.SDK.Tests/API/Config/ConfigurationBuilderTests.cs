/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class ConfigurationBuilderTests : ConfigurationBuilderSetup
{
    [Fact]
    public void MissingLanguageThrows()
    {
        Assert.Throws<InvalidOperationException>(() => IntegrationBuilder(TestData.AccessToken).Build());
        Assert.Throws<InvalidOperationException>(() => ProductionBuilder(TestData.AccessToken).Build());
        Assert.Throws<InvalidOperationException>(() => ReplayBuilder(TestData.AccessToken).Build());
        Assert.Throws<InvalidOperationException>(() => CustomBuilder(TestData.AccessToken).Build());
    }

    [Fact]
    public void AccessTokenHasCorrectValue()
    {
        Assert.Equal(TestData.AccessToken, IntegrationBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureHu).Build().AccessToken);
        Assert.Equal(TestData.AccessToken, ProductionBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureHu).Build().AccessToken);
        Assert.Equal(TestData.AccessToken, ReplayBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureHu).Build().AccessToken);
        Assert.Equal(TestData.AccessToken, CustomBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureHu).Build().AccessToken);
    }

    [Fact]
    public void DefaultLanguageSetHasCorrectValue()
    {
        var lang1 = ScheduleData.CultureDe;
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDefaultLanguage(lang1).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDefaultLanguage(lang1).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDefaultLanguage(lang1).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDefaultLanguage(lang1).Build();

        Assert.Equal(lang1, integrationConfig.DefaultLanguage);
        Assert.Equal(lang1, productionConfig.DefaultLanguage);
        Assert.Equal(lang1, replayConfig.DefaultLanguage);
        Assert.Equal(lang1, customConfig.DefaultLanguage);

        Assert.Single(integrationConfig.Languages);
        Assert.Single(productionConfig.Languages);
        Assert.Single(replayConfig.Languages);
        Assert.Single(customConfig.Languages);

        Assert.Equal(lang1, integrationConfig.Languages.First());
        Assert.Equal(lang1, productionConfig.Languages.First());
        Assert.Equal(lang1, replayConfig.Languages.First());
        Assert.Equal(lang1, customConfig.Languages.First());

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void LanguagesSetHasCorrectValue()
    {
        var lang1 = ScheduleData.Cultures3;
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();

        Assert.Equal(lang1.First(), integrationConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), productionConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), replayConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), customConfig.DefaultLanguage);

        Assert.Equal(3, integrationConfig.Languages.Count);
        Assert.Equal(3, productionConfig.Languages.Count);
        Assert.Equal(3, replayConfig.Languages.Count);
        Assert.Equal(3, customConfig.Languages.Count);

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void SettingLanguageMultipleTimesConfigsOnlyUniqueOnes()
    {
        var lang1 = new List<CultureInfo> { ScheduleData.CultureEn, ScheduleData.CultureEn, ScheduleData.CultureEn };
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).Build();

        Assert.Equal(lang1.First(), integrationConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), productionConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), replayConfig.DefaultLanguage);
        Assert.Equal(lang1.First(), customConfig.DefaultLanguage);

        Assert.Single(integrationConfig.Languages);
        Assert.Single(productionConfig.Languages);
        Assert.Single(replayConfig.Languages);
        Assert.Single(customConfig.Languages);

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void CombinationOfDefaultLanguageWithLanguages()
    {
        var lang1 = new List<CultureInfo> { ScheduleData.CultureEn, ScheduleData.CultureDe };
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();

        Assert.Equal(ScheduleData.CultureNl, integrationConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, productionConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, replayConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, customConfig.DefaultLanguage);

        Assert.Equal(3, integrationConfig.Languages.Count);
        Assert.Equal(3, productionConfig.Languages.Count);
        Assert.Equal(3, replayConfig.Languages.Count);
        Assert.Equal(3, customConfig.Languages.Count);

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void CombinationOfDefaultLanguageWithLanguagesIncludingDefaultLanguage()
    {
        var lang1 = new List<CultureInfo> { ScheduleData.CultureEn, ScheduleData.CultureDe, ScheduleData.CultureNl };
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureNl).SetDesiredLanguages(lang1).Build();

        Assert.Equal(ScheduleData.CultureNl, integrationConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, productionConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, replayConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, customConfig.DefaultLanguage);

        Assert.Equal(3, integrationConfig.Languages.Count);
        Assert.Equal(3, productionConfig.Languages.Count);
        Assert.Equal(3, replayConfig.Languages.Count);
        Assert.Equal(3, customConfig.Languages.Count);

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void SettingCombinationOfLanguagesWithDefaultLanguage()
    {
        var lang1 = new List<CultureInfo> { ScheduleData.CultureEn, ScheduleData.CultureDe };
        var integrationConfig = IntegrationBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).SetDefaultLanguage(ScheduleData.CultureNl).Build();
        var productionConfig = ProductionBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).SetDefaultLanguage(ScheduleData.CultureNl).Build();
        var replayConfig = ReplayBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).SetDefaultLanguage(ScheduleData.CultureNl).Build();
        var customConfig = CustomBuilder(TestData.AccessToken).SetDesiredLanguages(lang1).SetDefaultLanguage(ScheduleData.CultureNl).Build();

        Assert.Equal(ScheduleData.CultureNl, integrationConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, productionConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, replayConfig.DefaultLanguage);
        Assert.Equal(ScheduleData.CultureNl, customConfig.DefaultLanguage);

        Assert.Equal(3, integrationConfig.Languages.Count);
        Assert.Equal(3, productionConfig.Languages.Count);
        Assert.Equal(3, replayConfig.Languages.Count);
        Assert.Equal(3, customConfig.Languages.Count);

        ConfigHasDefaultValuesSet(integrationConfig);
        ConfigHasDefaultValuesSet(productionConfig);
        ConfigHasDefaultValuesSet(replayConfig);
        ConfigHasDefaultValuesSet(customConfig);
    }

    [Fact]
    public void DisabledProducersSetSingleHasCorrectValue()
    {
        var disableProducers = new List<int> { 1 };
        var integrationConfig = BuildConfig("i").SetDisabledProducers(disableProducers).Build();
        var productionConfig = BuildConfig("p").SetDisabledProducers(disableProducers).Build();
        var replayConfig = BuildConfig("r").SetDisabledProducers(disableProducers).Build();
        var customConfig = BuildCustomConfig().SetDisabledProducers(disableProducers).Build();

        Assert.Equal(disableProducers.First(), integrationConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), productionConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), replayConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), customConfig.Producer.DisabledProducers.First());

        Assert.Single(integrationConfig.Producer.DisabledProducers);
        Assert.Single(productionConfig.Producer.DisabledProducers);
        Assert.Single(replayConfig.Producer.DisabledProducers);
        Assert.Single(customConfig.Producer.DisabledProducers);
    }

    [Fact]
    public void DisabledProducersSetMultipleHasCorrectValue()
    {
        var disableProducers = new List<int> { 1, 3, 5 };
        var integrationConfig = BuildConfig("i").SetDisabledProducers(disableProducers).Build();
        var productionConfig = BuildConfig("p").SetDisabledProducers(disableProducers).Build();
        var replayConfig = BuildConfig("r").SetDisabledProducers(disableProducers).Build();
        var customConfig = BuildCustomConfig().SetDisabledProducers(disableProducers).Build();

        Assert.Equal(disableProducers.First(), integrationConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), productionConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), replayConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), customConfig.Producer.DisabledProducers.First());

        Assert.Equal(3, integrationConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, productionConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, replayConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, customConfig.Producer.DisabledProducers.Count);
    }

    [Fact]
    public void DisabledProducersSetMultipleOnlyUniqueAreSaved()
    {
        var disableProducers = new List<int> { 1, 3, 5, 5, 3, 1, 1, 1, 3 };
        var integrationConfig = BuildConfig("i").SetDisabledProducers(disableProducers).Build();
        var productionConfig = BuildConfig("p").SetDisabledProducers(disableProducers).Build();
        var replayConfig = BuildConfig("r").SetDisabledProducers(disableProducers).Build();
        var customConfig = BuildCustomConfig().SetDisabledProducers(disableProducers).Build();

        Assert.Equal(disableProducers.First(), integrationConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), productionConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), replayConfig.Producer.DisabledProducers.First());
        Assert.Equal(disableProducers.First(), customConfig.Producer.DisabledProducers.First());

        Assert.Equal(3, integrationConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, productionConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, replayConfig.Producer.DisabledProducers.Count);
        Assert.Equal(3, customConfig.Producer.DisabledProducers.Count);
    }

    [Theory]
    [InlineData(-111)]
    [InlineData(0)]
    [InlineData(111)]
    public void NodeIdIsSet(int nodeId)
    {
        var integrationConfig = BuildConfig("i").SetNodeId(nodeId).Build();
        var productionConfig = BuildConfig("p").SetNodeId(nodeId).Build();
        var replayConfig = BuildConfig("r").SetNodeId(nodeId).Build();
        var customConfig = BuildCustomConfig().SetNodeId(nodeId).Build();

        Assert.Equal(nodeId, integrationConfig.NodeId);
        Assert.Equal(nodeId, productionConfig.NodeId);
        Assert.Equal(nodeId, replayConfig.NodeId);
        Assert.Equal(nodeId, customConfig.NodeId);
    }

    [Theory]
    [InlineData(ExceptionHandlingStrategy.Throw)]
    [InlineData(ExceptionHandlingStrategy.Catch)]
    public void ExceptionHandlingStrategyIsSet(ExceptionHandlingStrategy strategy)
    {
        var integrationConfig = BuildConfig("i").SetExceptionHandlingStrategy(strategy).Build();
        var productionConfig = BuildConfig("p").SetExceptionHandlingStrategy(strategy).Build();
        var replayConfig = BuildConfig("r").SetExceptionHandlingStrategy(strategy).Build();
        var customConfig = BuildCustomConfig().SetExceptionHandlingStrategy(strategy).Build();

        Assert.Equal(strategy, integrationConfig.ExceptionHandlingStrategy);
        Assert.Equal(strategy, productionConfig.ExceptionHandlingStrategy);
        Assert.Equal(strategy, replayConfig.ExceptionHandlingStrategy);
        Assert.Equal(strategy, customConfig.ExceptionHandlingStrategy);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientTimeoutMax)]
    public void HttpClientTimeoutIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetHttpClientTimeout(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetHttpClientTimeout(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetHttpClientTimeout(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetHttpClientTimeout(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Api.HttpClientTimeout);
        Assert.Equal(timeout, productionConfig.Api.HttpClientTimeout);
        Assert.Equal(timeout, replayConfig.Api.HttpClientTimeout);
        Assert.Equal(timeout, customConfig.Api.HttpClientTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMax)]
    public void HttpClientRecoveryTimeoutIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetHttpClientRecoveryTimeout(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetHttpClientRecoveryTimeout(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetHttpClientRecoveryTimeout(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetHttpClientRecoveryTimeout(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Api.HttpClientRecoveryTimeout);
        Assert.Equal(timeout, productionConfig.Api.HttpClientRecoveryTimeout);
        Assert.Equal(timeout, replayConfig.Api.HttpClientRecoveryTimeout);
        Assert.Equal(timeout, customConfig.Api.HttpClientRecoveryTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMin)]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutDefault + 1)]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMax)]
    public void HttpClientFastFailingTimeoutIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetHttpClientFastFailingTimeout(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetHttpClientFastFailingTimeout(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetHttpClientFastFailingTimeout(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetHttpClientFastFailingTimeout(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Api.HttpClientFastFailingTimeout);
        Assert.Equal(timeout, productionConfig.Api.HttpClientFastFailingTimeout);
        Assert.Equal(timeout, replayConfig.Api.HttpClientFastFailingTimeout);
        Assert.Equal(timeout, customConfig.Api.HttpClientFastFailingTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.MaxRecoveryTimeMin)]
    [InlineData(ConfigLimit.MaxRecoveryTimeDefault + 1)]
    [InlineData(ConfigLimit.MaxRecoveryTimeMax)]
    public void MaxRecoveryTimeIsSet(int maxRecoveryTime)
    {
        var timeout = TimeSpan.FromSeconds(maxRecoveryTime);
        var integrationConfig = BuildConfig("i").SetMaxRecoveryTime(maxRecoveryTime).Build();
        var productionConfig = BuildConfig("p").SetMaxRecoveryTime(maxRecoveryTime).Build();
        var replayConfig = BuildConfig("r").SetMaxRecoveryTime(maxRecoveryTime).Build();
        var customConfig = BuildCustomConfig().SetMaxRecoveryTime(maxRecoveryTime).Build();

        Assert.Equal(timeout, integrationConfig.Producer.MaxRecoveryTime);
        Assert.Equal(timeout, productionConfig.Producer.MaxRecoveryTime);
        Assert.Equal(timeout, replayConfig.Producer.MaxRecoveryTime);
        Assert.Equal(timeout, customConfig.Producer.MaxRecoveryTime);
    }

    [Theory]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMin)]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault + 1)]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMax)]
    public void MinIntervalBetweenRecoveryRequestsIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetMinIntervalBetweenRecoveryRequests(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetMinIntervalBetweenRecoveryRequests(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetMinIntervalBetweenRecoveryRequests(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetMinIntervalBetweenRecoveryRequests(timeoutSeconds).Build();
        Assert.Equal(timeout, integrationConfig.Producer.MinIntervalBetweenRecoveryRequests);
        Assert.Equal(timeout, productionConfig.Producer.MinIntervalBetweenRecoveryRequests);
        Assert.Equal(timeout, replayConfig.Producer.MinIntervalBetweenRecoveryRequests);
        Assert.Equal(timeout, customConfig.Producer.MinIntervalBetweenRecoveryRequests);
    }

    [Fact]
    public void MaxConnectionsPerServerIsSet()
    {
        const int maxConnectionsPerServer = 123;
        var integrationConfig = BuildConfig("i").SetMaxConnectionsPerServer(maxConnectionsPerServer).Build();
        var productionConfig = BuildConfig("p").SetMaxConnectionsPerServer(maxConnectionsPerServer).Build();
        var replayConfig = BuildConfig("r").SetMaxConnectionsPerServer(maxConnectionsPerServer).Build();
        var customConfig = BuildCustomConfig().SetMaxConnectionsPerServer(maxConnectionsPerServer).Build();

        Assert.Equal(maxConnectionsPerServer, integrationConfig.Api.MaxConnectionsPerServer);
        Assert.Equal(maxConnectionsPerServer, productionConfig.Api.MaxConnectionsPerServer);
        Assert.Equal(maxConnectionsPerServer, replayConfig.Api.MaxConnectionsPerServer);
        Assert.Equal(maxConnectionsPerServer, customConfig.Api.MaxConnectionsPerServer);
    }

    [Theory]
    [InlineData(ConfigLimit.InactivitySecondsMin)]
    [InlineData(ConfigLimit.InactivitySecondsDefault + 1)]
    [InlineData(ConfigLimit.InactivitySecondsMax)]
    public void InactivitySecondsIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetInactivitySeconds(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetInactivitySeconds(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetInactivitySeconds(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetInactivitySeconds(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Producer.InactivitySeconds);
        Assert.Equal(timeout, productionConfig.Producer.InactivitySeconds);
        Assert.Equal(timeout, replayConfig.Producer.InactivitySeconds);
        Assert.Equal(timeout, customConfig.Producer.InactivitySeconds);
    }

    [Theory]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMin)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchDefault + 1)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMax)]
    public void InactivitySecondsPrematchIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetInactivitySecondsPrematch(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetInactivitySecondsPrematch(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetInactivitySecondsPrematch(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetInactivitySecondsPrematch(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Producer.InactivitySecondsPrematch);
        Assert.Equal(timeout, productionConfig.Producer.InactivitySecondsPrematch);
        Assert.Equal(timeout, replayConfig.Producer.InactivitySecondsPrematch);
        Assert.Equal(timeout, customConfig.Producer.InactivitySecondsPrematch);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AdjustAfterAgeIsSet(bool adjustAfterAge)
    {
        var integrationConfig = BuildConfig("i").SetAdjustAfterAge(adjustAfterAge).Build();
        var productionConfig = BuildConfig("p").SetAdjustAfterAge(adjustAfterAge).Build();
        var replayConfig = BuildConfig("r").SetAdjustAfterAge(adjustAfterAge).Build();
        var customConfig = BuildCustomConfig().SetAdjustAfterAge(adjustAfterAge).Build();

        Assert.Equal(adjustAfterAge, integrationConfig.Producer.AdjustAfterAge);
        Assert.Equal(adjustAfterAge, productionConfig.Producer.AdjustAfterAge);
        Assert.Equal(adjustAfterAge, replayConfig.Producer.AdjustAfterAge);
        Assert.Equal(adjustAfterAge, customConfig.Producer.AdjustAfterAge);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IgnoreBetPalTimelineSportEventStatusIsSet(bool ignoreBetPalTimelineSportEventStatus)
    {
        var integrationConfig = BuildConfig("i").SetIgnoreBetPalTimelineSportEventStatus(ignoreBetPalTimelineSportEventStatus).Build();
        var productionConfig = BuildConfig("p").SetIgnoreBetPalTimelineSportEventStatus(ignoreBetPalTimelineSportEventStatus).Build();
        var replayConfig = BuildConfig("r").SetIgnoreBetPalTimelineSportEventStatus(ignoreBetPalTimelineSportEventStatus).Build();
        var customConfig = BuildCustomConfig().SetIgnoreBetPalTimelineSportEventStatus(ignoreBetPalTimelineSportEventStatus).Build();

        Assert.Equal(ignoreBetPalTimelineSportEventStatus, integrationConfig.Cache.IgnoreBetPalTimelineSportEventStatus);
        Assert.Equal(ignoreBetPalTimelineSportEventStatus, productionConfig.Cache.IgnoreBetPalTimelineSportEventStatus);
        Assert.Equal(ignoreBetPalTimelineSportEventStatus, replayConfig.Cache.IgnoreBetPalTimelineSportEventStatus);
        Assert.Equal(ignoreBetPalTimelineSportEventStatus, customConfig.Cache.IgnoreBetPalTimelineSportEventStatus);
    }

    [Theory]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMin)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutDefault + 1)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMax)]
    public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutIsSet(int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);
        var integrationConfig = BuildConfig("i").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours).Build();
        var productionConfig = BuildConfig("p").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours).Build();
        var replayConfig = BuildConfig("r").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours).Build();
        var customConfig = BuildCustomConfig().SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours).Build();

        Assert.Equal(timeout, integrationConfig.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout);
        Assert.Equal(timeout, productionConfig.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout);
        Assert.Equal(timeout, replayConfig.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout);
        Assert.Equal(timeout, customConfig.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMin)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutDefault + 1)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMax)]
    public void ProfileCacheTimeoutIsSet(int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);
        var integrationConfig = BuildConfig("i").SetProfileCacheTimeout(timeoutHours).Build();
        var productionConfig = BuildConfig("p").SetProfileCacheTimeout(timeoutHours).Build();
        var replayConfig = BuildConfig("r").SetProfileCacheTimeout(timeoutHours).Build();
        var customConfig = BuildCustomConfig().SetProfileCacheTimeout(timeoutHours).Build();

        Assert.Equal(timeout, integrationConfig.Cache.ProfileCacheTimeout);
        Assert.Equal(timeout, productionConfig.Cache.ProfileCacheTimeout);
        Assert.Equal(timeout, replayConfig.Cache.ProfileCacheTimeout);
        Assert.Equal(timeout, customConfig.Cache.ProfileCacheTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMin)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutDefault + 1)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMax)]
    public void VariantMarketDescriptionCacheTimeoutIsSet(int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);
        var integrationConfig = BuildConfig("i").SetVariantMarketDescriptionCacheTimeout(timeoutHours).Build();
        var productionConfig = BuildConfig("p").SetVariantMarketDescriptionCacheTimeout(timeoutHours).Build();
        var replayConfig = BuildConfig("r").SetVariantMarketDescriptionCacheTimeout(timeoutHours).Build();
        var customConfig = BuildCustomConfig().SetVariantMarketDescriptionCacheTimeout(timeoutHours).Build();

        Assert.Equal(timeout, integrationConfig.Cache.VariantMarketDescriptionCacheTimeout);
        Assert.Equal(timeout, productionConfig.Cache.VariantMarketDescriptionCacheTimeout);
        Assert.Equal(timeout, replayConfig.Cache.VariantMarketDescriptionCacheTimeout);
        Assert.Equal(timeout, customConfig.Cache.VariantMarketDescriptionCacheTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMin)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutDefault + 1)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMax)]
    public void SportEventCacheTimeoutIsSet(int timeoutHours)
    {
        var timeout = TimeSpan.FromHours(timeoutHours);
        var integrationConfig = BuildConfig("i").SetSportEventCacheTimeout(timeoutHours).Build();
        var productionConfig = BuildConfig("p").SetSportEventCacheTimeout(timeoutHours).Build();
        var replayConfig = BuildConfig("r").SetSportEventCacheTimeout(timeoutHours).Build();
        var customConfig = BuildCustomConfig().SetSportEventCacheTimeout(timeoutHours).Build();
        Assert.Equal(timeout, integrationConfig.Cache.SportEventCacheTimeout);
        Assert.Equal(timeout, productionConfig.Cache.SportEventCacheTimeout);
        Assert.Equal(timeout, replayConfig.Cache.SportEventCacheTimeout);
        Assert.Equal(timeout, customConfig.Cache.SportEventCacheTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMin)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault + 1)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMax)]
    public void SportEventStatusCacheTimeoutIsSet(int timeoutMinutes)
    {
        var timeout = TimeSpan.FromMinutes(timeoutMinutes);
        var integrationConfig = BuildConfig("i").SetSportEventStatusCacheTimeout(timeoutMinutes).Build();
        var productionConfig = BuildConfig("p").SetSportEventStatusCacheTimeout(timeoutMinutes).Build();
        var replayConfig = BuildConfig("r").SetSportEventStatusCacheTimeout(timeoutMinutes).Build();
        var customConfig = BuildCustomConfig().SetSportEventStatusCacheTimeout(timeoutMinutes).Build();

        Assert.Equal(timeout, integrationConfig.Cache.SportEventStatusCacheTimeout);
        Assert.Equal(timeout, productionConfig.Cache.SportEventStatusCacheTimeout);
        Assert.Equal(timeout, replayConfig.Cache.SportEventStatusCacheTimeout);
        Assert.Equal(timeout, customConfig.Cache.SportEventStatusCacheTimeout);
    }

    [Theory]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMin)]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMax)]
    public void RabbitConnectionTimeoutIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetRabbitConnectionTimeout(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetRabbitConnectionTimeout(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetRabbitConnectionTimeout(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetRabbitConnectionTimeout(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Rabbit.ConnectionTimeout);
        Assert.Equal(timeout, productionConfig.Rabbit.ConnectionTimeout);
        Assert.Equal(timeout, replayConfig.Rabbit.ConnectionTimeout);
        Assert.Equal(timeout, customConfig.Rabbit.ConnectionTimeout);
    }

    [Fact]
    public void RabbitConnectionTimeoutIsSetWithValidValue()
    {
        RabbitConnectionTimeoutIsSet(ConfigLimit.RabbitConnectionTimeoutDefault + 1);
    }

    [Theory]
    [InlineData(ConfigLimit.RabbitHeartbeatMin)]
    [InlineData(ConfigLimit.RabbitHeartbeatMax)]
    public void RabbitHeartbeatIsSet(int timeoutSeconds)
    {
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);
        var integrationConfig = BuildConfig("i").SetRabbitHeartbeat(timeoutSeconds).Build();
        var productionConfig = BuildConfig("p").SetRabbitHeartbeat(timeoutSeconds).Build();
        var replayConfig = BuildConfig("r").SetRabbitHeartbeat(timeoutSeconds).Build();
        var customConfig = BuildCustomConfig().SetRabbitHeartbeat(timeoutSeconds).Build();

        Assert.Equal(timeout, integrationConfig.Rabbit.Heartbeat);
        Assert.Equal(timeout, productionConfig.Rabbit.Heartbeat);
        Assert.Equal(timeout, replayConfig.Rabbit.Heartbeat);
        Assert.Equal(timeout, customConfig.Rabbit.Heartbeat);
    }

    [Fact]
    public void RabbitHeartbeatIsSetWithValidValue()
    {
        RabbitHeartbeatIsSet(ConfigLimit.RabbitHeartbeatDefault + 1);
    }

    [Theory]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMin)]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesDefault + 1)]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMax)]
    public void StatisticsIntervalIsSet(int timeoutMinutes)
    {
        var timeout = TimeSpan.FromMinutes(timeoutMinutes);
        var integrationConfig = BuildConfig("i").SetStatisticsInterval(timeoutMinutes).Build();
        var productionConfig = BuildConfig("p").SetStatisticsInterval(timeoutMinutes).Build();
        var replayConfig = BuildConfig("r").SetStatisticsInterval(timeoutMinutes).Build();
        var customConfig = BuildCustomConfig().SetStatisticsInterval(timeoutMinutes).Build();

        Assert.Equal(timeout, integrationConfig.Additional.StatisticsInterval);
        Assert.Equal(timeout, productionConfig.Additional.StatisticsInterval);
        Assert.Equal(timeout, replayConfig.Additional.StatisticsInterval);
        Assert.Equal(timeout, customConfig.Additional.StatisticsInterval);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OmitMarketMappingsIsSet(bool omitMarketMappings)
    {
        var integrationConfig = BuildConfig("i").OmitMarketMappings(omitMarketMappings).Build();
        var productionConfig = BuildConfig("p").OmitMarketMappings(omitMarketMappings).Build();
        var replayConfig = BuildConfig("r").OmitMarketMappings(omitMarketMappings).Build();
        var customConfig = BuildCustomConfig().OmitMarketMappings(omitMarketMappings).Build();

        Assert.Equal(omitMarketMappings, integrationConfig.Additional.OmitMarketMappings);
        Assert.Equal(omitMarketMappings, productionConfig.Additional.OmitMarketMappings);
        Assert.Equal(omitMarketMappings, replayConfig.Additional.OmitMarketMappings);
        Assert.Equal(omitMarketMappings, customConfig.Additional.OmitMarketMappings);
    }
}
