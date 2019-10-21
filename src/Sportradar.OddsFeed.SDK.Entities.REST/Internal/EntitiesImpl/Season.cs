/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a season
    /// </summary>
    /// <seealso cref="ITournament" />
    internal class Season : SportEvent, ISeason
    {
        /// <summary>
        /// This <see cref="ILog"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Season));

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
        /// <param name="id">A <see cref="URN"/> uniquely identifying the tournament associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve tournament schedule</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A list of all languages for this instance</param>
        /// <param name="exceptionStrategy">The <see cref="ExceptionHandlingStrategy"/> indicating how to handle potential exceptions thrown during execution</param>
        public Season(URN id,
                      URN sportId,
                      ISportEntityFactory sportEntityFactory,
                      ISportEventCache sportEventCache,
                      ISportDataCache sportDataCache,
                      IEnumerable<CultureInfo> cultures,
                      ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(id).NotNull();
            Guard.Argument(sportEntityFactory).NotNull();
            Guard.Argument(sportDataCache).NotNull();
            Guard.Argument(sportEventCache).NotNull();

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
            var seasonCI = (TournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetSportIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(seasonCI.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No sportId for tournament={Id}.");
                return null;
            }

            var sportData = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false)
                : await new Func<URN, IEnumerable<CultureInfo>, Task<SportData>>(_sportDataCache.GetSportAsync).SafeInvokeAsync(sportId, Cultures, ExecutionLog, GetFetchErrorMessage("SportData")).ConfigureAwait(false);

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
            var seasonCI = (TournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var tournamentCoverageCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCI>>(seasonCI.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return tournamentCoverageCI == null
                ? null
                : new TournamentCoverage(tournamentCoverageCI);
        }

        /// <summary>
        /// Asynchronously get the list of available team <see cref="ReferenceIdCI"/>
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing an async operation</returns>
        public Task<IDictionary<URN, ReferenceIdCI>> GetCompetitorsReferencesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonCoverage"/> representing the season coverage
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonCoverage}"/> representing the asynchronous operation</returns>
        public async Task<ISeasonCoverage> GetSeasonCoverageAsync()
        {
            var seasonCI = (ITournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var seasonCoverageCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetSeasonCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<SeasonCoverageCI>>(seasonCI.GetSeasonCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SeasonCoverage")).ConfigureAwait(false);

            return seasonCoverageCI == null
                ? null
                : new SeasonCoverage(seasonCoverageCI);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IGroup>> GetGroupsAsync()
        {
            var seasonCI = (ITournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var groupsCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetGroupsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<GroupCI>>>(seasonCI.GetGroupsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Groups")).ConfigureAwait(false);

            var competitorsReferences = await seasonCI.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            var groupCis = groupsCI as IList<GroupCI>;

            return groupCis == null || !groupCis.Any()
                ? null
                : new List<IGroup>(groupCis.Select(g => new Group(g, Cultures, _sportEntityFactory, competitorsReferences)));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{IGroup}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{IRound}" /> representing the asynchronous operation</returns>
        public async Task<IRound> GetCurrentRoundAsync()
        {
            var seasonCI = (ITournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var currentRoundCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetCurrentRoundAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<RoundCI>>(seasonCI.GetCurrentRoundAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CurrentRound")).ConfigureAwait(false);

            return currentRoundCI == null
                ? null
                : new Round(currentRoundCI, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public async Task<string> GetYearAsync()
        {
            var seasonCI = (ITournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var year = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetYearAsync().ConfigureAwait(false)
                : await new Func<Task<string>>(seasonCI.GetYearAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("Year")).ConfigureAwait(false);

            return year;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITournamentInfo"/> representing the tournament info
        /// </summary>
        /// <returns>A <see cref="Task{ITournamentInfo}"/> representing the asynchronous operation</returns>
        public async Task<ITournamentInfo> GetTournamentInfoAsync()
        {
            var seasonCI = (ITournamentInfoCI) _sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var tournamentInfoBasicCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetTournamentInfoAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentInfoBasicCI>>(seasonCI.GetTournamentInfoAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentInfoBasic")).ConfigureAwait(false);

            if (tournamentInfoBasicCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament info basic for cache item with id={Id}.");
                return null;
            }

            var categoryId = await seasonCI.GetCategoryIdAsync().ConfigureAwait(false);
            if (categoryId == null)
            {
                return null;
            }
            var categoryCI = await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false);
            var categorySummary = categoryCI == null ? null : new CategorySummary(categoryCI.Id, categoryCI.Names, categoryCI.CountryCode);

            var currentSeasonCI = tournamentInfoBasicCI.CurrentSeason == null
                                      ? null
                                      : Id.Equals(tournamentInfoBasicCI.CurrentSeason.Id)
                                          ? seasonCI
                                          : (TournamentInfoCI) SportEventCache.GetEventCacheItem(tournamentInfoBasicCI.CurrentSeason.Id);

            // there is no current season - return empty TournamentInfo
            if (currentSeasonCI == null)
            {
                return new TournamentInfo(tournamentInfoBasicCI, categorySummary, null);
            }

            var competitorsReferences = await seasonCI.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            //we do have current season, but need to load the correct one
            var seasonTournamentInfoCurrentSeasonInfo = new CurrentSeasonInfo(currentSeasonCI, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences);

            return new TournamentInfo(tournamentInfoBasicCI, categorySummary, seasonTournamentInfoCurrentSeasonInfo);
        }

        /// <summary>
        /// Asynchronously gets the list of competitors
        /// </summary>
        /// <value>The list of competitors</value>
        public async Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var seasonCI = (ITournamentInfoCI)_sportEventCache.GetEventCacheItem(Id);
            if (seasonCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await seasonCI.GetCompetitorsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<CompetitorCI>>>(seasonCI.GetCompetitorsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Competitors")).ConfigureAwait(false);

            var competitorsReferences = await seasonCI.GetCompetitorsReferencesAsync().ConfigureAwait(false);
            return item?.Select(s => _sportEntityFactory.BuildCompetitor(s, Cultures, competitorsReferences));
        }

        /// <summary>
        /// Gets the list of all events that belongs to the tournament schedule
        /// </summary>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<ICompetition>> GetScheduleAsync()
        {
            IEnumerable<Tuple<URN, URN>> sportEventIds = null;
            if (ExceptionStrategy == ExceptionHandlingStrategy.THROW)
            {
                var tasks = Cultures.Select(s => _sportEventCache.GetEventIdsAsync(Id, s)).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
                sportEventIds = tasks.First().Result;
            }
            else
            {
                var tasks = Cultures.Select(s => new Func<URN, CultureInfo, Task<IEnumerable<Tuple<URN, URN>>>>(_sportEventCache.GetEventIdsAsync).SafeInvokeAsync(Id, s, ExecutionLog, GetFetchErrorMessage("Schedule"))).ToList();
                await Task.WhenAll(tasks).ConfigureAwait(false);
                sportEventIds = tasks.First().Result;
            }
            //var sportEventIds = ExceptionStrategy == ExceptionHandlingStrategy.THROW
            //    ? await _sportEventCache.GetEventIdsAsync(Id).ConfigureAwait(false)
            //    : await new Func<URN, Task<IEnumerable<Tuple<URN, URN>>>>(_sportEventCache.GetEventIdsAsync).SafeInvokeAsync(Id, ExecutionLog, GetFetchErrorMessage("Schedule")).ConfigureAwait(false);

            return sportEventIds?.Select(i => _sportEntityFactory.BuildSportEvent<ICompetition>(i.Item1, i.Item2, Cultures, ExceptionStrategy)).ToList();
        }
    }
}
