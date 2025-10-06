// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class ConfigurationBuilderWithSectionSetup
{
    protected const string CustomApiHost = "custom_api_host";
    protected const string CustomRabbitHost = "custom_mq_host";
    protected const string TestAccessToken = "myTestToken";
    internal readonly TestSection BaseSection;
    internal readonly TestSection CustomSection;

    protected ConfigurationBuilderWithSectionSetup()
    {
        BaseSection = (TestSection)TestSection.GetDefaultSection();
        CustomSection = (TestSection)TestSection.GetCustomSection();
    }

    internal static List<int> GetIntList(string value)
    {
        return value.Split([","], StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    internal static List<CultureInfo> GetCultureList(string cultureNames)
    {
        return cultureNames.Split([","], StringSplitOptions.RemoveEmptyEntries).Select(name => new CultureInfo(name)).ToList();
    }

    internal static ITokenSetter GetTokenSetter()
    {
        return new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider());
    }

    internal static ITokenSetter GetTokenSetter(IUofConfigurationSection section)
    {
        return new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider());
    }

    internal static IEnvironmentSelector GetEnvironmentSelector()
    {
        return GetTokenSetter().SetAccessToken(TestAccessToken);
    }

    internal static IEnvironmentSelector GetEnvironmentSelector(IUofConfigurationSection section)
    {
        return GetTokenSetter(section).SetAccessToken(TestAccessToken);
    }

    internal static IConfigurationBuilder IntegrationBuilder(IUofConfigurationSection section)
    {
        return new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessTokenFromConfigFile()
              .SelectEnvironment(SdkEnvironment.Integration)
              .LoadFromConfigFile();
    }

    internal static IConfigurationBuilder IntegrationBuilder(string token)
    {
        return new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessToken(token)
              .SelectEnvironment(SdkEnvironment.Integration);
    }

    internal static IConfigurationBuilder ProductionBuilder(IUofConfigurationSection section)
    {
        return new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessTokenFromConfigFile()
              .SelectEnvironment(SdkEnvironment.Production)
              .LoadFromConfigFile();
    }

    internal static IConfigurationBuilder ProductionBuilder(string token)
    {
        return new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessToken(token)
              .SelectEnvironment(SdkEnvironment.Production);
    }

    internal static IConfigurationBuilder ReplayBuilder(IUofConfigurationSection section)
    {
        return new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessTokenFromConfigFile()
              .SelectReplay()
              .LoadFromConfigFile();
    }

    internal static IConfigurationBuilder ReplayBuilder(string token)
    {
        return new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessToken(token)
              .SelectReplay();
    }

    internal static ICustomConfigurationBuilder CustomBuilder(IUofConfigurationSection section)
    {
        return new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessTokenFromConfigFile()
              .SelectCustom()
              .LoadFromConfigFile();
    }

    internal static ICustomConfigurationBuilder CustomBuilder(string token)
    {
        return new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessToken(token)
              .SelectCustom()
              .SetApiHost(CustomApiHost)
              .SetMessagingHost(CustomRabbitHost);
    }

    protected static IConfigurationBuilder BuildConfig(string builderType)
    {
        return builderType switch
        {
            "p" => ProductionBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureEn),
            "r" => ReplayBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureEn),
            _ => IntegrationBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureEn)
        };
    }

    protected static ICustomConfigurationBuilder BuildCustomConfig()
    {
        return CustomBuilder(TestData.AccessToken).SetDefaultLanguage(ScheduleData.CultureEn);
    }

    protected static void ConfigHasDefaultValuesSet(IUofConfiguration config)
    {
        ValidateDefaultProducerConfig(config.Producer);
        ValidateDefaultCacheConfig(config.Cache);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateDefaultAdditionalConfig(config.Additional);
        ValidateDefaultUsageConfig(config);
    }

    internal static void ValidateDefaultConfig(IUofConfiguration config, SdkEnvironment environment)
    {
        Assert.Equal(TestData.AccessToken, config.AccessToken);
        Assert.Equal(environment, config.Environment);
        Assert.Equal(TestData.Culture, config.DefaultLanguage);
        Assert.Single(config.Languages);
        Assert.Contains(TestData.Culture, config.Languages);
        Assert.Equal(ExceptionHandlingStrategy.Catch, config.ExceptionHandlingStrategy);
        Assert.Equal(0, config.NodeId);
    }

    internal static void ValidateDefaultProducerConfig(IUofProducerConfiguration producerConfig)
    {
        Assert.Empty(producerConfig.DisabledProducers);
        Assert.Equal(ConfigLimit.InactivitySecondsDefault, producerConfig.InactivitySeconds.TotalSeconds);
        Assert.Equal(ConfigLimit.InactivitySecondsPrematchDefault, producerConfig.InactivitySecondsPrematch.TotalSeconds);
        Assert.Equal(ConfigLimit.MaxRecoveryTimeDefault, producerConfig.MaxRecoveryTime.TotalSeconds);
        Assert.Equal(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault, producerConfig.MinIntervalBetweenRecoveryRequests.TotalSeconds);
    }

    internal static void ValidateDefaultCacheConfig(IUofCacheConfiguration cacheConfig)
    {
        Assert.Equal(ConfigLimit.SportEventCacheTimeoutDefault, cacheConfig.SportEventCacheTimeout.TotalHours);
        Assert.Equal(ConfigLimit.SportEventStatusCacheTimeoutMinutesDefault, cacheConfig.SportEventStatusCacheTimeout.TotalMinutes);
        Assert.Equal(ConfigLimit.ProfileCacheTimeoutDefault, cacheConfig.ProfileCacheTimeout.TotalHours);
        Assert.Equal(ConfigLimit.SingleVariantMarketTimeoutDefault, cacheConfig.VariantMarketDescriptionCacheTimeout.TotalHours);
        Assert.Equal(ConfigLimit.IgnoreBetpalTimelineTimeoutDefault, cacheConfig.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        Assert.False(cacheConfig.IgnoreBetPalTimelineSportEventStatus);
    }

    internal static void ValidateApiConfigForEnvironment(IUofApiConfiguration apiConfig, SdkEnvironment environment)
    {
        Assert.Equal(ConfigLimit.HttpClientTimeoutDefault, apiConfig.HttpClientTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientRecoveryTimeoutDefault, apiConfig.HttpClientRecoveryTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientFastFailingTimeoutDefault, apiConfig.HttpClientFastFailingTimeout.TotalSeconds);
        Assert.Equal(int.MaxValue, apiConfig.MaxConnectionsPerServer);
        if (environment != SdkEnvironment.Custom)
        {
            Assert.True(apiConfig.UseSsl);
            Assert.Equal(EnvironmentManager.GetApiHost(environment), apiConfig.Host);
        }
        Assert.False(apiConfig.Host.IsNullOrEmpty());
        Assert.False(apiConfig.BaseUrl.IsNullOrEmpty());
        Assert.False(apiConfig.ReplayHost.IsNullOrEmpty());
        Assert.False(apiConfig.ReplayBaseUrl.IsNullOrEmpty());
    }

    internal static void ValidateRabbitConfigForEnvironment(IUofConfiguration config, SdkEnvironment environment)
    {
        if (environment != SdkEnvironment.Custom)
        {
            Assert.True(config.Rabbit.UseSsl);
            Assert.Equal(EnvironmentManager.DefaultMqHostPort, config.Rabbit.Port);
            Assert.Equal(EnvironmentManager.GetMqHost(environment), config.Rabbit.Host);
            Assert.Equal(config.BookmakerDetails.VirtualHost, config.Rabbit.VirtualHost);
        }
        Assert.False(config.Rabbit.Username.IsNullOrEmpty());
        Assert.True(config.Rabbit.Port > 0);
        Assert.False(config.Rabbit.Host.IsNullOrEmpty());
        Assert.True(config.Rabbit.ConnectionTimeout > TimeSpan.Zero);
        Assert.True(config.Rabbit.Heartbeat > TimeSpan.Zero);
        Assert.NotNull(config.BookmakerDetails);
        Assert.NotNull(config.BookmakerDetails.VirtualHost);
    }

    internal static void ValidateDefaultAdditionalConfig(IUofAdditionalConfiguration addConfig)
    {
        Assert.Equal(ConfigLimit.StatisticsIntervalMinutesDefault, addConfig.StatisticsInterval.TotalMinutes);
        Assert.False(addConfig.OmitMarketMappings);
    }

    internal static void ValidateDefaultUsageConfig(IUofConfiguration config)
    {
        config.Usage.ExportTimeoutInSec.Should().Be(20);
        config.Usage.ExportIntervalInSec.Should().Be(300);
        config.Usage.IsExportEnabled.Should().BeTrue();
        config.Usage.Host.Should().NotBeNullOrEmpty();
        config.Usage.Host.Should().BeEquivalentTo(EnvironmentManager.GetUsageHost(config.Environment));
    }
}
