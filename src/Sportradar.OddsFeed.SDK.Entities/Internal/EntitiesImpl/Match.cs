/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event with home and away competitor
    /// </summary>
    /// <seealso cref="ICompetition" />
    /// <seealso cref="IMatch" />
    internal class Match : Competition, IMatchV1
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Match));

        private readonly ISportEntityFactory _sportEntityFactory;
        protected readonly ILocalizedNamedValueCache MatchStatusCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="Match"/> class.
        /// </summary>
        /// <param name="id">A <see cref="URN"/> uniquely identifying the match associated with the current instance</param>
        /// <param name="sportId">A <see cref="URN"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> instance used to construct <see cref="ITournament"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instances containing cache data associated with the current instance</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> instance containing cache data information about the progress of a match associated with the current instance</param>
        /// <param name="matchStatusCache">A localized match statuses cache</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Match(URN id,
                    URN sportId,
                    ISportEntityFactory sportEntityFactory,
                    ISportEventCache sportEventCache,
                    ISportEventStatusCache sportEventStatusCache,
                    ILocalizedNamedValueCache matchStatusCache,
                    IEnumerable<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(ExecutionLogPrivate, id, sportId, sportEntityFactory, sportEventStatusCache, sportEventCache, cultures, exceptionStrategy, matchStatusCache)
        {
            _sportEntityFactory = sportEntityFactory;
            MatchStatusCache = matchStatusCache;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMatchStatus"/> containing information about the progress of the match
        /// </summary>
        /// <returns>A <see cref="Task{IMatchStatus}"/> containing information about the progress of the match</returns>
        public new async Task<IMatchStatus> GetStatusAsync()
        {
            var item = await base.GetStatusAsync().ConfigureAwait(false);
            return item == null ? null : new MatchStatus(((CompetitionStatus) item).SportEventStatusCI, MatchStatusCache);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonInfo"/> representing the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonInfo}"/> representing the retrieval operation</returns>
        public async Task<ISeasonInfo> GetSeasonAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetSeasonAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<CacheItem>>(matchCI.GetSeasonAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Season")).ConfigureAwait(false);

            return item == null ? null : new SeasonInfo(item.Id, item.Name);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IRound"/> representing the tournament round to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{IRound}"/> representing the retrieval operation</returns>
        public async Task<IRound> GetTournamentRoundAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetTournamentRoundAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<RoundCI>>(matchCI.GetTournamentRoundAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Round")).ConfigureAwait(false);

            return item == null ? null : new Round(item, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing home competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        public async Task<ITeamCompetitor> GetHomeCompetitorAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var items = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetCompetitorsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(matchCI.GetCompetitorsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Competitors")).ConfigureAwait(false);

            if (items == null)
            {
                return null;
            }

            var competitorUrns = items.ToList();
            if (competitorUrns.Count == 2)
            {
                return await _sportEntityFactory.BuildTeamCompetitorAsync(competitorUrns[0], Cultures, matchCI).ConfigureAwait(false);
            }

            ExecutionLog.Error($"Received {competitorUrns.Count} competitors for match[Id = {Id}]. Match can have only 2 competitors");
            throw new InvalidOperationException($"Invalid number of competitors. Match must have exactly 2 competitors, received {competitorUrns.Count}");
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing away competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        public async Task<ITeamCompetitor> GetAwayCompetitorAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var items = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetCompetitorsAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<URN>>>(matchCI.GetCompetitorsAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Competitors")).ConfigureAwait(false);

            if (items == null)
            {
                return null;
            }

            var competitorUrns = items.ToList();
            if (competitorUrns.Count == 2)
            {
                return await _sportEntityFactory.BuildTeamCompetitorAsync(competitorUrns[1], Cultures, matchCI).ConfigureAwait(false);
            }

            ExecutionLog.Error($"Received {competitorUrns.Count} competitors for match[Id = {Id}]. Match can have only 2 competitors.");
            throw new InvalidOperationException($"Invalid number of competitors. Match must have exactly 2 competitors, received {competitorUrns.Count}");
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ILongTermEvent"/> representing the tournament to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ILongTermEvent}"/> representing the retrieval operation</returns>
        public async Task<ILongTermEvent> GetTournamentAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var tournamentId = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetTournamentIdAsync().ConfigureAwait(false)
                : await new Func<Task<URN>>(matchCI.GetTournamentIdAsync).SafeInvokeAsync(ExecutionLog, GetFetchErrorMessage("ILongTermEvent")).ConfigureAwait(false);

            return tournamentId == null
                ? null
                : _sportEntityFactory.BuildSportEvent<ILongTermEvent>(tournamentId, null, Cultures, ExceptionStrategy);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IFixture"/> instance containing information about the arranged sport event
        /// </summary>
        /// <returns>A <see cref="Task{IFixture}"/> representing the retrieval operation</returns>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        public async Task<IFixture> GetFixtureAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetFixtureAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IFixture>>(matchCI.GetFixtureAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Fixture")).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the associated event timeline
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        public async Task<IEventTimeline> GetEventTimelineAsync()
        {
            var matchCI = (IMatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var eventTimelineCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetEventTimelineAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<EventTimelineCI>>(matchCI.GetEventTimelineAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("EventTimeline")).ConfigureAwait(false);

            return eventTimelineCI == null
                ? null
                : new EventTimeline(eventTimelineCI);
        }

        /// <summary>
        /// Asynchronously gets the associated delayed info
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<IDelayedInfo> GetDelayedInfoAsync()
        {
            var matchCI = (MatchCI)SportEventCache.GetEventCacheItem(Id);
            if (matchCI == null)
            {
                ExecutionLog.Debug($"Missing data. No match cache item for id={Id}.");
                return null;
            }
            var delayedInfoCI = ExceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await matchCI.GetDelayedInfoAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<DelayedInfoCI>>(matchCI.GetDelayedInfoAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("DelayedInfo")).ConfigureAwait(false);

            return delayedInfoCI == null
                ? null
                : new DelayedInfo(delayedInfoCI);
        }

        /// <inheritdoc />
        public new async Task<string> GetNameAsync(CultureInfo culture)
        {
            var name = await base.GetNameAsync(culture).ConfigureAwait(false);

            if (string.IsNullOrEmpty(name))
            {
                var homeCompetitor = await GetHomeCompetitorAsync().ConfigureAwait(false);
                var awayCompetitor = await GetAwayCompetitorAsync().ConfigureAwait(false);

                if (homeCompetitor != null && awayCompetitor != null)
                {
                    name = $"{homeCompetitor.GetName(culture)} vs. {awayCompetitor.GetName(culture)}";
                }
            }

            return name;
        }
    }
}
