// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a season
    /// </summary>
    /// <seealso cref="ITournament" />
    internal class Season : SportEvent, ISeason
    {
        /// <summary>
        /// This <see cref="ILogger"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Season));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve basic information about the tournament(sport, category, names)
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// A <see cref="ISportEventCache"/> used to retrieve tournament schedule
        /// </summary>
        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tournament"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the tournament associated with the current instance</param>
        /// <param name="sportId">A <see cref="Urn"/> identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve tournament schedule</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A list of all languages for this instance</param>
        /// <param name="exceptionStrategy">The <see cref="ExceptionHandlingStrategy"/> indicating how to handle potential exceptions thrown during execution</param>
        public Season(Urn id,
                      Urn sportId,
                      ISportEntityFactory sportEntityFactory,
                      ISportEventCache sportEventCache,
                      ISportDataCache sportDataCache,
                      IReadOnlyCollection<CultureInfo> cultures,
                      ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportDataCache = sportDataCache;
            _sportEventCache = sportEventCache;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISportSummary"/> representing the sport to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportSummary}"/> representing the asynchronous operation</returns>
        public async Task<ISportSummary> GetSportAsync()
        {
            var seasonCacheItem = (TournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetSportIdAsync().ConfigureAwait(false)
                : await new Func<Task<Urn>>(seasonCacheItem.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No sportId for tournament={Id}.");
                return null;
            }

            var sportData = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false)
                : await new Func<Urn, IReadOnlyCollection<CultureInfo>, Task<SportData>>(_sportDataCache.GetSportAsync).SafeInvokeAsync(sportId, Cultures, ExecutionLog, GetFetchErrorMessage("SportData")).ConfigureAwait(false);

            return sportData == null
                ? null
                : new SportSummary(sportData.Id, sportData.Names);
        }

        /// <summary>
        /// Asynchronously get the <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance
        /// </summary>
        /// <returns>Task&lt;ITournamentCoverage&gt;.</returns>
        /// <value>The <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance</value>
        public async Task<ITournamentCoverage> GetTournamentCoverage()
        {
            var seasonCacheItem = (TournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var tournamentCoverageCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCacheItem>>(seasonCacheItem.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return tournamentCoverageCacheItem == null
                ? null
                : new TournamentCoverage(tournamentCoverageCacheItem);
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCacheItem"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public Task<IDictionary<Urn, ReferenceIdCacheItem>> GetCompetitorsReferencesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonCoverage"/> representing the season coverage
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonCoverage}"/> representing the asynchronous operation</returns>
        public async Task<ISeasonCoverage> GetSeasonCoverageAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var seasonCoverageCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetSeasonCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<SeasonCoverageCacheItem>>(seasonCacheItem.GetSeasonCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SeasonCoverage")).ConfigureAwait(false);

            return seasonCoverageCacheItem == null
                ? null
                : new SeasonCoverage(seasonCoverageCacheItem);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IGroup>> GetGroupsAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var groupsCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetGroupsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<GroupCacheItem>>>(seasonCacheItem.GetGroupsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Groups")).ConfigureAwait(false);

            var competitorsReferences = await seasonCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            var groupCis = groupsCacheItem as IList<GroupCacheItem>;

            return groupCis == null || !groupCis.Any()
                ? null
                : new List<IGroup>(groupCis.Select(g => new Group(g, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences)));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{IRound}" /> representing the asynchronous operation</returns>
        public async Task<IRound> GetCurrentRoundAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var currentRoundCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetCurrentRoundAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<RoundCacheItem>>(seasonCacheItem.GetCurrentRoundAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CurrentRound")).ConfigureAwait(false);

            return currentRoundCacheItem == null
                ? null
                : new Round(currentRoundCacheItem, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public async Task<string> GetYearAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var year = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetYearAsync().ConfigureAwait(false)
                : await new Func<Task<string>>(seasonCacheItem.GetYearAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("Year")).ConfigureAwait(false);

            return year;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITournamentInfo"/> representing the tournament info
        /// </summary>
        /// <returns>A <see cref="Task{ITournamentInfo}"/> representing the asynchronous operation</returns>
        public async Task<ITournamentInfo> GetTournamentInfoAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var tournamentInfoBasicCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetTournamentInfoAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentInfoBasicCacheItem>>(seasonCacheItem.GetTournamentInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentInfoBasic")).ConfigureAwait(false);

            if (tournamentInfoBasicCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament info basic for cache item with id={Id}.");
                return null;
            }

            var categoryId = await seasonCacheItem.GetCategoryIdAsync().ConfigureAwait(false);
            if (categoryId == null)
            {
                return null;
            }
            var categoryCacheItem = await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false);
            var categorySummary = categoryCacheItem == null ? null : new CategorySummary(categoryCacheItem.Id, categoryCacheItem.Names, categoryCacheItem.CountryCode);

            ITournamentInfoCacheItem currentSeasonCacheItem;
            if (tournamentInfoBasicCacheItem.CurrentSeason == null)
            {
                currentSeasonCacheItem = null;
            }
            else
            {
                currentSeasonCacheItem = Id.Equals(tournamentInfoBasicCacheItem.CurrentSeason.Id)
                    ? seasonCacheItem
                    : (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(tournamentInfoBasicCacheItem.CurrentSeason.Id);
            }

            // there is no current season - return empty TournamentInfo
            if (currentSeasonCacheItem == null)
            {
                return new TournamentInfo(tournamentInfoBasicCacheItem, categorySummary, null);
            }

            var competitorsReferences = await seasonCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            //we do have current season, but need to load the correct one
            var seasonTournamentInfoCurrentSeasonInfo = new CurrentSeasonInfo(currentSeasonCacheItem, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences);

            return new TournamentInfo(tournamentInfoBasicCacheItem, categorySummary, seasonTournamentInfoCurrentSeasonInfo);
        }

        /// <summary>
        /// Asynchronously gets the list of competitors
        /// </summary>
        /// <value>The list of competitors</value>
        public async Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var seasonCacheItem = (ITournamentInfoCacheItem)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var items = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await seasonCacheItem.GetCompetitorsIdsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(seasonCacheItem.GetCompetitorsIdsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CompetitorsIds")).ConfigureAwait(false);

            var competitorsIds = items == null ? new List<Urn>() : items.ToList();
            if (!competitorsIds.Any())
            {
                return new List<ICompetitor>();
            }

            var competitorsReferences = await seasonCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            var tasks = competitorsIds.Select(s => _sportEntityFactory.BuildCompetitorAsync(s, Cultures, competitorsReferences, ExceptionStrategy)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(s => s.GetAwaiter().GetResult());
        }

        /// <summary>
        /// Gets the list of all events that belongs to the tournament schedule
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<ICompetition>> GetScheduleAsync()
        {
            IEnumerable<Tuple<Urn, Urn>> sportEventIds;
            if (ExceptionStrategy == ExceptionHandlingStrategy.Throw)
            {
                var tasks = Cultures.Select(s => _sportEventCache.GetEventIdsAsync(Id, s)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
                sportEventIds = tasks.First().GetAwaiter().GetResult();
            }
            else
            {
                var tasks = Cultures.Select(s => new Func<Urn, CultureInfo, Task<IEnumerable<Tuple<Urn, Urn>>>>(_sportEventCache.GetEventIdsAsync).SafeInvokeAsync(Id, s, ExecutionLog, GetFetchErrorMessage("Schedule"))).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
                sportEventIds = tasks.First().GetAwaiter().GetResult();
            }

            return sportEventIds?.Select(i => _sportEntityFactory.BuildSportEvent<ICompetition>(i.Item1, i.Item2 ?? SportId, Cultures.ToList(), ExceptionStrategy)).ToList();
        }
    }
}
