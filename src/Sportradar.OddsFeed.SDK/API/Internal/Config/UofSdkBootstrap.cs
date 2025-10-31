// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Handlers;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Api.Internal.Replay;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Api.Replay;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    [SuppressMessage("CodeQuality", "IDE0058:Expression value is never used", Justification = "Allowed for registering services")]
    internal static class UofSdkBootstrap
    {
        internal const string HttpClientDefaultRequestHeaderForAccessToken = "x-access-token";
        internal const string HttpClientDefaultRequestHeaderForUserAgent = "User-Agent";
        internal const string HttpClientNameForFastFailing = "HttpClientFastFailing";
        internal const string HttpClientNameForRecovery = "HttpClientRecovery";
        internal const string HttpClientNameForNormal = "HttpClientNormal";

        internal const string CacheStoreNameForInvariantMarketDescriptionsCache = "MemoryCacheForInvariantMarketDescriptionsCache";
        internal const string CacheStoreNameForVariantMarketDescriptionCache = "MemoryCacheForVariantMarketDescriptionCache";
        internal const string CacheStoreNameForVariantDescriptionListCache = "MemoryCacheForVariantDescriptionListCache";
        internal const string CacheStoreNameForFixtureChangeCache = "MemoryCacheForFixtureChangeCache";
        internal const string CacheStoreNameForSportEventStatusCache = "MemoryCacheForSportEventStatusCache";
        internal const string CacheStoreNameForIgnoreEventsTimelineCache = "MemoryCacheForIgnoreEventsTimelineCache";
        internal const string CacheStoreNameForSportEventCache = "MemoryCacheForSportEventCache";
        internal const string CacheStoreNameForSportEventCacheFixtureTimestampCache = "MemoryCacheForSportEventCacheFixtureTimestampCache";
        internal const string CacheStoreNameForProfileCache = "MemoryCacheForProfileCache";

        internal const string NamedValueCacheNameForVoidReason = "VoidReason";
        internal const string NamedValueCacheNameForBetStopReason = "BetStopReason";
        internal const string NamedValueCacheNameForBettingStatus = "BettingStatus";
        internal const string NamedValueCacheNameForMatchStatus = "MatchStatus";

        internal const string TimerForNamedValueCache = "TimerForNamedValueCache";
        internal const string TimerForLocalizedNamedValueCache = "TimerForLocalizedNamedValueCache";
        internal const string TimerForRabbitChannel = "TimerForRabbitChannel";
        internal const string TimerForInvariantMarketDescriptionsCache = "TimerForInvariantMarketDescriptionsCache";
        internal const string TimerForVariantMarketDescriptionListCache = "TimerForVariantMarketDescriptionListCache";
        internal const string TimerForFixtureChange = "TimerForFixtureChange";
        internal const string TimerForResultChange = "TimerForResultChange";
        internal const string TimerForSportEventCache = "TimerForSportEventCache";
        internal const string TimerForSportDataCache = "TimerForSportDataCache";
        internal const string TimerForFeedRecoveryManager = "TimerForFeedRecoveryManager";

        internal const string DataProviderForFixtureChangeFixtureEndpoint = "DataProviderFixtureChangeFixtureEndpoint";
        internal const string DataProviderForFixtureChangeFixtureEndpointForTournamentInfo = "DataProviderFixtureChangeFixtureEndpointForTournamentInfo";
        internal const string DataProviderForAllTournamentsForAllSports = "DataProviderAllTournamentsForAllSports";
        internal const string DataProviderForAllSports = "DataProviderAllSports";
        internal const string DataProviderForDateSchedule = "DataProviderDateSchedule";
        internal const string DataProviderForTournamentSchedule = "DataProviderTournamentSchedule";
        internal const string DataProviderForStageTournamentSchedule = "DataProviderStageTournamentSchedule";
        internal const string DataProviderForInvariantMarketList = "DataProviderInvariantMarketList";
        internal const string DataProviderForVariantMarketList = "DataProviderVariantMarketList";
        internal const string DataProviderForVariantMarket = "DataProviderVariantMarket";
        internal const string DataProviderForLotteryDrawSummary = "DataProviderLotteryDrawSummary";
        internal const string DataProviderForLotteryDrawFixture = "DataProviderLotteryDrawFixture";
        internal const string DataProviderForSportEventList = "DataProviderSportEventList";
        internal const string DataProviderForNamedValueCacheVoidReason = "DataProviderVoidReason";
        internal const string DataProviderForNamedValueCacheBetStopReason = "DataProviderBetStopReason";
        internal const string DataProviderForNamedValueCacheBettingStatus = "DataProviderBettingStatus";
        internal const string DataProviderForNamedValueCacheMatchStatus = "DataProviderMatchStatus";

        private const string BookmakerDetailsProviderUrl = "{0}/v1/users/whoami.xml";

        private const string NonTimeCriticalDataProviderServiceKey = "NonTimeCriticalDataProvider";
        private const string TimeCriticalDataProviderServiceKey = "TimeCriticalDataProvider";

        public static void AddUofSdkServices(this IServiceCollection services, IUofConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            RegisterServices(services, configuration);
        }

        private static void RegisterServices(IServiceCollection services, IUofConfiguration configuration)
        {
            RegisterBaseTypes(services, configuration);
            RegisterHttpClients(services, configuration);
            RegisterBookmakerDetailsProvider(services);
            RegisterCacheStores(services, configuration);
            RegisterSdkTimers(services);
            RegisterBaseRabbitClasses(services);
            RegisterFeedSessionClasses(services);
            RegisterProducerManager(services, configuration);
            RegisterApiClasses(services, configuration);
            RegisterSdkCaches(services, configuration);
            RegisterUofSdkManagerAndProviders(services);
            RegisterTelemetry(services);
            RegisterHealthChecks(services);
        }

        private static void RegisterBaseTypes(IServiceCollection services, IUofConfiguration configuration)
        {
            services.AddSingleton<ISequenceGenerator, IncrementalSequenceGenerator>(serviceProvider =>
                                                                                        new IncrementalSequenceGenerator(logger: serviceProvider.GetRequiredService<ILogger<IncrementalSequenceGenerator>>(),
                                                                                                                         minValue: -1));

            services.AddSingleton(configuration);

            services.AddSingleton<IRequestDecorator, RequestDecorator>();
            services.AddSingleton<IMessageDataExtractor, MessageDataExtractor>();
            services.AddSingleton<IEntityTypeMapper, EntityTypeMapper>();
            services.AddSingleton<IDispatcherStore, DispatcherStore>();
            services.AddSingleton<ISemaphorePool, SemaphorePool>(serviceProvider =>
                                                                     new SemaphorePool(SdkInfo.SemaphorePoolSize, configuration.ExceptionHandlingStrategy, serviceProvider.GetRequiredService<ILogger<SemaphorePool>>()));

            services.AddSingleton<ISportEntityFactory, SportEntityFactory>(serviceProvider =>
                                                                               new SportEntityFactory(serviceProvider.GetRequiredService<ISportDataCache>(),
                                                                                                      serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                                      serviceProvider.GetRequiredService<ISportEventStatusCache>(),
                                                                                                      serviceProvider.GetLocalizedNamedValueCache(NamedValueCacheNameForMatchStatus),
                                                                                                      serviceProvider.GetRequiredService<IProfileCache>()));
        }

        private static void RegisterHttpClients(IServiceCollection services, IUofConfiguration configuration)
        {
            var userAgentData = string.Intern($"UfSdk-{SdkInfo.SdkType}/{SdkInfo.GetVersion()} (OS: {Environment.OSVersion}, NET: {Environment.Version}, Init: {DateTime.UtcNow:yyyyMMddHHmm})");

            services.AddHttpClient(HttpClientNameForNormal)
                    .ConfigureHttpClient(configureClient =>
                                         {
                                             configureClient.Timeout = configuration.Api.HttpClientTimeout;
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForAccessToken, configuration.AccessToken);
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForUserAgent, userAgentData);
                                         })
                    .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
                                                            new HttpRequestDecoratorHandler(serviceProvider.GetRequiredService<IRequestDecorator>(),
                                                                                            new HttpClientHandler
                                                                                            {
                                                                                                MaxConnectionsPerServer = configuration.Api.MaxConnectionsPerServer,
                                                                                                AllowAutoRedirect = true
                                                                                            }))
                    .SetHandlerLifetime(TimeSpan.FromMinutes(10));
            services.AddHttpClient(HttpClientNameForRecovery)
                    .ConfigureHttpClient(configureClient =>
                                         {
                                             configureClient.Timeout = configuration.Api.HttpClientRecoveryTimeout;
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForAccessToken, configuration.AccessToken);
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForUserAgent, userAgentData);
                                         })
                    .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
                                                            new HttpRequestDecoratorHandler(serviceProvider.GetRequiredService<IRequestDecorator>(),
                                                                                            new HttpClientHandler
                                                                                            {
                                                                                                MaxConnectionsPerServer = configuration.Api.MaxConnectionsPerServer,
                                                                                                AllowAutoRedirect = true
                                                                                            }))
                    .SetHandlerLifetime(TimeSpan.FromMinutes(10));
            services.AddHttpClient(HttpClientNameForFastFailing)
                    .ConfigureHttpClient(configureClient =>
                                         {
                                             configureClient.Timeout = configuration.Api.HttpClientFastFailingTimeout;
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForAccessToken, configuration.AccessToken);
                                             configureClient.DefaultRequestHeaders.Add(HttpClientDefaultRequestHeaderForUserAgent, userAgentData);
                                         })
                    .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
                                                            new HttpRequestDecoratorHandler(serviceProvider.GetRequiredService<IRequestDecorator>(),
                                                                                            new HttpClientHandler
                                                                                            {
                                                                                                MaxConnectionsPerServer = configuration.Api.MaxConnectionsPerServer,
                                                                                                AllowAutoRedirect = true
                                                                                            }))
                    .SetHandlerLifetime(TimeSpan.FromMinutes(10));

            services.AddTransient<ISdkHttpClient, SdkHttpClient>(serviceProvider => new SdkHttpClient(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientNameForNormal));
            services.AddTransient<ISdkHttpClientRecovery, SdkHttpClientRecovery>(serviceProvider => new SdkHttpClientRecovery(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientNameForRecovery));
            services.AddTransient<ISdkHttpClientFastFailing, SdkHttpClientFastFailing>(serviceProvider => new SdkHttpClientFastFailing(serviceProvider.GetRequiredService<IHttpClientFactory>(), HttpClientNameForFastFailing));

            var restTrafficLoggerName = SdkLoggerFactory.SdkLogRepositoryName + "." + Enum.GetName(typeof(LoggerType), LoggerType.RestTraffic);

            services.AddSingleton<IDataFetcher, LogHttpDataFetcher>(serviceProvider =>
                                                                        new LogHttpDataFetcher(serviceProvider.GetRequiredService<ISdkHttpClient>(),
                                                                                               serviceProvider.GetRequiredService<IDeserializer<response>>(),
                                                                                               serviceProvider.GetService<ILoggerFactory>().CreateLogger(restTrafficLoggerName)));

            services.AddSingleton<IDataPoster, LogHttpDataFetcherRecovery>(serviceProvider =>
                                                                               new LogHttpDataFetcherRecovery(serviceProvider.GetRequiredService<ISdkHttpClientRecovery>(),
                                                                                                              serviceProvider.GetRequiredService<IDeserializer<response>>(),
                                                                                                              serviceProvider.GetService<ILoggerFactory>().CreateLogger(restTrafficLoggerName)));

            services.AddSingleton<IDataRestful, HttpDataRestful>(serviceProvider =>
                                                                     new HttpDataRestful(serviceProvider.GetRequiredService<ISdkHttpClient>(),
                                                                                         serviceProvider.GetRequiredService<IDeserializer<response>>()));

            services.AddSingleton<ILogHttpDataFetcher, LogHttpDataFetcher>(serviceProvider =>
                                                                               new LogHttpDataFetcher(serviceProvider.GetRequiredService<ISdkHttpClient>(),
                                                                                                      serviceProvider.GetRequiredService<IDeserializer<response>>(),
                                                                                                      serviceProvider.GetService<ILoggerFactory>().CreateLogger(restTrafficLoggerName)));

            services.AddSingleton<ILogHttpDataFetcherRecovery, LogHttpDataFetcherRecovery>(serviceProvider =>
                                                                                               new LogHttpDataFetcherRecovery(serviceProvider.GetRequiredService<ISdkHttpClientRecovery>(),
                                                                                                                              serviceProvider.GetRequiredService<IDeserializer<response>>(),
                                                                                                                              serviceProvider.GetService<ILoggerFactory>().CreateLogger(restTrafficLoggerName)));

            services.AddSingleton<ILogHttpDataFetcherFastFailing, LogHttpDataFetcherFastFailing>(serviceProvider =>
                                                                                                     new LogHttpDataFetcherFastFailing(serviceProvider.GetRequiredService<ISdkHttpClientFastFailing>(),
                                                                                                                                       serviceProvider.GetRequiredService<IDeserializer<response>>(),
                                                                                                                                       serviceProvider.GetService<ILoggerFactory>().CreateLogger(restTrafficLoggerName)));
        }

        private static void RegisterBookmakerDetailsProvider(IServiceCollection services)
        {
            services.AddSingleton<IDeserializer<bookmaker_details>, Deserializer<bookmaker_details>>();
            services.AddSingleton<ISingleTypeMapperFactory<bookmaker_details, BookmakerDetailsDto>, BookmakerDetailsMapperFactory>();

            services.AddSingleton<IDataProvider<BookmakerDetailsDto>, Entities.Rest.Internal.BookmakerDetailsProvider>(servicesProvider =>
                                                                                                                           new Entities.Rest.Internal.BookmakerDetailsProvider(BookmakerDetailsProviderUrl,
                                                                                                                                                                               servicesProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                                                               servicesProvider.GetRequiredService<IDeserializer<bookmaker_details>>(),
                                                                                                                                                                               servicesProvider
                                                                                                                                                                                  .GetRequiredService<ISingleTypeMapperFactory<bookmaker_details,
                                                                                                                                                                                       BookmakerDetailsDto>>()));

            services.AddSingleton<IBookmakerDetailsProvider, BookmakerDetailsProvider>();
        }

        private static void RegisterCacheStores(IServiceCollection services, IUofConfiguration configuration)
        {
            services.AddSingleton<ICacheManager, CacheManager>();

            services.AddSdkCacheStore<string>(CacheStoreNameForInvariantMarketDescriptionsCache);
            services.AddSdkCacheStore<string>(CacheStoreNameForVariantMarketDescriptionCache, null, configuration.Cache.VariantMarketDescriptionCacheTimeout, 20);
            services.AddSdkCacheStore<string>(CacheStoreNameForVariantDescriptionListCache);
            services.AddSdkCacheStore<string>(CacheStoreNameForProfileCache, null, configuration.Cache.ProfileCacheTimeout, 20); // could use 2 (also for simpleTeam)
            services.AddSdkCacheStore<string>(CacheStoreNameForFixtureChangeCache, null, TimeSpan.FromSeconds(10));
            services.AddSdkCacheStore<string>(CacheStoreNameForSportEventStatusCache, configuration.Cache.SportEventStatusCacheTimeout);
            services.AddSdkCacheStore<string>(CacheStoreNameForIgnoreEventsTimelineCache, null, configuration.Cache.IgnoreBetPalTimelineSportEventStatusCacheTimeout, 20);
            services.AddSdkCacheStore<string>(CacheStoreNameForSportEventCache, null, configuration.Cache.SportEventCacheTimeout, 20);
            services.AddSdkCacheStore<string>(CacheStoreNameForSportEventCacheFixtureTimestampCache, TimeSpan.FromMinutes(2));
        }

        private static void RegisterSdkTimers(IServiceCollection services)
        {
            // timers to automatically execute onTimer methods (populating caches, checking state, ...)
            services.AddSdkTimer(TimerForSportEventCache, TimeSpan.FromSeconds(10), TimeSpan.FromHours(24));
            services.AddSdkTimer(TimerForSportDataCache, TimeSpan.FromSeconds(10), TimeSpan.FromHours(12));
            services.AddSdkTimer(TimerForInvariantMarketDescriptionsCache, TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));
            services.AddSdkTimer(TimerForVariantMarketDescriptionListCache, TimeSpan.FromSeconds(5), TimeSpan.FromHours(6));
            services.AddSdkTimer(TimerForNamedValueCache, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            services.AddSdkTimer(TimerForLocalizedNamedValueCache, TimeSpan.FromSeconds(1), TimeSpan.Zero);
            services.AddSdkTimer(TimerForFeedRecoveryManager, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(60));
            services.AddSdkTimer(TimerForRabbitChannel, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(60));
        }

        private static void RegisterSdkCaches(IServiceCollection services, IUofConfiguration configuration)
        {
            services.AddSingleton<ISportEventCacheItemFactory, SportEventCacheItemFactory>(serviceProvider =>
                                                                                               new SportEventCacheItemFactory(serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                                                              serviceProvider.GetRequiredService<ISemaphorePool>(),
                                                                                                                              serviceProvider.GetRequiredService<IUofConfiguration>(),
                                                                                                                              serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForSportEventCacheFixtureTimestampCache)));

            RegisterNamedValueCaches(services, configuration);
            RegisterSportEventCache(services);
            RegisterSportDataCache(services);
            RegisterSportEventStatusCache(services);
            RegisterMarketCaches(services);
        }

        private static void RegisterSportEventCache(IServiceCollection services)
        {
            services.AddSingleton<ISportEventCache, SportEventCache>(serviceProvider =>
                                                                         new SportEventCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForSportEventCache),
                                                                                             serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                             serviceProvider.GetRequiredService<ISportEventCacheItemFactory>(),
                                                                                             serviceProvider.GetSdkTimer(TimerForSportEventCache),
                                                                                             serviceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                                                                             serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                             serviceProvider.GetRequiredService<ILoggerFactory>()));
        }

        private static void RegisterSportDataCache(IServiceCollection services)
        {
            services.AddSingleton<ISportDataCache, SportDataCache>(serviceProvider =>
                                                                       new SportDataCache(serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                          serviceProvider.GetSdkTimer(TimerForSportDataCache),
                                                                                          serviceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                                                                          serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                          serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                          serviceProvider.GetRequiredService<ILoggerFactory>()));
        }

        private static void RegisterSportEventStatusCache(IServiceCollection services)
        {
            services.AddSingleton<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>, SportEventStatusMapperFactory>();

            services.AddSingleton<ISportEventStatusCache, SportEventStatusCache>(serviceProvider =>
                                                                                     new SportEventStatusCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForSportEventStatusCache),
                                                                                                               serviceProvider.GetRequiredService<ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto>>(),
                                                                                                               serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                                               serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                               serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForIgnoreEventsTimelineCache),
                                                                                                               serviceProvider.GetRequiredService<IUofConfiguration>(),
                                                                                                               serviceProvider.GetRequiredService<ILoggerFactory>()));
        }

        private static void RegisterProducerManager(IServiceCollection services, IUofConfiguration configuration)
        {
            var producerEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/producers.xml";
            services.AddSingleton<IDeserializer<producers>, Deserializer<producers>>();
            services.AddSingleton<IDataProvider<producers>, NonMappingDataProvider<producers>>(serviceProvider =>
                                                                                                   new NonMappingDataProvider<producers>(producerEndpoint,
                                                                                                                                         serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                         serviceProvider.GetRequiredService<IDeserializer<producers>>()));
            services.AddSingleton<IProducersProvider, ProducersProvider>();
            services.AddSingleton<IProducerManager, ProducerManager>();
        }

        private static void RegisterBaseRabbitClasses(IServiceCollection services)
        {
            services.AddSingleton<ConfiguredConnectionFactory, ConfiguredConnectionFactory>();
            services.AddSingleton<IChannelFactory, ChannelFactory>();
            services.AddSingleton<ConnectionValidator, ConnectionValidator>();
            services.AddSingleton<IDeserializer<FeedMessage>, Deserializer<FeedMessage>>();
            services.AddSingleton<IRoutingKeyParser, RegexRoutingKeyParser>();
            services.AddSingleton<IMappingValidatorFactory, MappingValidatorFactory>();
            services.AddSingleton<IFeedMessageHandler, FeedMessageHandler>(serviceProvider => new FeedMessageHandler(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForFixtureChangeCache)));

            services.AddSingleton<IMarketFactory, MarketFactory>(serviceProvider =>
                                                                     new MarketFactory(serviceProvider.GetRequiredService<IMarketCacheProvider>(),
                                                                                       serviceProvider.GetRequiredService<INameProviderFactory>(),
                                                                                       serviceProvider.GetRequiredService<IMarketMappingProviderFactory>(),
                                                                                       serviceProvider.GetRequiredService<INamedValuesProvider>(),
                                                                                       serviceProvider.GetNamedValueCache(NamedValueCacheNameForVoidReason),
                                                                                       serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy));

            services.AddSingleton<IFeedMessageMapper, FeedMessageMapper>(serviceProvider =>
                                                                             new FeedMessageMapper(serviceProvider.GetRequiredService<ISportEntityFactory>(),
                                                                                                   serviceProvider.GetRequiredService<IMarketFactory>(),
                                                                                                   serviceProvider.GetRequiredService<IProducerManager>(),
                                                                                                   serviceProvider.GetRequiredService<INamedValuesProvider>(),
                                                                                                   serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy));

            services.AddSingleton<IFeedMessageValidator, FeedMessageValidator>(serviceProvider =>
                                                                                   new FeedMessageValidator(serviceProvider.GetRequiredService<IMarketCacheProvider>(),
                                                                                                            serviceProvider.GetRequiredService<IUofConfiguration>().DefaultLanguage,
                                                                                                            serviceProvider.GetService<INamedValuesProvider>(),
                                                                                                            serviceProvider.GetService<IProducerManager>()));

            services.AddScoped<IRabbitMqChannel, RabbitMqChannel>(serviceProvider =>
                                                                      new RabbitMqChannel(serviceProvider.GetRequiredService<IChannelFactory>(),
                                                                                          serviceProvider.GetSdkTimer(TimerForRabbitChannel),
                                                                                          TimeSpan.FromSeconds(ConfigLimit.RabbitMaxTimeBetweenMessages),
                                                                                          serviceProvider.GetRequiredService<IUofConfiguration>().AccessToken));

            services.AddScoped<IMessageReceiver, RabbitMqMessageReceiver>();

            services.AddScoped<IFeedMessageProcessor, CacheMessageProcessor>();

            services.AddScoped<IFeedMessageProcessor, SessionMessageManager>(serviceProvider =>
                                                                                 (SessionMessageManager)serviceProvider.GetService<IAbstractFactory<IFeedRecoveryManager>>().Create().CreateSessionMessageManager());

            services.AddScoped(serviceProvider => new CompositeMessageProcessor(serviceProvider.GetServices<IFeedMessageProcessor>().ToList()));
        }

        private static void RegisterFeedSessionClasses(IServiceCollection services)
        {
            services.AddSingleton<IEventRecoveryRequestIssuer, RecoveryRequestIssuer>();
            services.AddSingleton<IRecoveryRequestIssuer, RecoveryRequestIssuer>();

            services.AddSingleton<IProducerRecoveryManagerFactory, ProducerRecoveryManagerFactory>();
            services.AddAbstractFactoryForSingleton<IFeedRecoveryManager, FeedRecoveryManager>(serviceProvider =>
                                                                                                   new FeedRecoveryManager(serviceProvider.GetService<IProducerRecoveryManagerFactory>(),
                                                                                                                           serviceProvider.GetService<IUofConfiguration>(),
                                                                                                                           serviceProvider.GetSdkTimer(TimerForFeedRecoveryManager),
                                                                                                                           serviceProvider.GetService<IProducerManager>(),
                                                                                                                           serviceProvider));
        }

        private static void RegisterUofSdkManagerAndProviders(IServiceCollection services)
        {
            services.AddSingleton<ISportDataProvider, SportDataProvider>(serviceProvider =>
                                                                             new SportDataProvider(serviceProvider.GetRequiredService<ISportEntityFactory>(),
                                                                                                   serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                                   serviceProvider.GetRequiredService<ISportEventStatusCache>(),
                                                                                                   serviceProvider.GetRequiredService<IProfileCache>(),
                                                                                                   serviceProvider.GetRequiredService<ISportDataCache>(),
                                                                                                   serviceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                                                                                   serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                                   serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                   serviceProvider.GetLocalizedNamedValueCache(NamedValueCacheNameForMatchStatus),
                                                                                                   serviceProvider.GetRequiredService<IDataRouterManager>()));

            services.AddSingleton<IBookingManager, BookingManager>();

            services.AddSingleton<IMarketDescriptionManager, MarketDescriptionManager>();

            services.AddTransient<ICustomBetSelectionBuilder, CustomBetSelectionBuilder>();

            services.AddSingleton<ICustomBetSelectionBuilderFactory, CustomBetSelectionBuilderFactory>();

            services.AddSingleton<ICustomBetManager, CustomBetManager>();

            services.AddSingleton<IEventChangeManager, EventChangeManager>(serviceProvider =>
                                                                               new EventChangeManager(TimeSpan.FromMinutes(60),
                                                                                                      TimeSpan.FromMinutes(60),
                                                                                                      serviceProvider.GetRequiredService<ISportDataProvider>(),
                                                                                                      serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                                      serviceProvider.GetRequiredService<IUofConfiguration>()));

            services.AddSingleton<IReplayManager, ReplayManager>();

            services.AddSingleton<IDeserializer<cashout>, Deserializer<cashout>>();

            services.AddSingleton<IDataProvider<cashout>, NonMappingDataProvider<cashout>>(serviceProvider =>
                                                                                               new NonMappingDataProvider<cashout>(serviceProvider.GetRequiredService<IUofConfiguration>().Api.BaseUrl + "/v1/probabilities/{0}",
                                                                                                                                   serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                   serviceProvider.GetRequiredService<IDeserializer<cashout>>()));

            services.AddSingleton<ICashOutProbabilitiesProvider, CashOutProbabilitiesProvider>();
        }

        private static void RegisterApiClasses(IServiceCollection services, IUofConfiguration configuration)
        {
            services.AddSingleton<IDeserializer<response>, Deserializer<response>>();

            services.AddSingleton<IDeserializer<scheduleEndpoint>, Deserializer<scheduleEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>, SportEventsScheduleMapperFactory>();

            RegisterDataRouterManager(services, configuration);
        }

        private static void RegisterDataRouterManager(IServiceCollection services, IUofConfiguration configuration)
        {
            var nodeIdStr = configuration.NodeId == 0 ? string.Empty : "?node_id=" + configuration.NodeId;

            // sport event summary provider
            services.AddSingleton<IDeserializer<RestMessage>, Deserializer<RestMessage>>();
            services.AddSingleton<ISingleTypeMapperFactory<RestMessage, SportEventSummaryDto>, SportEventSummaryMapperFactory>();
            var summaryEndpoint = configuration.Environment == SdkEnvironment.Replay
                                      ? configuration.Api.ReplayBaseUrl + "/sports/{1}/sport_events/{0}/summary.xml" + nodeIdStr
                                      : configuration.Api.BaseUrl + "/v1/sports/{1}/sport_events/{0}/summary.xml";
            services.AddSingleton<IDataProvider<SportEventSummaryDto>, DataProvider<RestMessage, SportEventSummaryDto>>(serviceProvider =>
                                                                                                                            new DataProvider<RestMessage, SportEventSummaryDto>(summaryEndpoint,
                                                                                                                                                                                serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                serviceProvider.GetRequiredService<IDeserializer<RestMessage>>(),
                                                                                                                                                                                serviceProvider
                                                                                                                                                                                   .GetRequiredService<ISingleTypeMapperFactory<RestMessage,
                                                                                                                                                                                        SportEventSummaryDto>>()));

            services.AddKeyedSingleton<IDataProvider<SportEventSummaryDto>, DataProvider<RestMessage, SportEventSummaryDto>>(TimeCriticalDataProviderServiceKey,
                                                                                                                             (serviceProvider, obj) =>
                                                                                                                                 new DataProvider<RestMessage, SportEventSummaryDto>(summaryEndpoint,
                                                                                                                                                                                     serviceProvider.GetRequiredService<ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                     serviceProvider.GetRequiredService<IDeserializer<RestMessage>>(),
                                                                                                                                                                                     serviceProvider
                                                                                                                                                                                        .GetRequiredService<ISingleTypeMapperFactory<RestMessage,
                                                                                                                                                                                             SportEventSummaryDto>>()));

            services.AddKeyedSingleton<IDataProvider<SportEventSummaryDto>, DataProvider<RestMessage, SportEventSummaryDto>>(NonTimeCriticalDataProviderServiceKey,
                                                                                                                             (serviceProvider, obj) =>
                                                                                                                                 new DataProvider<RestMessage, SportEventSummaryDto>(summaryEndpoint,
                                                                                                                                                                                     serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                     serviceProvider.GetRequiredService<IDeserializer<RestMessage>>(),
                                                                                                                                                                                     serviceProvider
                                                                                                                                                                                        .GetRequiredService<ISingleTypeMapperFactory<RestMessage,
                                                                                                                                                                                             SportEventSummaryDto>>()));

            // data routing provider
            services.AddSingleton<IExecutionPathDataProvider<SportEventSummaryDto>, ExecutionPathDataProvider<SportEventSummaryDto>>(serviceProvider => new ExecutionPathDataProvider<SportEventSummaryDto>(serviceProvider
                                                                                                                                                                                                               .GetRequiredKeyedService<
                                                                                                                                                                                                                    IDataProvider<
                                                                                                                                                                                                                        SportEventSummaryDto>>(TimeCriticalDataProviderServiceKey),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredKeyedService<
                                                                                                                                                                                                                    IDataProvider<
                                                                                                                                                                                                                        SportEventSummaryDto>>(NonTimeCriticalDataProviderServiceKey)));

            // sport event fixture provider
            services.AddSingleton<IDeserializer<fixturesEndpoint>, Deserializer<fixturesEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDto>, FixtureMapperFactory>();
            var fixtureEndpoint = configuration.Environment == SdkEnvironment.Replay
                                      ? configuration.Api.ReplayBaseUrl + "/sports/{1}/sport_events/{0}/fixture.xml" + nodeIdStr
                                      : configuration.Api.BaseUrl + "/v1/sports/{1}/sport_events/{0}/fixture.xml";
            services.AddSingleton<IDataProvider<FixtureDto>, DataProvider<fixturesEndpoint, FixtureDto>>(serviceProvider =>
                                                                                                             new DataProvider<fixturesEndpoint, FixtureDto>(fixtureEndpoint,
                                                                                                                                                            serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                            serviceProvider.GetRequiredService<IDeserializer<fixturesEndpoint>>(),
                                                                                                                                                            serviceProvider
                                                                                                                                                               .GetRequiredService<ISingleTypeMapperFactory<fixturesEndpoint, FixtureDto>>()));

            // fixtureChangeFixture endpoint provider
            var fixtureChangeFixtureEndpoint = configuration.Environment == SdkEnvironment.Replay
                                                   ? configuration.Api.ReplayBaseUrl + "/sports/{1}/sport_events/{0}/fixture.xml" + nodeIdStr
                                                   : configuration.Api.BaseUrl + "/v1/sports/{1}/sport_events/{0}/fixture_change_fixture.xml";
            services.AddSingleton<IDataProviderNamed<FixtureDto>, DataProviderNamed<fixturesEndpoint, FixtureDto>>(serviceProvider =>
                                                                                                                       new DataProviderNamed<fixturesEndpoint, FixtureDto>(DataProviderForFixtureChangeFixtureEndpoint,
                                                                                                                                                                           fixtureChangeFixtureEndpoint,
                                                                                                                                                                           serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                           serviceProvider.GetRequiredService<IDeserializer<fixturesEndpoint>>(),
                                                                                                                                                                           serviceProvider
                                                                                                                                                                              .GetRequiredService<ISingleTypeMapperFactory<fixturesEndpoint,
                                                                                                                                                                                   FixtureDto>>()));

            // fixture providers for tournamentInfo return data
            services.AddSingleton<IDeserializer<tournamentInfoEndpoint>, Deserializer<tournamentInfoEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<tournamentInfoEndpoint, TournamentInfoDto>, TournamentInfoMapperFactory>();
            services.AddSingleton<IDataProvider<TournamentInfoDto>, DataProvider<tournamentInfoEndpoint, TournamentInfoDto>>(serviceProvider =>
                                                                                                                                 new DataProvider<tournamentInfoEndpoint, TournamentInfoDto>(fixtureEndpoint,
                                                                                                                                                                                             serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                             serviceProvider
                                                                                                                                                                                                .GetRequiredService<IDeserializer<
                                                                                                                                                                                                     tournamentInfoEndpoint>>(),
                                                                                                                                                                                             serviceProvider
                                                                                                                                                                                                .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                     tournamentInfoEndpoint, TournamentInfoDto>>()));

            // fixtureChangeFixture endpoint provider for tournament info
            services.AddSingleton<IDataProviderNamed<TournamentInfoDto>, DataProviderNamed<tournamentInfoEndpoint, TournamentInfoDto>>(serviceProvider =>
                                                                                                                                           new DataProviderNamed<tournamentInfoEndpoint,
                                                                                                                                               TournamentInfoDto>(DataProviderForFixtureChangeFixtureEndpointForTournamentInfo,
                                                                                                                                                                  fixtureChangeFixtureEndpoint,
                                                                                                                                                                  serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                  serviceProvider.GetRequiredService<IDeserializer<tournamentInfoEndpoint>>(),
                                                                                                                                                                  serviceProvider
                                                                                                                                                                     .GetRequiredService<ISingleTypeMapperFactory<tournamentInfoEndpoint,
                                                                                                                                                                          TournamentInfoDto>>()));

            // all available tournaments for all sports
            var allTournamentsForAllSport = configuration.Api.BaseUrl + "/v1/sports/{0}/tournaments.xml";
            services.AddSingleton<IDeserializer<tournamentsEndpoint>, Deserializer<tournamentsEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<tournamentsEndpoint, EntityList<SportDto>>, TournamentsMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<SportDto>>, DataProviderNamed<tournamentsEndpoint, EntityList<SportDto>>>(serviceProvider =>
                                                                                                                                              new DataProviderNamed<tournamentsEndpoint, EntityList<SportDto>>(DataProviderForAllTournamentsForAllSports,
                                                                                                                                                                                                               allTournamentsForAllSport,
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<
                                                                                                                                                                                                                       ILogHttpDataFetcher>(),
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                       tournamentsEndpoint>>(),
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<
                                                                                                                                                                                                                       ISingleTypeMapperFactory<
                                                                                                                                                                                                                           tournamentsEndpoint, EntityList
                                                                                                                                                                                                                           <SportDto>>>()));

            // all available sports
            var allSports = configuration.Api.BaseUrl + "/v1/sports/{0}/sports.xml";
            services.AddSingleton<IDeserializer<sportsEndpoint>, Deserializer<sportsEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<sportsEndpoint, EntityList<SportDto>>, SportsMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<SportDto>>, DataProviderNamed<sportsEndpoint, EntityList<SportDto>>>(serviceProvider =>
                                                                                                                                         new DataProviderNamed<sportsEndpoint, EntityList<SportDto>>(DataProviderForAllSports,
                                                                                                                                                                                                     allSports,
                                                                                                                                                                                                     serviceProvider
                                                                                                                                                                                                        .GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                                     serviceProvider
                                                                                                                                                                                                        .GetRequiredService<IDeserializer<
                                                                                                                                                                                                             sportsEndpoint>>(),
                                                                                                                                                                                                     serviceProvider
                                                                                                                                                                                                        .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                             sportsEndpoint, EntityList<SportDto>>>()));

            // date schedule provider
            var liveSchedule = configuration.Api.BaseUrl + "/v1/sports/{0}/schedules/live/schedule.xml";
            var dateSchedule = configuration.Api.BaseUrl + "/v1/sports/{1}/schedules/{0}/schedule.xml";
            services.AddSingleton<IDataProviderNamed<EntityList<SportEventSummaryDto>>, DateScheduleProvider>(serviceProvider =>
                                                                                                                  new DateScheduleProvider(DataProviderForDateSchedule,
                                                                                                                                           liveSchedule,
                                                                                                                                           dateSchedule,
                                                                                                                                           serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                           serviceProvider.GetRequiredService<IDeserializer<scheduleEndpoint>>(),
                                                                                                                                           serviceProvider
                                                                                                                                              .GetRequiredService<ISingleTypeMapperFactory<scheduleEndpoint, EntityList<SportEventSummaryDto>>>()));

            // date schedule provider
            var tournamentSchedule = configuration.Api.BaseUrl + "/v1/sports/{1}/tournaments/{0}/schedule.xml";
            services.AddSingleton<IDeserializer<tournamentSchedule>, Deserializer<tournamentSchedule>>();
            services.AddSingleton<ISingleTypeMapperFactory<tournamentSchedule, EntityList<SportEventSummaryDto>>, TournamentScheduleMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<SportEventSummaryDto>>, DataProviderNamed<tournamentSchedule, EntityList<SportEventSummaryDto>>>(serviceProvider =>
                                                                                                                                                                     new DataProviderNamed<tournamentSchedule,
                                                                                                                                                                         EntityList<SportEventSummaryDto>>(DataProviderForTournamentSchedule,
                                                                                                                                                                                                           tournamentSchedule,
                                                                                                                                                                                                           serviceProvider
                                                                                                                                                                                                              .GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                                           serviceProvider
                                                                                                                                                                                                              .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                   tournamentSchedule>>(),
                                                                                                                                                                                                           serviceProvider
                                                                                                                                                                                                              .GetRequiredService<ISingleTypeMapperFactory
                                                                                                                                                                                                                   <tournamentSchedule, EntityList<
                                                                                                                                                                                                                       SportEventSummaryDto>>>()));

            // race schedule for stage tournament provider
            var stageTournamentSchedule = configuration.Api.BaseUrl + "/v1/sports/{1}/tournaments/{0}/schedule.xml";
            services.AddSingleton<IDeserializer<raceScheduleEndpoint>, Deserializer<raceScheduleEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>, TournamentRaceScheduleMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<SportEventSummaryDto>>, DataProviderNamed<raceScheduleEndpoint, EntityList<SportEventSummaryDto>>>(serviceProvider =>
                                                                                                                                                                       new DataProviderNamed<raceScheduleEndpoint,
                                                                                                                                                                           EntityList<SportEventSummaryDto>>(DataProviderForStageTournamentSchedule,
                                                                                                                                                                                                             stageTournamentSchedule,
                                                                                                                                                                                                             serviceProvider
                                                                                                                                                                                                                .GetRequiredService<
                                                                                                                                                                                                                     ILogHttpDataFetcher>(),
                                                                                                                                                                                                             serviceProvider
                                                                                                                                                                                                                .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                     raceScheduleEndpoint>>(),
                                                                                                                                                                                                             serviceProvider
                                                                                                                                                                                                                .GetRequiredService<
                                                                                                                                                                                                                     ISingleTypeMapperFactory<
                                                                                                                                                                                                                         raceScheduleEndpoint, EntityList<
                                                                                                                                                                                                                             SportEventSummaryDto>>>()));

            // provider for seasons for a tournament
            var tournamentSeasons = configuration.Api.BaseUrl + "/v1/sports/{1}/tournaments/{0}/seasons.xml";
            services.AddSingleton<IDeserializer<tournamentSeasons>, Deserializer<tournamentSeasons>>();
            services.AddSingleton<ISingleTypeMapperFactory<tournamentSeasons, TournamentSeasonsDto>, TournamentSeasonsMapperFactory>();
            services.AddSingleton<IDataProvider<TournamentSeasonsDto>, DataProvider<tournamentSeasons, TournamentSeasonsDto>>(serviceProvider =>
                                                                                                                                  new DataProvider<tournamentSeasons, TournamentSeasonsDto>(tournamentSeasons,
                                                                                                                                                                                            serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                               .GetRequiredService<IDeserializer<tournamentSeasons>>(),
                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                               .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                    tournamentSeasons, TournamentSeasonsDto>>()));

            // player profile provider
            var playerProfileEndpoint = configuration.Api.BaseUrl + "/v1/sports/{1}/players/{0}/profile.xml";
            services.AddSingleton<IDeserializer<playerProfileEndpoint>, Deserializer<playerProfileEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<playerProfileEndpoint, PlayerProfileDto>, PlayerProfileMapperFactory>();
            services.AddSingleton<IDataProvider<PlayerProfileDto>, DataProvider<playerProfileEndpoint, PlayerProfileDto>>(serviceProvider =>
                                                                                                                              new DataProvider<playerProfileEndpoint, PlayerProfileDto>(playerProfileEndpoint,
                                                                                                                                                                                        serviceProvider
                                                                                                                                                                                           .GetRequiredService<ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                        serviceProvider
                                                                                                                                                                                           .GetRequiredService<IDeserializer<playerProfileEndpoint>>(),
                                                                                                                                                                                        serviceProvider
                                                                                                                                                                                           .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                playerProfileEndpoint, PlayerProfileDto>>()));

            // competitor profile provider
            var competitorProfileEndpoint = configuration.Api.BaseUrl + "/v1/sports/{1}/competitors/{0}/profile.xml";
            services.AddSingleton<IDeserializer<competitorProfileEndpoint>, Deserializer<competitorProfileEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<competitorProfileEndpoint, CompetitorProfileDto>, CompetitorProfileMapperFactory>();
            services.AddSingleton<IDataProvider<CompetitorProfileDto>, DataProvider<competitorProfileEndpoint, CompetitorProfileDto>>(serviceProvider =>
                                                                                                                                          new DataProvider<competitorProfileEndpoint, CompetitorProfileDto>(competitorProfileEndpoint,
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<
                                                                                                                                                                                                                    ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                    competitorProfileEndpoint>>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<
                                                                                                                                                                                                                    ISingleTypeMapperFactory<
                                                                                                                                                                                                                        competitorProfileEndpoint,
                                                                                                                                                                                                                        CompetitorProfileDto>>()));

            // simpleTeam profile provider
            var simpleTeamProfileEndpoint = configuration.Api.BaseUrl + "/v1/sports/{1}/competitors/{0}/profile.xml";
            services.AddSingleton<IDeserializer<simpleTeamProfileEndpoint>, Deserializer<simpleTeamProfileEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<simpleTeamProfileEndpoint, SimpleTeamProfileDto>, SimpleTeamProfileMapperFactory>();
            services.AddSingleton<IDataProvider<SimpleTeamProfileDto>, DataProvider<simpleTeamProfileEndpoint, SimpleTeamProfileDto>>(serviceProvider =>
                                                                                                                                          new DataProvider<simpleTeamProfileEndpoint, SimpleTeamProfileDto>(simpleTeamProfileEndpoint,
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<
                                                                                                                                                                                                                    ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                    simpleTeamProfileEndpoint>>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<
                                                                                                                                                                                                                    ISingleTypeMapperFactory<
                                                                                                                                                                                                                        simpleTeamProfileEndpoint,
                                                                                                                                                                                                                        SimpleTeamProfileDto>>()));

            // provider for getting info about ongoing sport event (match timeline)
            var timelineEndpoint = configuration.Environment == SdkEnvironment.Replay
                                       ? configuration.Api.ReplayBaseUrl + "/sports/{1}/sport_events/{0}/timeline.xml" + nodeIdStr
                                       : configuration.Api.BaseUrl + "/v1/sports/{1}/sport_events/{0}/timeline.xml";
            services.AddSingleton<IDeserializer<matchTimelineEndpoint>, Deserializer<matchTimelineEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<matchTimelineEndpoint, MatchTimelineDto>, MatchTimelineMapperFactory>();
            services.AddSingleton<IDataProvider<MatchTimelineDto>, DataProvider<matchTimelineEndpoint, MatchTimelineDto>>(serviceProvider =>
                                                                                                                              new DataProvider<matchTimelineEndpoint, MatchTimelineDto>(timelineEndpoint,
                                                                                                                                                                                        serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                        serviceProvider
                                                                                                                                                                                           .GetRequiredService<IDeserializer<matchTimelineEndpoint>>(),
                                                                                                                                                                                        serviceProvider
                                                                                                                                                                                           .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                matchTimelineEndpoint, MatchTimelineDto>>()));

            // provider for getting info about sport categories
            var sportCategoriesEndpoint = configuration.Api.BaseUrl + "/v1/sports/{1}/sports/{0}/categories.xml";
            services.AddSingleton<IDeserializer<sportCategoriesEndpoint>, Deserializer<sportCategoriesEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<sportCategoriesEndpoint, SportCategoriesDto>, SportCategoriesMapperFactory>();
            services.AddSingleton<IDataProvider<SportCategoriesDto>, DataProvider<sportCategoriesEndpoint, SportCategoriesDto>>(serviceProvider =>
                                                                                                                                    new DataProvider<sportCategoriesEndpoint, SportCategoriesDto>(sportCategoriesEndpoint,
                                                                                                                                                                                                  serviceProvider
                                                                                                                                                                                                     .GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                                  serviceProvider
                                                                                                                                                                                                     .GetRequiredService<IDeserializer<
                                                                                                                                                                                                          sportCategoriesEndpoint>>(),
                                                                                                                                                                                                  serviceProvider
                                                                                                                                                                                                     .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                          sportCategoriesEndpoint,
                                                                                                                                                                                                          SportCategoriesDto>>()));

            // provider for getting info about fixture changes
            var fixtureChangeEndpoint = configuration.Api.BaseUrl + "/v1/sports/{0}/fixtures/changes.xml{1}";
            services.AddSingleton<IDeserializer<fixtureChangesEndpoint>, Deserializer<fixtureChangesEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>, FixtureChangesMapperFactory>();
            services.AddSingleton<IDataProvider<EntityList<FixtureChangeDto>>, DataProvider<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>>(serviceProvider =>
                                                                                                                                                       new DataProvider<fixtureChangesEndpoint, EntityList<FixtureChangeDto>>(fixtureChangeEndpoint,
                                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                                 .GetRequiredService<
                                                                                                                                                                                                                                      ILogHttpDataFetcher>(),
                                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                                 .GetRequiredService<
                                                                                                                                                                                                                                      IDeserializer<
                                                                                                                                                                                                                                          fixtureChangesEndpoint>>(),
                                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                                 .GetRequiredService<
                                                                                                                                                                                                                                      ISingleTypeMapperFactory
                                                                                                                                                                                                                                      <fixtureChangesEndpoint
                                                                                                                                                                                                                                          , EntityList<
                                                                                                                                                                                                                                              FixtureChangeDto>>>()));

            // provider for getting info about result changes
            var resultChangeEndpoint = configuration.Api.BaseUrl + "/v1/sports/{0}/results/changes.xml{1}";
            services.AddSingleton<IDeserializer<resultChangesEndpoint>, Deserializer<resultChangesEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<resultChangesEndpoint, EntityList<ResultChangeDto>>, ResultChangesMapperFactory>();
            services.AddSingleton<IDataProvider<EntityList<ResultChangeDto>>, DataProvider<resultChangesEndpoint, EntityList<ResultChangeDto>>>(serviceProvider =>
                                                                                                                                                    new DataProvider<resultChangesEndpoint, EntityList<ResultChangeDto>>(resultChangeEndpoint,
                                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                                            .GetRequiredService<
                                                                                                                                                                                                                                 ILogHttpDataFetcher>(),
                                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                                            .GetRequiredService<
                                                                                                                                                                                                                                 IDeserializer<
                                                                                                                                                                                                                                     resultChangesEndpoint>>(),
                                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                                            .GetRequiredService<
                                                                                                                                                                                                                                 ISingleTypeMapperFactory<
                                                                                                                                                                                                                                     resultChangesEndpoint
                                                                                                                                                                                                                                     , EntityList<
                                                                                                                                                                                                                                         ResultChangeDto>>>()));

            // invariant market description list provider
            var invariantMarketListEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/{0}/markets.xml?include_mappings=true";
            services.AddSingleton<IDeserializer<market_descriptions>, Deserializer<market_descriptions>>();
            services.AddSingleton<ISingleTypeMapperFactory<market_descriptions, EntityList<MarketDescriptionDto>>, MarketDescriptionsMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<MarketDescriptionDto>>, DataProviderNamed<market_descriptions, EntityList<MarketDescriptionDto>>>(serviceProvider =>
                                                                                                                                                                      new DataProviderNamed<market_descriptions,
                                                                                                                                                                          EntityList<MarketDescriptionDto>>(DataProviderForInvariantMarketList,
                                                                                                                                                                                                            invariantMarketListEndpoint,
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                    market_descriptions>>(),
                                                                                                                                                                                                            serviceProvider
                                                                                                                                                                                                               .GetRequiredService<
                                                                                                                                                                                                                    ISingleTypeMapperFactory<
                                                                                                                                                                                                                        market_descriptions, EntityList<
                                                                                                                                                                                                                            MarketDescriptionDto>>>()));

            // variant market description list provider
            var variantMarketListEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/{0}/variants.xml?include_mappings=true";
            services.AddSingleton<IDeserializer<variant_descriptions>, Deserializer<variant_descriptions>>();
            services.AddSingleton<ISingleTypeMapperFactory<variant_descriptions, EntityList<VariantDescriptionDto>>, VariantDescriptionsMapperFactory>();
            services.AddSingleton<IDataProviderNamed<EntityList<VariantDescriptionDto>>, DataProviderNamed<variant_descriptions, EntityList<VariantDescriptionDto>>>(serviceProvider =>
                                                                                                                                                                         new DataProviderNamed<variant_descriptions,
                                                                                                                                                                             EntityList<VariantDescriptionDto>>(DataProviderForVariantMarketList,
                                                                                                                                                                                                                variantMarketListEndpoint,
                                                                                                                                                                                                                serviceProvider
                                                                                                                                                                                                                   .GetRequiredService<
                                                                                                                                                                                                                        ILogHttpDataFetcher>(),
                                                                                                                                                                                                                serviceProvider
                                                                                                                                                                                                                   .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                        variant_descriptions>>(),
                                                                                                                                                                                                                serviceProvider
                                                                                                                                                                                                                   .GetRequiredService<
                                                                                                                                                                                                                        ISingleTypeMapperFactory<
                                                                                                                                                                                                                            variant_descriptions,
                                                                                                                                                                                                                            EntityList<
                                                                                                                                                                                                                                VariantDescriptionDto>>>()));

            // single variant market description provider
            var singleVariantMarketListEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/{1}/markets/{0}/variants/{2}?include_mappings=true";
            services.AddSingleton<ISingleTypeMapperFactory<market_descriptions, MarketDescriptionDto>, MarketDescriptionMapperFactory>();
            services.AddSingleton<IDataProviderNamed<MarketDescriptionDto>, DataProviderNamed<market_descriptions, MarketDescriptionDto>>(serviceProvider =>
                                                                                                                                              new DataProviderNamed<market_descriptions, MarketDescriptionDto>(DataProviderForVariantMarket,
                                                                                                                                                                                                               singleVariantMarketListEndpoint,
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<
                                                                                                                                                                                                                       ILogHttpDataFetcherFastFailing>(),
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                       market_descriptions>>(),
                                                                                                                                                                                                               serviceProvider
                                                                                                                                                                                                                  .GetRequiredService<
                                                                                                                                                                                                                       ISingleTypeMapperFactory<
                                                                                                                                                                                                                           market_descriptions,
                                                                                                                                                                                                                           MarketDescriptionDto>>()));

            // lottery draw summary provider
            var lotteryDrawSummaryEndpoint = configuration.Api.BaseUrl + "/v1/wns/{1}/sport_events/{0}/summary.xml";
            services.AddSingleton<IDeserializer<draw_summary>, Deserializer<draw_summary>>();
            services.AddSingleton<ISingleTypeMapperFactory<draw_summary, DrawDto>, DrawSummaryMapperFactory>();
            services.AddSingleton<IDataProviderNamed<DrawDto>, DataProviderNamed<draw_summary, DrawDto>>(serviceProvider =>
                                                                                                             new DataProviderNamed<draw_summary, DrawDto>(DataProviderForLotteryDrawSummary,
                                                                                                                                                          lotteryDrawSummaryEndpoint,
                                                                                                                                                          serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                          serviceProvider.GetRequiredService<IDeserializer<draw_summary>>(),
                                                                                                                                                          serviceProvider.GetRequiredService<ISingleTypeMapperFactory<draw_summary, DrawDto>>()));

            // lottery draw fixture provider
            var lotteryDrawFixtureEndpoint = configuration.Api.BaseUrl + "/v1/wns/{1}/sport_events/{0}/fixture.xml";
            services.AddSingleton<IDeserializer<draw_fixtures>, Deserializer<draw_fixtures>>();
            services.AddSingleton<ISingleTypeMapperFactory<draw_fixtures, DrawDto>, DrawFixtureMapperFactory>();
            services.AddSingleton<IDataProviderNamed<DrawDto>, DataProviderNamed<draw_fixtures, DrawDto>>(serviceProvider =>
                                                                                                              new DataProviderNamed<draw_fixtures, DrawDto>(DataProviderForLotteryDrawFixture,
                                                                                                                                                            lotteryDrawFixtureEndpoint,
                                                                                                                                                            serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                            serviceProvider.GetRequiredService<IDeserializer<draw_fixtures>>(),
                                                                                                                                                            serviceProvider.GetRequiredService<ISingleTypeMapperFactory<draw_fixtures, DrawDto>>()));

            // lottery schedule provider
            var lotteryScheduleEndpoint = configuration.Api.BaseUrl + "/v1/wns/{1}/lotteries/{0}/schedule.xml";
            services.AddSingleton<IDeserializer<lottery_schedule>, Deserializer<lottery_schedule>>();
            services.AddSingleton<ISingleTypeMapperFactory<lottery_schedule, LotteryDto>, LotteryScheduleMapperFactory>();
            services.AddSingleton<IDataProvider<LotteryDto>, DataProvider<lottery_schedule, LotteryDto>>(serviceProvider =>
                                                                                                             new DataProvider<lottery_schedule, LotteryDto>(lotteryScheduleEndpoint,
                                                                                                                                                            serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                            serviceProvider.GetRequiredService<IDeserializer<lottery_schedule>>(),
                                                                                                                                                            serviceProvider
                                                                                                                                                               .GetRequiredService<ISingleTypeMapperFactory<lottery_schedule, LotteryDto>>()));

            // lottery list provider
            var lotteryListEndpoint = configuration.Api.BaseUrl + "/v1/wns/{0}/lotteries.xml";
            services.AddSingleton<IDeserializer<lotteries>, Deserializer<lotteries>>();
            services.AddSingleton<ISingleTypeMapperFactory<lotteries, EntityList<LotteryDto>>, LotteriesMapperFactory>();
            services.AddSingleton<IDataProvider<EntityList<LotteryDto>>, DataProvider<lotteries, EntityList<LotteryDto>>>(serviceProvider =>
                                                                                                                              new DataProvider<lotteries, EntityList<LotteryDto>>(lotteryListEndpoint,
                                                                                                                                                                                  serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                  serviceProvider.GetRequiredService<IDeserializer<lotteries>>(),
                                                                                                                                                                                  serviceProvider
                                                                                                                                                                                     .GetRequiredService<ISingleTypeMapperFactory<lotteries,
                                                                                                                                                                                          EntityList<LotteryDto>>>()));

            // list sport event provider (example: /v1/sports/{0}/schedules/pre/schedule.xml?start={1}&limit={2})
            var sportEventListEndpoint = configuration.Api.BaseUrl + "/v1/sports/{0}/schedules/pre/schedule.xml?start={1}&limit={2}";
            services.AddSingleton<IDataProviderNamed<EntityList<SportEventSummaryDto>>, DataProviderNamed<scheduleEndpoint, EntityList<SportEventSummaryDto>>>(serviceProvider =>
                                                                                                                                                                   new DataProviderNamed<scheduleEndpoint,
                                                                                                                                                                       EntityList<SportEventSummaryDto>>(DataProviderForSportEventList,
                                                                                                                                                                                                         sportEventListEndpoint,
                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                            .GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                            .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                 scheduleEndpoint>>(),
                                                                                                                                                                                                         serviceProvider
                                                                                                                                                                                                            .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                                 scheduleEndpoint, EntityList<
                                                                                                                                                                                                                     SportEventSummaryDto>>>()));

            // sport available tournament list (example: /v1/sports/en/sports/sr:sport:55/tournaments.xml)
            var sportAvailableTournamentsEndpoint = configuration.Api.BaseUrl + "/v1/sports/{0}/sports/{1}/tournaments.xml";
            services.AddSingleton<IDeserializer<sportTournamentsEndpoint>, Deserializer<sportTournamentsEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDto>>, ListSportAvailableTournamentMapperFactory>();
            services.AddSingleton<IDataProvider<EntityList<TournamentInfoDto>>, DataProvider<sportTournamentsEndpoint, EntityList<TournamentInfoDto>>>(serviceProvider =>
                                                                                                                                                           new DataProvider<sportTournamentsEndpoint,
                                                                                                                                                               EntityList<TournamentInfoDto>>(sportAvailableTournamentsEndpoint,
                                                                                                                                                                                              serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                 .GetRequiredService<IDeserializer<
                                                                                                                                                                                                      sportTournamentsEndpoint>>(),
                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                 .GetRequiredService<ISingleTypeMapperFactory<
                                                                                                                                                                                                      sportTournamentsEndpoint,
                                                                                                                                                                                                      EntityList<TournamentInfoDto>>>()));

            // get the stage period summary (lap statistics - for formula 1) (example: /v1/sports/en/sport_events/sr:stage:{id}/period_summary.xml?competitors=sr:competitor:{id}&competitors=sr:competitor:{id}&periods=2&periods=3&periods=4)
            var stagePeriodSummaryEndpoint = configuration.Api.BaseUrl + "/v1/sports/{0}/sport_events/{1}/period_summary.xml{2}";
            services.AddSingleton<IDeserializer<stagePeriodEndpoint>, Deserializer<stagePeriodEndpoint>>();
            services.AddSingleton<ISingleTypeMapperFactory<stagePeriodEndpoint, PeriodSummaryDto>, PeriodSummaryMapperFactory>();
            services.AddSingleton<IDataProvider<PeriodSummaryDto>, DataProvider<stagePeriodEndpoint, PeriodSummaryDto>>(serviceProvider =>
                                                                                                                            new DataProvider<stagePeriodEndpoint, PeriodSummaryDto>(stagePeriodSummaryEndpoint,
                                                                                                                                                                                    serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                                                    serviceProvider
                                                                                                                                                                                       .GetRequiredService<IDeserializer<stagePeriodEndpoint>>(),
                                                                                                                                                                                    serviceProvider
                                                                                                                                                                                       .GetRequiredService<ISingleTypeMapperFactory<stagePeriodEndpoint,
                                                                                                                                                                                            PeriodSummaryDto>>()));

            // custom bet available selections provider
            var customBetAvailableSelectionsEndpoint = configuration.Api.BaseUrl + "/v1/custombet/{0}/available_selections";
            services.AddSingleton<IDeserializer<AvailableSelectionsType>, Deserializer<AvailableSelectionsType>>();
            services.AddSingleton<ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDto>, AvailableSelectionsMapperFactory>();
            services.AddSingleton<IDataProvider<AvailableSelectionsDto>, DataProvider<AvailableSelectionsType, AvailableSelectionsDto>>(serviceProvider =>
                                                                                                                                            new DataProvider<AvailableSelectionsType, AvailableSelectionsDto>(customBetAvailableSelectionsEndpoint,
                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                 .GetRequiredService<
                                                                                                                                                                                                                      ILogHttpDataFetcher>(),
                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                 .GetRequiredService<IDeserializer<
                                                                                                                                                                                                                      AvailableSelectionsType>>(),
                                                                                                                                                                                                              serviceProvider
                                                                                                                                                                                                                 .GetRequiredService<
                                                                                                                                                                                                                      ISingleTypeMapperFactory<
                                                                                                                                                                                                                          AvailableSelectionsType,
                                                                                                                                                                                                                          AvailableSelectionsDto>>()));

            // custom bet calculate provider
            var customBetCalculateEndpoint = configuration.Api.BaseUrl + "/v1/custombet/calculate";
            services.AddSingleton<IDeserializer<CalculationResponseType>, Deserializer<CalculationResponseType>>();
            services.AddSingleton<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>, CalculationMapperFactory>();
            services.AddSingleton<ICalculateProbabilityProvider, CalculateProbabilityProvider>(serviceProvider =>
                                                                                                   new CalculateProbabilityProvider(customBetCalculateEndpoint,
                                                                                                                                    serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                    serviceProvider.GetRequiredService<IDeserializer<CalculationResponseType>>(),
                                                                                                                                    serviceProvider.GetRequiredService<ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>>()));

            // custom bet calculate-filter provider
            var customBetCalculateFilterEndpoint = configuration.Api.BaseUrl + "/v1/custombet/calculate-filter";
            services.AddSingleton<IDeserializer<FilteredCalculationResponseType>, Deserializer<FilteredCalculationResponseType>>();
            services.AddSingleton<ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto>, CalculationFilteredMapperFactory>();
            services.AddSingleton<ICalculateProbabilityFilteredProvider, CalculateProbabilityFilteredProvider>(serviceProvider =>
                                                                                                                   new CalculateProbabilityFilteredProvider(customBetCalculateFilterEndpoint,
                                                                                                                                                            serviceProvider.GetRequiredService<ILogHttpDataFetcher>(),
                                                                                                                                                            serviceProvider.GetRequiredService<IDeserializer<FilteredCalculationResponseType>>(),
                                                                                                                                                            serviceProvider
                                                                                                                                                               .GetRequiredService<ISingleTypeMapperFactory<FilteredCalculationResponseType,
                                                                                                                                                                    FilteredCalculationDto>>()));

            services.AddSingleton<IDataRouterManager, DataRouterManager>(serviceProvider =>
                                                                             new DataRouterManager(serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                   serviceProvider.GetRequiredService<IProducerManager>(),
                                                                                                   serviceProvider.GetRequiredService<IUofConfiguration>(),
                                                                                                   serviceProvider.GetRequiredService<IExecutionPathDataProvider<SportEventSummaryDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<FixtureDto>>(),
                                                                                                   serviceProvider.GetDataProviderNamed<FixtureDto>(DataProviderForFixtureChangeFixtureEndpoint),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportDto>>(DataProviderForAllTournamentsForAllSports),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportDto>>(DataProviderForAllSports),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(DataProviderForDateSchedule),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(DataProviderForTournamentSchedule),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<PlayerProfileDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<CompetitorProfileDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<SimpleTeamProfileDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<TournamentSeasonsDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<MatchTimelineDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<SportCategoriesDto>>(),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<MarketDescriptionDto>>(DataProviderForInvariantMarketList),
                                                                                                   serviceProvider.GetDataProviderNamed<MarketDescriptionDto>(DataProviderForVariantMarket),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<VariantDescriptionDto>>(DataProviderForVariantMarketList),
                                                                                                   serviceProvider.GetDataProviderNamed<DrawDto>(DataProviderForLotteryDrawSummary),
                                                                                                   serviceProvider.GetDataProviderNamed<DrawDto>(DataProviderForLotteryDrawFixture),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<LotteryDto>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<EntityList<LotteryDto>>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<AvailableSelectionsDto>>(),
                                                                                                   serviceProvider.GetRequiredService<ICalculateProbabilityProvider>(),
                                                                                                   serviceProvider.GetRequiredService<ICalculateProbabilityFilteredProvider>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<EntityList<FixtureChangeDto>>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<EntityList<ResultChangeDto>>>(),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(DataProviderForSportEventList),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<EntityList<TournamentInfoDto>>>(),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<TournamentInfoDto>>(),
                                                                                                   serviceProvider.GetDataProviderNamed<TournamentInfoDto>(DataProviderForFixtureChangeFixtureEndpointForTournamentInfo),
                                                                                                   serviceProvider.GetRequiredService<IDataProvider<PeriodSummaryDto>>(),
                                                                                                   serviceProvider.GetDataProviderNamed<EntityList<SportEventSummaryDto>>(DataProviderForStageTournamentSchedule)));
        }

        private static void RegisterMarketCaches(IServiceCollection services)
        {
            services.AddSingleton<IOperandFactory, OperandFactory>();
            services.AddSingleton<INameExpressionFactory, NameExpressionFactory>();
            services.AddSingleton<INameProviderFactory, NameProviderFactory>(serviceProvider =>
                                                                                 new NameProviderFactory(serviceProvider.GetRequiredService<IMarketCacheProvider>(),
                                                                                                         serviceProvider.GetRequiredService<IProfileCache>(),
                                                                                                         serviceProvider.GetRequiredService<INameExpressionFactory>(),
                                                                                                         serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                                         serviceProvider.GetRequiredService<ILoggerFactory>()));
            services.AddSingleton<IMarketCacheProvider, MarketCacheProvider>();

            services.AddSingleton<IMarketMappingProviderFactory, MarketMappingProviderFactory>(serviceProvider =>
                                                                                                   new MarketMappingProviderFactory(serviceProvider.GetRequiredService<IMarketCacheProvider>(),
                                                                                                                                    serviceProvider.GetRequiredService<ISportEventStatusCache>(),
                                                                                                                                    serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy));

            // invariant market cache for market description list
            services.AddSingleton<IMarketDescriptionsCache, InvariantMarketDescriptionCache>(serviceProvider =>
                                                                                                 new InvariantMarketDescriptionCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForInvariantMarketDescriptionsCache),
                                                                                                                                     serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                                                                     serviceProvider.GetRequiredService<IMappingValidatorFactory>(),
                                                                                                                                     serviceProvider.GetSdkTimer(TimerForInvariantMarketDescriptionsCache),
                                                                                                                                     serviceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                                                                                                                     serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                                                     serviceProvider.GetRequiredService<ILoggerFactory>()));

            // single variant market cache
            services.AddSingleton<IMarketDescriptionCache, VariantMarketDescriptionCache>(serviceProvider =>
                                                                                              new VariantMarketDescriptionCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForVariantMarketDescriptionCache),
                                                                                                                                serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                                                                serviceProvider.GetRequiredService<IMappingValidatorFactory>(),
                                                                                                                                serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                                                serviceProvider.GetRequiredService<ILoggerFactory>()));

            // variant market cache for market description list
            services.AddSingleton<IVariantDescriptionsCache, VariantDescriptionListCache>(serviceProvider =>
                                                                                              new VariantDescriptionListCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForVariantDescriptionListCache),
                                                                                                                              serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                                                              serviceProvider.GetRequiredService<IMappingValidatorFactory>(),
                                                                                                                              serviceProvider.GetSdkTimer(TimerForVariantMarketDescriptionListCache),
                                                                                                                              serviceProvider.GetRequiredService<IUofConfiguration>().Languages,
                                                                                                                              serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                                                              serviceProvider.GetRequiredService<ILoggerFactory>()));

            // profile cache
            services.AddSingleton<IProfileCache, ProfileCache>(serviceProvider =>
                                                                   new ProfileCache(serviceProvider.GetSdkCacheStore<string>(CacheStoreNameForProfileCache),
                                                                                    serviceProvider.GetRequiredService<IDataRouterManager>(),
                                                                                    serviceProvider.GetRequiredService<ICacheManager>(),
                                                                                    serviceProvider.GetRequiredService<ISportEventCache>(),
                                                                                    serviceProvider.GetRequiredService<ILoggerFactory>()));
        }

        private static void RegisterNamedValueCaches(IServiceCollection services, IUofConfiguration configuration)
        {
            var voidReasonsEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/void_reasons.xml";
            services.AddSingleton<IDataProviderNamed<EntityList<NamedValueDto>>, NamedValueDataProvider>(serviceProvider =>
                                                                                                             new NamedValueDataProvider(DataProviderForNamedValueCacheVoidReason,
                                                                                                                                        voidReasonsEndpoint,
                                                                                                                                        serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                        "void_reason"));
            services.AddSingleton<INamedValueCache, NamedValueCache>(serviceProvider =>
                                                                         new NamedValueCache(NamedValueCacheNameForVoidReason,
                                                                                             serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                             serviceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(DataProviderForNamedValueCacheVoidReason),
                                                                                             serviceProvider.GetSdkTimer(TimerForNamedValueCache)));

            var betStopReasonsEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/betstop_reasons.xml";
            services.AddSingleton<IDataProviderNamed<EntityList<NamedValueDto>>, NamedValueDataProvider>(serviceProvider =>
                                                                                                             new NamedValueDataProvider(DataProviderForNamedValueCacheBetStopReason,
                                                                                                                                        betStopReasonsEndpoint,
                                                                                                                                        serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                        "betstop_reason"));
            services.AddSingleton<INamedValueCache, NamedValueCache>(serviceProvider =>
                                                                         new NamedValueCache(NamedValueCacheNameForBetStopReason,
                                                                                             serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                             serviceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(DataProviderForNamedValueCacheBetStopReason),
                                                                                             serviceProvider.GetSdkTimer(TimerForNamedValueCache)));

            var bettingStatusEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/betting_status.xml";
            services.AddSingleton<IDataProviderNamed<EntityList<NamedValueDto>>, NamedValueDataProvider>(serviceProvider =>
                                                                                                             new NamedValueDataProvider(DataProviderForNamedValueCacheBettingStatus,
                                                                                                                                        bettingStatusEndpoint,
                                                                                                                                        serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                        "betting_status"));
            services.AddSingleton<INamedValueCache, NamedValueCache>(serviceProvider =>
                                                                         new NamedValueCache(NamedValueCacheNameForBettingStatus,
                                                                                             serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                             serviceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(DataProviderForNamedValueCacheBettingStatus),
                                                                                             serviceProvider.GetSdkTimer(TimerForNamedValueCache)));

            var matchStatusEndpoint = configuration.Api.BaseUrl + "/v1/descriptions/{0}/match_status.xml";
            services.AddSingleton<IDataProviderNamed<EntityList<NamedValueDto>>, NamedValueDataProvider>(serviceProvider =>
                                                                                                             new NamedValueDataProvider(DataProviderForNamedValueCacheMatchStatus,
                                                                                                                                        matchStatusEndpoint,
                                                                                                                                        serviceProvider.GetRequiredService<IDataFetcher>(),
                                                                                                                                        "match_status"));
            services.AddSingleton<ILocalizedNamedValueCache, LocalizedNamedValueCache>(serviceProvider =>
                                                                                           new LocalizedNamedValueCache(NamedValueCacheNameForMatchStatus,
                                                                                                                        serviceProvider.GetRequiredService<IUofConfiguration>().ExceptionHandlingStrategy,
                                                                                                                        serviceProvider.GetDataProviderNamed<EntityList<NamedValueDto>>(DataProviderForNamedValueCacheMatchStatus),
                                                                                                                        serviceProvider.GetSdkTimer(TimerForNamedValueCache),
                                                                                                                        configuration.Languages));

            services.AddSingleton<INamedValuesProvider, NamedValuesProvider>(serviceProvider =>
                                                                                 new NamedValuesProvider(serviceProvider.GetNamedValueCache(NamedValueCacheNameForVoidReason),
                                                                                                         serviceProvider.GetNamedValueCache(NamedValueCacheNameForBetStopReason),
                                                                                                         serviceProvider.GetNamedValueCache(NamedValueCacheNameForBettingStatus),
                                                                                                         serviceProvider.GetLocalizedNamedValueCache(NamedValueCacheNameForMatchStatus)));
        }

        private static void RegisterHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                    .AddCheck<IProfileCache>("ProfileCache")
                    .AddCheck<ISportDataCache>("SportDataCache")
                    .AddCheck<ISportEventCache>("SportEventCache")
                    .AddCheck<ISportEventStatusCache>("SportEventStatusCache")
                    .AddCheck<IMarketDescriptionCache>("VariantDescriptionCacheSingle")
                    .AddCheck<IMarketDescriptionsCache>("InvariantDescriptionCacheList")
                    .AddCheck<IVariantDescriptionsCache>("VariantDescriptionCacheList");
        }

        private static void RegisterTelemetry(IServiceCollection services)
        {
            services.AddOpenTelemetry()
                    .WithTracing(traceProviderBuilder => traceProviderBuilder.AddSource(UofSdkTelemetry.ActivitySource.Name))
                    .WithMetrics(metricsProviderBuilder => metricsProviderBuilder.AddMeter(UofSdkTelemetry.DefaultMeter.Name));
        }
    }
}
