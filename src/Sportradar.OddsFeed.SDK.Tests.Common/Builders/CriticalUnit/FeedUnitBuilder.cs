// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CriticalUnit;

public class FeedUnitBuilder
{
    private IMarketCacheProvider _marketCacheProvider;
    private IMarketFactory _marketFactory;
    private ISportEntityFactory _sportEntityFactory;
    private ISportEventStatusCache _sportEventStatusCache;

    private readonly Dictionary<MessageInterest, UofSession> _sessions = [];
    private ICacheManager _cacheManager;
    private ILoggerFactory _loggerFactory;
    private INamedValuesProvider _namedValuesProvider;
    private IProducerManager _producerManager;
    private ISportEventCache _sportEventCache;
    private IUofConfiguration _uofConfig;

    private FeedUnitBuilder()
    {
        _loggerFactory = NullLoggerFactory.Instance;
    }

    public static FeedUnitBuilder Create()
    {
        return new FeedUnitBuilder();
    }

    public FeedUnitBuilder WithConfiguration(IUofConfiguration config)
    {
        _uofConfig = config;
        return this;
    }

    public FeedUnitBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    public FeedUnitBuilder WithProducerManager(IProducerManager producerManager)
    {
        _producerManager = producerManager;
        return this;
    }

    internal FeedUnitBuilder WithMarketCacheProvider(IMarketCacheProvider marketCacheProvider)
    {
        _marketCacheProvider = marketCacheProvider;
        return this;
    }

    internal FeedUnitBuilder WithSportEntityFactory(ISportEntityFactory sportEntityFactory)
    {
        _sportEntityFactory = sportEntityFactory;
        return this;
    }

    internal FeedUnitBuilder WithMarketFactory(IMarketFactory marketFactory)
    {
        _marketFactory = marketFactory;
        return this;
    }

    internal FeedUnitBuilder WithNamedValuesProvider(INamedValuesProvider namedValuesProvider)
    {
        _namedValuesProvider = namedValuesProvider;
        return this;
    }

    internal FeedUnitBuilder WithSportEventCache(ISportEventCache sportEventCache)
    {
        _sportEventCache = sportEventCache;
        return this;
    }

    internal FeedUnitBuilder WithSportEventStatusCache(ISportEventStatusCache sportEventStatusCache)
    {
        _sportEventStatusCache = sportEventStatusCache;
        return this;
    }

    internal FeedUnitBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    internal FeedUnitBuilder AddSession(MessageInterest messageInterest, IRabbitMqChannel rabbitMqChannel)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForFixtureChangeCache, null, TimeSpan.FromSeconds(10));
        serviceCollection.AddLogging(); // registers ILogger<T>, etc.
        serviceCollection.Replace(ServiceDescriptor.Singleton(_loggerFactory));

        var serviceProvider = serviceCollection.BuildServiceProvider().CreateScope().ServiceProvider;

        SdkLoggerFactory.SetLoggerFactory(serviceProvider.GetService<ILoggerFactory>());

        var entityMapper = new EntityTypeMapper();
        var dispatcherStore = new DispatcherStore(entityMapper);
        var feedMessageHandler = new FeedMessageHandler(serviceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForFixtureChangeCache));

        var cacheMessageProcessor = new CacheMessageProcessor(new SportEventStatusMapperFactory(),
                                                              _sportEventCache,
                                                              _cacheManager,
                                                              feedMessageHandler,
                                                              _sportEventStatusCache,
                                                              _producerManager);

        var feedMessageMapper = new FeedMessageMapper(_sportEntityFactory,
                                                      _marketFactory,
                                                      _producerManager,
                                                      _namedValuesProvider,
                                                      _uofConfig.ExceptionHandlingStrategy);

        var sessionMessageManager = new SessionMessageManager(feedMessageMapper);

        var feedMessageProcessor = new CompositeMessageProcessor([cacheMessageProcessor, sessionMessageManager]);

        var rabbitMqMessageReceiver = new RabbitMqMessageReceiver(rabbitMqChannel,
                                                                  new Deserializer<FeedMessage>(),
                                                                  new RegexRoutingKeyParser(),
                                                                  _producerManager,
                                                                  _uofConfig,
                                                                  _loggerFactory);

        var feedMessageValidator = new FeedMessageValidator(_marketCacheProvider,
                                                            _uofConfig.DefaultLanguage,
                                                            _namedValuesProvider,
                                                            _producerManager);

        var newSession = new UofSession(rabbitMqMessageReceiver,
                                        feedMessageProcessor,
                                        feedMessageMapper,
                                        feedMessageValidator,
                                        new MessageDataExtractor(),
                                        dispatcherStore,
                                        messageInterest,
                                        _uofConfig.Languages,
                                        GetRoutingKeys);

        _sessions.Add(messageInterest, newSession);

        return this;
    }

    internal FeedUnit Build()
    {
        return new FeedUnit(_sessions);
    }

    private static IEnumerable<string> GetRoutingKeys(UofSession session)
    {
        var keys = FeedRoutingKeyBuilder.GenerateKeys([session.MessageInterest]).ToList();
        return keys[0];
    }
}
