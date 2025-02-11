// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

// Ignore Spelling: Ssl

using System.Configuration;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable TooManyChainedReferences

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class ConfigurationBuilderWithCustomSectionTests : ConfigurationBuilderSetup
{
    private readonly ITestOutputHelper _outputHelper;

    public ConfigurationBuilderWithCustomSectionTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void AccessTokenHasCorrectValue()
    {
        CustomSection.AccessToken = "my_token";
        Assert.Equal(CustomSection.AccessToken, CustomBuilder(CustomSection).Build().AccessToken);
    }

    [Fact]
    public void DefaultLanguageFromSectionHasCorrectValue()
    {
        var customSection = (TestSection)TestSection.GetCustomSection();
        customSection.DefaultLanguage = "de";

        var config = CustomBuilder(customSection).Build();
        _outputHelper.WriteLine($"Default language: {config.DefaultLanguage.Name}");
        _outputHelper.WriteLine($"Languages: {SdkInfo.ConvertCultures(config.Languages)}");

        Assert.Equal(customSection.DefaultLanguage, config.DefaultLanguage.TwoLetterISOLanguageName);
        Assert.Equal(2, config.Languages.Count);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanBeOverrideManually()
    {
        CustomSection.DefaultLanguage = "de";
        var cultureEn = new CultureInfo("en");

        var config = CustomBuilder(CustomSection).SetDefaultLanguage(cultureEn).Build();
        _outputHelper.WriteLine($"Default language: {config.DefaultLanguage.Name}");
        _outputHelper.WriteLine($"Languages: {SdkInfo.ConvertCultures(config.Languages)}");

        Assert.Equal(cultureEn, config.DefaultLanguage);
        Assert.Equal(2, config.Languages.Count);
    }

    [Fact]
    public void DefaultLanguageFromSectionCanBeOverrideManuallyAndBack()
    {
        CustomSection.DefaultLanguage = "de";
        var cultureDe = new CultureInfo("de");
        var cultureEn = new CultureInfo("en");

        var config = CustomBuilder(CustomSection).SetDefaultLanguage(cultureEn).LoadFromConfigFile().Build();
        _outputHelper.WriteLine($"Default language: {config.DefaultLanguage.Name}");
        _outputHelper.WriteLine($"Languages: {SdkInfo.ConvertCultures(config.Languages)}");

        Assert.Equal(cultureDe, config.DefaultLanguage);
        Assert.Equal(2, config.Languages.Count);
    }

    [Fact]
    public void LanguagesHasCorrectValue()
    {
        CustomSection.DefaultLanguage = "it";
        CustomSection.Languages = "it,de,en";

        Assert.True(GetCultureList(CustomSection.Languages).SequenceEqual(CustomBuilder(CustomSection).Build().Languages));
        Assert.True(GetCultureList(CustomSection.DefaultLanguage).SequenceEqual(CustomBuilder(CustomSection).SetDesiredLanguages(null).Build().Languages));

        CustomSection.DefaultLanguage = "sl";
        var langString = "sl," + CustomSection.Languages;
        Assert.True(GetCultureList(langString).SequenceEqual(CustomBuilder(CustomSection).Build().Languages));
    }

    [Fact]
    public void DisabledProducersHasCorrectValue()
    {
        CustomSection.DisabledProducers = "1,3";
        Assert.True(GetIntList(CustomSection.DisabledProducers).SequenceEqual(CustomBuilder(CustomSection).Build().Producer.DisabledProducers));

        Assert.Empty(CustomBuilder(CustomSection).SetDisabledProducers(null).Build().Producer.DisabledProducers);
    }

    [Fact]
    public void NodeIdHasCorrectValue()
    {
        CustomSection.NodeId = 15;

        Assert.Equal(CustomSection.NodeId, CustomBuilder(CustomSection).Build().NodeId);
        Assert.Equal(0, CustomBuilder(CustomSection).SetNodeId(0).Build().NodeId);
        Assert.Equal(CustomSection.NodeId, CustomBuilder(CustomSection).SetNodeId(0).LoadFromConfigFile().Build().NodeId);
    }

    [Fact]
    public void EnvironmentHasCorrectValue()
    {
        Assert.Equal(SdkEnvironment.Custom, CustomBuilder(CustomSection).Build().Environment);
    }

    [Fact]
    public void ExceptionHandlingStrategyHasCorrectValue()
    {
        CustomSection.ExceptionHandlingStrategy = ExceptionHandlingStrategy.Throw;

        Assert.Equal(CustomSection.ExceptionHandlingStrategy, CustomBuilder(CustomSection).Build().ExceptionHandlingStrategy);
        Assert.Equal(ExceptionHandlingStrategy.Catch, CustomBuilder(CustomSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).Build().ExceptionHandlingStrategy);
        Assert.Equal(CustomSection.ExceptionHandlingStrategy, CustomBuilder(CustomSection).SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch).LoadFromConfigFile().Build().ExceptionHandlingStrategy);
    }

    [Fact]
    public void MessagingHostIsIgnored()
    {
        CustomSection.RabbitHost = CustomRabbitHost;

        Assert.Equal(CustomRabbitHost, CustomBuilder(CustomSection).Build().Rabbit.Host);
    }

    [Fact]
    public void PortHasCorrectValue()
    {
        CustomSection.RabbitPort = 2250;
        CustomSection.RabbitUseSsl = true;

        Assert.Equal(CustomSection.RabbitPort, CustomBuilder(CustomSection).Build().Rabbit.Port);

        CustomSection.RabbitUseSsl = false;
        Assert.Equal(CustomSection.RabbitPort, CustomBuilder(CustomSection).Build().Rabbit.Port);
    }

    [Fact]
    public void UsernameHasCorrectValue()
    {
        CustomSection.RabbitUsername = "username";
        CustomSection.AccessToken = "token";

        Assert.Equal(CustomSection.RabbitUsername, CustomBuilder(CustomSection).Build().Rabbit.Username);
    }

    [Fact]
    public void PasswordHasDefaultValue()
    {
        CustomSection.RabbitPassword = null;
        Assert.Null(CustomBuilder(CustomSection).Build().Rabbit.Password);

        CustomSection.RabbitPassword = "myPassword";
        Assert.NotNull(CustomBuilder(CustomSection).Build().Rabbit.Password);
        Assert.Equal(CustomSection.RabbitPassword, CustomBuilder(CustomSection).Build().Rabbit.Password);
    }

    [Fact]
    public void VirtualHostHasDefaultValue()
    {
        CustomSection.RabbitVirtualHost = null;
        Assert.Equal(TestData.VirtualHost, CustomBuilder(CustomSection).Build().Rabbit.VirtualHost);

        CustomSection.RabbitVirtualHost = "my_virtual_host";
        Assert.Equal(CustomSection.RabbitVirtualHost, CustomBuilder(CustomSection).Build().Rabbit.VirtualHost);
    }

    [Fact]
    public void UseMessagingSslHasCorrectValue()
    {
        CustomSection.RabbitUseSsl = false;
        Assert.False(CustomBuilder(CustomSection).Build().Rabbit.UseSsl);

        CustomSection.RabbitUseSsl = true;
        Assert.True(CustomBuilder(CustomSection).Build().Rabbit.UseSsl);
    }

    [Fact]
    public void ApiHostHasDefaultValue()
    {
        CustomSection.ApiHost = CustomApiHost;
        Assert.Equal(CustomApiHost, CustomBuilder(CustomSection).Build().Api.Host);
    }

    [Fact]
    public void UseApiSslHasCorrectValue()
    {
        CustomSection.ApiUseSsl = false;
        Assert.False(CustomBuilder(CustomSection).Build().Api.UseSsl);

        CustomSection.ApiUseSsl = true;
        Assert.True(CustomBuilder(CustomSection).Build().Api.UseSsl);
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
