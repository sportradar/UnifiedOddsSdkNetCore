// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class UofSdkBootstrapCacheTests : UofSdkBootstrapBase
{
    [Fact]
    public void SportEventCacheItemFactoryIsSingleton()
    {
        CheckSingletonType<ISportEventCacheItemFactory>();
    }

    [Fact]
    public void SportEventCacheIsSingleton()
    {
        CheckSingletonType<ISportEventCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISportEventCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<SportEventCache>(service1);
    }

    [Fact]
    public void SportDataCacheIsSingleton()
    {
        CheckSingletonType<ISportDataCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISportDataCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<SportDataCache>(service1);
    }

    [Fact]
    public void SportEventStatusCacheIsSingleton()
    {
        CheckSingletonType<ISportEventStatusCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISportEventStatusCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<SportEventStatusCache>(service1);
    }

    [Fact]
    public void SportEventStatusMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<SportEventStatusMapperFactory>(service1);
    }

    [Fact]
    public void NamedValueCacheVoidReasonIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForVoidReason);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForVoidReason);
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<NamedValueCache>(service1);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForVoidReason, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheBetStopReasonIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBetStopReason);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBetStopReason);
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<NamedValueCache>(service1);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForBetStopReason, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheBettingStatusIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBettingStatus);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBettingStatus);
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<NamedValueCache>(service1);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForBettingStatus, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheMatchStatusIsSingleton()
    {
        CheckSingletonLocalizedNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForMatchStatus);

        var service1 = ServiceScope1.ServiceProvider.GetLocalizedNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForMatchStatus);
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<LocalizedNamedValueCache>(service1);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForMatchStatus, service1.CacheName);
    }

    [Fact]
    public void OperandFactoryIsSingleton()
    {
        CheckSingletonType<IOperandFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IOperandFactory>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<OperandFactory>(service1);
    }

    [Fact]
    public void NameExpressionFactoryIsSingleton()
    {
        CheckSingletonType<INameExpressionFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<INameExpressionFactory>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<NameExpressionFactory>(service1);
    }

    [Fact]
    public void NameProviderFactoryIsSingleton()
    {
        CheckSingletonType<INameProviderFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<INameProviderFactory>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<NameProviderFactory>(service1);
    }

    [Fact]
    public void MarketMappingProviderFactoryIsSingleton()
    {
        CheckSingletonType<IMarketMappingProviderFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketMappingProviderFactory>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<MarketMappingProviderFactory>(service1);
    }

    [Fact]
    public void MarketCacheProviderIsSingleton()
    {
        CheckSingletonType<IMarketCacheProvider>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketCacheProvider>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<MarketCacheProvider>(service1);
    }

    [Fact]
    public void InvariantMarketDescriptionCacheIsSingleton()
    {
        CheckSingletonType<IMarketDescriptionsCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketDescriptionsCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<InvariantMarketDescriptionCache>(service1);
    }

    [Fact]
    public void VariantMarketDescriptionCacheIsSingleton()
    {
        CheckSingletonType<IMarketDescriptionCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketDescriptionCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<VariantMarketDescriptionCache>(service1);
    }

    [Fact]
    public void VariantDescriptionListCacheIsSingleton()
    {
        CheckSingletonType<IVariantDescriptionsCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IVariantDescriptionsCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<VariantDescriptionListCache>(service1);
    }

    [Fact]
    public void ProfileCacheIsSingleton()
    {
        CheckSingletonType<IProfileCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IProfileCache>();
        Assert.NotNull(service1);
        Assert.IsAssignableFrom<ProfileCache>(service1);
    }
}
