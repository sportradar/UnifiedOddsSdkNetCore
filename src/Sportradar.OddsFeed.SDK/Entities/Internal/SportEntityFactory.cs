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
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A factory used to construct <see cref="ISportEvent" /> and <see cref="ITournament" /> instances
    /// </summary>
    /// <seealso cref="ISportEntityFactory" />
    internal class SportEntityFactory : ISportEntityFactory
    {
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(SportEntityFactory));

        /// <summary>
        /// A <see cref="ISportDataCache"/> instance used to retrieve sport related info
        /// </summary>
        private readonly ISportDataCache _sportDataCache;

        /// <summary>
        /// A <see cref="ISportEventCache"/> instance used to retrieve sport events
        /// </summary>
        private readonly ISportEventCache _sportEventCache;

        /// <summary>
        /// A <see cref="ISportEventStatusCache"/> used to retrieve statuses of sport events
        /// </summary>
        private readonly ISportEventStatusCache _eventStatusCache;

        /// <summary>
        /// The match status cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusCache;

        /// <summary>
        /// The profile cache
        /// </summary>
        private readonly IProfileCache _profileCache;

        /// <summary>
        /// The list of sport urns that represents soccer sport
        /// </summary>
        private readonly IReadOnlyCollection<Urn> _soccerSportUrns;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityFactory"/> class
        /// </summary>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve sport related info</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance used to retrieve sport events</param>
        /// <param name="eventStatusCache">A <see cref="ISportEventStatusCache"/> used to retrieve statuses of sport events</param>
        /// <param name="matchStatusCache">A <see cref="ILocalizedNamedValueCache"/> used to retrieve match statuses</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used to retrieve player profiles</param>
        /// <param name="soccerSportUrns">A list of sport urns that have soccer matches</param>
        public SportEntityFactory(ISportDataCache sportDataCache,
                                  ISportEventCache sportEventCache,
                                  ISportEventStatusCache eventStatusCache,
                                  ILocalizedNamedValueCache matchStatusCache,
                                  IProfileCache profileCache,
                                  IReadOnlyCollection<Urn> soccerSportUrns)
        {
            Guard.Argument(sportDataCache, nameof(sportDataCache)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();
            Guard.Argument(eventStatusCache, nameof(eventStatusCache)).NotNull();
            Guard.Argument(matchStatusCache, nameof(matchStatusCache)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(soccerSportUrns, nameof(soccerSportUrns)).NotNull().NotEmpty();

            _sportDataCache = sportDataCache;
            _sportEventCache = sportEventCache;
            _eventStatusCache = eventStatusCache;
            _matchStatusCache = matchStatusCache;
            _profileCache = profileCache;
            _soccerSportUrns = soccerSportUrns;
        }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="ISport"/> from the provided data
        /// </summary>
        /// <param name="sportData">A <see cref="SportData"/> instance containing info about the sport</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying the languages to which the sport data is translated</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ISport"/> instance</returns>
        private ISport BuildSportInternal(SportData sportData, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            Guard.Argument(sportData, nameof(sportData)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            var categories = sportData.Categories?.Select(categoryData => new Category(
                categoryData.Id,
                categoryData.Names,
                categoryData.CountryCode,
                categoryData.Tournaments.Select(tournamentUrn => BuildSportEvent<ISportEvent>(tournamentUrn, sportData.Id, cultures, exceptionStrategy)).ToList())).Cast<ICategory>().ToList();

            return new Sport(
                sportData.Id,
                sportData.Names,
                categories);
        }

        /// <summary>
        /// build sports as an asynchronous operation.
        /// </summary>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public async Task<IEnumerable<ISport>> BuildSportsAsync(IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var sportsData = await _sportDataCache.GetSportsAsync(cultures).ConfigureAwait(false);
            return sportsData?.Select(data => BuildSportInternal(data, cultures, exceptionStrategy)).ToList();
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="ISport"/> representing the sport specified by its id
        /// </summary>
        /// <param name="sportId">A <see cref="Urn"/> identifying the sport which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{ITournament}"/> representing the asynchronous operation</returns>
        public async Task<ISport> BuildSportAsync(Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var sportData = await _sportDataCache.GetSportAsync(sportId, cultures).ConfigureAwait(false);
            return sportData == null
                ? null
                : BuildSportInternal(sportData, cultures, exceptionStrategy);
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player or competitor profile specified by its id
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player or competitor which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>        /// <returns>A <see cref="Task{IPlayer}"/> representing the asynchronous operation</returns>
        public async Task<IPlayer> BuildPlayerAsync(Urn playerId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            if (playerId.Type.Equals("competitor", StringComparison.OrdinalIgnoreCase))
            {
                var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(playerId, cultures, false).ConfigureAwait(false);
                return competitorCacheItem == null
                    ? null
                    : new Competitor(competitorCacheItem, _profileCache, cultures, this, exceptionStrategy, (ICompetitionCacheItem)null);
            }

            var playerProfileCacheItem = await _profileCache.GetPlayerProfileAsync(playerId, cultures, false).ConfigureAwait(false);
            return playerProfileCacheItem == null
                ? null
                : new PlayerProfile(playerProfileCacheItem, cultures);
        }

        public async Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, IDictionary<Urn, int> playersJerseyNumbers)
        {
            var result = new List<IPlayer>();

            var competitorPlayers = await BuildAssociatedCompetitorPlayersAsync(playersIds, cultures, playersJerseyNumbers).ConfigureAwait(false);
            var competitors = await BuildAssociatedCompetitorsAsync(playersIds, cultures, exceptionStrategy).ConfigureAwait(false);
            result.AddRange(competitorPlayers);
            result.AddRange(competitors);

            return result;
        }

        private async Task<IEnumerable<IPlayer>> BuildAssociatedCompetitorPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, int> playersJerseyNumbers)
        {
            var result = new List<IPlayer>();

            var playerTasks = new List<Task<PlayerProfileCacheItem>>();
            foreach (var id in playersIds)
            {
                if (id.Type.Equals("player", StringComparison.OrdinalIgnoreCase))
                {
                    playerTasks.Add(_profileCache.GetPlayerProfileAsync(id, cultures, false));
                }
            }
            await Task.WhenAll(playerTasks).ConfigureAwait(false);

            foreach (var playerTask in playerTasks)
            {
                if (playerTask.IsCompleted && playerTask.GetAwaiter().GetResult() != null)
                {
                    var playerJerseyNumber = playersJerseyNumbers.TryGetValue(playerTask.Result.Id, out var jerseyNumber) ? jerseyNumber : (int?)null;
                    result.Add(new CompetitorPlayer(playerTask.Result, cultures, playerJerseyNumber));
                }
            }

            return result;
        }

        private async Task<IEnumerable<IPlayer>> BuildAssociatedCompetitorsAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var result = new List<IPlayer>();

            var competitorTasks = new List<Task<CompetitorCacheItem>>();
            foreach (var id in playersIds)
            {
                if (id.Type.Equals("competitor", StringComparison.OrdinalIgnoreCase))
                {
                    competitorTasks.Add(_profileCache.GetCompetitorProfileAsync(id, cultures, false));
                }
            }
            await Task.WhenAll(competitorTasks).ConfigureAwait(false);

            foreach (var competitorTask in competitorTasks)
            {
                if (competitorTask.IsCompleted && competitorTask.GetAwaiter().GetResult() != null)
                {
                    result.Add(new Competitor(competitorTask.GetAwaiter().GetResult(), _profileCache, cultures, this, exceptionStrategy, (ICompetitionCacheItem)null));
                }
            }

            return result;
        }

        /// <summary>
        /// Builds the <see cref="ISportEvent" /> derived class based on specified id
        /// </summary>
        /// <typeparam name="T">A <see cref="ISportEvent" /> derived type</typeparam>
        /// <param name="eventId">The identifier</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">The cultures used for returned instance</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetition"/> derived instance</returns>
        // ReSharper disable once CognitiveComplexity
        public T BuildSportEvent<T>(Urn eventId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            ISportEvent sportEvent = null;
            switch (eventId.TypeGroup)
            {
                case ResourceTypeGroup.Match:
                    {
                        if (sportId != null && _soccerSportUrns.Contains(sportId))
                        {
                            sportEvent = new SoccerEvent(eventId, sportId, this, _sportEventCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        }
                        else
                        {
                            sportEvent = new Match(eventId, sportId, this, _sportEventCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        }
                        break;
                    }
                case ResourceTypeGroup.Stage:
                    {
                        sportEvent = new Stage(eventId, sportId, this, _sportEventCache, _sportDataCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.BasicTournament:
                    {
                        sportEvent = new BasicTournament(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Tournament:
                    {
                        sportEvent = new Tournament(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Season:
                    {
                        sportEvent = new Season(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Draw:
                    {
                        sportEvent = new Draw(eventId, sportId, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Lottery:
                    {
                        sportEvent = new Lottery(eventId, sportId, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Unknown:
                    {
                        _executionLog.LogWarning("Received entity with unknown resource type group: eventId={SportEventId}, sportId={SportId}", eventId, sportId);
                        sportEvent = new SportEvent(eventId, sportId, null, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Other:
                    break;
                default:
                    throw new ArgumentException($"ResourceTypeGroup:{eventId.TypeGroup} is not supported", nameof(eventId));
            }
            return (T)sportEvent;
        }

        public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCacheItem, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCacheItem);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="CompetitorCacheItem"/> id used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public ICompetitor BuildCompetitor(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCacheItem, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCacheItem teamCompetitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new TeamCompetitor(teamCompetitorCacheItem, cultures, this, exceptionStrategy, _profileCache, rootCompetitionCacheItem);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(competitorId, cultures, false).ConfigureAwait(false);
            if (competitorCacheItem != null)
            {
                return BuildCompetitor(competitorCacheItem, cultures, rootCompetitionCacheItem, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(competitorId, cultures, false).ConfigureAwait(false);
            if (competitorCacheItem != null)
            {
                return BuildCompetitor(competitorCacheItem, cultures, competitorsReferences, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="Urn"/> of the <see cref="TeamCompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public async Task<ITeamCompetitor> BuildTeamCompetitorAsync(Urn teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCacheItem = await _profileCache.GetCompetitorProfileAsync(teamCompetitorId, cultures, false).ConfigureAwait(false);
            if (competitorCacheItem is TeamCompetitorCacheItem teamCompetitorCacheItem)
            {
                return BuildTeamCompetitor(teamCompetitorCacheItem, cultures, rootCompetitionCacheItem, exceptionStrategy);
            }
            if (competitorCacheItem != null)
            {
                var teamCacheItem = new TeamCompetitorCacheItem(competitorCacheItem);
                return BuildTeamCompetitor(teamCacheItem, cultures, rootCompetitionCacheItem, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICategorySummary"/> class
        /// </summary>
        /// <param name="categoryId">A <see cref="Urn"/> of the <see cref="ICategorySummary"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="ICategorySummary"/></param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public async Task<ICategorySummary> BuildCategoryAsync(Urn categoryId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, cultures).ConfigureAwait(false);
            return categoryData != null ? new CategorySummary(categoryData.Id, categoryData.Names, categoryData.CountryCode) : null;
        }
    }
}
