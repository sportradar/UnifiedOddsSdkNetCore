/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;

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
        private readonly IReadOnlyCollection<URN> _soccerSportUrns;

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
                                  IReadOnlyCollection<URN> soccerSportUrns)
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
        /// <param name="sportId">A <see cref="URN"/> identifying the sport which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{ITournament}"/> representing the asynchronous operation</returns>
        public async Task<ISport> BuildSportAsync(URN sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var sportData = await _sportDataCache.GetSportAsync(sportId, cultures).ConfigureAwait(false);
            return sportData == null
                ? null
                : BuildSportInternal(sportData, cultures, exceptionStrategy);
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player or competitor profile specified by its id
        /// </summary>
        /// <param name="playerId">A <see cref="URN"/> specifying the id of the player or competitor which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>        /// <returns>A <see cref="Task{IPlayer}"/> representing the asynchronous operation</returns>
        public async Task<IPlayer> BuildPlayerAsync(URN playerId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            if (playerId.Type.Equals("competitor", StringComparison.InvariantCultureIgnoreCase))
            {
                var competitorCI = await _profileCache.GetCompetitorProfileAsync(playerId, cultures, false).ConfigureAwait(false);
                return competitorCI == null
                    ? null
                    : new Competitor(competitorCI, _profileCache, cultures, this, exceptionStrategy, (ICompetitionCI)null);
            }

            var playerProfileCI = await _profileCache.GetPlayerProfileAsync(playerId, cultures, false).ConfigureAwait(false);
            return playerProfileCI == null
                ? null
                : new PlayerProfile(playerProfileCI, cultures);
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player and/or competitor profiles specified by ids
        /// </summary>
        /// <param name="playersIds">A list of <see cref="URN"/> specifying the ids of the players or competitors which will be represented by the constructed instances</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<URN> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var playerTasks = new List<Task<PlayerProfileCI>>();
            var competitorTasks = new List<Task<CompetitorCI>>();
            foreach (var id in playersIds)
            {
                if (id.Type.Equals("competitor", StringComparison.InvariantCultureIgnoreCase))
                {
                    competitorTasks.Add(_profileCache.GetCompetitorProfileAsync(id, cultures, false));
                }
                else
                {
                    playerTasks.Add(_profileCache.GetPlayerProfileAsync(id, cultures, false));
                }
            }

            await Task.WhenAll(competitorTasks).ConfigureAwait(false);
            await Task.WhenAll(playerTasks).ConfigureAwait(false);

            var result = new List<IPlayer>();
            foreach (var competitorTask in competitorTasks)
            {
                if (competitorTask.IsCompleted && competitorTask.GetAwaiter().GetResult() != null)
                {
                    result.Add(new Competitor(competitorTask.GetAwaiter().GetResult(), _profileCache, cultures, this, exceptionStrategy, (ICompetitionCI)null));
                }
            }

            foreach (var playerTask in playerTasks)
            {
                if (playerTask.IsCompleted && playerTask.GetAwaiter().GetResult() != null)
                {
                    result.Add(new PlayerProfile(playerTask.GetAwaiter().GetResult(), cultures));
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
        public T BuildSportEvent<T>(URN eventId, URN sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            ISportEvent sportEvent = null;
            switch (eventId.TypeGroup)
            {
                case ResourceTypeGroup.MATCH:
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
                case ResourceTypeGroup.STAGE:
                    {
                        sportEvent = new Stage(eventId, sportId, this, _sportEventCache, _sportDataCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.BASIC_TOURNAMENT:
                    {
                        sportEvent = new BasicTournament(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.TOURNAMENT:
                    {
                        sportEvent = new Tournament(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.SEASON:
                    {
                        sportEvent = new Season(eventId, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.DRAW:
                    {
                        sportEvent = new Draw(eventId, sportId, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.LOTTERY:
                    {
                        sportEvent = new Lottery(eventId, sportId, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.UNKNOWN:
                    {
                        _executionLog.LogWarning($"Received entity with unknown resource type group: id={eventId}, id={sportId}");
                        sportEvent = new SportEvent(eventId, sportId, null, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }

                case ResourceTypeGroup.OTHER:
                    break;
                default:
                    throw new ArgumentException($"ResourceTypeGroup:{eventId.TypeGroup} is not supported", nameof(eventId));
            }
            return (T)sportEvent;
        }

        public ICompetitor BuildCompetitor(CompetitorCI competitorCI, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCI, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCI);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="CompetitorCI"/> id used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public ICompetitor BuildCompetitor(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ICompetitor BuildCompetitor(CompetitorCI competitorCI, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCI, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCI teamCompetitorCI, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new TeamCompetitor(teamCompetitorCI, cultures, this, exceptionStrategy, _profileCache, rootCompetitionCI);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(competitorId, cultures, false).ConfigureAwait(false);
            if (competitorCI != null)
            {
                return BuildCompetitor(competitorCI, cultures, rootCompetitionCI, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(competitorId, cultures, false).ConfigureAwait(false);
            if (competitorCI != null)
            {
                return BuildCompetitor(competitorCI, cultures, competitorsReferences, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="URN"/> of the <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public async Task<ITeamCompetitor> BuildTeamCompetitorAsync(URN teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(teamCompetitorId, cultures, false).ConfigureAwait(false);
            if (competitorCI is TeamCompetitorCI teamCompetitorCI)
            {
                return BuildTeamCompetitor(teamCompetitorCI, cultures, rootCompetitionCI, exceptionStrategy);
            }
            if (competitorCI != null)
            {
                var teamCI = new TeamCompetitorCI(competitorCI);
                return BuildTeamCompetitor(teamCI, cultures, rootCompetitionCI, exceptionStrategy);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICategorySummary"/> class
        /// </summary>
        /// <param name="categoryId">A <see cref="URN"/> of the <see cref="ICategorySummary"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="ICategorySummary"/></param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public async Task<ICategorySummary> BuildCategoryAsync(URN categoryId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, cultures).ConfigureAwait(false);
            return categoryData != null ? new CategorySummary(categoryData.Id, categoryData.Names, categoryData.CountryCode) : null;
        }
    }
}
