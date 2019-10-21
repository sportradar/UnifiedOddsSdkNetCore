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
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class BasicTournament.
    /// </summary>
    /// <seealso cref="SportEvent" />
    /// <seealso cref="IBasicTournament" />
    internal class BasicTournament : SportEvent, IBasicTournamentV1
    {
        /// <summary>
        /// This <see cref="ILog"/> should not be used since it is also exposed by the base class
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(BasicTournament));

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
        public BasicTournament(URN id,
                               URN sportId,
                               ISportEntityFactory sportEntityFactory,
                               ISportEventCache sportEventCache,
                               ISportDataCache sportDataCache,
                               IEnumerable<CultureInfo> cultures,
                               ExceptionHandlingStrategy exceptionStrategy)
            : base(id, sportId, ExecutionLogPrivate, sportEventCache, cultures, exceptionStrategy)
        {
            Guard.Argument(sportDataCache).NotNull();
            Guard.Argument(sportEntityFactory).NotNull();

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
            var tournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var sportId = await tournamentInfoCI.GetSportIdAsync().ConfigureAwait(false);
            if (sportId == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No sportId for tournament cache item with id={Id}.");
                return null;
            }
            var sportCI = await _sportDataCache.GetSportAsync(sportId, Cultures).ConfigureAwait(false);
            return sportCI == null ? null : new SportSummary(sportCI.Id, sportCI.Names);
        }

        /// <summary>
        /// Asynchronously get the <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance
        /// </summary>
        /// <returns>Task&lt;ITournamentCoverage&gt;</returns>
        /// <value>The <see cref="ITournamentCoverage" /> instance representing the tournament coverage associated with the current instance</value>
        public async Task<ITournamentCoverage> GetTournamentCoverage()
        {
            var tournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var cacheItem = ExceptionStrategy == ExceptionHandlingStrategy.CATCH
                ? await tournamentInfoCI.GetTournamentCoverageAsync().ConfigureAwait(false)
                : await new Func<Task<TournamentCoverageCI>>(tournamentInfoCI.GetTournamentCoverageAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("TournamentCoverage")).ConfigureAwait(false);

            return cacheItem == null ? null : new TournamentCoverage(cacheItem);
        }

        /// <summary>
        /// Get category as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;ICategorySummary&gt;</returns>
        /// <value>The category</value>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var categoryId = await tournamentInfoCI.GetCategoryIdAsync().ConfigureAwait(false);
            if (categoryId == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No categoryId for tournament cache item with id={Id}.");
                return null;
            }
            var categoryCI = await _sportDataCache.GetCategoryAsync(categoryId, Cultures).ConfigureAwait(false);
            return categoryCI == null ? null : new CategorySummary(categoryCI.Id, categoryCI.Names, categoryCI.CountryCode);
        }

        /// <summary>
        /// get competitors as an asynchronous operation.
        /// </summary>
        /// <returns>Task&lt;IEnumerable&lt;ICompetitor&gt;&gt;</returns>
        /// <value>The competitors</value>
        public async Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var tournamentInfoCI = (TournamentInfoCI)SportEventCache.GetEventCacheItem(Id);
            if (tournamentInfoCI == null)
            {
                ExecutionLogPrivate.Debug($"Missing data. No tournament cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await tournamentInfoCI.GetCompetitorsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<CompetitorCI>>>(tournamentInfoCI.GetCompetitorsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Competitors")).ConfigureAwait(false);

            var competitorsReferences = await tournamentInfoCI.GetCompetitorsReferencesAsync().ConfigureAwait(false);

            return item?.Select(s => _sportEntityFactory.BuildCompetitor(s, Cultures, competitorsReferences));
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
