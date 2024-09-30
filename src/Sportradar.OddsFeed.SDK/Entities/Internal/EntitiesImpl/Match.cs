// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event with home and away competitor
    /// </summary>
    /// <seealso cref="ICompetition" />
    /// <seealso cref="IMatch" />
    internal class Match : Competition, IMatch
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLogPrivate = SdkLoggerFactory.GetLogger(typeof(Match));

        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Match"/> class.
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> uniquely identifying the match associated with the current instance</param>
        /// <param name="sportId">A <see cref="Urn"/> uniquely identifying the sport associated with the current instance</param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> instance used to construct <see cref="ITournament"/> instances</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instances containing cache data associated with the current instance</param>
        /// <param name="sportEventStatusCache">A <see cref="ISportEventStatusCache"/> instance containing cache data information about the progress of a match associated with the current instance</param>
        /// <param name="matchStatusCache">A localized match statuses cache</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the initialized instance will handle potential exceptions</param>
        public Match(Urn id,
                    Urn sportId,
                    ISportEntityFactory sportEntityFactory,
                    ISportEventCache sportEventCache,
                    ISportEventStatusCache sportEventStatusCache,
                    ILocalizedNamedValueCache matchStatusCache,
                    IReadOnlyCollection<CultureInfo> cultures,
                    ExceptionHandlingStrategy exceptionStrategy)
            : base(ExecutionLogPrivate, id, sportId, sportEntityFactory, sportEventStatusCache, sportEventCache, cultures, exceptionStrategy, matchStatusCache)
        {
            _sportEntityFactory = sportEntityFactory;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IMatchStatus"/> containing information about the progress of the match
        /// </summary>
        /// <returns>A <see cref="Task{IMatchStatus}"/> containing information about the progress of the match</returns>
        public new async Task<IMatchStatus> GetStatusAsync()
        {
            var item = await GetSportEventStatusCacheItem().ConfigureAwait(false);
            return item == null ? null : new MatchStatus(item, MatchStatusCache);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ISeasonInfo"/> representing the season to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ISeasonInfo}"/> representing the retrieval operation</returns>
        public async Task<ISeasonInfo> GetSeasonAsync()
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetSeasonAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<SeasonCacheItem>>(matchCacheItem.GetSeasonAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Season")).ConfigureAwait(false);

            return item == null ? null : new SeasonInfo(item);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IRound"/> representing the tournament round to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{IRound}"/> representing the retrieval operation</returns>
        public async Task<IRound> GetTournamentRoundAsync()
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var item = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetTournamentRoundAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<RoundCacheItem>>(matchCacheItem.GetTournamentRoundAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Round")).ConfigureAwait(false);

            return item == null ? null : new Round(item, Cultures);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing home competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        public async Task<ITeamCompetitor> GetHomeCompetitorAsync()
        {
            return await GetMatchCompetitorAsync(0, Cultures).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing home competitor of the match associated with the current instance
        /// </summary>
        /// <param name="culture">The culture in which we want to return competitor data</param>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        private async Task<ITeamCompetitor> GetHomeCompetitorAsync(CultureInfo culture)
        {
            var cultureList = new[] { culture };
            return await GetMatchCompetitorAsync(0, cultureList).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing away competitor of the match associated with the current instance
        /// </summary>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        public async Task<ITeamCompetitor> GetAwayCompetitorAsync()
        {
            return await GetMatchCompetitorAsync(1, Cultures).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ITeamCompetitor" /> representing away competitor of the match associated with the current instance
        /// </summary>
        /// <param name="culture">The culture in which we want to return competitor data</param>
        /// <returns>A <see cref="Task{ITeamCompetitor}"/> representing the retrieval operation</returns>
        private async Task<ITeamCompetitor> GetAwayCompetitorAsync(CultureInfo culture)
        {
            var cultureList = new[] { culture };
            return await GetMatchCompetitorAsync(1, cultureList).ConfigureAwait(false);
        }

        private async Task<ITeamCompetitor> GetMatchCompetitorAsync(int competitorIndex, IReadOnlyCollection<CultureInfo> wantedCultures)
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var competitorsIds = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                                     ? await matchCacheItem.GetCompetitorsIdsAsync(wantedCultures).ConfigureAwait(false)
                                     : await new Func<IEnumerable<CultureInfo>, Task<IEnumerable<Urn>>>(matchCacheItem.GetCompetitorsIdsAsync).SafeInvokeAsync(wantedCultures, ExecutionLog, GetFetchErrorMessage("CompetitorsIds")).ConfigureAwait(false);

            if (competitorsIds == null)
            {
                return null;
            }

            var competitorUrns = competitorsIds.ToList();
            if (competitorUrns.Count == 2)
            {
                return await _sportEntityFactory.BuildTeamCompetitorAsync(competitorUrns[competitorIndex], wantedCultures, matchCacheItem, ExceptionStrategy).ConfigureAwait(false);
            }

            ExecutionLog.LogError("Received {CompetitorCount} competitors for match[Id = {SportEventId}]. Match can have only 2 competitors", competitorUrns.Count, Id);
            throw new InvalidOperationException($"Invalid number of competitors. Match must have exactly 2 competitors, received {competitorUrns.Count}");
        }

        /// <summary>
        /// Asynchronously gets a <see cref="ILongTermEvent"/> representing the tournament to which the sport event associated with the current instance belongs to
        /// </summary>
        /// <returns>A <see cref="Task{ILongTermEvent}"/> representing the retrieval operation</returns>
        public async Task<ILongTermEvent> GetTournamentAsync()
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var tournamentId = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetTournamentIdAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<Urn>>(matchCacheItem.GetTournamentIdAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Tournament")).ConfigureAwait(false);

            return tournamentId == null
                       ? null
                       : _sportEntityFactory.BuildSportEvent<ILongTermEvent>(tournamentId, SportId, Cultures.ToList(), ExceptionStrategy);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="IFixture"/> instance containing information about the arranged sport event
        /// </summary>
        /// <returns>A <see cref="Task{IFixture}"/> representing the retrieval operation</returns>
        /// <remarks>A Fixture is a sport event that has been arranged for a particular time and place</remarks>
        public async Task<IFixture> GetFixtureAsync()
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            return ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetFixtureAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<IFixture>>(matchCacheItem.GetFixtureAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("Fixture")).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the associated event timeline
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        public async Task<IEventTimeline> GetEventTimelineAsync()
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }

            var eventTimelineCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetEventTimelineAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<EventTimelineCacheItem>>(matchCacheItem.GetEventTimelineAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("EventTimeline")).ConfigureAwait(false);

            return eventTimelineCacheItem == null
                ? null
                : new EventTimeline(eventTimelineCacheItem);
        }

        /// <summary>
        /// Asynchronously gets the associated event timeline for single culture
        /// </summary>
        /// <param name="culture">The languages to which the returned instance should be translated</param>
        /// <remarks>Recommended to be used when only <see cref="IEventTimeline"/> is needed for this <see cref="IMatch"/></remarks>
        /// <returns>A <see cref="Task{IEventTimeline}"/> representing the retrieval operation</returns>
        public async Task<IEventTimeline> GetEventTimelineAsync(CultureInfo culture)
        {
            var matchCacheItem = (IMatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }

            var oneCulture = new List<CultureInfo> { culture ?? Cultures.First() };
            var eventTimelineCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetEventTimelineAsync(oneCulture).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<EventTimelineCacheItem>>(matchCacheItem.GetEventTimelineAsync).SafeInvokeAsync(oneCulture, ExecutionLog, GetFetchErrorMessage("EventTimeline")).ConfigureAwait(false);

            return eventTimelineCacheItem == null
                ? null
                : new EventTimeline(eventTimelineCacheItem);
        }

        /// <summary>
        /// Asynchronously gets the associated delayed info
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the retrieval operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IDelayedInfo> GetDelayedInfoAsync()
        {
            var matchCacheItem = (MatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var delayedInfoCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetDelayedInfoAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<DelayedInfoCacheItem>>(matchCacheItem.GetDelayedInfoAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("DelayedInfo")).ConfigureAwait(false);

            return delayedInfoCacheItem == null
                ? null
                : new DelayedInfo(delayedInfoCacheItem);
        }

        /// <summary>
        /// Asynchronously gets the associated coverage info
        /// </summary>
        /// <returns>A <see cref="Task{ICoverageInfo}"/> representing the retrieval operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICoverageInfo> GetCoverageInfoAsync()
        {
            var matchCacheItem = (MatchCacheItem)SportEventCache.GetEventCacheItem(Id);
            if (matchCacheItem == null)
            {
                LogMissingCacheItem();
                return null;
            }
            var coverageInfoCacheItem = ExceptionStrategy == ExceptionHandlingStrategy.Throw
                ? await matchCacheItem.GetCoverageInfoAsync(Cultures).ConfigureAwait(false)
                : await new Func<IEnumerable<CultureInfo>, Task<CoverageInfoCacheItem>>(matchCacheItem.GetCoverageInfoAsync).SafeInvokeAsync(Cultures, ExecutionLog, GetFetchErrorMessage("CoverageInfo")).ConfigureAwait(false);

            return coverageInfoCacheItem == null
                ? null
                : new CoverageInfo(coverageInfoCacheItem);
        }

        /// <inheritdoc />
        public new async Task<string> GetNameAsync(CultureInfo culture)
        {
            var name = await base.GetNameAsync(culture).ConfigureAwait(false);

            if (string.IsNullOrEmpty(name))
            {
                var homeCompetitor = await GetHomeCompetitorAsync(culture).ConfigureAwait(false);
                var awayCompetitor = await GetAwayCompetitorAsync(culture).ConfigureAwait(false);

                if (homeCompetitor != null && awayCompetitor != null)
                {
                    name = $"{homeCompetitor.GetName(culture)} vs. {awayCompetitor.GetName(culture)}";
                }
            }

            return name;
        }
    }
}
