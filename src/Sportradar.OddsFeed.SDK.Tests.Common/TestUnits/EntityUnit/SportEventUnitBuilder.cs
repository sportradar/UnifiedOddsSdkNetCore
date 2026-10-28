// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Helpers;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;

public class SportEventUnitBuilder
{
    private readonly List<matchSummaryEndpoint> _summaryResponses = new List<matchSummaryEndpoint>();
    private readonly List<tournamentInfoEndpoint> _tournamentInfoResponses = new List<tournamentInfoEndpoint>();
    private readonly List<fixturesEndpoint> _fixtureResponses = new List<fixturesEndpoint>();
    private readonly List<lotteries> _lotteriesResponses = new List<lotteries>();
    private readonly List<lottery_schedule> _lotteryScheduleResponses = new List<lottery_schedule>();
    private readonly List<draw_summary> _drawSummaryResponses = new List<draw_summary>();

    private ILoggerFactory _loggerFactory;
    private IUofConfiguration _uofConfig;

    private ISportEntityFactory _sportEntityFactory;
    private ISportEventStatusCache _sportEventStatusCache;
    private IProfileCache _profileCache;

    private ICacheManager _cacheManager;
    private ISportEventCache _sportEventCache;
    private ISportDataCache _sportDataCache;
    private IDataFetcher _summaryDataFetcher;
    private IDataFetcher _fixtureDataFetcher;
    private bool _summaryResponsesConfigured;
    private bool _tournamentInfoResponsesConfigured;
    private bool _fixtureResponsesConfigured;
    private bool _lotteriesResponsesConfigured;
    private bool _lotteryScheduleResponsesConfigured;
    private bool _drawSummaryResponsesConfigured;

    private SportEventUnitBuilder()
    {
        _loggerFactory = NullLoggerFactory.Instance;
    }

    public static SportEventUnitBuilder Create()
    {
        return new SportEventUnitBuilder();
    }

    public SportEventUnitBuilder WithConfiguration(IUofConfiguration config)
    {
        _uofConfig = config;
        return this;
    }

    public SportEventUnitBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    public SportEventUnitBuilder WithLoggerFactory(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new XunitLoggerFactory(outputHelper);
        return this;
    }

    public SportEventUnitBuilder WithoutLoggerFactory()
    {
        _loggerFactory = NullLoggerFactory.Instance;
        return this;
    }

    internal SportEventUnitBuilder WithSportEntityFactory(ISportEntityFactory sportEntityFactory)
    {
        _sportEntityFactory = sportEntityFactory;
        return this;
    }

    internal SportEventUnitBuilder WithSportEventCache(ISportEventCache sportEventCache)
    {
        _sportEventCache = sportEventCache;
        return this;
    }

    internal SportEventUnitBuilder WithSportDataCache(ISportDataCache sportDataCache)
    {
        _sportDataCache = sportDataCache;
        return this;
    }

    internal SportEventUnitBuilder WithSportEventStatusCache(ISportEventStatusCache sportEventStatusCache)
    {
        _sportEventStatusCache = sportEventStatusCache;
        return this;
    }

    internal SportEventUnitBuilder WithProfileCache(IProfileCache profileCache)
    {
        _profileCache = profileCache;
        return this;
    }

    internal SportEventUnitBuilder WithCacheManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
        return this;
    }

    internal SportEventUnitBuilder WithDefaultCacheManager()
    {
        _cacheManager = new CacheManager();
        return this;
    }

    internal SportEventUnitBuilder WithSummaryFetcher(IDataFetcher summaryDataFetcher)
    {
        _summaryDataFetcher = summaryDataFetcher;
        _summaryResponses.Clear();
        _summaryResponsesConfigured = false;
        return this;
    }

    internal SportEventUnitBuilder WithFixtureFetcher(IDataFetcher fixtureDataFetcher)
    {
        _fixtureDataFetcher = fixtureDataFetcher;
        _fixtureResponses.Clear();
        _fixtureResponsesConfigured = false;
        return this;
    }

    internal SportEventUnitBuilder WithMockedSummaryFetcher()
    {
        return WithSummaryFetcher(new Mock<IDataFetcher>().Object);
    }

    internal SportEventUnitBuilder WithMockedFixtureFetcher()
    {
        return WithFixtureFetcher(new Mock<IDataFetcher>().Object);
    }

    internal SportEventUnitBuilder WithSummaryFetcherReturning(matchSummaryEndpoint summaryResponse)
    {
        if (!_summaryResponsesConfigured)
        {
            _summaryResponses.Clear();
            _summaryDataFetcher = null;
            _summaryResponsesConfigured = true;
        }

        _summaryResponses.Add(summaryResponse);
        return this;
    }

    internal SportEventUnitBuilder WithTournamentInfoFetcherReturning(tournamentInfoEndpoint tournamentInfoResponse)
    {
        if (!_tournamentInfoResponsesConfigured)
        {
            _tournamentInfoResponses.Clear();
            _summaryDataFetcher = null;
            _tournamentInfoResponsesConfigured = true;
        }

        _tournamentInfoResponses.Add(tournamentInfoResponse);
        return this;
    }

    internal SportEventUnitBuilder WithFixtureFetcherReturning(fixturesEndpoint fixtureResponse)
    {
        if (!_fixtureResponsesConfigured)
        {
            _fixtureResponses.Clear();
            _fixtureDataFetcher = null;
            _fixtureResponsesConfigured = true;
        }

        _fixtureResponses.Add(fixtureResponse);
        return this;
    }

    internal SportEventUnitBuilder WithLotteriesFetcherReturning(lotteries lotteriesResponse)
    {
        if (!_lotteriesResponsesConfigured)
        {
            _lotteriesResponses.Clear();
            _lotteriesResponsesConfigured = true;
        }

        _lotteriesResponses.Add(lotteriesResponse);
        return this;
    }

    internal SportEventUnitBuilder WithLotteryScheduleFetcherReturning(lottery_schedule lotteryScheduleResponse)
    {
        if (!_lotteryScheduleResponsesConfigured)
        {
            _lotteryScheduleResponses.Clear();
            _lotteryScheduleResponsesConfigured = true;
        }

        _lotteryScheduleResponses.Add(lotteryScheduleResponse);
        return this;
    }

    internal SportEventUnitBuilder WithDrawSummaryFetcherReturning(draw_summary drawSummaryResponse)
    {
        if (!_drawSummaryResponsesConfigured)
        {
            _drawSummaryResponses.Clear();
            _drawSummaryResponsesConfigured = true;
        }

        _drawSummaryResponses.Add(drawSummaryResponse);
        return this;
    }

    internal SportEventUnit Build()
    {
        if (_uofConfig == null)
        {
            throw new InvalidOperationException("Configuration is required. Call WithConfiguration before Build.");
        }

        _cacheManager ??= new CacheManager();

        var summaryFetcher = CreateSummaryFetcher();
        var fixtureFetcher = CreateFixtureFetcher();
        var lotteriesFetcher = CreateLotteriesFetcher();
        var lotteryScheduleFetcher = CreateLotteryScheduleFetcher();
        var drawSummaryFetcher = CreateDrawSummaryFetcher();
        var dataRouterManager = CreateMockDataRouterManager(summaryFetcher, fixtureFetcher, lotteriesFetcher, lotteryScheduleFetcher, drawSummaryFetcher);
        var cacheItemFactory = CreateSportEventCacheItemFactory(dataRouterManager);
        var sportEventCache = _sportEventCache ?? CreateSportEventCache(dataRouterManager, cacheItemFactory);
        var sportDataCache = _sportDataCache ?? CreateSportDataCache(dataRouterManager, sportEventCache);
        var profileCache = _profileCache ?? CreateProfileCache(dataRouterManager, sportEventCache);
        var sportEventStatusCache = _sportEventStatusCache ?? CreateSportEventStatusCache(sportEventCache);
        var sportEntityFactory = _sportEntityFactory ?? CreateSportEntityFactory(sportEventCache, sportDataCache, sportEventStatusCache, profileCache);
        var sportDataProvider = CreateSportDataProvider(dataRouterManager, sportEventCache, sportEntityFactory, sportDataCache, sportEventStatusCache, profileCache);

        return new SportEventUnit(sportDataProvider);
    }

    private IDataFetcher CreateSummaryFetcher()
    {
        if (_summaryResponses.Count == 0 && _tournamentInfoResponses.Count == 0)
        {
            return _summaryDataFetcher ?? new Mock<IDataFetcher>().Object;
        }

        var summaryFetcherMock = new Mock<IDataFetcher>();
        foreach (var summaryResponse in _summaryResponses)
        {
            SetupSummaryEndpointReturning(summaryFetcherMock, summaryResponse);
        }

        foreach (var tournamentInfoResponse in _tournamentInfoResponses)
        {
            SetupTournamentInfoEndpointReturning(summaryFetcherMock, tournamentInfoResponse);
        }

        return summaryFetcherMock.Object;
    }

    private IDataFetcher CreateFixtureFetcher()
    {
        if (_fixtureResponses.Count == 0)
        {
            return _fixtureDataFetcher ?? new Mock<IDataFetcher>().Object;
        }

        var fixtureFetcherMock = new Mock<IDataFetcher>();
        foreach (var fixtureResponse in _fixtureResponses)
        {
            SetupFixtureEndpointReturning(fixtureFetcherMock, fixtureResponse);
        }

        return fixtureFetcherMock.Object;
    }

    private IDataFetcher CreateLotteriesFetcher()
    {
        if (_lotteriesResponses.Count == 0)
        {
            return new Mock<IDataFetcher>().Object;
        }

        var lotteriesFetcherMock = new Mock<IDataFetcher>();
        foreach (var lotteriesResponse in _lotteriesResponses)
        {
            SetupLotteriesEndpointReturning(lotteriesFetcherMock, lotteriesResponse);
        }

        return lotteriesFetcherMock.Object;
    }

    private IDataFetcher CreateLotteryScheduleFetcher()
    {
        if (_lotteryScheduleResponses.Count == 0)
        {
            return new Mock<IDataFetcher>().Object;
        }

        var lotteryScheduleFetcherMock = new Mock<IDataFetcher>();
        foreach (var lotteryScheduleResponse in _lotteryScheduleResponses)
        {
            SetupLotteryScheduleEndpointReturning(lotteryScheduleFetcherMock, lotteryScheduleResponse);
        }

        return lotteryScheduleFetcherMock.Object;
    }

    private IDataFetcher CreateDrawSummaryFetcher()
    {
        if (_drawSummaryResponses.Count == 0)
        {
            return new Mock<IDataFetcher>().Object;
        }

        var drawSummaryFetcherMock = new Mock<IDataFetcher>();
        foreach (var drawSummaryResponse in _drawSummaryResponses)
        {
            SetupDrawSummaryEndpointReturning(drawSummaryFetcherMock, drawSummaryResponse);
        }

        return drawSummaryFetcherMock.Object;
    }

    private IDataRouterManager CreateMockDataRouterManager(IDataFetcher summaryDataFetcher,
                                                            IDataFetcher fixtureDataFetcher,
                                                            IDataFetcher lotteriesDataFetcher,
                                                            IDataFetcher lotteryScheduleDataFetcher,
                                                            IDataFetcher drawSummaryDataFetcher)
    {
        var criticalSummaryProvider = CreateSummaryProvider(summaryDataFetcher);
        var nonCriticalSummaryProvider = CreateSummaryProvider(summaryDataFetcher);
        var executionPathDataProvider = new SDK.Entities.Rest.Internal.ExecutionPathDataProvider<SportEventSummaryDto>(criticalSummaryProvider, nonCriticalSummaryProvider);

        return new DataRouterManagerBuilder()
              .AddMockedDependencies()
              .WithConfiguration(_uofConfig)
              .WithCacheManager(_cacheManager)
              .WithSportEventFixtureProvider(CreateFixtureProvider(fixtureDataFetcher))
              .WithSportEventFixtureChangeFixtureProvider(CreateFixtureProvider(fixtureDataFetcher))
              .WithSportEventSummaryProvider(executionPathDataProvider)
              .WithSportEventsForTournamentProvider(CreateEmptyTournamentScheduleProvider())
              .WithLotteryListProvider(CreateLotteriesProvider(lotteriesDataFetcher))
              .WithLotteryScheduleProvider(CreateLotteryScheduleProvider(lotteryScheduleDataFetcher))
              .WithDrawSummaryProvider(CreateDrawSummaryProvider(drawSummaryDataFetcher))
              .Build();
    }

    private IDataProvider<SportEventSummaryDto> CreateSummaryProvider(IDataFetcher summaryDataFetcher)
    {
        return new DataProvider<RestMessage, SportEventSummaryDto>($"{_uofConfig.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/summary.xml",
                                                                    summaryDataFetcher,
                                                                    new Deserializer<RestMessage>(),
                                                                    new SportEventSummaryMapperFactory());
    }

    private IDataProvider<FixtureDto> CreateFixtureProvider(IDataFetcher fixtureDataFetcher)
    {
        return new DataProvider<fixturesEndpoint, FixtureDto>($"{_uofConfig.Api.BaseUrl}/v1/sports/{{1}}/sport_events/{{0}}/fixture.xml",
                                                              fixtureDataFetcher,
                                                              new Deserializer<fixturesEndpoint>(),
                                                              new FixtureMapperFactory());
    }

    private IDataProvider<EntityList<LotteryDto>> CreateLotteriesProvider(IDataFetcher lotteriesDataFetcher)
    {
        return new DataProvider<lotteries, EntityList<LotteryDto>>($"{_uofConfig.Api.BaseUrl}/v1/wns/{{0}}/lotteries.xml",
                                                                    lotteriesDataFetcher,
                                                                    new Deserializer<lotteries>(),
                                                                    new LotteriesMapperFactory());
    }

    private IDataProvider<DrawDto> CreateDrawSummaryProvider(IDataFetcher drawSummaryDataFetcher)
    {
        return new DataProvider<draw_summary, DrawDto>($"{_uofConfig.Api.BaseUrl}/v1/wns/{{1}}/sport_events/{{0}}/summary.xml",
                                                       drawSummaryDataFetcher,
                                                       new Deserializer<draw_summary>(),
                                                       new DrawSummaryMapperFactory());
    }

    private IDataProvider<LotteryDto> CreateLotteryScheduleProvider(IDataFetcher lotteryScheduleDataFetcher)
    {
        return new DataProvider<lottery_schedule, LotteryDto>($"{_uofConfig.Api.BaseUrl}/v1/wns/{{1}}/lotteries/{{0}}/schedule.xml",
                                                              lotteryScheduleDataFetcher,
                                                              new Deserializer<lottery_schedule>(),
                                                              new LotteryScheduleMapperFactory());
    }

    private ISportEventCacheItemFactory CreateSportEventCacheItemFactory(IDataRouterManager dataRouterManager)
    {
        var fixtureCacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache, _loggerFactory);
        var semaphorePool = new SemaphorePool(10, _uofConfig.ExceptionHandlingStrategy, _loggerFactory.CreateLogger<SemaphorePool>());

        return new SportEventCacheItemFactory(dataRouterManager, semaphorePool, _uofConfig, fixtureCacheStore);
    }

    private ISportEventCache CreateSportEventCache(IDataRouterManager dataRouterManager, ISportEventCacheItemFactory sportEventCacheItemFactory)
    {
        var cacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCache, _loggerFactory);

        return new SportEventCacheBuilder()
              .WithAllMockedDependencies()
              .WithDataRouterManager(dataRouterManager)
              .WithCacheManager(_cacheManager)
              .WithCache(cacheStore)
              .WithCacheItemFactory(sportEventCacheItemFactory)
              .WithLoggerFactory(_loggerFactory)
              .WithLanguages(_uofConfig.Languages)
              .Build();
    }

    private ISportDataCache CreateSportDataCache(IDataRouterManager dataRouterManager, ISportEventCache sportEventCache)
    {
        return new SportDataCache(dataRouterManager,
                                   new Mock<ISdkTimer>().Object,
                                   _uofConfig.Languages,
                                   sportEventCache,
                                   _cacheManager,
                                   _loggerFactory);
    }

    private IProfileCache CreateProfileCache(IDataRouterManager dataRouterManager, ISportEventCache sportEventCache)
    {
        var cacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForProfileCache, _loggerFactory);
        return new ProfileCache(cacheStore, dataRouterManager, _cacheManager, sportEventCache, _loggerFactory);
    }

    private ISportEventStatusCache CreateSportEventStatusCache(ISportEventCache sportEventCache)
    {
        var statusCacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventStatusCache, _loggerFactory);
        var ignoreTimelineCacheStore = SdkCacheStoreHelper.CreateSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForIgnoreEventsTimelineCache, _loggerFactory);

        return new SportEventStatusCache(statusCacheStore,
                                          new SportEventStatusMapperFactory(),
                                          sportEventCache,
                                          _cacheManager,
                                          ignoreTimelineCacheStore,
                                          _uofConfig,
                                          _loggerFactory);
    }

    private static ISportEntityFactory CreateSportEntityFactory(ISportEventCache sportEventCache,
                                                                 ISportDataCache sportDataCache,
                                                                 ISportEventStatusCache sportEventStatusCache,
                                                                 IProfileCache profileCache)
    {
        return new SportEntityFactoryBuilder()
              .WithAllMockedDependencies()
              .WithSportEventCache(sportEventCache)
              .WithSportDataCache(sportDataCache)
              .WithEventStatusCache(sportEventStatusCache)
              .WithProfileCache(profileCache)
              .Build();
    }

    private ISportDataProvider CreateSportDataProvider(IDataRouterManager dataRouterManager,
                                                        ISportEventCache sportEventCache,
                                                        ISportEntityFactory sportEntityFactory,
                                                        ISportDataCache sportDataCache,
                                                        ISportEventStatusCache sportEventStatusCache,
                                                        IProfileCache profileCache)
    {
        return new SportDataProviderBuilder()
              .WithAllMockedDependencies()
              .WithCacheManager(_cacheManager)
              .WithDataRouterManager(dataRouterManager)
              .WithDefaultCultures(_uofConfig.Languages)
              .WithExceptionStrategy(_uofConfig.ExceptionHandlingStrategy)
              .WithSportEventCache(sportEventCache)
              .WithSportEntityFactory(sportEntityFactory)
              .WithSportDataCache(sportDataCache)
              .WithSportEventStatusCache(sportEventStatusCache)
              .WithProfileCache(profileCache)
              .Build();
    }

    private static IDataProvider<EntityList<SportEventSummaryDto>> CreateEmptyTournamentScheduleProvider()
    {
        var provider = new Mock<IDataProvider<EntityList<SportEventSummaryDto>>>();
        provider.Setup(p => p.GetDataAsync(It.IsAny<string[]>()))
                .ReturnsAsync(new EntityList<SportEventSummaryDto>(new List<SportEventSummaryDto>()));
        return provider.Object;
    }

    private void SetupTournamentInfoEndpointReturning(Mock<IDataFetcher> summaryFetcherMock, tournamentInfoEndpoint tournamentInfo)
    {
        foreach (var language in _uofConfig.Languages)
        {
            SetupTournamentSummary(summaryFetcherMock, tournamentInfo, tournamentInfo.tournament.id, language);

            var currentSeasonId = tournamentInfo.tournament.current_season?.id;
            if (currentSeasonId != null && currentSeasonId != tournamentInfo.tournament.id)
            {
                SetupTournamentSummary(summaryFetcherMock, tournamentInfo, currentSeasonId, language);
            }

            var seasonId = tournamentInfo.season?.id;
            if (seasonId != null && seasonId != tournamentInfo.tournament.id && seasonId != currentSeasonId)
            {
                SetupTournamentSummary(summaryFetcherMock, tournamentInfo, seasonId, language);
            }
        }
    }

    private void SetupTournamentSummary(Mock<IDataFetcher> summaryFetcherMock, tournamentInfoEndpoint tournamentInfo, string sportEventId, CultureInfo language)
    {
        var summaryUri = tournamentInfo.GetTournamentSummaryUri(_uofConfig.Api.BaseUrl, language, sportEventId);
        summaryFetcherMock.Setup(fetcher => fetcher.GetDataAsync(summaryUri))
                          .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(tournamentInfo)));
    }

    private void SetupSummaryEndpointReturning(Mock<IDataFetcher> summaryFetcherMock, matchSummaryEndpoint matchSummary)
    {
        foreach (var language in _uofConfig.Languages)
        {
            var summaryUri = matchSummary.GetMatchSummaryUri(_uofConfig.Api.BaseUrl, language);
            summaryFetcherMock.Setup(fetcher => fetcher.GetDataAsync(summaryUri))
                              .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(matchSummary)));
        }
    }

    private void SetupFixtureEndpointReturning(Mock<IDataFetcher> fixtureFetcherMock, fixturesEndpoint fixturesEndpoint)
    {
        foreach (var language in _uofConfig.Languages)
        {
            var fixtureUri = fixturesEndpoint.GetFixtureUri(_uofConfig.Api.BaseUrl, language);
            fixtureFetcherMock.Setup(fetcher => fetcher.GetDataAsync(fixtureUri))
                              .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(fixturesEndpoint)));
        }
    }

    private void SetupLotteriesEndpointReturning(Mock<IDataFetcher> lotteriesFetcherMock, lotteries lotteriesResponse)
    {
        foreach (var language in _uofConfig.Languages)
        {
            var lotteriesUri = lotteriesResponse.GetLotteriesUri(_uofConfig.Api.BaseUrl, language);
            lotteriesFetcherMock.Setup(fetcher => fetcher.GetDataAsync(lotteriesUri))
                                .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(lotteriesResponse)));
        }
    }

    private void SetupLotteryScheduleEndpointReturning(Mock<IDataFetcher> lotteryScheduleFetcherMock, lottery_schedule lotteryScheduleResponse)
    {
        foreach (var language in _uofConfig.Languages)
        {
            var scheduleUri = lotteryScheduleResponse.GetLotteryScheduleUri(_uofConfig.Api.BaseUrl, language);
            lotteryScheduleFetcherMock.Setup(fetcher => fetcher.GetDataAsync(scheduleUri))
                                      .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(lotteryScheduleResponse)));
        }
    }

    private void SetupDrawSummaryEndpointReturning(Mock<IDataFetcher> drawSummaryFetcherMock, draw_summary drawSummaryResponse)
    {
        foreach (var language in _uofConfig.Languages)
        {
            var summaryUri = drawSummaryResponse.GetDrawSummaryUri(_uofConfig.Api.BaseUrl, language);
            drawSummaryFetcherMock.Setup(fetcher => fetcher.GetDataAsync(summaryUri))
                                  .Returns(() => Task.FromResult(DeserializerHelper.SerializeToMemoryStream(drawSummaryResponse)));
        }
    }
}
