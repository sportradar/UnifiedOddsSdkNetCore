/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
    /// Represents a sport tournament
    /// </summary>
    /// <seealso cref="ITournament" />
    internal class Tournament : SportEvent, ITournamentV1
    {
        /// <summary>
        /// This <see cref="ILog"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Tournament));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve basic information about the tournament(sport, category, names)
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tournament"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the tournament associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to construct <see cref="ISportEvent"/></param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve tournament schedule</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A list of all languages for this instance</param>
        /// <param name="exceptionStrategy">The <see cref="ExceptionHandlingStrategy"/> indicating how to handle potential exceptions thrown during execution</param>
        public Tournament(URN id,
                            URN sportId,
                            ISportEntityFactory sportEntityFactory,
                            ISportEventCache sportEventCache,
                            ISportDataCache sportDataCache,
                            IEnumerable<CultureInfo> cultures,
                            ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Contract.Requires(id != null);
            Contract.Requires(sportDataCache != null);
            Contract.Requires(sportEventCache != null);
            Contract.Requires(sportEntityFactory != null);

            _sportDataCache = sportDataCache;
            _sportEntityFactory = sportEntityFactory;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISportSummary"/> representing the sport to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportSummary}"/> representing the asynchronous operation</returns>
        public async Task<ISportSummary> GetSportAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI) SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetSportIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(tournamentInfoCI.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No sportId for tournament cache item with id={Id}.");
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
            var tournamentInfoCI = (TournamentInfoCI) SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var tournamentCoverageCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCI>>(tournamentInfoCI.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return tournamentCoverageCI == null
                ? null
                : new TournamentCoverage(tournamentCoverageCI);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICategorySummary"/> representing the category to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportCategory}"/> representing the asynchronous operation</returns>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI) SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var categoryId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetCategoryIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(tournamentInfoCI.GetCategoryIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("CategoryId")).ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No categoryId for tournament cache item with id={Id}.");
                return null;
            }
            var categoryData = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false)
                : await new Func<URN, IEnumerable<CultureInfo>, Task<CategoryData>>(_sportDataCache.GetCategoryAsync).SafeInvokeAsync(categoryId, Cultures, ExecutionLog, GetFetchErrorMessage("CategoryData")).ConfigureAwait(false);

            return categoryData == null
                ? null
                : new CategorySummary(categoryData.Id, categoryData.Names, categoryData.CountryCode);
        }

        /// <summary>
        /// Get current season as an asynchronous operation
        /// </summary>
        /// <returns>A <see cref="Task{ICurrentSeasonInfo}" /> representing the asynchronous operation</returns>
        public async Task<ICurrentSeasonInfo> GetCurrentSeasonAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI) SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var currentSeasonInfoCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetCurrentSeasonInfoAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<CurrentSeasonInfoCI>>(tournamentInfoCI.GetCurrentSeasonInfoAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CurrentSeasonInfo")).ConfigureAwait(false);

            if (currentSeasonInfoCI == null)
            {
                return null;
            }

            var competitorsReferences = await tournamentInfoCI.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            //we do have current season, but need to load the correct one
            var currentSeasonTournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(currentSeasonInfoCI.Id);
            var seasonTournamentInfoCurrentSeasonInfo = currentSeasonTournamentInfoCI == null
                ? new CurrentSeasonInfo(currentSeasonInfoCI, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences)
                : new CurrentSeasonInfo(currentSeasonTournamentInfoCI, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences);

            return seasonTournamentInfoCurrentSeasonInfo;
        }

        /// <summary>
        /// Asynchronously gets a list of <see cref="ISeason" /> associated with this tournament
        /// </summary>
        /// <returns>A list of <see cref="ISeason" /> associated with this tournament</returns>
        public async Task<IEnumerable<ISeason>> GetSeasonsAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI) SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var seasonIds = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetSeasonsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(tournamentInfoCI.GetSeasonsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Seasons")).ConfigureAwait(false);

            return seasonIds?.Select(s=> _sportEntityFactory.BuildSportEvent<ISeason>(s, SportId, Cultures, ExceptionStrategy));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        public async Task<bool?> GetExhibitionGamesAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var exhibitionGames = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetExhibitionGamesAsync().ConfigureAwait(false)
                : await new Func<Task<bool?>>(tournamentInfoCI.GetExhibitionGamesAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ExhibitionGames")).ConfigureAwait(false);

            return exhibitionGames;
        }
    }
}
