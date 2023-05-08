/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages;
using Xunit.Abstractions;
// ReSharper disable NotAccessedField.Local

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestSportEntityFactory : ISportEntityFactory
    {
        private readonly ITestOutputHelper _outputHelper;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
        private readonly ISportDataCache _sportDataCache;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
        private readonly ISportEventCache _sportEventCache;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
        private readonly ISportEventStatusCache _eventStatusCache;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
        private readonly ILocalizedNamedValueCache _matchStatusCache;
        private readonly IProfileCache _profileCache;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Allowed")]
        private readonly IReadOnlyCollection<URN> _soccerSportUrns;
        private readonly TestDataRouterManager _dataRouterManager;

        public TestSportEntityFactory(ITestOutputHelper outputHelper,
                                      ISportDataCache sportDataCache = null,
                                      ISportEventCache sportEventCache = null,
                                      ISportEventStatusCache eventStatusCache = null,
                                      ILocalizedNamedValueCache matchStatusCache = null,
                                      IProfileCache profileCache = null,
                                      IReadOnlyCollection<URN> soccerSportUrns = null)
        {
            var cacheManager = new CacheManager();
            var profileMemoryCache = new MemoryCache("ProfileCache");

            _outputHelper = outputHelper;
            _sportDataCache = sportDataCache;
            _sportEventCache = sportEventCache;
            _eventStatusCache = eventStatusCache;
            _matchStatusCache = matchStatusCache;
            _dataRouterManager = new TestDataRouterManager(cacheManager, outputHelper);
            _profileCache = profileCache ?? new ProfileCache(profileMemoryCache, _dataRouterManager, cacheManager, _sportEventCache);
            _soccerSportUrns = soccerSportUrns ?? SdkInfo.SoccerSportUrns;
        }

        public Task<IEnumerable<ISport>> BuildSportsAsync(IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            throw new NotImplementedException();
        }

        public Task<ISport> BuildSportAsync(URN sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            throw new NotImplementedException();
        }

        public Task<IPlayer> BuildPlayerAsync(URN playerId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            var playerNames = cultures.ToDictionary(culture => culture, culture => $"PlayerName {culture.TwoLetterISOLanguageName}");
            var player = new Player(playerId, playerNames);
            return Task.FromResult((IPlayer)player);
        }

        public async Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<URN> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            if (playersIds.IsNullOrEmpty())
            {
                return new List<IPlayer>();
            }

            var players = new List<IPlayer>();
            foreach (var playersId in playersIds)
            {
                var playerCi = await _profileCache.GetPlayerProfileAsync(playersId, cultures, true).ConfigureAwait(false);
                if (playerCi != null)
                {
                    players.Add(new Player(playerCi.Id, playerCi.Names));
                }
                else
                {
                    _outputHelper.WriteLine($"No player for {playersId}");
                }
            }
            return players;
        }

        public T BuildSportEvent<T>(URN eventId, URN sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            ICompetition competition;
            switch (eventId.TypeGroup)
            {
                case ResourceTypeGroup.MATCH:
                    {
                        competition = new TestMatch(eventId);
                        break;
                    }
                case ResourceTypeGroup.STAGE:
                    {
                        competition = new TestStage(eventId);
                        break;
                    }
                case ResourceTypeGroup.TOURNAMENT:
                case ResourceTypeGroup.BASIC_TOURNAMENT:
                case ResourceTypeGroup.SEASON:
                case ResourceTypeGroup.OTHER:
                case ResourceTypeGroup.UNKNOWN:
                case ResourceTypeGroup.DRAW:
                case ResourceTypeGroup.LOTTERY:
                default:
                    {
                        throw new ArgumentException($"ResourceTypeGroup '{eventId.TypeGroup}' is not supported.", nameof(eventId));
                    }
            }
            return (T)competition;
        }

        public ICompetitor BuildCompetitor(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences,
            ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ICompetitor BuildCompetitor(CompetitorCI competitorCI, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCI, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCI);
        }

        public ICompetitor BuildCompetitor(CompetitorCI competitorCI, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            return new Competitor(competitorCI, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
        }

        public ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCI teamCompetitorCI, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            var teamCompetitor = new TeamCompetitor(teamCompetitorCI, cultures, this, exceptionStrategy, _profileCache, rootCompetitionCI);
            return teamCompetitor;
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitor = new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, rootCompetitionCI.GetCompetitorsReferencesAsync().GetAwaiter().GetResult());
            return Task.FromResult((ICompetitor)competitor);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        public Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy)
        {
            var competitor = new Competitor(competitorId, _profileCache, cultures, this, exceptionStrategy, competitorsReferences);
            return Task.FromResult((ICompetitor)competitor);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="URN"/> of the <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public Task<ITeamCompetitor> BuildTeamCompetitorAsync(URN teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCI rootCompetitionCI, ExceptionHandlingStrategy exceptionStrategy)
        {
            var teamCompetitor = MessageFactorySdk.GetTeamCompetitor((int)teamCompetitorId.Id);
            return Task.FromResult(teamCompetitor);
        }

        /// <summary>
        /// Builds the instance of the <see cref="ICategorySummary"/> class
        /// </summary>
        /// <param name="categoryId">A <see cref="URN"/> of the <see cref="ICategorySummary"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="ICategorySummary"/></param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        public Task<ICategorySummary> BuildCategoryAsync(URN categoryId, IReadOnlyCollection<CultureInfo> cultures)
        {
            var category = MessageFactorySdk.GetCategory((int)categoryId.Id);
            var categorySummary = new CategorySummary(category.Id, category.Names, category.CountryCode);
            return Task.FromResult((ICategorySummary)categorySummary);
        }
    }
}
