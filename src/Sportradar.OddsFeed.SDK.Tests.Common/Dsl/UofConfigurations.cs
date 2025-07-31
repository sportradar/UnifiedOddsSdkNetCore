// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl;

public static class UofConfigurations
{
    public static IUofConfiguration GetUofConfigurationFromBuilder(CultureInfo defaultLanguage, List<CultureInfo> desiredLanguages = null, int nodeId = 1, string token = "", ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.Throw)
    {
        var builder = new UofConfigurationBuilder()
            .WithAccessToken(token)
            .WithEnvironment(SdkEnvironment.Custom)
            .WithNodeId(nodeId)
            .WithDefaultLanguage(defaultLanguage)
            .WithExceptionStrategy(exceptionHandlingStrategy)
            .WithApiConfiguration(api => api
                .WithBaseUrl("http://localhost")
                .WithReplayBaseUrl("http://localhost/replay")
                .WithHost("localhost")
                .WithReplayHost("localhost")
                .WithHttpClientTimeout(TimeSpan.FromSeconds(30))
                .WithHttpClientRecoveryTimeout(TimeSpan.FromSeconds(10))
                .WithHttpClientFastFailingTimeout(TimeSpan.FromSeconds(5))
                .WithMaxConnectionsPerServer(20)
                .WithUseSsl(false))
            .WithUsageConfiguration(usage => usage
                .WithExportEnabled(false));

        if (!desiredLanguages.IsNullOrEmpty())
        {
            builder.WithLanguages(desiredLanguages);
        }

        return builder.Build();
    }

    public static IUofConfiguration GetUofConfiguration(CultureInfo defaultLanguage, List<CultureInfo> desiredLanguages = null, int nodeId = 1, string token = "", ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.Throw)
    {
        var builder = new TokenSetter(new TestSectionProvider(null), new TestBookmakerDetailsProvider(), new TestProducersProvider())
            .SetAccessToken(token)
            .SelectCustom()
            .SetNodeId(nodeId)
            .SetDefaultLanguage(defaultLanguage)
            .SetExceptionHandlingStrategy(exceptionHandlingStrategy)
            .SetApiHost("localhost")
            .SetMessagingHost("localhost")
            .SetHttpClientTimeout(30)
            .SetHttpClientRecoveryTimeout(10)
            .SetHttpClientFastFailingTimeout(5)
            .UseApiSsl(false)
            .UseMessagingSsl(false)
            .EnableUsageExport(false);

        if (!desiredLanguages.IsNullOrEmpty())
        {
            builder.SetDesiredLanguages(desiredLanguages);
        }

        return builder.Build();
    }

    public static IUofConfiguration SingleLanguage => GetUofConfiguration(TestData.Culture, null, 1, "AnyToken");

    public static IUofConfiguration TwoLanguages => GetUofConfiguration(TestData.Culture, [TestData.Culture, TestData.CultureNl], 1, "AnyToken");
}
