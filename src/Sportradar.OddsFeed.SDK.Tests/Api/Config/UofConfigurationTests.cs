// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using BookmakerDetailsProvider = Sportradar.OddsFeed.SDK.Entities.Rest.Internal.BookmakerDetailsProvider;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

// Ignore Spelling: Uof
public class UofConfigurationTests
{
    private readonly BookmakerDetailsProvider _defaultBookmakerDetailsProvider;

    public UofConfigurationTests()
    {
        var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
                                                                              new TestDataFetcher(),
                                                                              new Deserializer<bookmaker_details>(),
                                                                              new BookmakerDetailsMapperFactory());
        bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestBookmakerDetailsProvider.GetBookmakerDetails());
        _defaultBookmakerDetailsProvider = bookmakerDetailsProviderMock.Object;
    }

    [Fact]
    public void ConfigurationConstructorWithNullSectionProviderThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new UofConfiguration(null));
    }

    [Fact]
    public void ConfigurationWithSectionWithoutAccessTokenThrows()
    {
        var section = (TestSection)TestSection.GetDefaultSection();
        section.AccessToken = null;
        var config = new UofConfiguration(new TestSectionProvider(section));

        Assert.Throws<InvalidOperationException>(() => config.UpdateFromAppConfigSection(true));
    }

    [Fact]
    public void ConfigurationWithSectionWithoutLanguagesThrows()
    {
        var section = (TestSection)TestSection.GetDefaultSection();
        section.DefaultLanguage = null;
        section.Languages = null;
        var config = new UofConfiguration(new TestSectionProvider(section));

        Assert.Throws<InvalidOperationException>(() => config.UpdateFromAppConfigSection(true));
    }

    [Fact]
    public void ConfigurationWithCustomSectionLoadsAllValues()
    {
        var section = (TestSection)TestSection.GetCustomSection();
        var config = new UofConfiguration(new TestSectionProvider(section));
        config.UpdateFromAppConfigSection(true);

        Assert.Equal(section.Environment, config.Environment);
        Assert.Equal(section.AccessToken, config.AccessToken);
        Assert.Equal(section.DefaultLanguage, config.DefaultLanguage.TwoLetterISOLanguageName);
        Assert.NotNull(config.Languages);
        Assert.NotEmpty(config.Languages);
        Assert.Equal(section.NodeId, config.NodeId);
        Assert.Equal(section.ExceptionHandlingStrategy, config.ExceptionHandlingStrategy);
        Assert.Equal(section.ApiHost, config.Api.Host);
        Assert.Equal(section.ApiUseSsl, config.Api.UseSsl);
        Assert.Equal(section.RabbitHost, config.Rabbit.Host);
        Assert.Equal(section.RabbitPort, config.Rabbit.Port);
        Assert.Equal(section.RabbitUseSsl, config.Rabbit.UseSsl);
        Assert.Equal(section.RabbitUsername, config.Rabbit.Username);
        Assert.Equal(section.RabbitPassword, config.Rabbit.Password);
        Assert.Equal(section.RabbitVirtualHost, config.Rabbit.VirtualHost);
        Assert.NotNull(config.Producer.DisabledProducers);
        Assert.NotEmpty(config.Producer.DisabledProducers);
    }

    [Fact]
    public void ConstructWithCustomHttpApiHostIsCorrected()
    {
        const string pureHost = "custom.apihost.com";
        var section = (TestSection)TestSection.GetCustomSection();
        section.ApiHost = "HTTP://" + pureHost;
        var config = new UofConfiguration(new TestSectionProvider(section));
        config.UpdateFromAppConfigSection(true);

        Assert.NotEqual(section.ApiHost, config.Api.Host);
        Assert.Equal(pureHost, config.Api.Host);
    }

    [Fact]
    public void ConstructWithCustomHttpsApiHostIsCorrected()
    {
        const string pureHost = "custom.apihost.com";
        var section = (TestSection)TestSection.GetCustomSection();
        section.ApiHost = "HTTPS://" + pureHost;
        var config = new UofConfiguration(new TestSectionProvider(section));
        config.UpdateFromAppConfigSection(true);

        Assert.NotEqual(section.ApiHost, config.Api.Host);
        Assert.Equal(pureHost, config.Api.Host);
    }

    [Fact]
    public void ConfigCanLoadMoreThenOnce()
    {
        var config = (UofConfiguration)new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                                      .SetAccessToken(TestData.AccessToken)
                                      .SelectEnvironment(SdkEnvironment.Integration)
                                      .SetDefaultLanguage(ScheduleData.CultureEn)
                                      .Build();

        config.UpdateFromAppConfigSection(true);
        config.UpdateFromAppConfigSection(true);
        Assert.NotNull(config);
    }

    [Fact]
    // ReSharper disable once MethodTooLong
    public void ValuesForwardedFromSectionHaveCorrectValues()
    {
        var customSection = TestSection.GetCustomSection();
        var config = new TokenSetter(new TestSectionProvider(customSection), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .Build();

        Assert.Equal(customSection.AccessToken, config.AccessToken);
        Assert.Equal(ConfigLimit.InactivitySecondsDefault, config.Producer.InactivitySeconds.TotalSeconds);
        Assert.Equal(customSection.DefaultLanguage, config.DefaultLanguage.TwoLetterISOLanguageName);
        Assert.Equal(customSection.Languages.Split(',').Length, config.Languages.Count);
        Assert.Equal(customSection.DisabledProducers.Split(',').Length, config.Producer.DisabledProducers.Count);
        Assert.True(customSection.DisabledProducers.Split(',').All(a => config.Producer.DisabledProducers.Contains(int.Parse(a))));
        Assert.Equal(config.NodeId, config.NodeId);
        Assert.Equal(SdkEnvironment.Custom, config.Environment);
        Assert.Equal(config.ExceptionHandlingStrategy, config.ExceptionHandlingStrategy);
        Assert.Equal(customSection.RabbitHost, config.Rabbit.Host);
        Assert.Equal(customSection.RabbitVirtualHost, config.Rabbit.VirtualHost);
        Assert.Equal(customSection.RabbitUsername, config.Rabbit.Username);
        Assert.Equal(customSection.RabbitPassword, config.Rabbit.Password);
        Assert.Equal(customSection.RabbitPort, config.Rabbit.Port);
        Assert.Equal(customSection.RabbitUseSsl, config.Rabbit.UseSsl);
        Assert.Equal(customSection.ApiHost, config.Api.Host);
        Assert.Equal(customSection.ApiUseSsl, config.Api.UseSsl);
        Assert.Equal(customSection.ApiUseSsl ? "https://" + customSection.ApiHost : "http://" + customSection.ApiHost, config.Api.BaseUrl);
        Assert.Equal(customSection.ApiHost + "/v1/replay", config.Api.ReplayHost);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    public void SettingEnvironmentHasCorrectValue(SdkEnvironment environment)
    {
        var config = (UofConfiguration)new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                                      .SetAccessToken(TestData.AccessToken)
                                      .SelectEnvironment(environment)
                                      .SetDefaultLanguage(TestData.Culture)
                                      .Build();

        config.UpdateBookmakerDetails(new BookmakerDetails(_defaultBookmakerDetailsProvider.GetData("en")), EnvironmentManager.GetApiHost(config.Environment));

        ValidateDefaultConfig(config, environment);
        ValidateDefaultProducerConfig(config);
        ValidateDefaultCacheConfig(config);
        ValidateApiConfigForEnvironment(config, environment);
        ValidateRabbitConfigForEnvironment(config, environment);
    }

    [Fact]
    public void SettingReplayEnvironmentHasCorrectValue()
    {
        const SdkEnvironment environment = SdkEnvironment.Replay;
        var config = (UofConfiguration)new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                                      .SetAccessToken(TestData.AccessToken)
                                      .SelectEnvironment(environment)
                                      .SetDefaultLanguage(TestData.Culture)
                                      .Build();

        config.UpdateBookmakerDetails(new BookmakerDetails(_defaultBookmakerDetailsProvider.GetData("en")), EnvironmentManager.GetApiHost(config.Environment));

        ValidateDefaultConfig(config, environment);
        ValidateDefaultProducerConfig(config);
        ValidateDefaultCacheConfig(config);
        ValidateApiConfigForEnvironment(config, environment);
        ValidateRabbitConfigForEnvironment(config, environment);
    }

    [Fact]
    public void SelectingReplayEnvironmentHasCorrectValue()
    {
        const SdkEnvironment environment = SdkEnvironment.Replay;
        var config = (UofConfiguration)new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                                      .SetAccessToken(TestData.AccessToken)
                                      .SelectReplay()
                                      .SetDefaultLanguage(TestData.Culture)
                                      .Build();

        config.UpdateBookmakerDetails(new BookmakerDetails(_defaultBookmakerDetailsProvider.GetData("en")), EnvironmentManager.GetApiHost(config.Environment));

        ValidateDefaultConfig(config, environment);
        ValidateDefaultProducerConfig(config);
        ValidateDefaultCacheConfig(config);
        ValidateApiConfigForEnvironment(config, environment);
        ValidateRabbitConfigForEnvironment(config, environment);
    }

    [Fact]
    // ReSharper disable once MethodTooLong
    public void SettingCustomEnvironmentWithCustomValuesHasCorrectValue()
    {
        const SdkEnvironment environment = SdkEnvironment.Custom;
        var config = (UofConfiguration)new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                                      .SetAccessToken(TestData.AccessToken)
                                      .SelectCustom()
                                      .SetDefaultLanguage(TestData.Culture)
                                      .UseApiSsl(false)
                                      .SetApiHost("custom_api_host")
                                      .SetVirtualHost("custom_virtual_host")
                                      .UseMessagingSsl(false)
                                      .SetMessagingHost("custom_mq_host")
                                      .SetMessagingUsername("custom_username")
                                      .SetMessagingPassword("custom_password")
                                      .SetMessagingPort(222)
                                      .Build();

        config.UpdateBookmakerDetails(new BookmakerDetails(_defaultBookmakerDetailsProvider.GetData("en")), EnvironmentManager.GetApiHost(config.Environment));

        ValidateDefaultConfig(config, environment);
        ValidateDefaultProducerConfig(config);
        ValidateDefaultCacheConfig(config);

        Assert.Equal(ConfigLimit.HttpClientTimeoutDefault, config.Api.HttpClientTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientRecoveryTimeoutDefault, config.Api.HttpClientRecoveryTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientFastFailingTimeoutDefault, config.Api.HttpClientFastFailingTimeout.TotalSeconds);
        Assert.Equal(int.MaxValue, config.Api.MaxConnectionsPerServer);
        Assert.False(config.Api.UseSsl);
        Assert.Equal("custom_api_host", config.Api.Host);
        Assert.False(config.Api.BaseUrl.IsNullOrEmpty());
        Assert.False(config.Api.ReplayHost.IsNullOrEmpty());
        Assert.False(config.Api.ReplayBaseUrl.IsNullOrEmpty());

        Assert.True(config.Rabbit.ConnectionTimeout > TimeSpan.Zero);
        Assert.True(config.Rabbit.Heartbeat > TimeSpan.Zero);
        Assert.False(config.Rabbit.UseSsl);
        Assert.NotNull(config.BookmakerDetails);
        Assert.NotNull(config.BookmakerDetails.VirtualHost);
        Assert.Equal("custom_virtual_host", config.Rabbit.VirtualHost);
        Assert.Equal("custom_mq_host", config.Rabbit.Host);
        Assert.Equal("custom_username", config.Rabbit.Username);
        Assert.Equal("custom_password", config.Rabbit.Password);
        Assert.Equal(222, config.Rabbit.Port);
    }

    [Fact]
    public void AdditionalConfigurationToStringHasAllTheValues()
    {
        var summary = new UofAdditionalConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("AdditionalConfiguration{", summary);
        Assert.Contains("OmitMarketMappings=", summary);
        Assert.Contains("StatisticsInterval=", summary);
    }

    [Fact]
    public void ApiConfigurationToStringHasAllTheValues()
    {
        var summary = new UofApiConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("ApiConfiguration{", summary);
        Assert.Contains("Host=", summary);
        Assert.Contains("UseSsl=", summary);
        Assert.Contains("HttpClientTimeout=", summary);
        Assert.Contains("HttpClientRecoveryTimeout=", summary);
        Assert.Contains("HttpClientFastFailingTimeout=", summary);
        Assert.Contains("MaxConnectionsPerServer=", summary);
    }

    [Fact]
    public void CacheConfigurationToStringHasAllTheValues()
    {
        var summary = new UofCacheConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("CacheConfiguration{", summary);
        Assert.Contains("SportEventCacheTimeout=", summary);
        Assert.Contains("SportEventStatusCacheTimeout=", summary);
        Assert.Contains("ProfileCacheTimeout=", summary);
        Assert.Contains("VariantMarketDescriptionCacheTimeout=", summary);
        Assert.Contains("IgnoreBetPalTimelineSportEventStatusCacheTimeout=", summary);
        Assert.Contains("IgnoreBetPalTimelineSportEventStatus=", summary);
    }

    [Fact]
    public void ProducerConfigurationToStringHasAllTheValues()
    {
        var summary = new UofProducerConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("ProducerConfiguration{", summary);
        Assert.Contains("InactivitySeconds=", summary);
        Assert.Contains("InactivitySecondsPrematch=", summary);
        Assert.Contains("MaxRecoveryTime=", summary);
        Assert.Contains("MinIntervalBetweenRecoveryRequests=", summary);
        Assert.Contains("DisabledProducers=", summary);
    }

    [Fact]
    public void RabbitConfigurationToStringHasAllTheValues()
    {
        var summary = new UofRabbitConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("RabbitConfiguration{", summary);
        Assert.Contains("Host=", summary);
        Assert.Contains("Port=", summary);
        Assert.Contains("UseSsl=", summary);
        Assert.Contains("VirtualHost=", summary);
        Assert.Contains("Username=", summary);
        Assert.Contains("Password=", summary);
        Assert.Contains("ConnectionTimeout=", summary);
        Assert.Contains("Heartbeat=", summary);
    }

    [Fact]
    public void UsageConfigurationToStringHasAllTheValues()
    {
        var summary = new UofUsageConfiguration().ToString();

        Assert.NotNull(summary);
        Assert.Contains("UsageConfiguration{", summary);
        Assert.Contains("IsExportEnabled=", summary);
        Assert.Contains("ExportIntervalInSec=", summary);
        Assert.Contains("ExportTimeoutInSec=", summary);
        Assert.Contains("Host=", summary);
    }

    [Fact]
    public void UofConfigurationToStringHasAllTheValues()
    {
        var summary = new UofConfiguration(new TestSectionProvider(null)).ToString();

        CheckConfigurationSummaryHasAllValue(summary);
    }

    [Fact]
    public void UofConfigurationWithFullValuesToStringHasAllTheValues()
    {
        var section = TestSection.GetDefaultSection();
        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectEnvironmentFromConfigFile()
                    .LoadFromConfigFile()
                    .Build();
        var summary = config.ToString();

        CheckConfigurationSummaryHasAllValue(summary);
    }

    private static void CheckConfigurationSummaryHasAllValue(string summary)
    {
        Assert.NotNull(summary);
        Assert.Contains("UofConfiguration{", summary);
        Assert.Contains("ApiConfiguration{", summary);
        Assert.Contains("CacheConfiguration{", summary);
        Assert.Contains("ProducerConfiguration{", summary);
        Assert.Contains("RabbitConfiguration{", summary);
        Assert.Contains("AdditionalConfiguration{", summary);
        Assert.Contains("UsageConfiguration{", summary);
        Assert.Contains("BookmakerId=", summary);
        Assert.Contains("AccessToken=", summary);
        Assert.Contains("NodeId=", summary);
        Assert.Contains("DefaultLanguage=", summary);
        Assert.Contains("Languages=", summary);
        Assert.Contains("Environment=", summary);
        Assert.Contains("ExceptionHandlingStrategy=", summary);
    }

    private static void ValidateDefaultConfig(IUofConfiguration config, SdkEnvironment environment)
    {
        Assert.Equal(TestData.AccessToken, config.AccessToken);
        Assert.Equal(environment, config.Environment);
        Assert.Single(config.Languages);
        Assert.Contains(TestData.Culture, config.Languages);
        Assert.Equal(TestData.Culture, config.DefaultLanguage);
        Assert.Equal(ExceptionHandlingStrategy.Throw, config.ExceptionHandlingStrategy);
        Assert.Equal(0, config.NodeId);
    }

    private static void ValidateDefaultProducerConfig(IUofConfiguration config)
    {
        Assert.Empty(config.Producer.DisabledProducers);
        Assert.Equal(ConfigLimit.InactivitySecondsDefault, config.Producer.InactivitySeconds.TotalSeconds);
        Assert.Equal(ConfigLimit.InactivitySecondsPrematchDefault, config.Producer.InactivitySecondsPrematch.TotalSeconds);
        Assert.Equal(ConfigLimit.MaxRecoveryTimeDefault, config.Producer.MaxRecoveryTime.TotalSeconds);
        Assert.Equal(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault, config.Producer.MinIntervalBetweenRecoveryRequests.TotalSeconds);
    }

    private static void ValidateDefaultCacheConfig(IUofConfiguration config)
    {
        Assert.Equal(5, config.Cache.SportEventStatusCacheTimeout.TotalMinutes);
        Assert.Equal(24, config.Cache.ProfileCacheTimeout.TotalHours);
        Assert.Equal(3, config.Cache.VariantMarketDescriptionCacheTimeout.TotalHours);
        Assert.Equal(3, config.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        Assert.False(config.Cache.IgnoreBetPalTimelineSportEventStatus);
    }

    private static void ValidateApiConfigForEnvironment(IUofConfiguration config, SdkEnvironment environment)
    {
        Assert.Equal(ConfigLimit.HttpClientTimeoutDefault, config.Api.HttpClientTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientRecoveryTimeoutDefault, config.Api.HttpClientRecoveryTimeout.TotalSeconds);
        Assert.Equal(ConfigLimit.HttpClientFastFailingTimeoutDefault, config.Api.HttpClientFastFailingTimeout.TotalSeconds);
        Assert.Equal(int.MaxValue, config.Api.MaxConnectionsPerServer);
        Assert.True(config.Api.UseSsl);
        Assert.Equal(EnvironmentManager.GetApiHost(environment), config.Api.Host);
        Assert.False(config.Api.BaseUrl.IsNullOrEmpty());
        Assert.False(config.Api.ReplayHost.IsNullOrEmpty());
        Assert.False(config.Api.ReplayBaseUrl.IsNullOrEmpty());
    }

    private static void ValidateRabbitConfigForEnvironment(IUofConfiguration config, SdkEnvironment environment)
    {
        Assert.True(config.Rabbit.UseSsl);
        Assert.Equal(EnvironmentManager.GetMqHost(environment), config.Rabbit.Host);
        Assert.True(config.Rabbit.ConnectionTimeout > TimeSpan.Zero);
        Assert.True(config.Rabbit.Heartbeat > TimeSpan.Zero);
        Assert.NotNull(config.BookmakerDetails);
        Assert.NotNull(config.BookmakerDetails.VirtualHost);
        Assert.Equal(config.BookmakerDetails.VirtualHost, config.Rabbit.VirtualHost);
    }
}
