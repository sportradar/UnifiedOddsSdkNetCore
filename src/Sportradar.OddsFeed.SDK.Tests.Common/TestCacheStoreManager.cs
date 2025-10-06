// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestCacheStoreManager
{
    internal IServiceProvider ServiceProvider { get; }

    internal CacheManager CacheManager { get; }

    internal UofConfiguration UofConfig { get; }

    internal IProducersProvider ProducersProvider { get; }

    public TestCacheStoreManager()
    {
        ProducersProvider = new TestProducersProvider();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddUofSdkServices(TestConfiguration.GetConfig());
        serviceCollection.AddScoped(_ => ProducersProvider);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        CacheManager = (CacheManager)ServiceProvider.GetRequiredService<ICacheManager>();
        UofConfig = (UofConfiguration)ServiceProvider.GetRequiredService<IUofConfiguration>();
    }
}
