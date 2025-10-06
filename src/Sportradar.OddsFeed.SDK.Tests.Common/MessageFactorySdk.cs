// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit.Abstractions;
using MFR = Sportradar.OddsFeed.SDK.Tests.Common.MessageFactoryRest;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

/// <summary>
/// Class used to manually create SDK exposed entities
/// </summary>
public static class MessageFactorySdk
{
    private static ITestOutputHelper OutputHelper;
    private static TestSportEntityFactoryBuilder SportEntityFactoryBuilder;
    private static TestCacheStoreManager TestCacheStoreManager;

    public static void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        SportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(OutputHelper, ScheduleData.Cultures3.ToList());
        TestCacheStoreManager = new TestCacheStoreManager();
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
        return new Assist(SR.Urn(id == 0 ? SR.I1000 : id),
                          new Dictionary<CultureInfo, string> { { new CultureInfo("en"), "Assist name" } },
                          "assist type");
    }

    public static IBookmakerDetails GetBookmakerDetails(int id = 0, bool idSpecified = true, bool expireAtSpecified = true, bool responseCodeSpecified = true)
    {
        return new BookmakerDetails(new BookmakerDetailsDto(MFR.GetBookmakerDetails(id, idSpecified, expireAtSpecified, responseCodeSpecified), TimeSpan.Zero));
    }

    public static ICategory GetCategory(int id = 0)
    {
        return new Category(SR.Urn(id == 0 ? SR.I1000 : id, "category"), GetNames(null), "SR", GetTournamentListStatic(SR.I(15)));
    }

    public static ICompetitor GetCompetitor(int id = 0, int playerCount = 0)
    {
        return new Competitor(new CompetitorCacheItem(new CompetitorDto(MFR.GetTeam(id)), ScheduleData.CultureEn, SportEntityFactoryBuilder.DataRouterManager),
                              SportEntityFactoryBuilder.ProfileCache,
                              ScheduleData.Cultures3.ToList(),
                              SportEntityFactoryBuilder.SportEntityFactory,
                              ExceptionHandlingStrategy.Throw,
                              (ICompetitionCacheItem)null);
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
        return new Fixture(new FixtureDto(MFR.GetFixture(id, subItemCount), null));
    }

    public static IGroup GetGroup()
    {
        return new Group(new GroupCacheItem(new GroupDto(MFR.GetGroup()), TestData.Culture), TestData.Cultures3, SportEntityFactoryBuilder.SportEntityFactory, ExceptionHandlingStrategy.Throw, null);
    }

    public static IGroup GetGroupWithCompetitors()
    {
        return new Group(new GroupCacheItem(new GroupDto(MFR.GetTournamentGroup()), TestData.Culture), TestData.Cultures3, SportEntityFactoryBuilder.SportEntityFactory, ExceptionHandlingStrategy.Throw, null);
    }

    public static IPeriodScore GetPeriodScore()
    {
        return new PeriodScore(new PeriodScoreDto(new periodScore
        {
            home_score = SR.S100,
            away_score = SR.S100,
            type = "regular_period",
            numberSpecified = true,
            number = 1
        }),
                               null);
    }

    public static IPlayer GetPlayer(int id = 0)
    {
        return new Player(id == 0
                              ? SR.Urn("", 10000)
                              : SR.Urn(id),
                          new Dictionary<CultureInfo, string> { { new CultureInfo("en"), "Player " + SR.I1000 } });
    }

    internal static IPlayerProfile GetPlayerProfile(int id = 0)
    {
        return new PlayerProfile(new PlayerProfileCacheItem(new PlayerProfileDto(MFR.GetPlayerExtended(id), null), null, TestData.Culture, new TestDataRouterManager(new CacheManager(), OutputHelper)), TestData.Cultures3.ToList());
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
        return new ProductInfo(new ProductInfoDto(new productInfo()
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
        return new Referee(new RefereeCacheItem(new RefereeDto(MFR.GetReferee(id)), new CultureInfo("en")),
                           new[] { new CultureInfo("en") });
    }

    public static IRound GetRoundSummary(int id = 0)
    {
        return new Round(new RoundCacheItem(new RoundDto(MFR.GetMatchRound(id)), TestData.Culture), TestData.Cultures3);
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
        return new SportEventConditions(new SportEventConditionsCacheItem(new SportEventConditionsDto(MFR.GetSportEventConditions()), new CultureInfo("en")),
                                        new[] { new CultureInfo("en") });
    }

    internal static IStreamingChannel GetStreamingChannel(int id = 0)
    {
        var finalId = id == 0 ? SR.I1000 : id;
        return new StreamingChannel(finalId, "Name " + finalId);
    }

    public static ITeamCompetitor GetTeamCompetitor(int id = 0)
    {
        return new TeamCompetitor(new TeamCompetitorCacheItem(new TeamCompetitorDto(MFR.GetTeamCompetitor(id)),
                                                              ScheduleData.CultureEn,
                                                              SportEntityFactoryBuilder.DataRouterManager),
                                  ScheduleData.Cultures3.ToList(),
                                  SportEntityFactoryBuilder.SportEntityFactory,
                                  ExceptionHandlingStrategy.Throw,
                                  SportEntityFactoryBuilder.ProfileCache,
                                  null);
    }

    public static ITournament GetTournament(int id = 0)
    {
        var sef = new TestSportEntityFactoryBuilder(OutputHelper, ScheduleData.Cultures3);
        sef.InitializeSportEntities().GetAwaiter().GetResult();
        sef.LoadTournamentMissingValues().GetAwaiter().GetResult();
        return (Tournament)sef.GetNewTournament();
    }

    internal static ITvChannel GetTvChannel(bool startTimeSpecified = true)
    {
        return new TvChannel("Name " + SR.S1000, startTimeSpecified ? DateTime.Now : null, "Url:" + SR.S10000P);
    }

    public static IVenue GetVenue(int id = 0)
    {
        return new Venue(new VenueCacheItem(new VenueDto(MFR.GetVenue(id)), TestData.Culture), TestData.Cultures3.ToList());
    }

    internal static IWeatherInfo GetWeatherInfo()
    {
        return new WeatherInfo(new WeatherInfoCacheItem(new WeatherInfoDto(MFR.GetWeatherInfo())));
    }

    public static List<ITournament> GetTournamentList(int count)
    {
        var sef = new TestSportEntityFactoryBuilder(OutputHelper, ScheduleData.Cultures3);
        sef.InitializeSportEntities().GetAwaiter().GetResult();
        sef.LoadTournamentMissingValues().GetAwaiter().GetResult();

        var tours = new List<ITournament>();
        for (var i = 0; i < count; i++)
        {
            tours.Add(sef.GetNewTournament());
        }
        return tours;
    }

    public static List<ITournament> GetTournamentListStatic(int count)
    {
        var semaphorePool = new SemaphorePool(20, ExceptionHandlingStrategy.Throw);

        var tours = new List<ITournament>();
        for (var i = 0; i < count; i++)
        {
            var tid = StaticRandom.I();
            var tour = MessageFactoryRest.GetTournamentInfoEndpoint(tid);
            if (tour != null)
            {
                var tourDto = new TournamentInfoDto(tour);
                var tourCacheItem = new TournamentInfoCacheItem(tourDto,
                                                                SportEntityFactoryBuilder.DataRouterManager,
                                                                semaphorePool,
                                                                ScheduleData.CultureEn,
                                                                ScheduleData.CultureEn,
                                                                TestCacheStoreManager.ServiceProvider.GetSdkCacheStore<string>(UofSdkBootstrap.CacheStoreNameForSportEventCacheFixtureTimestampCache));
                var tourEntity = new Tournament(tourCacheItem.Id,
                                                tourCacheItem.GetSportIdAsync().GetAwaiter().GetResult(),
                                                SportEntityFactoryBuilder.SportEntityFactory,
                                                SportEntityFactoryBuilder.SportEventCache,
                                                SportEntityFactoryBuilder.SportDataCache,
                                                TestData.Cultures.ToList(),
                                                ExceptionHandlingStrategy.Throw);
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
