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

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Class BasicTournament.
    /// </summary>
    /// <seealso cref="SportEvent" />
    /// <seealso cref="IBasicTournament" />
    internal class BasicTournament : SportEvent, IBasicTournament
    {
        /// <summary>
        /// This <see cref="ILogger"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(BasicTournament));

        /// <summary>
        /// The sport data cache
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTournament"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="sportEntityFactory">An instance of a <see cref="ISportEntityFactory"/> used to create <see cref="ISportEvent"/> instances</param>
        /// <param name="sportEventCache">The sport event cache</param>
        /// <param name="sportDataCache">The sport data cache</param>
        /// <param name="cultures">The cultures</param>
        /// <param name="exceptionStrategy">The exception strategy</param>
        public BasicTournament(Urn id,
                               Urn sportId,
                               ISportEntityFactory sportEntityFactory,
                               ISportEventCache sportEventCache,
                               ISportDataCache sportDataCache,
                               IReadOnlyCollection<CultureInfo> cultures,
                               ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _sportDataCache = sportDataCache;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// get sport as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;ISportSummary&gt;</returns>
        /// <value>The <see cref="ISportSummary" /> instance representing the sport associated with the current instance</value>
        public async Task<ISportSummary> GetSportAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var sportId = await tournamentInfoCacheItem.GetSportIdAsync().ConfigureAwait(false);
            if (sportId == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No sportId for tournament cache item with id={Id}.");
                return null;
            }
            var sportCacheItem = await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false);
            return sportCacheItem == null ? null : new SportSummary(sportCacheItem.Id, sportCacheItem.Names);
        }

        /// <summary>
        /// Asynchronously get the <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance
        /// </summary>
        /// <returns>Task&lt;ITournamentCoverage&gt;</returns>
        /// <value>The <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance</value>
        public async Task<ITournamentCoverage> GetTournamentCoverage()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var cacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Catch
                ? await tournamentInfoCacheItem.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCacheItem>>(tournamentInfoCacheItem.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return cacheItem == null ? null : new TournamentCoverage(cacheItem);
        }

        /// <summary>
        /// Get category as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;ICategorySummary&gt;</returns>
        /// <value>The category</value>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var categoryId = await tournamentInfoCacheItem.GetCategoryIdAsync().ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No categoryId for tournament cache item with id={Id}.");
                return null;
            }
            var categoryCacheItem = await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false);
            return categoryCacheItem == null ? null : new CategorySummary(categoryCacheItem.Id, categoryCacheItem.Names, categoryCacheItem.CountryCode);
        }

        /// <summary>
        /// get competitors as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;IEnumerable&lt;ICompetitor&gt;&gt;</returns>
        /// <value>The competitors</value>
        public async Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var tournamentInfoCacheItem = (TournamentInfoCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCacheItem == null)
            {
                ExecutionLogPrivate.LogDebug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var items = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await tournamentInfoCacheItem.GetCompetitorsIdsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(tournamentInfoCacheItem.GetCompetitorsIdsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CompetitorsIds")).ConfigureAwait(false);

            var competitorsIds = items == null ? new List<Urn>() : items.ToList();
            if (!competitorsIds.Any())
            {
                return new List<ICompetitor>();
            }

            var competitorsReferences = await tournamentInfoCacheItem.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            var tasks = competitorsIds.Select(s => _sportEntityFactory.BuildCompetitorAsync(s, Cultures, competitorsReferences, ExceptionStrategy)).ToList();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return tasks.Select(s => s.GetAwaiter().GetResult());
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
    }
}
