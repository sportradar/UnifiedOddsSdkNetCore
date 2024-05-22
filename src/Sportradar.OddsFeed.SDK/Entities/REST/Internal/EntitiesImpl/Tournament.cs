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
    /// Represents a sport tournament
    /// </summary>
    /// <seealso cref="ITournament" />
    internal class Tournament : SportEvent, ITournament
    {
        /// <summary>
        /// This <see cref="ILogger"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Tournament));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve basic information about the tournament(sport, category, names)
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tournament"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the tournament associated with the current instance</param>
        /// <param name="sportId">A <see cref="Urn"/> identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to construct <see cref="ISportEvent"/></param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> used to retrieve tournament schedule</param>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve basic tournament information</param>
        /// <param name="cultures">A list of all languages for this instance</param>
        /// <param name="exceptionStrategy">The <see cref="ExceptionHandlingStrategy"/> indicating how to handle potential exceptions thrown during execution</param>
        public Tournament(Urn id,
                          Urn sportId,
                          ISportEntityFactory sportEntityFactory,
                          ISportEventCache sportEventCache,
                          ISportDataCache sportDataCache,
                          IReadOnlyCollection<CultureInfo> cultures,
                          ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();

            _sportDataCache = sportDataCache;
            _sportEntityFactory = sportEntityFactory;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISportSummary"/> representing the sport to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportSummary}"/> representing the asynchronous operation</returns>
        public async Task<ISportSummary> GetSportAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var sportId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetSportIdAsync().ConfigureAwait(false)
                : await new Func<Task<Urn>>(tournamentInfoCacheItem.GetSportIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("SportId")).ConfigureAwait(false);

            if (sportId == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No sportId for tournament cache item with id={Id}.");
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
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var tournamentCoverageCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCacheItem>>(tournamentInfoCacheItem.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return tournamentCoverageCacheItem == null
                ? null
                : new TournamentCoverage(tournamentCoverageCacheItem);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ICategorySummary"/> representing the category to which the tournament belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISportCategory}"/> representing the asynchronous operation</returns>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var categoryId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetCategoryIdAsync().ConfigureAwait(false)
                : await new Func<Task<Urn>>(tournamentInfoCacheItem.GetCategoryIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("CategoryId")).ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No categoryId for tournament cache item with id={Id}.");
                return null;
            }
            var categoryData = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false)
                : await new Func<Urn, IReadOnlyCollection<CultureInfo>, Task<CategoryData>>(_sportDataCache.GetCategoryAsync).SafeInvokeAsync(categoryId, Cultures, ExecutionLog, GetFetchErrorMessage("CategoryData")).ConfigureAwait(false);

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
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var currentSeasonInfoCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetCurrentSeasonInfoAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<CurrentSeasonInfoCacheItem>>(tournamentInfoCacheItem.GetCurrentSeasonInfoAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CurrentSeasonInfo")).ConfigureAwait(false);

            if (currentSeasonInfoCacheItem == null)
            {
                return null;
            }

            var competitorsReferences = await tournamentInfoCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            //we do have current season, but need to load the correct one
            var currentSeasonTournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(currentSeasonInfoCacheItem.Id);
            var seasonTournamentInfoCurrentSeasonInfo = currentSeasonTournamentInfoCacheItem == null
                ? new CurrentSeasonInfo(currentSeasonInfoCacheItem, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences)
                : new CurrentSeasonInfo(currentSeasonTournamentInfoCacheItem, Cultures, _sportEntityFactory, ExceptionStrategy, competitorsReferences);

            return seasonTournamentInfoCurrentSeasonInfo;
        }

        /// <summary>
        /// Asynchronously gets a list of <see cref="ISeason" /> associated with this tournament
        /// </summary>
        /// <returns>A list of <see cref="ISeason" /> associated with this tournament</returns>
        public async Task<IEnumerable<ISeason>> GetSeasonsAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var seasonIds = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetSeasonsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(tournamentInfoCacheItem.GetSeasonsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Seasons")).ConfigureAwait(false);

            return seasonIds?.Select(s => _sportEntityFactory.BuildSportEvent<ISeason>(s, SportId, Cultures.ToList(), ExceptionStrategy));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="bool"/> specifying if the tournament is exhibition game
        /// </summary>
        /// <returns>A <see cref="bool"/> specifying if the tournament is exhibition game</returns>
        public async Task<bool?> GetExhibitionGamesAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }

            var exhibitionGames = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetExhibitionGamesAsync().ConfigureAwait(false)
                : await new Func<Task<bool?>>(tournamentInfoCacheItem.GetExhibitionGamesAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ExhibitionGames")).ConfigureAwait(false);

            return exhibitionGames;
        }

        /// <summary>
        /// Gets the list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule
        /// </summary>
        /// <returns>The list of all <see cref="ICompetition"/> that belongs to the basic tournament schedule</returns>
        public async Task<IEnumerable<ISportEvent>> GetScheduleAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetScheduleAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(tournamentInfoCacheItem.GetScheduleAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Schedule")).ConfigureAwait(false);

            return item?.Select(s => _sportEntityFactory.BuildSportEvent<ISportEvent>(s, SportId, Cultures.ToList(), ExceptionStrategy));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{T}"/> representing competitors in the sport event associated with the current instance
        /// </summary>
        /// <param name="culture">The culture in which we want to return competitor data</param>
        /// <returns>A <see cref="Task{T}"/> representing the retrieval operation</returns>
        public async Task<ICollection<ICompetitor>> GetCompetitorsAsync(CultureInfo culture)
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                return null;
            }

            var cultureList = new[] { culture };
            var items = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                            ? await tournamentInfoCacheItem.GetCompetitorsIdsAsync(cultureList).ConfigureAwait(false)
                            : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(tournamentInfoCacheItem.GetCompetitorsIdsAsync).SafeInvokeAsync(cultureList, ExecutionLog, GetFetchErrorMessage("CompetitorsIds")).ConfigureAwait(false);

            var competitorsIds = items == null ? new List<Urn>() : items.ToList();
            if (!competitorsIds.Any())
            {
                return new List<ICompetitor>();
            }

            var competitorsReferences = await tournamentInfoCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);
            var tasks = competitorsIds.Select(s => _sportEntityFactory.BuildCompetitorAsync(s, cultureList, competitorsReferences, ExceptionStrategy)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(s => s.GetAwaiter().GetResult()).ToList();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{T}"/> representing competitors in the sport event associated with the current instance
        /// </summary>
        /// <param name="culture">Optional culture in which we want to fetch competitor data (otherwise default is used)</param>
        /// <returns>A <see cref="Task{T}"/> representing the retrieval operation</returns>
        public async Task<ICollection<Urn>> GetCompetitorIdsAsync(CultureInfo culture = null)
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                return new List<Urn>();
            }

            var competitorIds = await tournamentInfoCacheItem.GetCompetitorsIdsAsync(culture).ConfigureAwait(false);
            return competitorIds.ToList();
        }
    }
}
