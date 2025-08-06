// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

// Ignore Spelling: Ssl
// Ignore Spelling: App
[SuppressMessage("ReSharper", "TooManyChainedReferences")]
public class ConfigurationBuilderWithSectionTests : ConfigurationBuilderSetup
{
    [Fact]
    public void AccessTokenHasCorrectValue()
    {
        BaseSection.AccessToken = "my_token";
        Assert.Equal(BaseSection.AccessToken, IntegrationBuilder(BaseSection).Build().AccessToken);
        Assert.Equal(BaseSection.AccessToken, ProductionBuilder(BaseSection).Build().AccessToken);
        Assert.Equal(BaseSection.AccessToken, ReplayBuilder(BaseSection).Build().AccessToken);
    }

    [Fact]
    public void DefaultLanguageFromSectionHasCorrectValue()
    {
        BaseSection.DefaultLanguage = "de";
        var cultureEn = new CultureInfo("en");
        var cultureDe = new CultureInfo("de");

        Assert.Equal(cultureDe, IntegrationBuilder(BaseSection).Build().DefaultLanguage);
        Assert.Equal(cultureDe, ProductionBuilder(BaseSection).Build().DefaultLanguage);
        Assert.Equal(cultureDe, ReplayBuilder(BaseSection).Build().DefaultLanguage);

        Assert.Equal(2, IntegrationBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ProductionBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ReplayBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanOverrideManually()
    {
        BaseSection.DefaultLanguage = "de";
        var cultureEn = new CultureInfo("en");

        Assert.Equal(cultureEn, IntegrationBuilder(BaseSection).SetDefaultLanguage(cultureEn).Build().DefaultLanguage);
        Assert.Equal(cultureEn, ProductionBuilder(BaseSection).SetDefaultLanguage(cultureEn).Build().DefaultLanguage);
        Assert.Equal(cultureEn, ReplayBuilder(BaseSection).SetDefaultLanguage(cultureEn).Build().DefaultLanguage);

        Assert.Equal(2, IntegrationBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ProductionBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ReplayBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanOverrideManuallyAndBack()
    {
        BaseSection.DefaultLanguage = "de";
        var cultureEn = new CultureInfo("en");
        var cultureDe = new CultureInfo("de");

        Assert.Equal(cultureDe, IntegrationBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLanguage);
        Assert.Equal(cultureDe, ProductionBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLanguage);
        Assert.Equal(cultureDe, ReplayBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().DefaultLanguage);

        Assert.Equal(2, IntegrationBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ProductionBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
        Assert.Equal(2, ReplayBuilder(BaseSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build().Languages.Count);
    }

    [Fact]
    public void LanguagesHasCorrectValue()
    {
        BaseSection.DefaultLanguage = "it";
        BaseSection.Languages = "it,de,en";

        Assert.True(GetCultureList(BaseSection.Languages).SequenceEqual(IntegrationBuilder(BaseSection).Build().Languages));
        Assert.True(GetCultureList(BaseSection.Languages).SequenceEqual(ProductionBuilder(BaseSection).Build().Languages));
        Assert.True(GetCultureList(BaseSection.Languages).SequenceEqual(ReplayBuilder(BaseSection).Build().Languages));

        Assert.True(GetCultureList(BaseSection.DefaultLanguage).SequenceEqual(IntegrationBuilder(BaseSection).SetDesiredLanguages(null).Build().Languages));
        Assert.True(GetCultureList(BaseSection.DefaultLanguage).SequenceEqual(ProductionBuilder(BaseSection).SetDesiredLanguages(null).Build().Languages));
        Assert.True(GetCultureList(BaseSection.DefaultLanguage).SequenceEqual(ReplayBuilder(BaseSection).SetDesiredLanguages(null).Build().Languages));

        BaseSection.DefaultLanguage = "sl";
        var langString = "sl," + BaseSection.Languages;
        Assert.True(GetCultureList(langString).SequenceEqual(IntegrationBuilder(BaseSection).Build().Languages));
        Assert.True(GetCultureList(langString).SequenceEqual(ProductionBuilder(BaseSection).Build().Languages));
        Assert.True(GetCultureList(langString).SequenceEqual(ReplayBuilder(BaseSection).Build().Languages));
    }

    [Fact]
    public void DisabledProducersHasCorrectValue()
    {
        BaseSection.DisabledProducers = "1,3";
        Assert.True(GetIntList(BaseSection.DisabledProducers).SequenceEqual(IntegrationBuilder(BaseSection).Build().Producer.DisabledProducers));
        Assert.True(GetIntList(BaseSection.DisabledProducers).SequenceEqual(ProductionBuilder(BaseSection).Build().Producer.DisabledProducers));
        Assert.True(GetIntList(BaseSection.DisabledProducers).SequenceEqual(ReplayBuilder(BaseSection).Build().Producer.DisabledProducers));

        Assert.Empty(IntegrationBuilder(BaseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
        Assert.Empty(ProductionBuilder(BaseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
        Assert.Empty(ReplayBuilder(BaseSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
    }

    [Fact]
    public void NodeIdHasCorrectValue()
    {
        BaseSection.NodeId = 15;

        Assert.Equal(BaseSection.NodeId, IntegrationBuilder(BaseSection).Build().NodeId);
        Assert.Equal(BaseSection.NodeId, ProductionBuilder(BaseSection).Build().NodeId);
        Assert.Equal(BaseSection.NodeId, ReplayBuilder(BaseSection).Build().NodeId);

        Assert.Equal(0, IntegrationBuilder(BaseSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(0, ProductionBuilder(BaseSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(0, ReplayBuilder(BaseSection).SetNodeId(0).Build().NodeId);

        Assert.Equal(BaseSection.NodeId, IntegrationBuilder(BaseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        Assert.Equal(BaseSection.NodeId, ProductionBuilder(BaseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
        Assert.Equal(BaseSection.NodeId, ReplayBuilder(BaseSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
    }

    [Fact]
    public void EnvironmentHasCorrectValue()
    {
        BaseSection.Environment = SdkEnvironment.Integration;

        Assert.Equal(SdkEnvironment.Integration, IntegrationBuilder(BaseSection).Build().Environment);
        Assert.Equal(SdkEnvironment.Production, ProductionBuilder(BaseSection).Build().Environment);
        Assert.Equal(SdkEnvironment.Replay, ReplayBuilder(BaseSection).Build().Environment);
    }

    [Fact]
    public void ExceptionHandlingStrategyHasCorrectValue()
    {
        BaseSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Throw;

        Assert.Equal(BaseSection.ExceptionHandlingStrategy, IntegrationBuilder(BaseSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(BaseSection.ExceptionHandlingStrategy, ProductionBuilder(BaseSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(BaseSection.ExceptionHandlingStrategy, ReplayBuilder(BaseSection).Build().ExceptionHandlingStrategy);

        Assert.Equal(ExceptionHandlingStrategy.Catch, IntegrationBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, ProductionBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, ReplayBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);

        Assert.Equal(BaseSection.ExceptionHandlingStrategy, IntegrationBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        Assert.Equal(BaseSection.ExceptionHandlingStrategy, ProductionBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
        Assert.Equal(BaseSection.ExceptionHandlingStrategy, ReplayBuilder(BaseSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
    }

    [Fact]
    public void MessagingHostIsIgnored()
    {
        BaseSection.RabbitHost = "mq.localhost.local";

        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Integration), IntegrationBuilder(BaseSection).Build().Rabbit.Host);
        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Production), ProductionBuilder(BaseSection).Build().Rabbit.Host);
        Assert.Equal(EnvironmentManager.GetMqHost(SdkEnvironment.Replay), ReplayBuilder(BaseSection).Build().Rabbit.Host);
    }

    [Fact]
    public void PortHasCorrectValue()
    {
        BaseSection.RabbitPort = 2250;
        BaseSection.RabbitUseSsl = true;

        Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(BaseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(BaseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(BaseSection).Build().Rabbit.Port);

        BaseSection.RabbitUseSsl = false;
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, IntegrationBuilder(BaseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ProductionBuilder(BaseSection).Build().Rabbit.Port);
        Assert.Equal(EnvironmentManager.DefaultMqHostPort, ReplayBuilder(BaseSection).Build().Rabbit.Port);
    }

    [Fact]
    public void UsernameHasCorrectValue()
    {
        BaseSection.RabbitUsername = "username";
        BaseSection.AccessToken = "token";

        Assert.Equal(BaseSection.AccessToken, IntegrationBuilder(BaseSection).Build().Rabbit.Username);
        Assert.Equal(BaseSection.AccessToken, ProductionBuilder(BaseSection).Build().Rabbit.Username);
        Assert.Equal(BaseSection.AccessToken, ReplayBuilder(BaseSection).Build().Rabbit.Username);
    }

    [Fact]
    public void PasswordHasDefaultValue()
    {
        BaseSection.RabbitPassword = null;
        Assert.Null(IntegrationBuilder(BaseSection).Build().Rabbit.Password);
        Assert.Null(ProductionBuilder(BaseSection).Build().Rabbit.Password);
        Assert.Null(ReplayBuilder(BaseSection).Build().Rabbit.Password);

        BaseSection.RabbitPassword = "myPassword";
        Assert.Null(IntegrationBuilder(BaseSection).Build().Rabbit.Password);
        Assert.Null(ProductionBuilder(BaseSection).Build().Rabbit.Password);
        Assert.Null(ReplayBuilder(BaseSection).Build().Rabbit.Password);
    }

    [Fact]
    public void VirtualHostHasDefaultValue()
    {
        BaseSection.RabbitVirtualHost = null;
        Assert.Equal(TestData.VirtualHost, IntegrationBuilder(BaseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ProductionBuilder(BaseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ReplayBuilder(BaseSection).Build().Rabbit.VirtualHost);

        BaseSection.RabbitVirtualHost = "my_virtual_host";
        Assert.Equal(TestData.VirtualHost, IntegrationBuilder(BaseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ProductionBuilder(BaseSection).Build().Rabbit.VirtualHost);
        Assert.Equal(TestData.VirtualHost, ReplayBuilder(BaseSection).Build().Rabbit.VirtualHost);
    }

    [Fact]
    public void UseMessagingSslHasCorrectValue()
    {
        BaseSection.RabbitUseSsl = false;
        Assert.True(IntegrationBuilder(BaseSection).Build().Rabbit.UseSsl);
        Assert.True(ProductionBuilder(BaseSection).Build().Rabbit.UseSsl);
        Assert.True(ReplayBuilder(BaseSection).Build().Rabbit.UseSsl);
    }

    [Fact]
    public void ApiHostHasDefaultValue()
    {
        BaseSection.ApiHost = "api.localhost.local";
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder(BaseSection).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), ProductionBuilder(BaseSection).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Replay), ReplayBuilder(BaseSection).Build().Api.Host);

        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), IntegrationBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Production), ProductionBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
        Assert.Equal(EnvironmentManager.GetApiHost(SdkEnvironment.Replay), ReplayBuilder("token").SetDefaultLanguage(TestData.Culture).Build().Api.Host);
    }

    [Fact]
    public void UseApiSslHasCorrectValue()
    {
        BaseSection.ApiUseSsl = false;
        Assert.True(IntegrationBuilder(BaseSection).Build().Api.UseSsl);
        Assert.True(ProductionBuilder(BaseSection).Build().Api.UseSsl);
        Assert.True(ReplayBuilder(BaseSection).Build().Api.UseSsl);
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
