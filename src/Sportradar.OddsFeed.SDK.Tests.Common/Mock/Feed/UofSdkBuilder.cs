// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class UofSdkBuilder
{
    private readonly ICollection<CultureInfo> _cultures = new List<CultureInfo>();
    private readonly ICollection<int> _enabledProducers = new List<int>();
    private readonly ICollection<MessageInterest> _messageInterests = new List<MessageInterest>();
    private bool _useSdkLogging;
    private bool _openSdk;
    private string _apiUrl;
    private ITestOutputHelper _outputHelper;
    private ExceptionHandlingStrategy _exceptionHandlingStrategy;
    private ProjectConfiguration _projectConfiguration;
    private UofClientAuthentication.IPrivateKeyJwtData _clientAuthentication;
    private string _authenticationUrl;

    public static UofSdkBuilder Create()
    {
        return new UofSdkBuilder();
    }

    public UofSdkBuilder WithLanguage(CultureInfo culture)
    {
        _cultures.Add(culture);
        return this;
    }

    public UofSdkBuilder WithEnabledProducer(int producerId)
    {
        _enabledProducers.Add(producerId);
        return this;
    }

    public UofSdkBuilder WithMessageInterest(MessageInterest messageInterest)
    {
        _messageInterests.Add(messageInterest);
        return this;
    }

    public UofSdkBuilder WithSdkLogging(bool useSdkLogging)
    {
        _useSdkLogging = useSdkLogging;
        return this;
    }

    public UofSdkBuilder WithOpenSdk(bool openSdk)
    {
        _openSdk = openSdk;
        return this;
    }

    public UofSdkBuilder WithApiUrl(string apiUrl)
    {
        _apiUrl = apiUrl;
        return this;
    }

    public UofSdkBuilder WithClientAuthentication(UofClientAuthentication.IPrivateKeyJwtData clientAuthentication)
    {
        _clientAuthentication = clientAuthentication;
        return this;
    }

    public UofSdkBuilder WithAuthenticationUrl(string authenticationUrl)
    {
        _authenticationUrl = authenticationUrl;
        return this;
    }

    public UofSdkBuilder WithOutputHelper(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        return this;
    }

    public UofSdkBuilder WithExceptionHandlingStrategy(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        _exceptionHandlingStrategy = exceptionHandlingStrategy;
        return this;
    }

    public UofSdkBuilder WithConfiguration(ProjectConfiguration projectConfiguration)
    {
        _projectConfiguration = projectConfiguration;
        return this;
    }

    private void ValidateRequiredFields()
    {
        if (_enabledProducers.IsNullOrEmpty())
        {
            throw new InvalidOperationException("At least one producer must be enabled");
        }
        if (_messageInterests.IsNullOrEmpty())
        {
            throw new InvalidOperationException("At least one message interest must be set");
        }
        if (_cultures.IsNullOrEmpty())
        {
            throw new InvalidOperationException("At least one language must be set");
        }
        if (_outputHelper == null)
        {
            throw new InvalidOperationException("TestOutputHelper must be set");
        }
    }

    private List<int> GetDisabledProducers()
    {
        var allProducers = new List<int>
                           {
                               1, 2, 3, 4,
                               5, 6, 7, 8,
                               9, 10, 11, 12,
                               13, 14, 15, 16,
                               17
                           };
        return allProducers.Where(x => !_enabledProducers.Contains(x)).ToList();
    }

    private IServiceCollection ConfigureServices(ProjectConfiguration projectConfiguration)
    {
        var serviceCollection = new ServiceCollection();
        if (_useSdkLogging)
        {
            var xLoggerFactory = new XunitLoggerFactory(_outputHelper);
            serviceCollection.AddSingleton(projectConfiguration.Configuration);
            serviceCollection.AddLogging(options =>
            {
                options.AddConfiguration(projectConfiguration.Configuration.GetSection("Logging"));
                options.SetMinimumLevel(LogLevel.Information);
                options.AddProvider(new XUnitLoggerProvider(xLoggerFactory));
            });
        }
        return serviceCollection;
    }

    private ICustomConfigurationBuilder GetConfigurationBuilderFor(IEnvironmentSelector environmentSelector, ProjectConfiguration projectConfiguration)
    {
        return environmentSelector
                    .SelectCustom()
                    .SetMessagingHost(projectConfiguration.RabbitHost)
                    .UseMessagingSsl(false)
                    .SetMessagingPort(projectConfiguration.RabbitPort)
                    .SetApiHost(UrlUtils.ExtractDomainName(_apiUrl))
                    .UseApiSsl(false)
                    .SetDesiredLanguages(_cultures)
                    .SetMinIntervalBetweenRecoveryRequests(20)
                    .SetDisabledProducers(GetDisabledProducers())
                    .SetExceptionHandlingStrategy(_exceptionHandlingStrategy)
                    .EnableUsageExport(false);
    }

    public UofSdkWrapper Build()
    {
        ValidateRequiredFields();

        var tokenSetter = UofSdk.GetConfigurationBuilder();
        var environmentSelector = _clientAuthentication != null
                                      ? tokenSetter.SetClientAuthentication(_clientAuthentication)
                                      : tokenSetter.SetAccessToken(_projectConfiguration.SdkRabbitUsername);

        var configurationBuilder = GetConfigurationBuilderFor(environmentSelector, _projectConfiguration);
        if (_clientAuthentication == null)
        {
            configurationBuilder.SetMessagingCredentials(_projectConfiguration.SdkRabbitUsername, _projectConfiguration.SdkRabbitPassword);
        }
        if (!string.IsNullOrEmpty(_authenticationUrl))
        {
            configurationBuilder.SetClientAuthenticationHost(GetUrlHostName(_authenticationUrl))
                                .SetClientAuthenticationPort(GetUrlPort(_authenticationUrl))
                                .SetClientAuthenticationUseSsl(UrlSchemaIsHttps(_authenticationUrl));

        }

        var sdkConfig = configurationBuilder.Build();

        var serviceCollection = ConfigureServices(_projectConfiguration);
        serviceCollection.AddUofSdk(sdkConfig);
        var xUnitLoggerFactory = new XunitLoggerFactory(_outputHelper, LogLevel.Information);
        serviceCollection.AddSingleton<ILoggerFactory>(xUnitLoggerFactory);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new UofSdkWrapper(serviceProvider, _messageInterests, _outputHelper, _openSdk);
    }

    private static string GetUrlHostName(string url)
    {
        return new UriBuilder(url).Host;
    }

    private static int GetUrlPort(string url)
    {
        return new UriBuilder(url).Port;
    }

    private static bool UrlSchemaIsHttps(string url)
    {
        return url.StartsWith("https", StringComparison.OrdinalIgnoreCase);
    }

    public UofSdkWrapper BuildWithCustomConfig(IUofConfiguration sdkConfig)
    {
        ValidateRequiredFields();

        var projectConfiguration = ProjectConfigurationBuilder.Create()
                                                              .UseTestRabbitConfiguration()
                                                              .LoadConfigurationFromAppSettingsFile()
                                                              .Build();
        var serviceCollection = ConfigureServices(projectConfiguration);
        serviceCollection.AddUofSdk(sdkConfig);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new UofSdkWrapper(serviceProvider, _messageInterests, _outputHelper, _openSdk);
    }
}
