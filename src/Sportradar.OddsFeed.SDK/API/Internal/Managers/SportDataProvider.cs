﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// Provides access to sport related data (sports, tournaments, sport events, ...)
    /// </summary>
    internal class SportDataProvider : ISportDataProvider
    {
        private static readonly HashSet<Urn> PrefetchableSportEvents = new HashSet<Urn>
                                                                           {
                                                                               Urn.Parse("sr:simple_tournament:86"),
                                                                               Urn.Parse("sr:tournament:853")
                                                                           };

        private static readonly ILogger LogInt = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(SportDataProvider));
        private static readonly RequestOptions NonTimeCriticalRequestOptions = new RequestOptions(ExecutionPath.NonTimeCritical);

        /// <summary>
        /// A <see cref="ISportEntityFactory"/> used to construct <see cref="ITournament"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// A <see cref="ISportEventCache"/> used to retrieve schedules for sport events
        /// </summary>
        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// A <see cref="ISportEventStatusCache"/> used to retrieve status for sport event
        /// </summary>
        private readonly ISportEventStatusCache _sportEventStatusCache;

        /// <summary>
        /// The profile cache used to retrieve competitor or player profile
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// The sport data cache used to retrieve sport data
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// A <see cref="IList{T}"/> specified as default cultures (from configuration)
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _defaultCultures;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying enum member specifying how instances provided by the current provider will handle exceptions
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// The cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// The match status cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusCache;

        /// <summary>
        /// The data router manager
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDataProvider"/> class
        /// </summary>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to construct <see cref="ITournament"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve schedules for sport events</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> used to retrieve status for sport event</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used to retrieve competitor or player profile</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> used to retrieve sport data</param>
        /// <param name="defaultCultures"> A <see cref="IList{CultureInfo}"/> specified as default cultures (from configuration)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying enum member specifying how instances provided by the current provider will handle exceptions</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="matchStatusCache">A <see cref="ILocalizedNamedValueCache"/> used to retrieve match statuses</param>
        /// <param name="dataRouterManager">A <see cref="IDataRouterManager"/> used to invoke API requests</param>
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed")]
        public SportDataProvider(ISportEntityFactory sportEntityFactory,
                                 ISportEventCache sportEventCache,
                                 ISportEventStatusCache sportEventStatusCache,
                                 IProfileCache profileCache,
                                 ISportDataCache sportDataCache,
                                 IEnumerable<CultureInfo> defaultCultures,
                                 ExceptionHandlingStrategy exceptionStrategy,
                                 ICacheManager cacheManager,
                                 ILocalizedNamedValueCache matchStatusCache,
                                 IDataRouterManager dataRouterManager)
        {
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(defaultCultures, nameof(defaultCultures)).NotNull();
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();
            Guard.Argument(matchStatusCache, nameof(matchStatusCache)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportEventCache = sportEventCache;
            _sportEventStatusCache = sportEventStatusCache;
            _profileCache = profileCache;
            _sportDataCache = sportDataCache;
            _defaultCultures = defaultCultures as IReadOnlyCollection<CultureInfo>;
            _exceptionStrategy = exceptionStrategy;
            _cacheManager = cacheManager;
            _matchStatusCache = matchStatusCache;
            _dataRouterManager = dataRouterManager;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ISport}"/> representing all available sports in language specified by the <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISport>> GetSportsAsync(CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            try
            {
                LogInt.LogInformation("Invoked GetSportsAsync: [Cultures={Langs}]", s);
                var result = await _sportEntityFactory.BuildSportsAsync(cs, _exceptionStrategy).ConfigureAwait(false);

                return result;
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetSportsAsync: [Cultures={Langs}]", s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISport"/> instance representing the sport specified by it's id in the language specified by <c>culture</c>, or a null reference if sport with specified id does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> identifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{ISport}"/> representing the async operation</returns>
        public async Task<ISport> GetSportAsync(Urn id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            try
            {
                LogInt.LogInformation("Invoked GetSportsAsync: [Id={SportId}, Cultures={Langs}]", id, s);
                return await _sportEntityFactory.BuildSportAsync(id, cs, _exceptionStrategy).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetSportsAsync: [Id={SportId}, Cultures={Langs}]", id, s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing currently live sport events in the language specified by <c>culture</c>
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetLiveSportEventsAsync(CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetLiveSportEventsAsync: [Cultures={Langs}]", s);

            var tasks = cs.Select(c => _sportEventCache.GetEventIdsAsync((DateTime?)null, c)).ToList();

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetLiveSportEventsAsync: [Cultures={Langs}]", s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }

            var ids = tasks.First().GetAwaiter().GetResult().ToList();
            if (ids.IsNullOrEmpty())
            {
                return new List<ICompetition>();
            }
            return ids.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                               item.Item2,
                                                                                               culture == null ? _defaultCultures.ToList() : new List<CultureInfo> { culture },
                                                                                               _exceptionStrategy)).ToList();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{ICompetition}"/> representing sport events scheduled for date specified by <c>date</c> in language specified by <c>culture</c>
        /// </summary>
        /// <param name="scheduleDate">A <see cref="DateTime"/> specifying the day for which to retrieve the schedule</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetSportEventsByDateAsync(DateTime scheduleDate, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetSportEventsByDateAsync: [Date={ScheduleDate}, Cultures={Langs}]", scheduleDate, s);

            var tasks = cs.Select(c => _sportEventCache.GetEventIdsAsync(scheduleDate, c)).ToList();
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetSportEventsByDateAsync: [Date={ScheduleDate}, Cultures={Langs}]", scheduleDate, s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }

            var ids = tasks.First().GetAwaiter().GetResult().ToList();
            if (ids.IsNullOrEmpty())
            {
                return new List<ICompetition>();
            }
            return ids.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                        item.Item2,
                                                                                        culture == null ? _defaultCultures.ToList() : new List<CultureInfo> { culture },
                                                                                        _exceptionStrategy)).ToList();
        }

        /// <summary>
        /// Gets a <see cref="ILongTermEvent"/> representing the specified tournament in language specified by <c>culture</c> or a null reference if the tournament with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the tournament to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ILongTermEvent"/> representing the specified tournament or a null reference if requested tournament does not exist</returns>
        public ILongTermEvent GetTournament(Urn id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetTournament: [Id={SportEventId}, Cultures={Langs}]", id, s);

            var defaultCultures = culture == null ? _defaultCultures.ToList() : new List<CultureInfo> { culture };
            var result = _sportEntityFactory.BuildSportEvent<ILongTermEvent>(id,
                                                                             null,
                                                                             defaultCultures,
                                                                             _exceptionStrategy);

            LogInt.LogInformation("GetTournament returned: {SportEventId}", result?.Id);

            EnsureClubFriendlySimpleTournamentSummaryIsFetchedWithNonCriticalPathTimeout(defaultCultures, result).GetAwaiter().GetResult();

            return result;
        }

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <c>culture</c> or a null reference if the sport event with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the sport event to retrieve</param>
        /// <param name="sportId">A <see cref="Urn"/> of the sport this event belongs to</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        public ICompetition GetCompetition(Urn id, Urn sportId, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetCompetition: [Id={SportEventId}, SportId={SportId}, Cultures={Langs}]", id, sportId, s);

            if (sportId == null && id.TypeGroup.Equals(ResourceTypeGroup.Match))
            {
                sportId = _sportEventCache.GetEventSportIdAsync(id).GetAwaiter().GetResult();
            }

            var result = _sportEntityFactory.BuildSportEvent<ICompetition>(id,
                                                                           sportId,
                                                                           culture == null ? _defaultCultures.ToList() : new List<CultureInfo> { culture },
                                                                           _exceptionStrategy);

            LogInt.LogInformation("GetCompetition returned: {SportEventId}", result?.Id);
            return result;
        }

        /// <summary>
        /// Gets a <see cref="ICompetition"/> representing the specified sport event in language specified by <c>culture</c> or a null reference if the sport event with
        /// specified <c>id</c> does not exist
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the sport event to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetition"/> representing the specified sport event or a null reference if the requested sport event does not exist</returns>
        /// <remarks>It is recommended to use the GetCompetition method with sportId</remarks>
        public ICompetition GetCompetition(Urn id, CultureInfo culture = null)
        {
            return GetCompetition(id, null, culture);
        }

        /// <inheritdoc />
        public ISportEvent GetSportEvent(Urn id, Urn sportId = null, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetSportEvent: [Id={SportEventId}, SportId={SportId}, Cultures={Langs}]", id, sportId, s);

            var cultures = culture == null ? _defaultCultures.ToList() : new List<CultureInfo> { culture };
            var result = _sportEntityFactory.BuildSportEvent<ISportEvent>(id,
                                                                          sportId,
                                                                          cultures,
                                                                          _exceptionStrategy);

            LogInt.LogInformation("GetSportEvent returned: {SportEventId} for sport {SportId}", result?.Id, sportId);

            EnsureClubFriendlySimpleTournamentSummaryIsFetchedWithNonCriticalPathTimeout(cultures, result).GetAwaiter().GetResult();

            return result;
        }

        internal ISportEvent GetSportEventForEventChange(Urn id)
        {
            var result = _sportEntityFactory.BuildSportEvent<ISportEvent>(id,
                id.TypeGroup == ResourceTypeGroup.Match ? _sportEventCache.GetEventSportIdAsync(id).GetAwaiter().GetResult() : null,
                _defaultCultures.ToList(),
                _exceptionStrategy);

            return result;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitionStatus"/> for specific sport event
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the event for which <see cref="ICompetitionStatus"/> to be retrieved</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<ICompetitionStatus> GetSportEventStatusAsync(Urn id)
        {
            LogInt.LogInformation("Invoked GetSportEventStatusAsync: [Id={SportEventId}]", id);
            try
            {
                var sportEventStatusCi = await _sportEventStatusCache.GetSportEventStatusAsync(id).ConfigureAwait(false);
                if (sportEventStatusCi == null)
                {
                    return null;
                }

                return new CompetitionStatus(sportEventStatusCi, _matchStatusCache);
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetSportEventStatusAsync: [Id={SportEventId}]", id);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICompetitor"/>
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id for which <see cref="ICompetitor"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="ICompetitor"/> representing the specified competitor or a null reference</returns>
        public async Task<ICompetitor> GetCompetitorAsync(Urn id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetCompetitorAsync: [Id={CompetitorId}, Cultures={Langs}]", id, s);
            try
            {
                var cacheItem = await _profileCache.GetCompetitorProfileAsync(id, cs, false).ConfigureAwait(false);
                return cacheItem == null
                           ? null
                           : _sportEntityFactory.BuildCompetitor(cacheItem, cs, (ICompetitionCacheItem)null, _exceptionStrategy);
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetCompetitorAsync: [Id={CompetitorId}, Cultures={Langs}]", id, s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IPlayerProfile"/>
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id for which <see cref="IPlayerProfile"/> to be retrieved</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="IPlayerProfile"/> representing the specified player or a null reference</returns>
        public async Task<IPlayerProfile> GetPlayerProfileAsync(Urn id, CultureInfo culture = null)
        {
            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = cs.Aggregate(string.Empty, (current, cultureInfo) => current + ";" + cultureInfo.TwoLetterISOLanguageName);
            s = s.Substring(1);

            LogInt.LogInformation("Invoked GetPlayerProfileAsync: [Id={PlayerId}, Cultures={Langs}]", id, s);
            try
            {
                var cacheItem = await _profileCache.GetPlayerProfileAsync(id, cs, false).ConfigureAwait(false);
                return cacheItem == null
                           ? null
                           : new PlayerProfile(cacheItem, cs.ToList());
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetPlayerProfileAsync: [Id={PlayerId}, Cultures={Langs}]", id, s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Delete the sport event from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ISportEvent"/> to be deleted</param>
        /// <param name="includeEventStatusDeletion">Delete also <see cref="ISportEventStatus"/> from the cache</param>
        public void DeleteSportEventFromCache(Urn id, bool includeEventStatusDeletion = false)
        {
            LogInt.LogInformation("Invoked DeleteSportEventFromCache: [Id={SportEventId}]", id);
            _cacheManager.RemoveCacheItem(id, CacheItemType.SportEvent, "SportDataProvider");

            if (includeEventStatusDeletion)
            {
                _cacheManager.RemoveCacheItem(id, CacheItemType.SportEventStatus, "SportDataProvider");
            }
        }

        /// <summary>
        /// Delete the tournament from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ILongTermEvent"/> to be deleted</param>
        public void DeleteTournamentFromCache(Urn id)
        {
            LogInt.LogInformation("Invoked DeleteTournamentFromCache: [Id={SportEventId}]", id);
            _cacheManager.RemoveCacheItem(id, CacheItemType.SportEvent, "SportDataProvider");
        }

        /// <summary>
        /// Delete the competitor from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="ICompetitor"/> to be deleted</param>
        public void DeleteCompetitorFromCache(Urn id)
        {
            LogInt.LogInformation("Invoked DeleteCompetitorFromCache: [Id={CompetitorId}]", id);
            _cacheManager.RemoveCacheItem(id, CacheItemType.Competitor, "SportDataProvider");
        }

        /// <summary>
        /// Delete the player profile from cache
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> specifying the id of <see cref="IPlayerProfile"/> to be deleted</param>
        public void DeletePlayerProfileFromCache(Urn id)
        {
            LogInt.LogInformation("Invoked DeletePlayerProfileFromCache: [Id={PlayerId}]", id);
            _cacheManager.RemoveCacheItem(id, CacheItemType.Player, "SportDataProvider");
        }

        /// <summary>
        /// Asynchronously gets a list of <see cref="IEnumerable{ICompetition}"/>
        /// </summary>
        /// <remarks>Lists almost all events we are offering prematch odds for. This endpoint can be used during early startup to obtain almost all fixtures. This endpoint is one of the few that uses pagination.</remarks>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ICompetition>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture = null)
        {
            if (startIndex < 0)
            {
                throw new ArgumentException("Wrong value", nameof(startIndex));
            }
            if (limit < 1 || limit > 1000)
            {
                throw new ArgumentException("Wrong value", nameof(limit));
            }

            var cs = culture == null ? _defaultCultures : new[] { culture };
            var s = string.Join(";", cs);

            LogInt.LogInformation("Invoked GetListOfSportEventsAsync: [StartIndex={StartIndex}, Limit={Limit}, Cultures={Langs}]", startIndex, limit, s);

            try
            {
                var ids = await _dataRouterManager.GetListOfSportEventsAsync(startIndex, limit, culture ?? _defaultCultures.First()).ConfigureAwait(false);

                return ids?.Select(item => _sportEntityFactory.BuildSportEvent<ICompetition>(item.Item1,
                                                                                             item.Item2,
                                                                                             cs.ToList(),
                                                                                             _exceptionStrategy)).ToList();
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetListOfSportEventsAsync: [StartIndex={StartIndex}, Limit={Limit}, Cultures={Langs}]", startIndex, limit, s);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a list of active <see cref="IEnumerable{ISportEvent}"/>
        /// </summary>
        /// <remarks>Lists all <see cref="ISportEvent"/> that are cached. (once schedule is loaded)</remarks>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISportEvent>> GetActiveTournamentsAsync(CultureInfo culture = null)
        {
            var cul = culture ?? _defaultCultures.First();
            LogInt.LogInformation("Invoked GetActiveTournamentsAsync: [Culture={Lang}]", cul.TwoLetterISOLanguageName);
            try
            {
                await _sportDataCache.LoadAllTournamentsForAllSportsAsync().ConfigureAwait(false);
                var tours = await _sportEventCache.GetActiveTournamentsAsync(cul).ConfigureAwait(false);
                return tours?.Select(t => _sportEntityFactory.BuildSportEvent<ISportEvent>(t.Id, t.GetSportIdAsync().GetAwaiter().GetResult(), new[] { cul }, _exceptionStrategy));
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetActiveTournamentsAsync: [Culture={Lang}]", cul.TwoLetterISOLanguageName);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Asynchronously gets a list of available <see cref="IEnumerable{ISportEvent}"/> for a specific sport
        /// </summary>
        /// <remarks>Lists all available tournaments for a sport event we provide coverage for</remarks>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport to retrieve</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        public async Task<IEnumerable<ISportEvent>> GetAvailableTournamentsAsync(Urn sportId, CultureInfo culture = null)
        {
            var cul = culture ?? _defaultCultures.First();
            LogInt.LogInformation("Invoked GetAvailableTournamentsAsync: [SportId={SportId}, Culture={Lang}]", sportId, cul.TwoLetterISOLanguageName);
            try
            {
                var tours = await _dataRouterManager.GetSportAvailableTournamentsAsync(sportId, cul).ConfigureAwait(false);
                return tours?.Select(t => _sportEntityFactory.BuildSportEvent<ISportEvent>(t.Item1, t.Item2, new[] { cul }, _exceptionStrategy));
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetAvailableTournamentsAsync: [SportId={SportId}, Culture={Lang}]", sportId, cul.TwoLetterISOLanguageName);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }
        }

        /// <summary>
        /// Deletes the sport events from cache which are scheduled before specific DateTime
        /// </summary>
        /// <param name="before">The scheduled DateTime used to delete sport events from cache</param>
        /// <returns>Number of deleted items</returns>
        public int DeleteSportEventsFromCache(DateTime before)
        {
            LogInt.LogInformation("Invoked DeleteSportEventsFromCache: [Before={Before}]", before);
            return _sportEventCache.DeleteSportEventsFromCache(before);
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="cacheType">Specifies what type of cache items will be exported</param>
        /// <returns>Collection of <see cref="ExportableBase"/> containing all the items currently in the cache</returns>
        public async Task<IEnumerable<ExportableBase>> CacheExportAsync(CacheType cacheType)
        {
            LogInt.LogInformation("Invoked CacheExportAsync: [CacheType={CacheType}]", cacheType);
            var tasks = new List<Task<IEnumerable<ExportableBase>>>();
            if (cacheType.HasFlag(CacheType.SportData))
            {
                tasks.Add(_sportDataCache.ExportAsync());
            }
            if (cacheType.HasFlag(CacheType.SportEvent))
            {
                tasks.Add(_sportEventCache.ExportAsync());
            }
            if (cacheType.HasFlag(CacheType.Profile))
            {
                tasks.Add(_profileCache.ExportAsync());
            }
            tasks.ForEach(t => t.ConfigureAwait(false));
            return (await Task.WhenAll(tasks).ConfigureAwait(false)).SelectMany(e => e);
        }

        /// <summary>
        /// Exports current items in the cache
        /// </summary>
        /// <param name="items">Collection of <see cref="ExportableBase"/> containing the items to be imported</param>
        public Task CacheImportAsync(IEnumerable<ExportableBase> items)
        {
            var cacheItems = items.ToList();
            LogInt.LogInformation("Invoked CacheImportAsync: [items={CacheItemsCount}]", cacheItems.Count);
            var tasks = new List<Task>
            {
                _sportDataCache.ImportAsync(cacheItems),
                _sportEventCache.ImportAsync(cacheItems),
                _profileCache.ImportAsync(cacheItems)
            };
            tasks.ForEach(t => t.ConfigureAwait(false));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(CultureInfo culture = null)
        {
            return GetFixtureChangesAsync(null, null, culture);
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all fixtures that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, Urn sportId, CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = _defaultCultures.First();
            }

            LogInt.LogInformation("Invoked GetFixtureChangesAsync: [After={After}, SportId={SportId}, Culture={Lang}]", after, sportId, culture.TwoLetterISOLanguageName);

            var result = (await _dataRouterManager.GetFixtureChangesAsync(after, sportId, culture).ConfigureAwait(false))?.ToList();

            LogInt.LogInformation("GetFixtureChangesAsync returned {ResultCount} results", result?.Count);
            return result;
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IResultChange>> GetResultChangesAsync(CultureInfo culture = null)
        {
            return GetResultChangesAsync(null, null, culture);
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of all results that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, Urn sportId, CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = _defaultCultures.First();
            }

            LogInt.LogInformation("Invoked GetResultChangesAsync: [After={After}, SportId={SportId}, Culture={Lang}]", after, sportId, culture.TwoLetterISOLanguageName);

            var result = (await _dataRouterManager.GetResultChangesAsync(after, sportId, culture).ConfigureAwait(false))?.ToList();

            LogInt.LogInformation("GetResultChangesAsync returned {ResultCount} results", result?.Count);
            return result;
        }

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language or a null reference to use the languages specified in the configuration</param>
        /// <returns>A list of available lotteries</returns>
        public async Task<IEnumerable<ILottery>> GetLotteriesAsync(CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = _defaultCultures.First();
            }

            LogInt.LogInformation("Invoked GetLotteriesAsync: [Culture={Lang}]", culture.TwoLetterISOLanguageName);

            var result = (await _dataRouterManager.GetAllLotteriesAsync(culture, false).ConfigureAwait(false))?.ToList();

            if (!result.IsNullOrEmpty())
            {
                var lotteries = result.Select(s => _sportEntityFactory.BuildSportEvent<ILottery>(s.Item1, s.Item2, new[] { culture }, _exceptionStrategy)).ToList();
                LogInt.LogInformation("GetLotteriesAsync returned {LotteriesCount} results", lotteries.Count);
                return lotteries;
            }

            LogInt.LogInformation("GetLotteriesAsync returned 0 results");
            return new List<ILottery>();
        }

        /// <summary>
        /// Get sport event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>The period statuses or empty if not found</returns>
        public async Task<IEnumerable<IPeriodStatus>> GetPeriodStatusesAsync(Urn id, CultureInfo culture = null, IEnumerable<Urn> competitorIds = null, IEnumerable<int> periods = null)
        {
            if (culture == null)
            {
                culture = _defaultCultures.First();
            }

            var urns = competitorIds?.ToList();
            var periodList = periods?.ToList();
            var compIds = urns.IsNullOrEmpty() ? "null" : string.Join(", ", urns);
            var periodIds = periodList.IsNullOrEmpty() ? "null" : string.Join(", ", periodList);

            LogInt.LogInformation("Invoked GetPeriodStatusesAsync: [Id={Id}, Culture={Lang}, Competitors={CompetitorIds}, Periods={PeriodIds}]", id, culture.TwoLetterISOLanguageName, compIds, periodIds);

            var periodSummaryDto = await _dataRouterManager.GetPeriodSummaryAsync(id, culture, null, urns, periodList).ConfigureAwait(false);

            if (periodSummaryDto != null && !periodSummaryDto.PeriodStatuses.IsNullOrEmpty())
            {
                var periodStatuses = periodSummaryDto.PeriodStatuses.Select(s => new PeriodStatus(s)).ToList();
                LogInt.LogInformation("GetPeriodStatusesAsync returned {PeriodStatusesCount} results", periodStatuses.Count);
                return periodStatuses;
            }

            LogInt.LogInformation("GetPeriodStatusesAsync returned 0 results");
            return new List<IPeriodStatus>();
        }

        /// <summary>
        /// Get the associated event timeline for single culture
        /// </summary>
        /// <param name="sportEventId">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <returns>The event timeline or empty if not found</returns>
        public async Task<IEnumerable<ITimelineEvent>> GetTimelineEventsAsync(Urn sportEventId, CultureInfo culture = null)
        {
            if (culture == null)
            {
                culture = _defaultCultures.First();
            }

            try
            {
                LogInt.LogInformation("Invoked GetTimelineEventsAsync: [Id={SportEventId}, Culture={Lang}]", sportEventId, culture.TwoLetterISOLanguageName);

                var matchTimelineDto = await _dataRouterManager.GetInformationAboutOngoingEventAsync(sportEventId, culture, null).ConfigureAwait(false);

                if (matchTimelineDto != null && !matchTimelineDto.BasicEvents.IsNullOrEmpty())
                {
                    var matchTimeline = new EventTimeline(new EventTimelineCacheItem(matchTimelineDto, culture));
                    LogInt.LogInformation("GetTimelineEventsAsync returned {ResultCount} results", matchTimeline.TimelineEvents?.Count() ?? 0);
                    return matchTimeline.TimelineEvents;
                }
            }
            catch (Exception e)
            {
                LogInt.LogError(e, "Error executing GetTimelineEventsAsync: [Id={SportEventId}, Culture={Lang}]", sportEventId, culture.TwoLetterISOLanguageName);
                if (_exceptionStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
                return null;
            }

            LogInt.LogInformation("GetTimelineEventsAsync returned 0 results");
            return new List<ITimelineEvent>();
        }

        private static async Task EnsureClubFriendlySimpleTournamentSummaryIsFetchedWithNonCriticalPathTimeout(List<CultureInfo> cultures, ISportEvent sportEvent)
        {
            if (sportEvent == null || !PrefetchableSportEvents.Contains(sportEvent.Id))
            {
                return;
            }

            if (sportEvent is IPreloadableEntity preloadableEntity)
            {
                await preloadableEntity.EnsureSummaryIsFetchedForLanguages(cultures, NonTimeCriticalRequestOptions).ConfigureAwait(false);
            }
        }
    }
}
