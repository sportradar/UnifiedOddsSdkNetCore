using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Xunit.Abstractions;
using MFR = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    /// <summary>
    /// Class used to manually create SDK exposed entities
    /// </summary>
    public static class MessageFactorySdk
    {
        private static ITestOutputHelper OutputHelper;

        public static void SetOutputHelper(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private static Dictionary<CultureInfo, string> GetNames(List<CultureInfo> cultures)
        {
            if (cultures == null || !cultures.Any())
            {
                cultures = TestData.Cultures3.ToList();
            }
            return cultures.ToDictionary(c => c, c => $"{c.TwoLetterISOLanguageName} name " + SR.S1000);
        }

        public static IAssist GetAssist(int id = 0)
        {
            return new Assist(
                SR.Urn(id == 0 ? SR.I1000 : id),
                new Dictionary<CultureInfo, string> { { new CultureInfo("en"), "Assist name" } }, "assist type");
        }

        public static IBookmakerDetails GetBookmakerDetails(int id = 0, bool idSpecified = true, bool expireAtSpecified = true, bool responseCodeSpecified = true)
        {
            return new BookmakerDetails(new BookmakerDetailsDTO(MFR.GetBookmakerDetails(id, idSpecified, expireAtSpecified, responseCodeSpecified), TimeSpan.Zero));
        }

        public static ICategory GetCategory(int id = 0)
        {
            return new Category(SR.Urn(id == 0 ? SR.I1000 : id, "category"), GetNames(null), "SR", GetTournamentListStatic(SR.I(15)));
        }

        public static ICompetitor GetCompetitor(int id = 0, int playerCount = 0)
        {
            return new Competitor(new CompetitorCI(new CompetitorDTO(MFR.GetTeam(id)), TestData.Culture, null), null, TestData.Cultures3.ToList(), new TestSportEntityFactory(OutputHelper), ExceptionHandlingStrategy.THROW, (ICompetitionCI)null);
        }

        internal static ICoverageInfo GetCoverageInfo(int subItemCount = 0)
        {
            if (subItemCount < 1)
            {
                subItemCount = SR.I(20);
            }
            var items = new List<string>();
            for (var j = 0; j < subItemCount; j++)
            {
                items.Add(SR.S1000);
            }
            return new CoverageInfo(SR.S100, true, new List<string>(items), CoveredFrom.Tv);
        }

        internal static IEventClock GetEventClock()
        {
            return new EventClock(DateTime.Now.ToString(TestData.Culture), DateTime.Now.AddDays(1).ToString(TestData.Culture), DateTime.Today.ToString(TestData.Culture), DateTime.Today.ToString(TestData.Culture), "10", false);
        }

        public static IFixture GetFixture(int id = 0, int subItemCount = 0)
        {
            return new Fixture(new FixtureDTO(MFR.GetFixture(id, subItemCount), null));
        }

        public static IGroup GetGroup()
        {
            return new Group(new GroupCI(new GroupDTO(MFR.GetGroup()), TestData.Culture), TestData.Cultures3, new TestSportEntityFactory(OutputHelper), ExceptionHandlingStrategy.THROW, null);
        }

        public static IGroup GetGroupWithCompetitors()
        {
            return new Group(new GroupCI(new GroupDTO(MFR.GetTournamentGroup()), TestData.Culture), TestData.Cultures3, new TestSportEntityFactory(OutputHelper), ExceptionHandlingStrategy.THROW, null);
        }

        public static IPeriodScore GetPeriodScore()
        {
            return new PeriodScore(new PeriodScoreDTO(new periodScore
            {
                home_score = SR.S100,
                away_score = SR.S100,
                type = "regular_period",
                numberSpecified = true,
                number = 1
            }), null);
        }

        public static IPlayer GetPlayer(int id = 0)
        {
            return new Player(
                id == 0
                    ? SR.Urn("", 10000)
                    : SR.Urn(id),
                new Dictionary<CultureInfo, string> { { new CultureInfo("en"), "Player " + SR.I1000 } });
        }

        internal static IPlayerProfile GetPlayerProfile(int id = 0)
        {
            return new PlayerProfile(new PlayerProfileCI(new PlayerProfileDTO(MFR.GetPlayerExtended(id), null), null, TestData.Culture, new TestDataRouterManager(new CacheManager(), OutputHelper)), TestData.Cultures3.ToList());
        }

        internal static IProductInfo GetProductInfo(int subItemCount = 0)
        {
            if (subItemCount < 1)
            {
                subItemCount = SR.I(20);
            }
            var pil = new List<productInfoLink>();
            var sc = new List<streamingChannel>();
            while (subItemCount > 0)
            {
                subItemCount--;
                pil.Add(GetProductInfoLinkApi());
                sc.Add(GetStreamingChannelApi());
            }
            return new ProductInfo(new ProductInfoDTO(new productInfo()
            {
                is_auto_traded = new productInfoItem(),
                is_in_hosted_statistics = new productInfoItem(),
                is_in_live_center_soccer = new productInfoItem(),
                is_in_live_match_tracker = new productInfoItem(),
                is_in_live_score = new productInfoItem(),
                links = pil.ToArray(),
                streaming = sc.ToArray()
            }));
        }

        internal static productInfoLink GetProductInfoLinkApi()
        {
            return new productInfoLink { @ref = "Ref " + SR.S1000, name = "Name " + SR.S1000 };
        }

        internal static streamingChannel GetStreamingChannelApi(int id = 0)
        {
            return new streamingChannel { id = id == 0 ? SR.I1000 : id, name = "Name " + SR.S1000 };
        }

        internal static IProductInfoLink GetProductInfoLink()
        {
            return new ProductInfoLink("Ref " + SR.S1000, "Name " + SR.S1000);
        }

        internal static IReferee GetReferee(int id = 0)
        {
            return new Referee(
                new RefereeCI(new RefereeDTO(MFR.GetReferee(id)), new CultureInfo("en")),
                new[] { new CultureInfo("en") });
        }

        public static IRound GetRoundSummary(int id = 0)
        {
            return new Round(new RoundCI(new RoundDTO(MFR.GetMatchRound(id)), TestData.Culture), TestData.Cultures3);
        }

        public static ISport GetSport(int id = 0, int subItemCount = 0)
        {
            if (subItemCount < 1)
            {
                subItemCount = SR.I(20);
            }
            var c = new List<ICategory>();
            while (subItemCount > 0)
            {
                subItemCount--;
                c.Add(GetCategory());
            }
            return new Sport(SR.Urn(id == 0 ? SR.I1000 : id, "sport"), GetNames(null), c);
        }

        public static ISportEventConditions GetSportEventConditions()
        {
            return new SportEventConditions(
                new SportEventConditionsCI(new SportEventConditionsDTO(MFR.GetSportEventConditions()), new CultureInfo("en")),
                new[] { new CultureInfo("en") });
        }

        internal static IStreamingChannel GetStreamingChannel(int id = 0)
        {
            var finalId = id == 0 ? SR.I1000 : id;
            return new StreamingChannel(finalId, "Name " + finalId);
        }

        public static ITeamCompetitor GetTeamCompetitor(int id = 0)
        {
            return new TeamCompetitor(new TeamCompetitorCI(new TeamCompetitorDTO(MFR.GetTeamCompetitor(id)),
                                                           TestData.Culture,
                                                           new TestDataRouterManager(new CacheManager(), OutputHelper)),
                                      TestData.Cultures3.ToList(),
                                      new TestSportEntityFactory(OutputHelper),
                                      ExceptionHandlingStrategy.THROW,
                                      null,
                                      null);
        }

        public static ITournament GetTournament(int id = 0)
        {
            var sef = new TestSportEntityFactoryBuilder(OutputHelper);
            sef.InitializeSportEntities().Wait();
            sef.LoadTournamentMissingValues().Wait();
            return (Tournament)sef.GetNewTournament();
        }

        internal static ITvChannel GetTvChannel(bool startTimeSpecified = true)
        {
            return new TvChannel("Name " + SR.S1000, startTimeSpecified ? DateTime.Now : (DateTime?)null, "Url:" + SR.S10000P);
        }

        public static IVenue GetVenue(int id = 0)
        {
            return new Venue(new VenueCI(new VenueDTO(MFR.GetVenue(id)), TestData.Culture), TestData.Cultures3.ToList());
        }

        internal static IWeatherInfo GetWeatherInfo()
        {
            return new WeatherInfo(new WeatherInfoCI(new WeatherInfoDTO(MFR.GetWeatherInfo())));
        }

        public static List<ITournament> GetTournamentList(int count)
        {
            var sef = new TestSportEntityFactoryBuilder(OutputHelper);
            sef.InitializeSportEntities().Wait();
            sef.LoadTournamentMissingValues().Wait();

            var tours = new List<ITournament>();
            for (var i = 0; i < count; i++)
            {
                tours.Add(sef.GetNewTournament());
            }
            return tours;
        }

        public static List<ITournament> GetTournamentListStatic(int count)
        {
            var timer = new TestTimer(false);
            var cacheManager = new CacheManager();
            var dataRouterManager = new TestDataRouterManager(cacheManager, OutputHelper);
            var semaphorePool = new SemaphorePool(20, ExceptionHandlingStrategy.THROW);
            var fixtureMemoryCache = new MemoryCache("fixtureMemoryCache");
            var eventMemoryCache = new MemoryCache("EventCache");
            var profilesCache = new MemoryCache("ProfileCache");
            var statusMemoryCache = new MemoryCache("StatusCache");
            var ignoreTimelineMemoryCache = new MemoryCache("IgnoreTimeline");
            var sportEventCacheItemFactory = new SportEventCacheItemFactory(
                                                                            dataRouterManager,
                                                                            new SemaphorePool(5, ExceptionHandlingStrategy.THROW),
                                                                            TestData.Culture,
                                                                            new MemoryCache("FixtureTimestampCache"));

            var profileCache = new ProfileCache(profilesCache, dataRouterManager, cacheManager);

            var sportEventCache = new SportEventCache(eventMemoryCache, dataRouterManager, sportEventCacheItemFactory, timer, TestData.Cultures3, cacheManager);

            var sportDataCache = new SportDataCache(dataRouterManager, timer, TestData.Cultures3, sportEventCache, cacheManager);

            var sportEventStatusCache = TestLocalizedNamedValueCache.CreateMatchStatusCache(TestData.Cultures3, ExceptionHandlingStrategy.THROW);
            var namedValuesProviderMock = new Mock<INamedValuesProvider>();
            namedValuesProviderMock.Setup(args => args.MatchStatuses).Returns(sportEventStatusCache);

            var eventStatusCache = new SportEventStatusCache(statusMemoryCache, new SportEventStatusMapperFactory(), sportEventCache, cacheManager, ignoreTimelineMemoryCache);

            var sportEntityFactory = new SportEntityFactory(sportDataCache, sportEventCache, eventStatusCache, sportEventStatusCache, profileCache, SdkInfo.SoccerSportUrns);

            var tours = new List<ITournament>();
            for (var i = 0; i < count; i++)
            {
                var tid = StaticRandom.I();
                var tour = MessageFactoryRest.GetTournamentInfoEndpoint(tid);
                if (tour != null)
                {
                    var tourDto = new TournamentInfoDTO(tour);
                    var tourCI = new TournamentInfoCI(tourDto, dataRouterManager, semaphorePool, TestData.Culture, TestData.Culture, fixtureMemoryCache);
                    var tourEntity = new Tournament(tourCI.Id,
                                                    tourCI.GetSportIdAsync().Result,
                                                    sportEntityFactory,
                                                    sportEventCache,
                                                    sportDataCache,
                                                    TestData.Cultures.ToList(),
                                                    ExceptionHandlingStrategy.THROW);
                    tours.Add(tourEntity);
                }
            }
            return tours;
        }

        public static IAvailableSelections GetAvailableSelections(int eventId = 0, int nbrMarkets = 10)
        {
            var matchId = SR.Urn(eventId == 0 ? SR.I1000 : eventId, "match");
            return new AvailableSelections(new AvailableSelectionsDto(MFR.GetAvailableSelections(matchId, nbrMarkets)));
        }

        public static ICalculation GetCalculation(int eventId = 0, int nbrSelections = 5)
        {
            var matchId = SR.Urn(eventId == 0 ? SR.I1000 : eventId, "match");
            return new Calculation(new CalculationDto(MFR.GetCalculationResponse(matchId, nbrSelections)));
        }

        public static ICalculationFilter GetCalculationFilter(int eventId = 0, int nbrSelections = 5)
        {
            var matchId = SR.Urn(eventId == 0 ? SR.I1000 : eventId, "match");
            return new CalculationFilter(new FilteredCalculationDto(MFR.GetFilteredCalculationResponse(matchId, nbrSelections)));
        }

        public static IAvailableSelections GetAvailableSelections(AvailableSelectionsType availableSelections)
        {
            return new AvailableSelections(new AvailableSelectionsDto(availableSelections));
        }

        public static ICalculation GetCalculation(CalculationResponseType calculationResponse)
        {
            return new Calculation(new CalculationDto(calculationResponse));
        }

        public static ICalculationFilter GetCalculationFilter(FilteredCalculationResponseType calculationResponse)
        {
            return new CalculationFilter(new FilteredCalculationDto(calculationResponse));
        }
    }
}
