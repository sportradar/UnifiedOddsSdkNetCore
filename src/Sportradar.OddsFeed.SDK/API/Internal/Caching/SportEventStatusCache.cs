/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    /// <summary>
    /// Implementation of <see cref="ISportEventStatusCache"/>
    /// </summary>
    /// <seealso cref="ISportEventStatusCache" />
    internal class SportEventStatusCache : SdkCache, ISportEventStatusCache
    {
        /// <summary>
        /// A <see cref="ISingleTypeMapperFactory{restSportEventStatus, SportEventStatusDto}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDto}"/> instances
        /// </summary>
        private readonly ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto> _mapperFactory;

        /// <summary>
        /// A <see cref="ICacheStore{T}"/> used to cache <see cref="ISportEventStatus"/> instances
        /// </summary>
        private readonly ICacheStore<string> _sportEventStatusCache;
        private readonly ICacheStore<string> _ignoreEventsTimelineCache;
        private readonly ISportEventCache _sportEventCache;
        private readonly bool _ignoreBetPalTimelineSportEventStatus;

        /// <summary>
        /// Value indicating whether the current instance was already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to synchronize multi-threaded fetching
        /// </summary>
        private readonly SemaphoreSlim _fetchSemaphore = new SemaphoreSlim(1);

        /// <summary>
        /// A <see cref="object"/> to ensure thread safety when adding items to cache
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ISportEventStatusCache"/> class
        /// </summary>
        /// <param name="sportEventStatusCache">A <see cref="ICacheStore{T}"/> used to cache <see cref="SportEventStatusCacheItem"/> instances</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{TIn,TOut}"/> used to created <see cref="ISingleTypeMapper{SportEventStatusDto}"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to cache <see cref="ISportEvent"/></param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="ignoreEventsTimelineCache">A <see cref="ICacheStore{T}"/> used to cache event ids for which the SES from timeline endpoint should be ignored</param>
        /// <param name="config">The configuration to get IgnoreBetPalTimelineSportEventStatus</param>
        public SportEventStatusCache(ICacheStore<string> sportEventStatusCache,
                                     ISingleTypeMapperFactory<sportEventStatus, SportEventStatusDto> mapperFactory,
                                     ISportEventCache sportEventCache,
                                     ICacheManager cacheManager,
                                     ICacheStore<string> ignoreEventsTimelineCache,
                                     IUofConfiguration config)
            : base(cacheManager)
        {
            Guard.Argument(sportEventStatusCache, nameof(sportEventStatusCache)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(ignoreEventsTimelineCache, nameof(ignoreEventsTimelineCache)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();

            _sportEventStatusCache = sportEventStatusCache;
            _mapperFactory = mapperFactory;
            _sportEventCache = sportEventCache;
            _ignoreEventsTimelineCache = ignoreEventsTimelineCache;
            _ignoreBetPalTimelineSportEventStatus = config.Cache.IgnoreBetPalTimelineSportEventStatus;

            _isDisposed = false;
        }

        /// <summary>
        /// Gets the cached <see cref="SportEventStatusCacheItem" /> instance associated with the sport event specified by the <c>eventId</c>. If the instance associated
        /// with the specified event is not found, it tries to obtain it via API, if still cant, a <see cref="SportEventStatusCacheItem" /> instance indicating a 'not started' event is returned.
        /// </summary>
        /// <param name="eventId">A <see cref="Urn" /> representing the id of the sport event whose status to get</param>
        /// <returns>A <see cref="SportEventStatusCacheItem" /> representing the status of the specified sport event</returns>
        public async Task<SportEventStatusCacheItem> GetSportEventStatusAsync(Urn eventId)
        {
            if (_isDisposed)
            {
                return null;
            }

            Guard.Argument(eventId, nameof(eventId)).NotNull();

            using (new TelemetryTracker(UofSdkTelemetry.SportEventStatusCacheGetSportEventStatus))
            {
                try
                {
                    // fetch from api
                    await _fetchSemaphore.WaitAsync().ConfigureAwait(false);

                    var item = _sportEventStatusCache.Get(eventId.ToString());

                    if (item != null)
                    {
                        return (SportEventStatusCacheItem)item;
                    }

                    if (_sportEventCache.GetEventCacheItem(eventId) is ICompetitionCacheItem cachedEvent)
                    {
                        await cachedEvent.FetchSportEventStatusAsync().ConfigureAwait(false);
                    }

                    item = _sportEventStatusCache.Get(eventId.ToString());

                    if (item != null)
                    {
                        return (SportEventStatusCacheItem)item;
                    }
                }
                finally
                {
                    _fetchSemaphore.ReleaseSafe();
                }
            }

            return ((SportEventStatusMapperBase)_mapperFactory).CreateNotStarted();
        }

        /// <inheritdoc />
        public void AddEventIdForTimelineIgnore(Urn eventId, int producerId, Type messageType)
        {
            if (producerId == 4) // BetPal
            {
                if (_ignoreEventsTimelineCache.Contains(eventId.ToString()))
                {
                    _ignoreEventsTimelineCache.Get(eventId.ToString()); // to update sliding expiration
                }
                else
                {
                    CacheLog.LogDebug("Received {MessageType} - added {EventId} to the ignore timeline list", messageType.Name, eventId);
                    _ignoreEventsTimelineCache.Add(eventId.ToString(), DateTime.Now.ToString());
                }
            }
        }

        /// <summary>
        /// Adds the sport event status to the internal cache
        /// </summary>
        /// <param name="eventId">The eventId of the sport event status to be cached</param>
        /// <param name="sportEventStatus">The sport event status to be cached</param>
        /// <param name="statusOnEvent">The sport event status received directly on event level</param>
        /// <param name="source">The source of the SES</param>
        private void AddSportEventStatus(Urn eventId, SportEventStatusCacheItem sportEventStatus, string statusOnEvent, string source)
        {
            if (_isDisposed)
            {
                return;
            }

            if (sportEventStatus == null)
            {
                return;
            }

            Guard.Argument(eventId, nameof(eventId)).NotNull();

            if (string.IsNullOrEmpty(source) ||
                source.Equals("OddsChange", StringComparison.InvariantCultureIgnoreCase) ||
                source.Equals("SportEventSummary", StringComparison.InvariantCultureIgnoreCase) ||
                !_sportEventStatusCache.Contains(eventId.ToString()))
            {
                lock (_lock)
                {
                    if (!string.IsNullOrEmpty(source))
                    {
                        if (_ignoreBetPalTimelineSportEventStatus && source.Contains("Timeline") && _ignoreEventsTimelineCache.Contains(eventId.ToString()))
                        {
                            ExecutionLog.LogDebug("Received SES for {EventId} from {Source} with EventStatus:{SportEventStatus} (timeline ignored) {StatusOnEvent}", eventId, source, sportEventStatus.Status, statusOnEvent);
                            return;
                        }

                        source = $" from {source}";
                    }

                    ExecutionLog.LogDebug("Received SES for {EventId}{Source} with EventStatus:{SportEventStatus}", eventId, source, sportEventStatus.Status);
                    var cacheItem = (SportEventStatusCacheItem)_sportEventStatusCache.Get(eventId.ToString());
                    if (cacheItem != null)
                    {
                        cacheItem.SetFeedStatus(sportEventStatus.FeedStatusDto);
                        cacheItem.SetSapiStatus(sportEventStatus.SapiStatusDto);
                    }
                    else
                    {
                        _sportEventStatusCache.Add(eventId.ToString(), sportEventStatus);
                    }
                }
            }
            else
            {
                ExecutionLog.LogDebug("Received SES for {EventId}{Source} with EventStatus:{SportEventStatus} (ignored)", eventId, source, sportEventStatus.Status);
            }
        }

        /// <summary>
        /// Set the list of <see cref="DtoType"/> in the this cache
        /// </summary>
        public override void SetDtoTypes()
        {
            RegisteredDtoTypes = new List<DtoType>
                                 {
                                     DtoType.Fixture,
                                     DtoType.MatchSummary,
                                     DtoType.MatchTimeline,
                                     DtoType.RaceSummary,
                                     DtoType.SportEventStatus,
                                     DtoType.SportEventSummary,
                                     DtoType.SportEventSummaryList
                                 };
        }

        /// <summary>
        /// Purges item from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item in the cache to be purged</param>
        /// <param name="cacheItemType">A cache item type</param>
        public override void CacheDeleteItem(Urn id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull();

            if (_isDisposed)
            {
                return;
            }

            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEventStatus)
            {
                _sportEventStatusCache.Remove(id.ToString());
            }
        }

        /// <summary>
        /// Does item exists in the cache
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the id of the item to be checked</param>
        /// <param name="cacheItemType">A cache item type</param>
        /// <returns><c>true</c> if exists, <c>false</c> otherwise</returns>
        public override bool CacheHasItem(Urn id, CacheItemType cacheItemType)
        {
            Guard.Argument(id, nameof(id)).NotNull();

            if (_isDisposed)
            {
                return false;
            }
            var result = false;
            if (cacheItemType == CacheItemType.All || cacheItemType == CacheItemType.SportEventStatus)
            {
                result = _sportEventStatusCache.Contains(id.ToString());
            }
            return result;
        }

        /// <inheritdoc />
        protected override Task<bool> CacheAddDtoItemAsync(Urn id, object item, CultureInfo culture, DtoType dtoType, ISportEventCacheItem requester)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(item, nameof(item)).NotNull();

            if (_isDisposed)
            {
                return Task.FromResult(false);
            }

            var saved = false;
            switch (dtoType)
            {
                case DtoType.Category:
                    break;
                case DtoType.Competitor:
                    break;
                case DtoType.CompetitorProfile:
                    break;
                case DtoType.SimpleTeamProfile:
                    break;
                case DtoType.Fixture:
                    if (item is FixtureDto fixtureDto)
                    {
                        if (fixtureDto.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCacheItem(null, fixtureDto.SportEventStatus), fixtureDto.StatusOnEvent, "Fixture");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(FixtureDto), item.GetType());
                    }
                    break;
                case DtoType.MarketDescription:
                    break;
                case DtoType.MatchSummary:
                    if (item is MatchDto matchDto)
                    {
                        if (matchDto.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCacheItem(null, matchDto.SportEventStatus), matchDto.StatusOnEvent, "Match");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchDto), item.GetType());
                    }
                    break;
                case DtoType.MatchTimeline:
                    if (item is MatchTimelineDto matchTimelineDto)
                    {
                        if (matchTimelineDto.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCacheItem(null, matchTimelineDto.SportEventStatus), matchTimelineDto.SportEvent.StatusOnEvent, "MatchTimeline");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(MatchTimelineDto), item.GetType());
                    }
                    break;
                case DtoType.PlayerProfile:
                    break;
                case DtoType.RaceSummary:
                    if (item is StageDto stageDto)
                    {
                        if (stageDto.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCacheItem(null, stageDto.SportEventStatus), stageDto.StatusOnEvent, "Stage");
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(StageDto), item.GetType());
                    }
                    break;
                case DtoType.Sport:
                    break;
                case DtoType.SportList:
                    break;
                case DtoType.SportEventStatus:
                    if (item is SportEventStatusDto sportEventStatusDto)
                    {
                        AddSportEventStatus(id, new SportEventStatusCacheItem(sportEventStatusDto, null), sportEventStatusDto.Status.ToString(), "OddsChange");
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(SportEventStatusDto), item.GetType());
                    }
                    break;
                case DtoType.SportEventSummary:
                    if (item is CompetitionDto competitionDto)
                    {
                        if (competitionDto.SportEventStatus != null)
                        {
                            AddSportEventStatus(id, new SportEventStatusCacheItem(null, competitionDto.SportEventStatus), competitionDto.StatusOnEvent, "SportEventSummary");
                        }
                        saved = true;
                    }
                    break;
                case DtoType.SportEventSummaryList:
                    if (item is EntityList<SportEventSummaryDto> summaryList)
                    {
                        foreach (var s in summaryList.Items)
                        {
                            var compDto = s as CompetitionDto;
                            if (compDto?.SportEventStatus != null)
                            {
                                AddSportEventStatus(id, new SportEventStatusCacheItem(null, compDto.SportEventStatus), s.StatusOnEvent, "SportEventSummaryList");
                            }
                        }
                        saved = true;
                    }
                    else
                    {
                        LogSavingDtoConflict(id, typeof(EntityList<SportEventSummaryDto>), item.GetType());
                    }
                    break;
                case DtoType.Tournament:
                    break;
                case DtoType.TournamentInfo:
                    break;
                case DtoType.TournamentSeasons:
                    break;
                case DtoType.MarketDescriptionList:
                    break;
                case DtoType.VariantDescription:
                    break;
                case DtoType.VariantDescriptionList:
                    break;
                case DtoType.Lottery:
                    break;
                case DtoType.LotteryDraw:
                    break;
                case DtoType.LotteryList:
                    break;
                case DtoType.BookingStatus:
                    break;
                case DtoType.SportCategories:
                    break;
                case DtoType.AvailableSelections:
                    break;
                case DtoType.TournamentInfoList:
                    break;
                case DtoType.PeriodSummary:
                    break;
                case DtoType.Calculation:
                    break;
                default:
                    ExecutionLog.LogWarning("Trying to add unchecked dto type: {DtoType} for id: {EventId}", dtoType, id);
                    break;
            }
            return Task.FromResult(saved);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (disposing)
            {
                _fetchSemaphore.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(HealthCheckResult.Healthy($"Cache has {_sportEventStatusCache.Count().ToString()} items"));
        }
    }
}
