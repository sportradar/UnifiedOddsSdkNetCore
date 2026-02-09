// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

// Ignore Spelling: Ssl

using System.Configuration;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;
using Xunit;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class ConfigurationBuilderWithSectionWithCustomSectionTests : ConfigurationBuilderWithSectionSetup
{
    [Fact]
    public void AccessTokenHasCorrectValue()
    {
        const string accessToken = "my_token";
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithAccessToken(accessToken).Build();

        var config = CustomBuilder(customSection).Build();

        Assert.Equal(accessToken, config.AccessToken);
    }

    [Fact]
    public void DefaultLanguageFromSectionHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName)
                                                    .Build();

        var config = CustomBuilder(customSection).Build();

        Assert.Equal(TestConsts.CultureDe, config.DefaultLanguage);
        Assert.Single(config.Languages);
        Assert.Equal(TestConsts.CultureDe, config.Languages[0]);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanBeOverrideManually()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName).Build();

        var config = CustomBuilder(customSection).SetDefaultLanguage(TestConsts.CultureEn).Build();

        Assert.Equal(TestConsts.CultureEn, config.DefaultLanguage);
        Assert.Equal(2, config.Languages.Count);
    }

    [Fact]
    public void SettingDefaultLanguageFromSectionAddsToLanguages()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName)
                                                    .Build();

        var config = CustomBuilder(customSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build();

        Assert.Equal(TestConsts.CultureDe, config.DefaultLanguage);
        Assert.Single(config.Languages);
    }

    [Fact]
    public void OverridingDefaultLanguageFromSection()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields()
                                                    .WithDefaultLanguage(TestConsts.CultureDe.TwoLetterISOLanguageName)
                                                    .WithLanguages(TestConsts.CultureDe.TwoLetterISOLanguageName)
                                                    .Build();

        var config = CustomBuilder(customSection).SetDefaultLanguage(TestConsts.CultureEn).LoadFromConfigFile().Build();

        Assert.Equal(TestConsts.CultureDe, config.DefaultLanguage);
        Assert.Single(config.Languages);
    }

    [Fact]
    public void LanguagesHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage("it").WithLanguages("it,de,en").Build();

        Assert.True(GetCultureList(customSection.Languages).SequenceEqual(CustomBuilder(customSection).Build().Languages));
        Assert.True(GetCultureList(customSection.DefaultLanguage).SequenceEqual(CustomBuilder(customSection).SetDesiredLanguages(null).Build().Languages));

        var langString = "sl," + customSection.Languages;
        customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDefaultLanguage("sl").WithLanguages(langString).Build();

        Assert.True(GetCultureList(langString).SequenceEqual(CustomBuilder(customSection).Build().Languages));
    }

    [Fact]
    public void DisabledProducersHasCorrectValue()
    {
        const string disabledProducers = "2,4,6";

        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithDisabledProducers(disabledProducers).Build();

        Assert.True(GetIntList(disabledProducers).SequenceEqual(CustomBuilder(customSection).Build().Producer.DisabledProducers));

        Assert.Empty(CustomBuilder(customSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
    }

    [Fact]
    public void NodeIdHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithNodeId(11).Build();

        Assert.Equal(customSection.NodeId, CustomBuilder(customSection).Build().NodeId);
        Assert.Equal(0, CustomBuilder(customSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(customSection.NodeId, CustomBuilder(customSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
    }

    [Fact]
    public void EnvironmentHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithEnvironment(SdkEnvironment.Custom).Build();

        Assert.Equal(SdkEnvironment.Custom, CustomBuilder(customSection).Build().Environment);
    }

    [Fact]
    public void ExceptionHandlingStrategyHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw).Build();

        Assert.Equal(customSection.ExceptionHandlingStrategy, CustomBuilder(customSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, CustomBuilder(customSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(customSection.ExceptionHandlingStrategy, CustomBuilder(customSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
    }

    [Fact]
    public void MessagingHostIsIgnored()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitHost(TestConsts.AnyRabbitHost).Build();

        Assert.Equal(CustomRabbitHost, CustomBuilder(customSection).Build().Rabbit.Host);
    }

    [Fact]
    public void PortHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPort(2250).WithRabbitUseSsl(true).Build();

        Assert.Equal(customSection.RabbitPort, CustomBuilder(customSection).Build().Rabbit.Port);

        customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitPort(2250).WithRabbitUseSsl(false).Build();

        Assert.Equal(customSection.RabbitPort, CustomBuilder(customSection).Build().Rabbit.Port);
    }

    [Fact]
    public void MessagingCredentialsHasCorrectValue()
    {
        const string rabbitUsername = "custom-username";
        const string rabbitPassword = "custom-password";

        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitUsername(rabbitUsername).WithRabbitPassword(rabbitPassword).Build();

        Assert.Equal(rabbitUsername, CustomBuilder(customSection).Build().Rabbit.Username);
        Assert.Equal(rabbitPassword, CustomBuilder(customSection).Build().Rabbit.Password);
    }

    [Fact]
    public void VirtualHostHasDefaultValue()
    {
        const string rabbitVirtualHost = "custom-virtual-host";

        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitVirtualHost(null).Build();

        Assert.Equal(TestData.VirtualHost, CustomBuilder(customSection).Build().Rabbit.VirtualHost);

        customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitVirtualHost(rabbitVirtualHost).Build();

        Assert.Equal(rabbitVirtualHost, CustomBuilder(customSection).Build().Rabbit.VirtualHost);
    }

    [Fact]
    public void UseMessagingSslHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitUseSsl(false).Build();

        Assert.False(CustomBuilder(customSection).Build().Rabbit.UseSsl);

        customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithRabbitUseSsl(true).Build();

        Assert.True(CustomBuilder(customSection).Build().Rabbit.UseSsl);
    }

    [Fact]
    public void ApiHostHasDefaultValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithApiHost(TestConsts.AnyApiHost).Build();

        Assert.Equal(TestConsts.AnyApiHost, CustomBuilder(customSection).Build().Api.Host);
    }

    [Fact]
    public void UseApiSslHasCorrectValue()
    {
        var customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithApiUseSsl(false).Build();

        Assert.False(CustomBuilder(customSection).Build().Api.UseSsl);

        customSection = UofConfigurationSections.GetBuilderWithOnlyRequiredFields().WithApiUseSsl(true).Build();

        Assert.True(CustomBuilder(customSection).Build().Api.UseSsl);
    }

    [Fact]
    public void InvalidSectionNodeName()
    {
        Assert.Throws<ConfigurationErrorsException>(() => "<customSdkSection accessToken='1234567ab' desiredLanguages='en,de' nodeId='11' environment='Integration' />".ToSection());
    }

    [Fact]
    public void CustomEnvironmentSetMessagingConfig()
    {
        const string accessToken = "CustomAccessToken";
        const int port = 1234;
        const bool useSsl = false;
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' environment='Custom' host='{CustomRabbitHost}' port='{port}' useSsl='{useSsl}' />".ToSection();
        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(55)
                    .Build();
        Assert.Equal(SdkEnvironment.Custom, config.Environment);

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Custom,
                              "en",
                              2,
                              CustomRabbitHost,
                              EnvironmentManager.GetApiHost(SdkEnvironment.Integration),
                              port,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              useSsl,
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
    public void CustomEnvironmentSetApiConfig()
    {
        const string accessToken = "CustomAccessToken";
        const bool useApiSsl = false;
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' environment='Custom' apiHost='{CustomApiHost}' apiUseSsl='{useApiSsl}' />".ToSection();
        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(55)
                    .Build();
        Assert.Equal(SdkEnvironment.Custom, config.Environment);

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Custom,
                              "en",
                              2,
                              EnvironmentManager.GetMqHost(SdkEnvironment.Integration),
                              CustomApiHost,
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              true,
                              useApiSsl,
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
    public void CustomEnvironmentSetMessagingAndApiConfig()
    {
        const string accessToken = "CustomAccessToken";
        const int port = 1234;
        const bool useSsl = false;
        const bool useApiSsl = false;
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' environment='Custom' host='{CustomRabbitHost}' port='{port}' useSsl='{useSsl}' apiHost='{CustomApiHost}' apiUseSsl='{useApiSsl}' />".ToSection();
        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider())
                    .SetAccessTokenFromConfigFile()
                    .SelectCustom()
                    .LoadFromConfigFile()
                    .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Throw)
                    .SetHttpClientTimeout(45)
                    .SetInactivitySeconds(45)
                    .SetMaxRecoveryTime(750)
                    .SetMinIntervalBetweenRecoveryRequests(45)
                    .SetHttpClientRecoveryTimeout(55)
                    .Build();
        Assert.Equal(SdkEnvironment.Custom, config.Environment);

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Custom,
                              "en",
                              2,
                              CustomRabbitHost,
                              CustomApiHost,
                              port,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              useSsl,
                              useApiSsl,
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
    public void DirectlyBuildCustomEnvironmentConfigSection()
    {
        const string accessToken = "CustomAccessToken";
        const bool useApiSsl = false;
        var section = $"<uofSdkSection accessToken='{accessToken}' desiredLanguages='en,de' nodeId='11' environment='Custom' apiHost='{CustomApiHost}' apiUseSsl='{useApiSsl}' />".ToSection();
        var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider()).BuildFromConfigFile();
        //var config = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider()).SetAccessTokenFromConfigFile().SelectCustom().LoadFromConfigFile().Build();

        Assert.Equal(SdkEnvironment.Custom, config.Environment);

        ValidateConfiguration(config,
                              accessToken,
                              SdkEnvironment.Custom,
                              "en",
                              2,
                              EnvironmentManager.GetMqHost(SdkEnvironment.Integration),
                              CustomApiHost,
                              EnvironmentManager.DefaultMqHostPort,
                              accessToken,
                              null,
                              TestData.VirtualHost,
                              true,
                              useApiSsl,
                              ConfigLimit.InactivitySecondsDefault,
                              ConfigLimit.MaxRecoveryTimeDefault,
                              ConfigLimit.MinIntervalBetweenRecoveryRequestDefault,
                              11);
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
    }
}
