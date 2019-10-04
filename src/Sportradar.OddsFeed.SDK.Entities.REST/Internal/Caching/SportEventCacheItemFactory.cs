/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Caching;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A factory used to build <see cref="SportEventCI"/> instances
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
        /// A <see cref="ObjectCache"/> which will be used to cache the sport events fixture timestamps
        /// </summary>
        private readonly ObjectCache _fixtureTimestampCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEventCacheItemFactory"/> class.
        /// </summary>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to obtain summary and fixture</param>
        /// <param name="semaphorePool">A <see cref="ISemaphorePool"/> instance to be used by instances constructed by this factory</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo"/> specifying the default culture of the built cache items</param>
        /// <param name="fixtureTimestampCache">The in-memory cache of sport events fixture timestamps</param>
        public SportEventCacheItemFactory(IDataRouterManager dataRouterManager, ISemaphorePool semaphorePool, CultureInfo defaultCulture, ObjectCache fixtureTimestampCache)
        {
            Contract.Requires(dataRouterManager != null);
            Contract.Requires(semaphorePool != null);
            Contract.Requires(defaultCulture != null);
            Contract.Requires(fixtureTimestampCache != null);

            _dataRouterManager = dataRouterManager;
            _semaphorePool = semaphorePool;
            _defaultCulture = defaultCulture;
            _fixtureTimestampCache = fixtureTimestampCache;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_dataRouterManager != null);
            Contract.Invariant(_defaultCulture != null);
            Contract.Invariant(_fixtureTimestampCache != null);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCI" /> instance from the provided sport event id
        /// </summary>
        /// <param name="eventId">A <see cref="URN"/> identifying the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCI" /> instance</returns>
        public SportEventCI Build(URN eventId)
        {
            if (eventId.TypeGroup == ResourceTypeGroup.STAGE)
            {
                return new StageCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.MATCH)
            {
                return new MatchCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.SEASON
                || eventId.TypeGroup == ResourceTypeGroup.BASIC_TOURNAMENT
                || eventId.TypeGroup == ResourceTypeGroup.TOURNAMENT)
            {
                return new TournamentInfoCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.DRAW)
            {
                return new DrawCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.LOTTERY)
            {
                return new LotteryCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            return new SportEventCI(eventId, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCI" /> instance from the provided <see cref="SportEventSummaryDTO" /> instance
        /// </summary>
        /// <param name="eventSummary">A <see cref="SportEventSummaryDTO" /> instance containing basic sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the <see cref="SportEventSummaryDTO"/> data</param>
        /// <returns>a new instance of <see cref="SportEventCI" /> instance</returns>
        public SportEventCI Build(SportEventSummaryDTO eventSummary, CultureInfo currentCulture)
        {
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.STAGE)
            {
                var item = eventSummary as StageDTO;
                if (item != null)
                {
                    return new StageCI(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
                var tour = eventSummary as TournamentInfoDTO;
                if (tour != null)
                {
                    return new StageCI(tour, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.MATCH)
            {
                var item = eventSummary as MatchDTO;
                if (item != null)
                {
                    return new MatchCI(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.SEASON
                || eventSummary.Id.TypeGroup == ResourceTypeGroup.BASIC_TOURNAMENT
                || eventSummary.Id.TypeGroup == ResourceTypeGroup.TOURNAMENT)
            {
                var item = eventSummary as TournamentInfoDTO;
                if (item != null)
                {
                    return new TournamentInfoCI(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.DRAW)
            {
                var item = eventSummary as DrawDTO;
                if (item != null)
                {
                    return new DrawCI(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
            }
            if (eventSummary.Id.TypeGroup == ResourceTypeGroup.LOTTERY)
            {
                var item = eventSummary as LotteryDTO;
                if (item != null)
                {
                    return new LotteryCI(item, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
                }
            }
            return new SportEventCI(eventSummary, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCI" /> instance from the provided <see cref="FixtureDTO" /> instance
        /// </summary>
        /// <param name="fixture">A <see cref="FixtureDTO" /> instance containing fixture (pre-match) related sport event info</param>
        /// <param name="currentCulture">A <see cref="CultureInfo"/> of the <see cref="FixtureDTO"/> data</param>
        /// <returns>a new instance of <see cref="SportEventCI" /> instance</returns>
        public SportEventCI Build(FixtureDTO fixture, CultureInfo currentCulture)
        {
            if (fixture.Id.TypeGroup == ResourceTypeGroup.STAGE)
            {
                return new StageCI(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.MATCH)
            {
                return new MatchCI(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.SEASON
                || fixture.Id.TypeGroup == ResourceTypeGroup.BASIC_TOURNAMENT
                || fixture.Id.TypeGroup == ResourceTypeGroup.TOURNAMENT)
            {
                return new TournamentInfoCI(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
            }
            if (fixture.Id.TypeGroup == ResourceTypeGroup.DRAW
                || fixture.Id.TypeGroup == ResourceTypeGroup.LOTTERY)
            {
                // should not be any fixture
                var a = 1;
            }
            return new SportEventCI(fixture, _dataRouterManager, _semaphorePool, currentCulture, _defaultCulture, _fixtureTimestampCache);
        }

        /// <summary>
        /// Builds a <see cref="SportEventCI"/> instance from the provided exportable cache item
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCI"/> representing the the sport event</param>
        /// <returns>a new instance of <see cref="SportEventCI"/> instance</returns>
        public SportEventCI Build(ExportableCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));
            var exportableSportEvent = exportable as ExportableSportEventCI;
            if (exportableSportEvent == null)
                throw new ArgumentOutOfRangeException(nameof(exportableSportEvent), "The exportable must be a subclass of ExportableSportEventCI");

            var eventId = URN.Parse(exportable.Id);
            if (eventId.TypeGroup == ResourceTypeGroup.STAGE)
            {
                return new StageCI(exportableSportEvent as ExportableStageCI, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.MATCH)
            {
                return new MatchCI(exportableSportEvent as ExportableMatchCI, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.SEASON
                || eventId.TypeGroup == ResourceTypeGroup.BASIC_TOURNAMENT
                || eventId.TypeGroup == ResourceTypeGroup.TOURNAMENT)
            {
                return new TournamentInfoCI(exportableSportEvent as ExportableTournamentInfoCI, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.DRAW)
            {
                return new DrawCI(exportableSportEvent as ExportableDrawCI, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            if (eventId.TypeGroup == ResourceTypeGroup.LOTTERY)
            {
                return new LotteryCI(exportableSportEvent as ExportableLotteryCI, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
            }
            return new SportEventCI(exportableSportEvent, _dataRouterManager, _semaphorePool, _defaultCulture, _fixtureTimestampCache);
        }

        /// <summary>
        /// Gets a derived <see cref="SportEventCI" /> instance from the cache object
        /// </summary>
        /// <param name="cacheItem">A <see cref="SportEventCI" /> instance from the cache</param>
        /// <returns>A new instance of <see cref="SportEventCI" /> instance</returns>
        public SportEventCI Get(object cacheItem)
        {
            var stage = cacheItem as StageCI;
            if (stage != null)
            {
                return stage;
            }
            var match = cacheItem as MatchCI;
            if (match != null)
            {
                return match;
            }
            var tour = cacheItem as TournamentInfoCI;
            if (tour != null)
            {
                return tour;
            }
            var draw = cacheItem as DrawCI;
            if (draw != null)
            {
                return draw;
            }
            var lottery = cacheItem as LotteryCI;
            if (lottery != null)
            {
                return lottery;
            }
            return (SportEventCI) cacheItem;
        }

        /// <summary>
        /// Gets a <see cref="ObjectCache"/> used to cache fixture timestamps
        /// </summary>
        /// <returns>A <see cref="ObjectCache"/> used to cache fixture timestamps</returns>
        public ObjectCache GetFixtureTimestampCache()
        {
            return _fixtureTimestampCache;
        }
    }
}