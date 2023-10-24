/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// Ignore Spelling: Uof

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Api.Internal.Replay;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;
using CacheMessageProcessor = Sportradar.OddsFeed.SDK.Entities.Internal.CacheMessageProcessor;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

[SuppressMessage("ReSharper", "TooManyChainedReferences")]
public class UofSdkBootstrapTests : UofSdkBootstrapBase
{
    [Fact]
    public void UofSdkCanNotBeAddedWithoutServices()
    {
        Assert.Throws<ArgumentNullException>(() => UofSdkBootstrap.AddUofSdkServices(null, UofConfig));
    }

    [Fact]
    public void UofSdkCanNotBeAddedWithoutConfiguration()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddUofSdkServices(null));
    }

    [Fact]
    public void UofSdkWithoutLoggingWillBeAdded()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddUofSdkServices(UofConfig);

        Assert.Contains(services, x => x.ServiceType == typeof(ILoggerFactory));
    }

    [Fact]
    public void UofSdkConfigurationIsSingleton()
    {
        CheckSingletonType<IUofConfiguration>();
    }

    [Fact]
    public void SequenceGeneratorIsSingleton()
    {
        CheckSingletonType<ISequenceGenerator>();
    }

    [Fact]
    public void ApiDeserializerForResponseIsSingleton()
    {
        CheckSingletonType<IDeserializer<response>>();
    }

    [Fact]
    public void MessageDataExtractorIsSingleton()
    {
        CheckSingletonType<IMessageDataExtractor>();
    }

    [Fact]
    public void EntityTypeMapperIsSingleton()
    {
        CheckSingletonType<IEntityTypeMapper>();
    }

    [Fact]
    public void DispatcherStoreIsSingleton()
    {
        CheckSingletonType<IDispatcherStore>();
    }

    [Fact]
    public void SemaphorePoolIsSingleton()
    {
        CheckSingletonType<ISemaphorePool>();
    }

    [Fact]
    public void SemaphorePoolDefaultSize()
    {
        var semaphorePool = (SemaphorePool)ServiceScope1.ServiceProvider.GetRequiredService<ISemaphorePool>();
        Assert.NotNull(semaphorePool);
        Assert.Equal(SdkInfo.SemaphorePoolSize, semaphorePool.SemaphoreHolders.Count);
    }

    [Fact]
    public void ApiBookmakerDetailsDeserializerIsSingleton()
    {
        CheckSingletonType<IDeserializer<bookmaker_details>>();
    }

    [Fact]
    public void MapperFactoryForBookmakerDetailsIsSingleton()
    {
        CheckSingletonType<ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDto>>();
    }

    [Fact]
    public void DataProviderForBookmakerDetailsIsSingleton()
    {
        CheckSingletonType<IDataProvider<BookmakerDetailsDto>>();
    }

    [Fact]
    public void ProviderForBookmakerDetailsIsSingleton()
    {
        CheckSingletonType<IBookmakerDetailsProvider>();
    }

    [Fact]
    public void CacheManagerIsSingleton()
    {
        CheckSingletonType<ICacheManager>();
    }

    [Fact]
    public void MemoryCacheIsSingleton()
    {
        CheckSingletonType<IMemoryCache>(false);
    }

    [Fact]
    public void AllMemoryCachesAreRegistered()
    {
        var memoryCaches = ServiceScope1.ServiceProvider.GetServices<IMemoryCache>().ToList();
        Assert.NotNull(memoryCaches);
        Assert.NotEmpty(memoryCaches);
        Assert.Equal(9, memoryCaches.Count);
    }

    [Fact]
    public void CacheStoreIsSingleton()
    {
        CheckSingletonType<ICacheStore<string>>(false);
    }

    [Theory]
    [InlineData(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForFixtureChangeCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForIgnoreEventsTimelineCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForProfileCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventStatusCache)]
    public void AllCacheStoresAreRegistered(string cacheStoreName)
    {
        var cacheStores = ServiceScope1.ServiceProvider.GetServices<ICacheStore<string>>().ToList();
        Assert.NotNull(cacheStores);
        Assert.NotEmpty(cacheStores);
        Assert.Equal(9, cacheStores.Count);
        Assert.True(cacheStores.Count(w => w.StoreName.Equals(cacheStoreName, StringComparison.OrdinalIgnoreCase)) == 1);
    }

    [Theory]
    [InlineData(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache, UofSdkBootstrap.CacheStoreNameForVariantDescriptionListCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache, UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForInvariantMarketDescriptionsCache, UofSdkBootstrap.CacheStoreNameForProfileCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForSportEventStatusCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForProfileCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForFixtureChangeCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForIgnoreEventsTimelineCache)]
    [InlineData(UofSdkBootstrap.CacheStoreNameForSportEventCache, UofSdkBootstrap.CacheStoreNameForVariantMarketDescriptionCache)]
    public void CacheStoresUsesUniqueMemoryCache(string cacheStoreName1, string cacheStoreName2)
    {
        var cacheStore1 = ServiceScope1.ServiceProvider.GetServices<ICacheStore<string>>().First(w => w.StoreName.Equals(cacheStoreName1, StringComparison.OrdinalIgnoreCase));
        var cacheStore2 = ServiceScope1.ServiceProvider.GetServices<ICacheStore<string>>().First(w => w.StoreName.Equals(cacheStoreName2, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(cacheStore1);
        Assert.NotNull(cacheStore2);
        Assert.NotSame(cacheStore1, cacheStore2);
        Assert.NotStrictEqual(cacheStore1, cacheStore2);
        Assert.Equal(0, cacheStore1.Size());
        Assert.Equal(0, cacheStore2.Size());

        cacheStore1.Add("some-key", 111.ToString());
        Assert.Equal(1, cacheStore1.Size());
        Assert.Equal(0, cacheStore2.Size());
    }

    [Fact]
    public void SdkTimerIsSingleton()
    {
        CheckSingletonType<ISdkTimer>(false);
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimerForSportEventCache)]
    [InlineData(UofSdkBootstrap.TimerForSportDataCache)]
    [InlineData(UofSdkBootstrap.TimerForNamedValueCache)]
    [InlineData(UofSdkBootstrap.TimerForLocalizedNamedValueCache)]
    [InlineData(UofSdkBootstrap.TimerForInvariantMarketDescriptionsCache)]
    [InlineData(UofSdkBootstrap.TimerForVariantMarketDescriptionListCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager)]
    [InlineData(UofSdkBootstrap.TimerForRabbitChannel)]
    public void AllSdkTimersAreRegistered(string timerName)
    {
        var sdkTimers = ServiceScope1.ServiceProvider.GetServices<ISdkTimer>().ToList();
        Assert.NotNull(sdkTimers);
        Assert.NotEmpty(sdkTimers);
        Assert.Equal(8, sdkTimers.Count);
        Assert.True(sdkTimers.Count(w => w.TimerName.Equals(timerName, StringComparison.OrdinalIgnoreCase)) == 1);
    }

    [Theory]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForInvariantMarketDescriptionsCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForVariantMarketDescriptionListCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForLocalizedNamedValueCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForNamedValueCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForRabbitChannel)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForSportEventCache)]
    [InlineData(UofSdkBootstrap.TimerForFeedRecoveryManager, UofSdkBootstrap.TimerForSportDataCache)]
    [InlineData(UofSdkBootstrap.TimerForSportEventCache, UofSdkBootstrap.TimerForSportDataCache)]
    [InlineData(UofSdkBootstrap.TimerForInvariantMarketDescriptionsCache, UofSdkBootstrap.TimerForVariantMarketDescriptionListCache)]
    public void RegisteredSdkTimersUsesUniqueTimers(string timerName1, string timerName2)
    {
        var sdkTimer1 = ServiceScope1.ServiceProvider.GetServices<ISdkTimer>().First(w => w.TimerName.Equals(timerName1, StringComparison.OrdinalIgnoreCase));
        var sdkTimer2 = ServiceScope1.ServiceProvider.GetServices<ISdkTimer>().First(w => w.TimerName.Equals(timerName2, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(sdkTimer1);
        Assert.NotNull(sdkTimer2);
        Assert.NotSame(sdkTimer1, sdkTimer2);
        Assert.NotStrictEqual(sdkTimer1, sdkTimer2);
    }

    [Fact]
    public void ConfiguredConnectionFactoryIsSingleton()
    {
        CheckSingletonType<ConfiguredConnectionFactory>();
    }

    [Fact]
    public void ConnectionValidatorIsSingleton()
    {
        CheckSingletonType<ConnectionValidator>();
    }

    [Fact]
    public void ProducerManagerDeserializerIsSingleton()
    {
        CheckSingletonType<IDeserializer<producers>>();
    }

    [Fact]
    public void ProducerManagerDataProviderIsSingleton()
    {
        CheckSingletonType<IDataProvider<producers>>();
    }

    [Fact]
    public void ProducerManagerProducerProviderIsSingleton()
    {
        CheckSingletonType<IProducersProvider>();
    }

    [Fact]
    public void ProducerManagerIsSingleton()
    {
        CheckSingletonType<IProducerManager>();

        var producerManager = ServiceScope1.ServiceProvider.GetRequiredService<IProducerManager>();
        Assert.NotNull(producerManager);
        Assert.NotEmpty(producerManager.Producers);
    }

    [Fact]
    public void SportEntityFactoryIsSingleton()
    {
        CheckSingletonType<ISportEntityFactory>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<ISportEntityFactory>();
        Assert.NotNull(service);
    }

    [Fact]
    public void DataRouterManagerIsSingleton()
    {
        CheckSingletonType<IDataRouterManager>();
    }

    [Fact]
    public void DeserializerForFeedMessagesIsSingleton()
    {
        CheckSingletonType<IDeserializer<FeedMessage>>();
    }

    [Fact]
    public void RoutingKeyParserIsSingleton()
    {
        CheckSingletonType<IRoutingKeyParser>();
    }

    [Fact]
    public void EventRecoveryRequestIssuerIsSingleton()
    {
        CheckSingletonType<IEventRecoveryRequestIssuer>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IEventRecoveryRequestIssuer>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<RecoveryRequestIssuer>(service);
    }

    [Fact]
    public void RecoveryRequestIssuerIsSingleton()
    {
        CheckSingletonType<IRecoveryRequestIssuer>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IRecoveryRequestIssuer>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<RecoveryRequestIssuer>(service);
    }

    [Fact]
    public void MappingValidatorFactoryIsSingleton()
    {
        CheckSingletonType<IMappingValidatorFactory>();
    }

    [Fact]
    public void RabbitChannelIsScoped()
    {
        CheckScopedType<IRabbitMqChannel>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IRabbitMqChannel>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<RabbitMqChannel>(service);
    }

    [Fact]
    public void MessageReceiverIsScoped()
    {
        CheckScopedType<IMessageReceiver>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IMessageReceiver>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<RabbitMqMessageReceiver>(service);
    }

    [Fact]
    public void FeedMessageValidatorIsSingleton()
    {
        CheckSingletonType<IFeedMessageValidator>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IFeedMessageValidator>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<FeedMessageValidator>(service);
    }

    [Fact]
    public void FeedMessageMapperIsSingleton()
    {
        CheckSingletonType<IFeedMessageMapper>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IFeedMessageMapper>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<FeedMessageMapper>(service);
    }

    [Fact]
    public void FeedMessageHandlerIsSingleton()
    {
        CheckSingletonType<IFeedMessageHandler>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IFeedMessageHandler>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<FeedMessageHandler>(service);
    }

    [Fact]
    public void FeedMessageProcessorIsScoped()
    {
        CheckScopedType<IFeedMessageProcessor>(false);
    }

    [Fact]
    public void CacheMessageProcessorIsScoped()
    {
        CheckScopedType<IFeedMessageProcessor>(false);

        var services = ServiceScope1.ServiceProvider.GetServices<IFeedMessageProcessor>().ToList();
        Assert.NotNull(services);
        Assert.Equal(2, services.Count);
        Assert.Contains(typeof(CacheMessageProcessor), services.Select(s => s.GetType()));
    }

    [Fact]
    public void SessionMessageManagerIsScoped()
    {
        CheckScopedType<IFeedMessageProcessor>(false);

        var services = ServiceScope1.ServiceProvider.GetServices<IFeedMessageProcessor>().ToList();
        Assert.NotNull(services);
        Assert.Equal(2, services.Count);
        Assert.Contains(typeof(SessionMessageManager), services.Select(s => s.GetType()));
    }

    [Fact]
    public void CompositeMessageProcessorIsScoped()
    {
        CheckScopedType<CompositeMessageProcessor>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<CompositeMessageProcessor>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<CompositeMessageProcessor>(service);
    }

    [Fact]
    public void ProducerRecoveryManagerFactoryIsSingleton()
    {
        CheckSingletonType<IProducerRecoveryManagerFactory>();
    }

    [Fact]
    public void FeedRecoveryManagerIsSingleton()
    {
        CheckSingletonType<IFeedRecoveryManager>();
    }

    [Fact]
    public void FactoryForFeedRecoveryManagerIsSingleton()
    {
        CheckSingletonType<IAbstractFactory<IFeedRecoveryManager>>();
    }

    [Fact]
    public void FuncForFeedRecoveryManagerIsSingleton()
    {
        CheckSingletonType<Func<IFeedRecoveryManager>>();
    }

    [Fact]
    public void SportDataProviderIsSingleton()
    {
        CheckSingletonType<ISportDataProvider>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<ISportDataProvider>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<SportDataProvider>(service);
    }

    [Fact]
    public void BookingManagerIsSingleton()
    {
        CheckSingletonType<IBookingManager>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IBookingManager>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<BookingManager>(service);
    }

    [Fact]
    public void MarketDescriptionManagerIsSingleton()
    {
        CheckSingletonType<IMarketDescriptionManager>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IMarketDescriptionManager>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<MarketDescriptionManager>(service);
    }

    [Fact]
    public void CustomBetSelectionBuilderIsScoped()
    {
        CheckScopedType<ICustomBetSelectionBuilder>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<ICustomBetSelectionBuilder>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<CustomBetSelectionBuilder>(service);
    }

    [Fact]
    public void CustomBetManagerIsScoped()
    {
        CheckScopedType<ICustomBetManager>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<ICustomBetManager>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<CustomBetManager>(service);
    }

    [Fact]
    public void EventChangeManagerIsSingleton()
    {
        CheckSingletonType<IEventChangeManager>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IEventChangeManager>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<EventChangeManager>(service);
    }

    [Fact]
    public void ReplayManagerIsSingleton()
    {
        CheckSingletonType<IReplayManager>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IReplayManager>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ReplayManager>(service);
    }

    [Fact]
    public void CashoutDeserializerIsSingleton()
    {
        CheckSingletonType<IDeserializer<cashout>>();
    }

    [Fact]
    public void CashoutDataProviderIsSingleton()
    {
        CheckSingletonType<IDataProvider<cashout>>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<IDataProvider<cashout>>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<NonMappingDataProvider<cashout>>(service);
    }

    [Fact]
    public void CashOutProbabilitiesProviderIsSingleton()
    {
        CheckSingletonType<ICashOutProbabilitiesProvider>();

        var service = ServiceScope1.ServiceProvider.GetRequiredService<ICashOutProbabilitiesProvider>();
        Assert.NotNull(service);
        Assert.IsAssignableFrom<CashOutProbabilitiesProvider>(service);
    }

    [Fact]
    public void UofSessionCreatesUniquePipeline() // how to test that
    {
        var uofSdk = new UofSdk(ServiceCollection.BuildServiceProvider());
        Assert.NotNull(uofSdk);

        var session1 = uofSdk.GetSessionBuilder().SetMessageInterest(MessageInterest.LiveMessagesOnly).Build();
        var session2 = uofSdk.GetSessionBuilder().SetMessageInterest(MessageInterest.PrematchMessagesOnly).Build();

        Assert.NotNull(session1);
        Assert.NotNull(session2);
        Assert.NotEqual(session1, session2);
    }

    [Theory]
    [InlineData("ProfileCache")]
    [InlineData("SportDataCache")]
    [InlineData("SportEventCache")]
    [InlineData("SportEventStatusCache")]
    [InlineData("VariantDescriptionCacheSingle")]
    [InlineData("InvariantDescriptionCacheList")]
    [InlineData("VariantDescriptionCacheList")]
    public async Task HealthChecksAreRegistered(string registeredHealthCheck)
    {
        var healthCheckService = ServiceScope1.ServiceProvider.GetService<HealthCheckService>();

        Assert.NotNull(healthCheckService);

        var healthStatus = await healthCheckService.CheckHealthAsync().ConfigureAwait(false);

        Assert.Contains(registeredHealthCheck, healthStatus.Entries.Keys);
    }

    [Fact]
    public void TelemetryMeterIsRegistered()
    {
        var exportedItems = new List<Metric>();
        ServiceCollection.AddOpenTelemetry().WithMetrics(config => config.AddInMemoryExporter(exportedItems));

        var meterProvider = ServiceCollection.BuildServiceProvider().GetRequiredService<MeterProvider>();
        Assert.NotNull(meterProvider);

        UofSdkTelemetry.SportDataCacheGetAll.Record(1234);

        meterProvider.ForceFlush();

        Assert.NotEmpty(exportedItems);
        Assert.NotEmpty(exportedItems.Where(w => w.MeterName.Equals(UofSdkTelemetry.ServiceName)));
        Assert.Single(exportedItems.Where(w => w.Name.Contains("sportdatacache-getall", StringComparison.OrdinalIgnoreCase)));
    }
}
