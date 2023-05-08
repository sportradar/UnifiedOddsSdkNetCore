/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Xunit.Abstractions;
using RestMessage = Sportradar.OddsFeed.SDK.Messages.REST.RestMessage;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class ScheduleData
    {
        public static readonly string RestXmlPath = Directory.GetCurrentDirectory() + "/REST XMLs/";
        public static readonly string FeedXmlPath = Directory.GetCurrentDirectory() + "/XMLs/";

        public static readonly URN MatchId = URN.Parse("sr:match:34152237");
        public static readonly URN MatchSportId = URN.Parse("sr:sport:1");
        public static readonly URN MatchSeasonId = URN.Parse("sr:season:93741");
        public static readonly URN MatchTournamentId = URN.Parse("sr:tournament:17");
        public static readonly URN MatchCompetitorId1 = URN.Parse("sr:competitor:17");
        public static readonly URN MatchCompetitorId2 = URN.Parse("sr:competitor:37");
        public static readonly URN MatchCompetitor1PlayerId1 = URN.Parse("sr:player:44614");
        public static readonly URN MatchCompetitor1PlayerId2 = URN.Parse("sr:player:318941");
        public static readonly URN MatchCompetitor2PlayerId1 = URN.Parse("sr:player:15479");
        public static readonly URN MatchCompetitor2PlayerId2 = URN.Parse("sr:player:1372516");
        public static readonly int MatchCompetitor1PlayerCount = 26;
        public static readonly int MatchCompetitor2PlayerCount = 37;
        public static readonly int MatchSeasonCompetitorCount = 20;
        public static readonly int MatchTournamentCompetitorCount = 20;

        public static readonly CultureInfo CultureEn = new CultureInfo("en");
        public static readonly CultureInfo CultureDe = new CultureInfo("de");
        public static readonly CultureInfo CultureHu = new CultureInfo("hu");
        public static readonly CultureInfo CultureNl = new CultureInfo("nl");
        public static readonly IReadOnlyCollection<CultureInfo> Cultures1 = new Collection<CultureInfo> { CultureEn };
        public static readonly IReadOnlyCollection<CultureInfo> Cultures3 = new Collection<CultureInfo>(new[] { CultureEn, CultureDe, CultureHu });
        public static readonly IReadOnlyCollection<CultureInfo> Cultures4 = new Collection<CultureInfo>(new[] { CultureEn, CultureDe, CultureHu, CultureNl });

        public const int CacheSportCount = 136;
        public const int CacheCategoryCount = 391;
        public const int CacheCategoryCountPlus = 408;
        public const int CacheTournamentCount = 8455;

        public const int InvariantListCacheCount = 1080;
        public const int VariantListCacheCount = 110;

        public readonly TestSportEntityFactoryBuilder SportEntityFactoryBuilder;
        private readonly ITestOutputHelper _outputHelper;
        public readonly ExceptionHandlingStrategy ThrowingStrategy;

        public readonly IMatch Match;
        public readonly ITeamCompetitor MatchCompetitor1;
        public readonly ITeamCompetitor MatchCompetitor2;
        public readonly IPlayerProfile MatchCompetitor1Player1;
        public readonly IPlayerProfile MatchCompetitor1Player2;
        public readonly IPlayerProfile MatchCompetitor2Player1;
        public readonly IPlayerProfile MatchCompetitor2Player2;

        public ScheduleData(TestSportEntityFactoryBuilder sportEntityFactoryBuilder, ITestOutputHelper outputHelper, ExceptionHandlingStrategy throwingStrategy = ExceptionHandlingStrategy.THROW)
        {
            SportEntityFactoryBuilder = sportEntityFactoryBuilder;
            _outputHelper = outputHelper;
            ThrowingStrategy = throwingStrategy;

            Match = GetMatch();
            MatchCompetitor1 = Match.GetHomeCompetitorAsync().GetAwaiter().GetResult();
            MatchCompetitor2 = Match.GetAwayCompetitorAsync().GetAwaiter().GetResult();
            MatchCompetitor1Player1 = GetPlayerProfile(MatchCompetitor1PlayerId1, MatchCompetitorId1);
            MatchCompetitor1Player2 = GetPlayerProfile(MatchCompetitor1PlayerId2, MatchCompetitorId1);
            MatchCompetitor2Player1 = GetPlayerProfile(MatchCompetitor2PlayerId1, MatchCompetitorId2);
            MatchCompetitor2Player2 = GetPlayerProfile(MatchCompetitor2PlayerId2, MatchCompetitorId2);
        }

        public IMatch GetMatch()
        {
            var matchDtoEn = XmlParseSummaryAsync(MatchId, CultureEn).GetAwaiter().GetResult();
            var matchDtoDe = XmlParseSummaryAsync(MatchId, CultureDe).GetAwaiter().GetResult();
            var matchDtoHu = XmlParseSummaryAsync(MatchId, CultureHu).GetAwaiter().GetResult();
            var matchCi = new MatchCI(matchDtoEn, SportEntityFactoryBuilder.DataRouterManager, new SemaphorePool(500, ThrowingStrategy), CultureEn, CultureEn, new MemoryCache("fixture"));
            matchCi.Merge(matchDtoDe, CultureDe, false);
            matchCi.Merge(matchDtoHu, CultureHu, false);

            var homeCompetitor = GetTeamCompetitorFromCompetition(matchDtoEn.Competitors.First(),
                                                                  matchDtoDe.Competitors.First(),
                                                                  matchDtoHu.Competitors.First(),
                                                                  matchCi);
            var awayCompetitor = GetTeamCompetitorFromCompetition(matchDtoEn.Competitors.Skip(1).First(),
                                                                  matchDtoDe.Competitors.Skip(1).First(),
                                                                  matchDtoHu.Competitors.Skip(1).First(),
                                                                  matchCi);
            var coverageInfo = new CoverageInfo(matchCi.GetCoverageInfoAsync(Cultures3).GetAwaiter().GetResult());
            var delayedInfoCi = matchCi.GetDelayedInfoAsync(Cultures3).GetAwaiter().GetResult();
            var delayedInfo = delayedInfoCi == null ? null : new DelayedInfo(delayedInfoCi);

            var match = new Mock<IMatch>();
            match.Setup(m => m.Id).Returns(matchCi.Id);
            match.Setup(m => m.GetSportIdAsync()).ReturnsAsync(matchCi.GetSportIdAsync().GetAwaiter().GetResult);
            match.Setup(s => s.GetNameAsync(CultureEn)).ReturnsAsync(matchCi.GetNamesAsync(Cultures3).GetAwaiter().GetResult()[CultureEn]);
            match.Setup(s => s.GetNameAsync(CultureDe)).ReturnsAsync(matchCi.GetNamesAsync(Cultures3).GetAwaiter().GetResult()[CultureDe]);
            match.Setup(s => s.GetNameAsync(CultureHu)).ReturnsAsync(matchCi.GetNamesAsync(Cultures3).GetAwaiter().GetResult()[CultureHu]);
            match.Setup(m => m.GetHomeCompetitorAsync()).ReturnsAsync(homeCompetitor);
            match.Setup(m => m.GetAwayCompetitorAsync()).ReturnsAsync(awayCompetitor);
            match.Setup(m => m.GetCoverageInfoAsync()).ReturnsAsync(coverageInfo);
            match.Setup(m => m.GetDelayedInfoAsync()).ReturnsAsync(delayedInfo);
            match.Setup(m => m.GetCompetitorsAsync()).ReturnsAsync(new List<ICompetitor> { homeCompetitor, awayCompetitor });
            match.Setup(m => m.GetBookingStatusAsync()).ReturnsAsync(matchCi.GetBookingStatusAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetEventStatusAsync()).ReturnsAsync(EventStatus.Closed);
            match.Setup(m => m.GetConditionsAsync()).ReturnsAsync(new SportEventConditions(matchCi.GetConditionsAsync(Cultures3).GetAwaiter().GetResult(), Cultures3.ToList()));
            match.Setup(m => m.GetReplacedByAsync()).ReturnsAsync(matchCi.GetReplacedByAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetLiveOddsAsync()).ReturnsAsync(matchCi.GetLiveOddsAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetSportEventTypeAsync()).ReturnsAsync(matchCi.GetSportEventTypeAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetScheduledTimeAsync()).ReturnsAsync(matchCi.GetScheduledAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetScheduledEndTimeAsync()).ReturnsAsync(matchCi.GetScheduledEndAsync().GetAwaiter().GetResult);
            match.Setup(m => m.GetVenueAsync()).ReturnsAsync(new Venue(matchCi.GetVenueAsync(Cultures3).GetAwaiter().GetResult(), Cultures3));
            match.Setup(s => s.GetTournamentAsync())
                 .ReturnsAsync(new Tournament(matchCi.GetTournamentIdAsync(Cultures3).GetAwaiter().GetResult(),
                                              MatchSportId,
                                              SportEntityFactoryBuilder.SportEntityFactory,
                                              SportEntityFactoryBuilder.SportEventCache,
                                              SportEntityFactoryBuilder.SportDataCache,
                                              Cultures3,
                                              ThrowingStrategy));
            match.Setup(m => m.GetSeasonAsync()).ReturnsAsync(new SeasonInfo(matchCi.GetSeasonAsync(Cultures3).GetAwaiter().GetResult()));
            return match.Object;
        }

        public ICompetitor GetCompetitor(URN competitorId)
        {
            var competitorProfileDtoEn = XmlParseCompetitorAsync(competitorId, CultureEn).GetAwaiter().GetResult();
            var competitorProfileDtoDe = XmlParseCompetitorAsync(competitorId, CultureDe).GetAwaiter().GetResult();
            var competitorProfileDtoHu = XmlParseCompetitorAsync(competitorId, CultureHu).GetAwaiter().GetResult();
            var competitorProfileCi = new CompetitorCI(competitorProfileDtoEn, CultureEn, SportEntityFactoryBuilder.DataRouterManager);
            competitorProfileCi.Merge(competitorProfileDtoDe, CultureDe);
            competitorProfileCi.Merge(competitorProfileDtoHu, CultureHu);

            var players = new List<IPlayer>();
            for (var i = 0; i < competitorProfileDtoEn.Players.Count(); i++)
            {
                var player = GetPlayerFromCompetitorProfile(competitorProfileDtoEn.Players.ToList()[i],
                                                            competitorProfileDtoDe.Players.ToList()[i],
                                                            competitorProfileDtoHu.Players.ToList()[i],
                                                            competitorProfileCi.Id);
                players.Add(player);
            }

            var competitor = new Mock<ICompetitor>();
            competitor.Setup(s => s.Id).Returns(competitorProfileCi.Id);
            competitor.Setup(s => s.Names).Returns(competitorProfileCi.Names.ToDictionary(c => c.Key, c => competitorProfileCi.GetName(c.Key)));
            competitor.Setup(s => s.CountryCode).Returns(competitorProfileCi.CountryCode);
            competitor.Setup(s => s.Abbreviations).Returns(Cultures3.ToDictionary(c => c, c => competitorProfileCi.GetAbbreviation(c)));
            competitor.Setup(s => s.AgeGroup).Returns(competitorProfileCi.AgeGroup);
            competitor.Setup(s => s.AssociatedPlayers).Returns(players);
            competitor.Setup(s => s.Countries).Returns(Cultures3.ToDictionary(c => c, c => competitorProfileCi.GetCountry(c)));
            competitor.Setup(s => s.Gender).Returns(competitorProfileCi.Gender);
            competitor.Setup(s => s.IsVirtual).Returns(competitorProfileCi.IsVirtual);
            competitor.Setup(s => s.Jerseys).Returns(competitorProfileCi.Jerseys.Select(j => new Jersey(j)));
            competitor.Setup(s => s.Manager).Returns(new Manager(competitorProfileCi.Manager));
            competitor.Setup(s => s.RaceDriverProfile).Returns(competitorProfileCi.RaceDriverProfile == null ? null : new RaceDriverProfile(competitorProfileCi.RaceDriverProfile));
            competitor.Setup(s => s.References).Returns(new Reference(competitorProfileCi.ReferenceId));
            competitor.Setup(s => s.ShortName).Returns(competitorProfileCi.ShortName);
            competitor.Setup(s => s.State).Returns(competitorProfileCi.State);
            competitor.Setup(s => s.Venue).Returns(new Venue(competitorProfileCi.Venue, Cultures3));
            return competitor.Object;
        }

        private ITeamCompetitor GetTeamCompetitorFromCompetition(TeamCompetitorDTO teamCompetitorEn, TeamCompetitorDTO teamCompetitorDe, TeamCompetitorDTO teamCompetitorHu, ICompetitionCI rootCompetitionCI)
        {
            var teamCompetitorCI = new TeamCompetitorCI(teamCompetitorEn, CultureEn, SportEntityFactoryBuilder.DataRouterManager);
            teamCompetitorCI.Merge(teamCompetitorDe, CultureDe);
            teamCompetitorCI.Merge(teamCompetitorHu, CultureHu);
            return new TeamCompetitor(teamCompetitorCI, Cultures3, SportEntityFactoryBuilder.SportEntityFactory, ThrowingStrategy, SportEntityFactoryBuilder.ProfileCache, rootCompetitionCI);
        }

        private IPlayer GetPlayerFromCompetitorProfile(PlayerProfileDTO playerProfileEn, PlayerProfileDTO playerProfileDe, PlayerProfileDTO playerProfileHu, URN competitorId)
        {
            var playerProfileCi = new PlayerProfileCI(playerProfileEn, competitorId, CultureEn, SportEntityFactoryBuilder.DataRouterManager);
            playerProfileCi.Merge(playerProfileDe, competitorId, CultureDe);
            playerProfileCi.Merge(playerProfileHu, competitorId, CultureHu);
            return new PlayerProfile(playerProfileCi, Cultures3);
        }

        public IPlayerProfile GetPlayerProfile(URN playerId, URN competitorId)
        {
            var playerProfileDtoEn = XmlParsePlayerAsync(playerId, CultureEn).GetAwaiter().GetResult();
            var playerProfileDtoDe = XmlParsePlayerAsync(playerId, CultureDe).GetAwaiter().GetResult();
            var playerProfileDtoHu = XmlParsePlayerAsync(playerId, CultureHu).GetAwaiter().GetResult();
            var playerProfileCi = new PlayerProfileCI(playerProfileDtoEn, competitorId, CultureEn, SportEntityFactoryBuilder.DataRouterManager);
            playerProfileCi.Merge(playerProfileDtoDe, competitorId, CultureDe);
            playerProfileCi.Merge(playerProfileDtoHu, competitorId, CultureHu);

            var playerProfile = new PlayerProfile(playerProfileCi, Cultures3);
            return playerProfile;
        }

        private static async Task<MatchDTO> XmlParseSummaryAsync(URN id, CultureInfo culture)
        {
            var fileId = id.ToString().Replace(":", "_");
            var resourceName = $"summary_{fileId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {

                var restDeserializer = new Deserializer<RestMessage>();
                var mapper = new SportEventSummaryMapperFactory();
                var result = (MatchDTO)mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                return result;
            }

            return null;
        }

        private static async Task<FixtureDTO> XmlParseFixtureAsync(URN id, CultureInfo culture)
        {
            var fileId = id.ToString().Replace(":", "_");
            var resourceName = $"fixture_{fileId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                var restDeserializer = new Deserializer<fixturesEndpoint>();
                var mapper = new FixtureMapperFactory();
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                return result;
            }

            return null;
        }

        private static async Task<CompetitorProfileDTO> XmlParseCompetitorAsync(URN id, CultureInfo culture)
        {
            var fileId = id.Id.ToString();
            var resourceName = $"competitor_{fileId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                var restDeserializer = new Deserializer<competitorProfileEndpoint>();
                var mapper = new CompetitorProfileMapperFactory();
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                return result;
            }

            return null;
        }

        private static async Task<PlayerProfileDTO> XmlParsePlayerAsync(URN id, CultureInfo culture)
        {
            var fileId = id.Id.ToString();
            var resourceName = $"player_{fileId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                var restDeserializer = new Deserializer<playerProfileEndpoint>();
                var mapper = new PlayerProfileMapperFactory();
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                return result;
            }

            return null;
        }
    }
}
