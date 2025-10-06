// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// A factory used to build <see cref="SportEventCacheItem"/> instances
    /// </summary>
    /// <seealso cref="ISportEventCacheItemFactory" />
    internal class SportEventCacheItemFactory : ISportEventCacheItemFactory
    {
        /// <summary>
        /// A <see cref="IDataRouterManager"/> used to fetch sport event summary and fixture
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// A <see cref="ISemaphorePool"/> instance to be used by instances constructed by this factory
        /// </summary>
        private readonly ISemaphorePool _semaphorePool;

        /// <summary>
        /// A <see cref="CultureInfo"/> specifying the default culture of the built cache items
        /// </summary>
        private readonly CultureInfo _defaultCulture;

        /// <summary>
        /// A <see cref="ICacheStore{T}"/> which will be used to cache the sport events fixture timestamps
        /// </summary>
        private readonly ICacheStore<string> _fixtureTimestampCacheStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCacheItemFactory"/> class.
        /// </summary>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool"/> instance to be used by instances constructed by this factory</param>
        /// <param name="configuration">A configuration to get default language</param>
        /// <param name="fixtureTimestampCacheStore">The in-memory cache of sport events fixture timestamps</param>
        public SportEventCacheItemFactory(IDataRouterManager dataRouterManager, ISemaphorePool semaphorePool, IUofConfiguration configuration, ICacheStore<string> fixtureTimestampCacheStore)
        {
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();
            Guard.Argument(semaphorePool, nameof(semaphorePool)).NotNull();
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(fixtureTimestampCacheStore, nameof(fixtureTimestampCacheStore)).NotNull();

            _dataRouterManager = dataRouterManager;
            _semaphorePool = semaphorePool;
            _defaultCulture = configuration.DefaultLanguage;
            _fixtureTimestampCacheStore = fixtureTimestampCacheStore;
        }

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem" /> instance from the provided sport event id
        /// </summary>
        /// <param name="eventId">A <see cref="Urn"/> identifying the sport event</param>
        /// <returns>A new instance of <see cref="SportEventCacheItem" /> instance</returns>
        public SportEventCacheItem Build(Urn eventId)
        {
            if (eventId.TypeGroup == ResourceTypeGroup.Stage)
            {
                return new StageCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.Match)
            {
                return new MatchCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.Season
                || eventId.TypeGroup == ResourceTypeGroup.BasicTournament
                || eventId.TypeGroup == ResourceTypeGroup.Tournament)
            {
                return new TournamentInfoCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.Draw)
            {
                return new DrawCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.Lottery)
            {
                return new LotteryCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }
            return new SportEventCacheItem(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem" /> instance from the provided <see cref="SportEventSummaryDto" /> instance
        /// </summary>
        /// <param name="eventSummary">A <see cref="SportEventSummaryDto" /> instance containing basic sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the <see cref="SportEventSummaryDto"/> data</param>
        /// <returns>a new instance of <see cref="SportEventCacheItem" /> instance</returns>
        public SportEventCacheItem Build(SportEventSummaryDto eventSummary, CultureInfo currentCulture)
        {
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.Stage)
            {
                var item = eventSummary as StageDto;
                if (item != null)
                {
                    return new StageCacheItem(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
                var tour = eventSummary as TournamentInfoDto;
                if (tour != null)
                {
                    return new StageCacheItem(tour, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.Match)
            {
                var item = eventSummary as MatchDto;
                if (item != null)
                {
                    return new MatchCacheItem(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.Season
                || eventSummary.Id.TypeGroup == ResourceTypeGroup.BasicTournament
                || eventSummary.Id.TypeGroup == ResourceTypeGroup.Tournament)
            {
                var item = eventSummary as TournamentInfoDto;
                if (item != null)
                {
                    return new TournamentInfoCacheItem(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.Draw)
            {
                var item = eventSummary as DrawDto;
                if (item != null)
                {
                    return new DrawCacheItem(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.Lottery)
            {
                var item = eventSummary as LotteryDto;
                if (item != null)
                {
                    return new LotteryCacheItem(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
                }
            }
            return new SportEventCacheItem(eventSummary, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem" /> instance from the provided <see cref="FixtureDto" /> instance
        /// </summary>
        /// <param name="fixture">A <see cref="FixtureDto" /> instance containing fixture (pre-match) related sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the <see cref="FixtureDto"/> data</param>
        /// <returns>a new instance of <see cref="SportEventCacheItem" /> instance</returns>
        public SportEventCacheItem Build(FixtureDto fixture, CultureInfo currentCulture)
        {
            if (fixture.Id.TypeGroup == ResourceTypeGroup.Stage)
            {
                return new StageCacheItem(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.Match)
            {
                return new MatchCacheItem(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.Season
                || fixture.Id.TypeGroup == ResourceTypeGroup.BasicTournament
                || fixture.Id.TypeGroup == ResourceTypeGroup.Tournament)
            {
                return new TournamentInfoCacheItem(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.Draw || fixture.Id.TypeGroup == ResourceTypeGroup.Lottery)
            {
                // should not be any fixture
            }
            return new SportEventCacheItem(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCacheStore);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCacheItem"/> instance from the provided exportable cache item
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableBase"/> representing the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCacheItem"/> instance</returns>
        public SportEventCacheItem Build(ExportableBase exportable)
        {
            var exportableSportEvent = exportable as ExportableSportEvent;
            if (exportableSportEvent == null)
            {
                return null;
            }

            if (exportable is ExportableStage extStageCacheItem)
            {
                return new StageCacheItem(extStageCacheItem, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }

            if (exportable is ExportableMatch extMatchCacheItem)
            {
                return new MatchCacheItem(extMatchCacheItem, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }

            if (exportable is ExportableTournamentInfo extTournamentInfoCacheItem)
            {
                return new TournamentInfoCacheItem(extTournamentInfoCacheItem, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }

            if (exportable is ExportableLottery extLotteryCacheItem)
            {
                return new LotteryCacheItem(extLotteryCacheItem, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }

            if (exportable is ExportableDraw extDrawCacheItem)
            {
                return new DrawCacheItem(extDrawCacheItem, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
            }

            return new SportEventCacheItem(exportableSportEvent, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCacheStore);
        }

        /// <summary>
        /// Gets a derived <see cref="SportEventCacheItem" /> instance from the cache object
        /// </summary>
        /// <param name="cacheItem">A <see cref="SportEventCacheItem" /> instance from the cache</param>
        /// <returns>A new instance of <see cref="SportEventCacheItem" /> instance</returns>
        public SportEventCacheItem Get(object cacheItem)
        {
            if (cacheItem is StageCacheItem stage)
            {
                return stage;
            }

            if (cacheItem is MatchCacheItem match)
            {
                return match;
            }

            if (cacheItem is TournamentInfoCacheItem tour)
            {
                return tour;
            }

            if (cacheItem is DrawCacheItem draw)
            {
                return draw;
            }

            if (cacheItem is LotteryCacheItem lottery)
            {
                return lottery;
            }
            return (SportEventCacheItem)cacheItem;
        }

        /// <summary>
        /// Gets a <see cref="ICacheStore{T}"/> used to cache fixture timestamps
        /// </summary>
        /// <returns>A <see cref="ICacheStore{T}"/> used to cache fixture timestamps</returns>
        public ICacheStore<string> GetFixtureTimestampCache()
        {
            return _fixtureTimestampCacheStore;
        }
    }
}
