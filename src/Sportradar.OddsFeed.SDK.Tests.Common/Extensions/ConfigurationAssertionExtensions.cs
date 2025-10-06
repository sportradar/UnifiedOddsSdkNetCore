// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

internal static class ConfigurationAssertionExtensions
{
    public static void ShouldHaveDefaultValuesFromConfigLimit(this IUofProducerConfiguration producerConfig)
    {
        producerConfig.ShouldNotBeNull();

        producerConfig.DisabledProducers.ShouldBeEmpty();
        producerConfig.InactivitySeconds.TotalSeconds.ShouldBe(ConfigLimit.InactivitySecondsDefault);
        producerConfig.InactivitySecondsPrematch.TotalSeconds.ShouldBe(ConfigLimit.InactivitySecondsPrematchDefault);
        producerConfig.MaxRecoveryTime.TotalSeconds.ShouldBe(ConfigLimit.MaxRecoveryTimeDefault);
        producerConfig.MinIntervalBetweenRecoveryRequests.TotalSeconds.ShouldBe(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault);
    }

    public static void ShouldHaveDefaultValuesFromConfigLimit(this IUofCacheConfiguration cacheConfig)
    {
        cacheConfig.ShouldNotBeNull();

        cacheConfig.SportEventCacheTimeout.TotalHours.ShouldBe(ConfigLimit.SportEventCacheTimeoutDefault);
        cacheConfig.SportEventStatusCacheTimeout.TotalMinutes.ShouldBe(ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault);
        cacheConfig.ProfileCacheTimeout.TotalHours.ShouldBe(ConfigLimit.ProfileCacheTimeoutDefault);
        cacheConfig.VariantMarketDescriptionCacheTimeout.TotalHours.ShouldBe(ConfigLimit.SingleVariantMarketTimeoutDefault);
        cacheConfig.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours.ShouldBe(ConfigLimit.IgnoreBetpalTimelineTimeoutDefault);
        cacheConfig.IgnoreBetPalTimelineSportEventStatus.ShouldBeFalse();
    }

    public static void ShouldHaveDefaultValuesForEnvironment(this IUofUsageConfiguration usageConfig, SdkEnvironment environment)
    {
        usageConfig.ShouldNotBeNull();

        usageConfig.ExportTimeoutInSec.ShouldBe(20);
        usageConfig.ExportIntervalInSec.ShouldBe(300);
        usageConfig.IsExportEnabled.ShouldBeTrue();

        usageConfig.Host.ShouldNotBeNullOrEmpty();
        usageConfig.Host.ShouldBe(EnvironmentManager.GetUsageHost(environment));
    }

    public static void ShouldHaveDefaultValuesFromConfigLimit(this IUofAdditionalConfiguration additionalConfig)
    {
        additionalConfig.ShouldNotBeNull();

        additionalConfig.StatisticsInterval.TotalMinutes.ShouldBe(ConfigLimit.StatisticsIntervalMinutesDefault);
        additionalConfig.OmitMarketMappings.ShouldBeFalse();
    }

    public static void ShouldMatchApiDefaultsForNonCustomEnvironment(this IUofApiConfiguration apiConfig, SdkEnvironment environment)
    {
        AssertCommonApi(apiConfig);

        environment.ShouldNotBe(SdkEnvironment.Custom);
        apiConfig.UseSsl.ShouldBeTrue();
        apiConfig.Host.ShouldBe(EnvironmentManager.GetApiHost(environment));
    }

    public static void ShouldMatchApiDefaultsForCustomEnvironment(this IUofApiConfiguration apiConfig)
    {
        AssertCommonApi(apiConfig);
    }

    public static void ShouldMatchRabbitDefaultsForNonCustomEnvironment(this IUofRabbitConfiguration rabbitConfig, SdkEnvironment environment)
    {
        AssertCommonRabbit(rabbitConfig);

        environment.ShouldNotBe(SdkEnvironment.Custom);
        rabbitConfig.UseSsl.ShouldBeTrue();
        rabbitConfig.Port.ShouldBe(EnvironmentManager.DefaultMqHostPort);
        rabbitConfig.Host.ShouldBe(EnvironmentManager.GetMqHost(environment));
    }

    public static void ShouldMatchRabbitDefaultsForCustomEnvironment(this IUofRabbitConfiguration rabbitConfig)
    {
        AssertCommonRabbit(rabbitConfig);
    }

    public static void ShouldNotBeEmpty(this IBookmakerDetails bookmakerConfiguration)
    {
        bookmakerConfiguration.ShouldNotBeNull();
        bookmakerConfiguration.VirtualHost.ShouldNotBeNull();
    }

    private static void AssertCommonApi(IUofApiConfiguration apiConfig)
    {
        apiConfig.ShouldNotBeNull();

        apiConfig.HttpClientTimeout.TotalSeconds.ShouldBe(ConfigLimit.HttpClientTimeoutDefault);
        apiConfig.HttpClientRecoveryTimeout.TotalSeconds.ShouldBe(ConfigLimit.HttpClientRecoveryTimeoutDefault);
        apiConfig.HttpClientFastFailingTimeout.TotalSeconds.ShouldBe(ConfigLimit.HttpClientFastFailingTimeoutDefault);
        apiConfig.MaxConnectionsPerServer.ShouldBe(int.MaxValue);

        apiConfig.Host.ShouldNotBeNullOrEmpty();
        apiConfig.BaseUrl.ShouldNotBeNullOrEmpty();
        apiConfig.ReplayHost.ShouldNotBeNullOrEmpty();
        apiConfig.ReplayBaseUrl.ShouldNotBeNullOrEmpty();
    }

    private static void AssertCommonRabbit(IUofRabbitConfiguration rabbitConfig)
    {
        rabbitConfig.ShouldNotBeNull();

        rabbitConfig.Username.ShouldNotBeNullOrEmpty();
        rabbitConfig.Port.ShouldBeGreaterThan(0);
        rabbitConfig.Host.ShouldNotBeNullOrEmpty();
        rabbitConfig.ConnectionTimeout.ShouldBeGreaterThan(TimeSpan.Zero);
        rabbitConfig.Heartbeat.ShouldBeGreaterThan(TimeSpan.Zero);
    }
}
