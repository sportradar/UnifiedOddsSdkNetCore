// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Authentication;
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
using ZiggyCreatures.Caching.Fusion;

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
        Assert.IsType<SportEventCache>(service1, false);
    }

    [Fact]
    public void SportDataCacheIsSingleton()
    {
        CheckSingletonType<ISportDataCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISportDataCache>();
        Assert.NotNull(service1);
        Assert.IsType<SportDataCache>(service1, false);
    }

    [Fact]
    public void SportEventStatusCacheIsSingleton()
    {
        CheckSingletonType<ISportEventStatusCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISportEventStatusCache>();
        Assert.NotNull(service1);
        Assert.IsType<SportEventStatusCache>(service1, false);
    }

    [Fact]
    public void SportEventStatusMapperFactoryIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>>();
        Assert.NotNull(service1);
        Assert.IsType<SportEventStatusMapperFactory>(service1, false);
    }

    [Fact]
    public void NamedValueCacheVoidReasonIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForVoidReason);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForVoidReason);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueCache>(service1, false);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForVoidReason, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheBetStopReasonIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBetStopReason);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBetStopReason);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueCache>(service1, false);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForBetStopReason, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheBettingStatusIsSingleton()
    {
        CheckSingletonNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBettingStatus);

        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForBettingStatus);
        Assert.NotNull(service1);
        Assert.IsType<NamedValueCache>(service1, false);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForBettingStatus, service1.CacheName);
    }

    [Fact]
    public void NamedValueCacheMatchStatusIsSingleton()
    {
        CheckSingletonLocalizedNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForMatchStatus);

        var service1 = ServiceScope1.ServiceProvider.GetLocalizedNamedValueCache(UofSdkBootstrap.NamedValueCacheNameForMatchStatus);
        Assert.NotNull(service1);
        Assert.IsType<LocalizedNamedValueCache>(service1, false);
        Assert.Equal(UofSdkBootstrap.NamedValueCacheNameForMatchStatus, service1.CacheName);
    }

    [Fact]
    public void OperandFactoryIsSingleton()
    {
        CheckSingletonType<IOperandFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IOperandFactory>();
        Assert.NotNull(service1);
        Assert.IsType<OperandFactory>(service1, false);
    }

    [Fact]
    public void NameExpressionFactoryIsSingleton()
    {
        CheckSingletonType<INameExpressionFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<INameExpressionFactory>();
        Assert.NotNull(service1);
        Assert.IsType<NameExpressionFactory>(service1, false);
    }

    [Fact]
    public void NameProviderFactoryIsSingleton()
    {
        CheckSingletonType<INameProviderFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<INameProviderFactory>();
        Assert.NotNull(service1);
        Assert.IsType<NameProviderFactory>(service1, false);
    }

    [Fact]
    public void MarketMappingProviderFactoryIsSingleton()
    {
        CheckSingletonType<IMarketMappingProviderFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketMappingProviderFactory>();
        Assert.NotNull(service1);
        Assert.IsType<MarketMappingProviderFactory>(service1, false);
    }

    [Fact]
    public void MarketCacheProviderIsSingleton()
    {
        CheckSingletonType<IMarketCacheProvider>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketCacheProvider>();
        Assert.NotNull(service1);
        Assert.IsType<MarketCacheProvider>(service1, false);
    }

    [Fact]
    public void InvariantMarketDescriptionCacheIsSingleton()
    {
        CheckSingletonType<IMarketDescriptionsCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketDescriptionsCache>();
        Assert.NotNull(service1);
        Assert.IsType<InvariantMarketDescriptionCache>(service1, false);
    }

    [Fact]
    public void VariantMarketDescriptionCacheIsSingleton()
    {
        CheckSingletonType<IMarketDescriptionCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IMarketDescriptionCache>();
        Assert.NotNull(service1);
        Assert.IsType<VariantMarketDescriptionCache>(service1, false);
    }

    [Fact]
    public void VariantDescriptionListCacheIsSingleton()
    {
        CheckSingletonType<IVariantDescriptionsCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IVariantDescriptionsCache>();
        Assert.NotNull(service1);
        Assert.IsType<VariantDescriptionListCache>(service1, false);
    }

    [Fact]
    public void ProfileCacheIsSingleton()
    {
        CheckSingletonType<IProfileCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IProfileCache>();
        Assert.NotNull(service1);
        Assert.IsType<ProfileCache>(service1, false);
    }

    [Fact]
    public void AuthenticationTokenCacheIsSingleton()
    {
        CheckSingletonType<IAuthenticationTokenCache>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IAuthenticationTokenCache>();
        Assert.NotNull(service1);
        Assert.IsType<AuthenticationTokenCache>(service1, false);
    }

    [Fact]
    public void JsonWebTokenFactoryIsSingleton()
    {
        CheckSingletonType<IJsonWebTokenFactory>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IJsonWebTokenFactory>();
        Assert.NotNull(service1);
        Assert.IsType<JsonWebTokenFactory>(service1, false);
    }

    [Fact]
    public void AuthenticationClientIsSingleton()
    {
        CheckSingletonType<IAuthenticationClient>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IAuthenticationClient>();
        Assert.NotNull(service1);
        Assert.IsType<AuthenticationClient>(service1, false);
    }

    [Fact]
    public void AuthenticationDelegatingHandlerIsTransient()
    {
        CheckTransientType<AuthenticationDelegatingHandler>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<AuthenticationDelegatingHandler>();
        Assert.NotNull(service1);
        Assert.IsType<AuthenticationDelegatingHandler>(service1, false);
    }

    [Fact]
    public void FusionCacheProviderIsSingleton()
    {
        CheckSingletonType<IFusionCacheProvider>();

        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<IFusionCacheProvider>();
        Assert.NotNull(service1);
        Assert.IsType<IFusionCacheProvider>(service1, false);
    }

    [Fact]
    public void FusionCacheForAuthenticationCacheIsRegistered()
    {
        var fusionCacheProvider = ServiceScope1.ServiceProvider.GetRequiredService<IFusionCacheProvider>();

        var fusionCache = fusionCacheProvider.GetCache(AuthenticationTokenCache.FusionCacheInstanceName);

        fusionCache.ShouldNotBeNull();
        fusionCache.ShouldBeAssignableTo<IFusionCache>();
    }
}
