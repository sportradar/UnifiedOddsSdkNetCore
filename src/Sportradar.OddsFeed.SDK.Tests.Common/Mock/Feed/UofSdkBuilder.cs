// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Extensions;
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

    public UofSdkBuilder WithOutputHelper(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        return this;
    }

    public UofSdkWrapper Build()
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
        var allProducers = new List<int>
                           {
                               1, 2, 3, 4,
                               5, 6, 7, 8,
                               9, 10, 11, 12,
                               13, 14, 15, 16,
                               17
                           };

        var projectConfiguration = new ProjectConfiguration();
        var disabledProducers = allProducers.Where(x => !_enabledProducers.Contains(x)).ToList();
        var sdkConfig = UofSdk.GetConfigurationBuilder()
                              .SetAccessToken(projectConfiguration.SdkRabbitUsername)
                              .SelectCustom()
                              .SetMessagingUsername(projectConfiguration.SdkRabbitUsername)
                              .SetMessagingPassword(projectConfiguration.SdkRabbitPassword)
                              .SetMessagingHost(projectConfiguration.GetRabbitIp())
                              .UseMessagingSsl(false)
                              .SetMessagingPort(projectConfiguration.DefaultRabbitPort)
                              .SetApiHost(UrlUtils.ExtractDomainName(_apiUrl))
                              .UseApiSsl(false)
                              .SetDesiredLanguages(_cultures)
                              .SetMinIntervalBetweenRecoveryRequests(20)
                              .SetDisabledProducers(disabledProducers)
                              .Build();

        var serviceCollection = new ServiceCollection();
        if (_useSdkLogging)
        {
            var xLoggerFactory = new XunitLoggerFactory(_outputHelper);
            serviceCollection.AddSingleton<IConfiguration>(projectConfiguration.Configuration);
            serviceCollection.AddLogging(options =>
                                         {
                                             options.AddConfiguration(projectConfiguration.Configuration.GetSection("Logging"));
                                             options.SetMinimumLevel(LogLevel.Information);
                                             options.AddProvider(new XUnitLoggerProvider(xLoggerFactory));
                                         });
        }
        serviceCollection.AddUofSdk(sdkConfig);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return new UofSdkWrapper(serviceProvider, _messageInterests, _outputHelper, _openSdk);
    }
}
