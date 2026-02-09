// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

// Ignore Spelling: Ssl
// Ignore Spelling: App
[SuppressMessage("ReSharper", "TooManyChainedReferences")]
public class ConfigurationBuilderWithSectionWithSectionTests : ConfigurationBuilderWithSectionSetup
{
    [Fact]
    public void AccessTokenHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithAccessToken("my_token").Build();

        Assert.Equal(baseSection.AccessToken, IntegrationBuilder(baseSection).Build().AccessToken);
        Assert.Equal(baseSection.AccessToken, ProductionBuilder(baseSection).Build().AccessToken);
        Assert.Equal(baseSection.AccessToken, ReplayBuilder(baseSection).Build().AccessToken);
    }

    [Fact]
    public void DefaultLanguageFromSectionHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName).Build();

        Assert.Equal(TestConsts.CultureDe, IntegrationBuilder(baseSection).Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureDe, ProductionBuilder(baseSection).Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureDe, ReplayBuilder(baseSection).Build().DefaultLanguage);

        Assert.Single(IntegrationBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ProductionBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ReplayBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanOverrideManually()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage("de").Build();

        Assert.Equal(TestConsts.CultureEn, IntegrationBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureEn, ProductionBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureEn, ReplayBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).Build().DefaultLanguage);

        Assert.Single(IntegrationBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ProductionBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ReplayBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanOverrideManuallyAndBack()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName).Build();

        Assert.Equal(TestConsts.CultureDe, IntegrationBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureDe, ProductionBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().DefaultLanguage);
        Assert.Equal(TestConsts.CultureDe, ReplayBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().DefaultLanguage);

        Assert.Single(IntegrationBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ProductionBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
        Assert.Single(ReplayBuilder(baseSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build().Languages);
    }

    [Fact]
    public void LanguagesHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage("it").WithLanguages("it,de,en").Build();

        Assert.True(GetCultureList(baseSection.Languages).SequenceEqual(IntegrationBuilder(baseSection).Build().Languages));
        Assert.True(GetCultureList(baseSection.Languages).SequenceEqual(ProductionBuilder(baseSection).Build().Languages));
        Assert.True(GetCultureList(baseSection.Languages).SequenceEqual(ReplayBuilder(baseSection).Build().Languages));

        Assert.True(GetCultureList(baseSection.DefaultLanguage).SequenceEqual(IntegrationBuilder(baseSection).SetDesiredLanguages(null).Build().Languages));
        Assert.True(GetCultureList(baseSection.DefaultLanguage).SequenceEqual(ProductionBuilder(baseSection).SetDesiredLanguages(null).Build().Languages));
        Assert.True(GetCultureList(baseSection.DefaultLanguage).SequenceEqual(ReplayBuilder(baseSection).SetDesiredLanguages(null).Build().Languages));

        var langString = "sl," + baseSection.Languages;
        baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage("sl").WithLanguages("sl,it,de,en").Build();
        Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(baseSection).Build().Languages));
        Assert.True(GetCultureList(langString).SequenceEqual(ProductionBuilder(baseSection).Build().Languages));
        Assert.True(GetCultureList(langString).SequenceEqual(ReplayBuilder(baseSection).Build().Languages));
    }

    [Fact]
    public void DisabledProducersHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDisabledProducers("1,3").Build();

        Assert.True(GetIntList(baseSection.DisabledProducers).SequenceEqual(IntegrationBuilder(baseSection).Build().Producer.DisabledProducers));
        Assert.True(GetIntList(baseSection.DisabledProducers).SequenceEqual(ProductionBuilder(baseSection).Build().Producer.DisabledProducers));
        Assert.True(GetIntList(baseSection.DisabledProducers).SequenceEqual(ReplayBuilder(baseSection).Build().Producer.DisabledProducers));

        Assert.Empty(IntegrationBuilder(baseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
        Assert.Empty(ProductionBuilder(baseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
        Assert.Empty(ReplayBuilder(baseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
    }

    [Fact]
    public void NodeIdHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithNodeId(15).Build();

        Assert.Equal(baseSection.NodeId, IntegrationBuilder(baseSection).Build().NodeId);
        Assert.Equal(baseSection.NodeId, ProductionBuilder(baseSection).Build().NodeId);
        Assert.Equal(baseSection.NodeId, ReplayBuilder(baseSection).Build().NodeId);

        Assert.Equal(0, IntegrationBuilder(baseSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(0, ProductionBuilder(baseSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(0, ReplayBuilder(baseSection).SetNodeId(0).Build().NodeId);

        Assert.Equal(baseSection.NodeId, IntegrationBuilder(baseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        Assert.Equal(baseSection.NodeId, ProductionBuilder(baseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        Assert.Equal(baseSection.NodeId, ReplayBuilder(baseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
    }

    [Fact]
    public void EnvironmentHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithEnvironment(SdkEnvironment.Integration).Build();

        Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder(baseSection).Build().Environment);
        Assert.Equal(SdkEnvironment.Production, ProductionBuilder(baseSection).Build().Environment);
        Assert.Equal(SdkEnvironment.Replay, ReplayBuilder(baseSection).Build().Environment);
    }

    [Fact]
    public void ExceptionHandlingStrategyHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw).Build();

        Assert.Equal(baseSection.ExceptionHandlingStrategy, IntegrationBuilder(baseSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(baseSection.ExceptionHandlingStrategy, ProductionBuilder(baseSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(baseSection.ExceptionHandlingStrategy, ReplayBuilder(baseSection).Build().ExceptionHandlingStrategy);

        Assert.Equal(ExceptionHandlingStrategy.Catch, IntegrationBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, ProductionBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, ReplayBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);

        Assert.Equal(baseSection.ExceptionHandlingStrategy, IntegrationBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        Assert.Equal(baseSection.ExceptionHandlingStrategy, ProductionBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        Assert.Equal(baseSection.ExceptionHandlingStrategy, ReplayBuilder(baseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
    }

    [Fact]
    public void MessagingHostIsIgnored()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitHost("mq.localhost.local").Build();

        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Integration), IntegrationBuilder(baseSection).Build().Rabbit.Host);
        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Production), ProductionBuilder(baseSection).Build().Rabbit.Host);
        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), ReplayBuilder(baseSection).Build().Rabbit.Host);
    }

    [Fact]
    public void PortHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPort(2250).WithRabbitUseSsl(true).Build();

        Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(baseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(baseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(baseSection).Build().Rabbit.Port);

        baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPort(2250).WithRabbitUseSsl(false).Build();
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(baseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(baseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(baseSection).Build().Rabbit.Port);
    }

    [Fact]
    public void UsernameHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithAccessToken("token").WithRabbitUsername("username").Build();

        Assert.Equal(baseSection.AccessToken, IntegrationBuilder(baseSection).Build().Rabbit.Username);
        Assert.Equal(baseSection.AccessToken, ProductionBuilder(baseSection).Build().Rabbit.Username);
        Assert.Equal(baseSection.AccessToken, ReplayBuilder(baseSection).Build().Rabbit.Username);
    }

    [Fact]
    public void PasswordHasDefaultValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPassword(null).Build();

        Assert.Null(IntegrationBuilder(baseSection).Build().Rabbit.Password);
        Assert.Null(ProductionBuilder(baseSection).Build().Rabbit.Password);
        Assert.Null(ReplayBuilder(baseSection).Build().Rabbit.Password);

        baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPassword("SecretPassword").Build();

        Assert.Null(IntegrationBuilder(baseSection).Build().Rabbit.Password);
        Assert.Null(ProductionBuilder(baseSection).Build().Rabbit.Password);
        Assert.Null(ReplayBuilder(baseSection).Build().Rabbit.Password);
    }

    [Fact]
    public void VirtualHostHasDefaultValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitVirtualHost(null).Build();

        Assert.Equal(TestData.VirtualHost, IntegrationBuilder(baseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ProductionBuilder(baseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ReplayBuilder(baseSection).Build().Rabbit.VirtualHost);

        baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitVirtualHost("my-virtual-host").Build();

        Assert.Equal(TestData.VirtualHost, IntegrationBuilder(baseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ProductionBuilder(baseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ReplayBuilder(baseSection).Build().Rabbit.VirtualHost);
    }

    [Fact]
    public void UseMessagingSslHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitUseSsl(false).Build();

        Assert.True(IntegrationBuilder(baseSection).Build().Rabbit.UseSsl);
        Assert.True(ProductionBuilder(baseSection).Build().Rabbit.UseSsl);
        Assert.True(ReplayBuilder(baseSection).Build().Rabbit.UseSsl);
    }

    [Fact]
    public void ApiHostHasDefaultValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithApiHost("api.localhost.local").Build();

        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder(baseSection).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), ProductionBuilder(baseSection).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Replay), ReplayBuilder(baseSection).Build().Api.Host);

        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Replay), ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
    }

    [Fact]
    public void UseApiSslHasCorrectValue()
    {
        var baseSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithApiUseSsl(false).Build();

        Assert.True(IntegrationBuilder(baseSection).Build().Api.UseSsl);
        Assert.True(ProductionBuilder(baseSection).Build().Api.UseSsl);
        Assert.True(ReplayBuilder(baseSection).Build().Api.UseSsl);
    }

    [Fact]
    public void LoadBasicAppConfig()
    {
        const string accessToken = "AccessToken";
        var config = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en' />".ToSdkConfiguration();

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Integration,
                              "en",
                              1,
                              EnvironmentManager.GetMqHost(SdkEnvironment.Integration),
                              EnvironmentManager.GetApiHost(SdkEnvironment.Integration),
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost);
    }

    [Fact]
    public void BuilderEnvironmentGlobalProduction()
    {
        const string accessToken = "AccessToken";
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' />".ToSection();

        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectEnvironment(SdkEnvironment.GlobalProduction)
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(55)
                    .Build();

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.GlobalProduction,
                              "en",
                              2,
                              EnvironmentManager.GetMqHost(SdkEnvironment.GlobalProduction),
                              EnvironmentManager.GetApiHost(SdkEnvironment.GlobalProduction),
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              true,
                              true,
                              45,
                              750,
                              45,
                              11,
                              0,
                              ExceptionHandlingStrategy.Throw,
                              45,
                              55);
    }

    [Fact]
    public void BuilderEnvironmentGlobalIntegration()
    {
        const string accessToken = "AccessToken";
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' />".ToSection();

        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectEnvironment(SdkEnvironment.GlobalIntegration)
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(55)
                    .Build();

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.GlobalIntegration,
                              "en",
                              2,
                              EnvironmentManager.GetMqHost(SdkEnvironment.GlobalIntegration),
                              EnvironmentManager.GetApiHost(SdkEnvironment.GlobalIntegration),
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              true,
                              true,
                              45,
                              750,
                              45,
                              11,
                              0,
                              ExceptionHandlingStrategy.Throw,
                              45,
                              55);
    }

    [Fact]
    public void BuilderEnvironmentReplay()
    {
        const string accessToken = "AccessToken";
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' />".ToSection();

        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectEnvironment(SdkEnvironment.Replay)
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(45)
                    .Build();

        Assert.Equal(SdkEnvironment.Replay, config.Environment);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), config.Api.Host);
        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Replay,
                              "en",
                              2,
                              EnvironmentManager.GetMqHost(SdkEnvironment.Replay),
                              EnvironmentManager.GetApiHost(SdkEnvironment.Replay),
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              true,
                              true,
                              45,
                              750,
                              45,
                              11,
                              0,
                              ExceptionHandlingStrategy.Throw,
                              45,
                              45);
    }

    // ReSharper disable once TooManyArguments
    // ReSharper disable once MethodTooLong
    private static void ValidateConfiguration(IUofConfiguration config,
                                              string accessToken,
                                              SdkEnvironment environment,
                                              string defaultCulture,
                                              int wantedCultures,
                                              string mqHost,
                                              string apiHost,
                                              int port,
                                              string username,
                                              string password = null,
                                              string virtualHost = null,
                                              bool useMqSsl = true,
                                              bool useApiSsl = true,
                                              int inactivitySeconds = ConfigLimit.InactivitySecondsDefault,
                                              int maxRecoveryExecutionInSeconds = ConfigLimit.MaxRecoveryTimeDefault,
                                              int minIntervalBetweenRecoveryRequests = ConfigLimit.MinIntervalBetweenRecoveryRequestDefault,
                                              int nodeId = 0,
                                              int disabledProducers = 0,
                                              ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.Throw,
                                              int httpClientTimeout = ConfigLimit.HttpClientTimeoutDefault,
                                              int recoveryHttpClientTimeout = ConfigLimit.HttpClientRecoveryTimeoutDefault)
    {
        Assert.NotNull(config);
        Assert.Equal(accessToken, config.AccessToken);
        Assert.Equal(environment, config.Environment);
        Assert.Equal(defaultCulture, config.DefaultLanguage.TwoLetterISOLanguageName);
        Assert.Equal(wantedCultures, config.Languages.Count);
        Assert.Equal(mqHost, config.Rabbit.Host);
        Assert.Equal(apiHost, config.Api.Host);
        Assert.Equal(port, config.Rabbit.Port);
        Assert.Equal(username, config.Rabbit.Username);
        Assert.Equal(password, config.Rabbit.Password);
        Assert.Equal(virtualHost, config.Rabbit.VirtualHost);
        Assert.Equal(useMqSsl, config.Rabbit.UseSsl);
        Assert.Equal(useApiSsl, config.Api.UseSsl);
        Assert.Equal(inactivitySeconds, config.Producer.InactivitySeconds.TotalSeconds);
        Assert.Equal(maxRecoveryExecutionInSeconds, config.Producer.MaxRecoveryTime.TotalSeconds);
        Assert.Equal(minIntervalBetweenRecoveryRequests, config.Producer.MinIntervalBetweenRecoveryRequests.TotalSeconds);
        Assert.Equal(nodeId, config.NodeId);
        Assert.Equal(disabledProducers, config.Producer.DisabledProducers?.Count ?? 0);
        Assert.Equal(exceptionHandlingStrategy, config.ExceptionHandlingStrategy);
        Assert.Equal(httpClientTimeout, config.Api.HttpClientTimeout.TotalSeconds);
        Assert.Equal(recoveryHttpClientTimeout, config.Api.HttpClientRecoveryTimeout.TotalSeconds);

        Assert.NotNull(config.BookmakerDetails);
        Assert.NotNull(config.BookmakerDetails.VirtualHost);
        Assert.True(config.BookmakerDetails.BookmakerId > 0);
        Assert.NotNull(config.Producer.Producers);
        Assert.NotEmpty(config.Producer.Producers);
    }
}
