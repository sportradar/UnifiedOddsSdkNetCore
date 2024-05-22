// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

//[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]

[SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to", Justification = "If min value changes, 0 will still not be allowed")]
public class ConfigurationBuilderLimitsTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(ConfigLimit.HttpClientTimeoutMin - 1)]
    [InlineData(ConfigLimit.HttpClientTimeoutMax + 1)]
    public void HttpClientTimeoutIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetHttpClientTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetHttpClientTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetHttpClientTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetHttpClientTimeout(timeoutSeconds));
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMin - 1)]
    [InlineData(ConfigLimit.HttpClientRecoveryTimeoutMax + 1)]
    public void HttpClientRecoveryTimeoutIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetHttpClientRecoveryTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetHttpClientRecoveryTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetHttpClientRecoveryTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetHttpClientRecoveryTimeout(timeoutSeconds));
    }

    [Theory]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMin - 1)]
    [InlineData(ConfigLimit.HttpClientFastFailingTimeoutMax + 1)]
    public void HttpClientFastFailingTimeoutIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetHttpClientFastFailingTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetHttpClientFastFailingTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetHttpClientFastFailingTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetHttpClientFastFailingTimeout(timeoutSeconds));
    }

    [Theory]
    [InlineData(ConfigLimit.MaxRecoveryTimeMin - 1)]
    [InlineData(ConfigLimit.MaxRecoveryTimeMax + 1)]
    public void MaxRecoveryTimeIsSet(int maxRecoveryTime)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetMaxRecoveryTime(maxRecoveryTime));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetMaxRecoveryTime(maxRecoveryTime));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetMaxRecoveryTime(maxRecoveryTime));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetMaxRecoveryTime(maxRecoveryTime));
    }

    [Theory]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMin - 1)]
    [InlineData(ConfigLimit.MinIntervalBetweenRecoveryRequestMax + 1)]
    public void MinIntervalBetweenRecoveryRequestsIsSet(int minIntervalBetweenRequests)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetMinIntervalBetweenRecoveryRequests(minIntervalBetweenRequests));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetMinIntervalBetweenRecoveryRequests(minIntervalBetweenRequests));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetMinIntervalBetweenRecoveryRequests(minIntervalBetweenRequests));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetMinIntervalBetweenRecoveryRequests(minIntervalBetweenRequests));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MaxConnectionsPerServerIsSet(int maxConnectionsPerServer)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetMaxConnectionsPerServer(maxConnectionsPerServer));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetMaxConnectionsPerServer(maxConnectionsPerServer));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetMaxConnectionsPerServer(maxConnectionsPerServer));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetMaxConnectionsPerServer(maxConnectionsPerServer));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(ConfigLimit.InactivitySecondsMin - 1)]
    [InlineData(ConfigLimit.InactivitySecondsMax + 1)]
    public void InactivitySecondsIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetInactivitySeconds(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetInactivitySeconds(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetInactivitySeconds(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetInactivitySeconds(timeoutSeconds));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMin - 1)]
    [InlineData(ConfigLimit.InactivitySecondsPrematchMax + 1)]
    public void InactivitySecondsPrematchIIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetInactivitySecondsPrematch(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetInactivitySecondsPrematch(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetInactivitySecondsPrematch(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetInactivitySecondsPrematch(timeoutSeconds));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMin - 1)]
    [InlineData(ConfigLimit.IgnoreBetpalTimelineTimeoutMax + 1)]
    public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutIsSet(int timeoutHours)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(timeoutHours));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMin - 1)]
    [InlineData(ConfigLimit.ProfileCacheTimeoutMax + 1)]
    public void ProfileCacheTimeoutIsSet(int timeoutHours)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetProfileCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetProfileCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetProfileCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetProfileCacheTimeout(timeoutHours));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMin - 1)]
    [InlineData(ConfigLimit.SportEventCacheTimeoutMax + 1)]
    public void SportEventCacheTimeoutIsSet(int timeoutHours)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetSportEventCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetSportEventCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetSportEventCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetSportEventCacheTimeout(timeoutHours));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMin - 1)]
    [InlineData(ConfigLimit.SingleVariantMarketTimeoutMax + 1)]
    public void VariantMarketDescriptionCacheTimeoutIsSet(int timeoutHours)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetVariantMarketDescriptionCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetVariantMarketDescriptionCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetVariantMarketDescriptionCacheTimeout(timeoutHours));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetVariantMarketDescriptionCacheTimeout(timeoutHours));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMin - 1)]
    [InlineData(ConfigLimit.SportEventStatusCacheTimeoutMinutesMax + 1)]
    public void SportEventStatusCacheTimeoutIsSet(int timeoutMinutes)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetSportEventStatusCacheTimeout(timeoutMinutes));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetSportEventStatusCacheTimeout(timeoutMinutes));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetSportEventStatusCacheTimeout(timeoutMinutes));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetSportEventStatusCacheTimeout(timeoutMinutes));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMin - 1)]
    [InlineData(ConfigLimit.RabbitConnectionTimeoutMax + 1)]
    public void RabbitConnectionTimeoutIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetRabbitConnectionTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetRabbitConnectionTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetRabbitConnectionTimeout(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetRabbitConnectionTimeout(timeoutSeconds));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(ConfigLimit.RabbitHeartbeatMin - 1)]
    [InlineData(ConfigLimit.RabbitHeartbeatMax + 1)]
    public void RabbitHeartbeatIsSet(int heartbeatSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetRabbitHeartbeat(heartbeatSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetRabbitHeartbeat(heartbeatSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetRabbitHeartbeat(heartbeatSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetRabbitHeartbeat(heartbeatSeconds));
    }

    [Theory]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMin - 1)]
    [InlineData(ConfigLimit.StatisticsIntervalMinutesMax + 1)]
    public void StatisticsIntervalIsSet(int timeoutSeconds)
    {
        Assert.Throws<ArgumentException>(() => BuildConfig("i").SetStatisticsInterval(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("p").SetStatisticsInterval(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildConfig("r").SetStatisticsInterval(timeoutSeconds));
        Assert.Throws<ArgumentException>(() => BuildCustomConfig().SetStatisticsInterval(timeoutSeconds));
    }
}
