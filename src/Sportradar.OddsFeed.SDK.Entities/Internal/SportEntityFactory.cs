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
        protected readonly ILog ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(SportEntityFactory));
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
        /// Initializes a new instance of the <see cref="SportEntityFactory"/> class
        /// </summary>
        /// <param name="sportDataCache">A <see cref="ISportDataCache"/> instance used to retrieve sport related info</param>
        /// <param name="sportEventCache">A <see cref="ISportEventCache"/> instance used to retrieve sport events</param>
        /// <param name="eventStatusCache">A <see cref="ISportEventStatusCache"/> used to retrieve statuses of sport events</param>
        /// <param name="matchStatusCache">A <see cref="ILocalizedNamedValueCache"/> used to retrieve match statuses</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used to retrieve player profiles</param>
        public SportEntityFactory(
            ISportDataCache sportDataCache,
            ISportEventCache sportEventCache,
            ISportEventStatusCache eventStatusCache,
            ILocalizedNamedValueCache matchStatusCache,
            IProfileCache profileCache)
        {
            Contract.Requires(sportDataCache != null);
            Contract.Requires(sportEventCache != null);
            Contract.Requires(eventStatusCache != null);
            Contract.Requires(matchStatusCache != null);
            Contract.Requires(profileCache != null);

            _sportDataCache = sportDataCache;
            _sportEventCache = sportEventCache;
            _eventStatusCache = eventStatusCache;
            _matchStatusCache = matchStatusCache;
            _profileCache = profileCache;
        }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="ISport"/> from the provided data
        /// </summary>
        /// <param name="sportData">A <see cref="SportData"/> instance containing info about the sport</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the languages to which the sport data is translated</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ISport"/> instance</returns>
        private ISport BuildSportInternal(SportData sportData, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            Contract.Requires(sportData != null);
            Contract.Requires(cultures != null && cultures.Any());

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
        public async Task<IEnumerable<ISport>> BuildSportsAsync(IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            var sportsData = await _sportDataCache.GetSportsAsync(cultureList).ConfigureAwait(false);
            return sportsData?.Select(data => BuildSportInternal(data, cultureList, exceptionStrategy)).ToList();
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="ISport"/> representing the sport specified by its id
        /// </summary>
        /// <param name="sportId">A <see cref="URN"/> identifying the sport which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{ITournament}"/> representing the asynchronous operation</returns>
        public async Task<ISport> BuildSportAsync(URN sportId, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            var sportData = await _sportDataCache.GetSportAsync(sportId, cultureList).ConfigureAwait(false);
            return sportData == null
                ? null
                : BuildSportInternal(sportData, cultureList, exceptionStrategy);
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player or competitor profile specified by its id
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the player or competitor which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <returns>A <see cref="Task{IPlayer}"/> representing the asynchronous operation</returns>
        public async Task<IPlayer> BuildPlayerAsync(URN id, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            if (id.Type.Equals("competitor", StringComparison.InvariantCultureIgnoreCase))
            {
                var competitorCI = await _profileCache.GetCompetitorProfileAsync(id, cultureList).ConfigureAwait(false);
                return competitorCI == null
                    ? null
                    : new Competitor(competitorCI, _profileCache, cultureList, this, (ICompetitionCI) null);
            }

            var playerProfileCI = await _profileCache.GetPlayerProfileAsync(id, cultureList).ConfigureAwait(false);
            return playerProfileCI == null
                ? null
                : new PlayerProfile(playerProfileCI, cultureList);
        }

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player and/or competitor profiles specified by ids
        /// </summary>
        /// <param name="ids">A list of <see cref="URN"/> specifying the ids of the players or competitors which will be represented by the constructed instances</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        public async Task<IEnumerable<IPlayer>> BuildPlayersAsync(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            var playerTasks = new List<Task<PlayerProfileCI>>();
            var competitorTasks = new List<Task<CompetitorCI>>();
            foreach (var id in ids)
            {
                if (id.Type.Equals("competitor", StringComparison.InvariantCultureIgnoreCase))
                {
                    competitorTasks.Add(_profileCache.GetCompetitorProfileAsync(id, cultureList));
                }
                else
                {
                    playerTasks.Add(_profileCache.GetPlayerProfileAsync(id, cultureList));
                }
            }
            await Task.WhenAll(competitorTasks).ConfigureAwait(false);
            await Task.WhenAll(playerTasks).ConfigureAwait(false);

            var result = new List<IPlayer>();
            foreach (var competitorTask in competitorTasks)
            {
                result.Add(new Competitor(competitorTask.Result, _profileCache, cultureList, this, (ICompetitionCI) null));
            }

            foreach (var playerTask in playerTasks)
            {
                result.Add(new PlayerProfile(playerTask.Result, cultureList));
            }

            return result;
        }

        /// <summary>
        /// Builds the <see cref="ISportEvent" /> derived class based on specified id
        /// </summary>
        /// <typeparam name="T">A <see cref="ISportEvent" /> derived type</typeparam>
        /// <param name="id">The identifier</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">The cultures used for returned instance</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetition"/> derived instance</returns>
        public T BuildSportEvent<T>(URN id, URN sportId, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            ISportEvent sportEvent;
            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.MATCH:
                    {
                        if (sportId != null && sportId.Equals(URN.Parse("sr:sport:1")))
                        {
                            sportEvent = new SoccerEvent(id, sportId, this, _sportEventCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        }
                        else
                        {
                            sportEvent = new Match(id, sportId, this, _sportEventCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        }
                        break;
                    }
                case ResourceTypeGroup.STAGE:
                    {
                        sportEvent = new Stage(id, sportId, this, _sportEventCache, _sportDataCache, _eventStatusCache, _matchStatusCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.BASIC_TOURNAMENT:
                    {
                        sportEvent = new BasicTournament(id, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.TOURNAMENT:
                    {
                        sportEvent = new Tournament(id, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.SEASON:
                    {
                        sportEvent = new Season(id, sportId, this, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.DRAW:
                    {
                        sportEvent = new Draw(id, sportId, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.LOTTERY:
                    {
                        sportEvent = new Lottery(id, sportId, _sportEventCache, _sportDataCache, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.UNKNOWN:
                    {
                        sportEvent = new SportEvent(id, sportId, null, _sportEventCache, cultures, exceptionStrategy);
                        break;
                    }
                default:
                    throw new ArgumentException($"ResourceTypeGroup:{id.TypeGroup} is not supported", nameof(id));
            }
            return (T)sportEvent;
        }

        public ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            return new Competitor(ci, _profileCache, cultures, this, rootCompetitionCI);
        }

        public ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences)
        {
            return new Competitor(ci, _profileCache, cultures, this, competitorsReferences);
        }

        public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCI ci, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            return new TeamCompetitor(ci, cultures, this, _profileCache, rootCompetitionCI);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            var cultureInfos = cultures.ToList();
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(competitorId, cultureInfos).ConfigureAwait(false);
            if (competitorCI != null)
            {
                return BuildCompetitor(competitorCI, cultureInfos, rootCompetitionCI);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public async Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences)
        {
            var cultureInfos = cultures.ToList();
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(competitorId, cultureInfos).ConfigureAwait(false);
            if (competitorCI != null)
            {
                return BuildCompetitor(competitorCI, cultureInfos, competitorsReferences);
            }
            return null;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="URN"/> of the <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public async Task<ITeamCompetitor> BuildTeamCompetitorAsync(URN teamCompetitorId, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI)
        {
            var cultureInfos = cultures.ToList();
            var competitorCI = await _profileCache.GetCompetitorProfileAsync(teamCompetitorId, cultureInfos).ConfigureAwait(false);
            var teamCompetitorCI = competitorCI as TeamCompetitorCI;
            if (teamCompetitorCI != null)
            {
                return BuildTeamCompetitor(teamCompetitorCI, cultureInfos, rootCompetitionCI);
            }
            if (competitorCI != null)
            {
                ExecutionLog.Warn($"Transforming CompetitorCI to TeamCompetitorCI for {teamCompetitorId}");
                var teamCI = new TeamCompetitorCI(competitorCI);
                return BuildTeamCompetitor(teamCI, cultureInfos, rootCompetitionCI);
            }
            return null;
        }
    }
}
