// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Config;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class UofSdkTests
{
    [Fact]
    public void UofSdkIsConstructedWithNormalConfig()
    {
        var uofSdk = ConstructUofSdkWithConfig(TestConfiguration.GetConfig());

        Assert.NotNull(uofSdk);
    }

    [Fact]
    public void UofSdkIsConstructedWithCustomConfig()
    {
        var defaultConfig = TestConfiguration.GetConfig();
        var usageConfig = new UofUsageConfigurationStub(false, defaultConfig.Usage.Host, 20, 10);
        var customConfig = new UofConfigurationStub(defaultConfig, usageConfig);

        var uofSdk = ConstructUofSdkWithConfig(customConfig);

        Assert.NotNull(uofSdk);
    }

    private static IUofSdk ConstructUofSdkWithConfig(IUofConfiguration uofConfig)
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(uofConfig);

        var serviceProvider = serviceCollection.BuildServiceProvider(true);

        return new UofSdk(serviceProvider);
    }
}
