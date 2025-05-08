// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;

public static class IceHockey
{
    public static matchSummaryEndpoint Summary()
    {
        var match = GetSportEvent().Build();
        return new Endpoints.SummaryEndpoint()
              .WithSportEvent(_ => GetSportEvent())
              .WithSportEventConditions(_ => new SportEventConditionsBuilder().WithVenue(match.venue))
              .WithRestSportEventStatus(GetSportEventStatus())
              .WithCoverageInfo(_ => GetCoverageInfoBuilder())
              .BuildMatchSummary();
    }

    public static fixturesEndpoint Fixture()
    {
        var fixture = new FixtureBuilder()
                     .WithSportEvent(_ => GetSportEvent())
                     .WithStartTime(DateTime.Parse("2025-04-08T17:30:00+00:00"))
                     .AddExtraInfo("RTS", "not_available")
                     .AddExtraInfo("coverage_source", "venue")
                     .AddExtraInfo("extended_live_markets_offered", "false")
                     .AddExtraInfo("streaming", "false")
                     .AddExtraInfo("auto_traded", "false")
                     .AddExtraInfo("neutral_ground", "false")
                     .AddExtraInfo("period_length", "20")
                     .AddExtraInfo("overtime_length", "20")
                     .WithCoverageInfo(_ => GetCoverageInfoBuilder())
                     .WithProductInfo(_ => GetProductInfoBuilder())
                     .WithReferences(b => b.WithBetradarCtrl(144965313))
                     .AddScheduledStartTimeChange(DateTime.Parse("2025-04-01T13:00:00+00:00"), DateTime.Parse("2025-04-08T17:30:00+00:00"), DateTime.Parse("2025-03-08T05:31:36+00:00"))
                     .AddScheduledStartTimeChange(DateTime.Parse("2025-03-16T13:00:00+00:00"), DateTime.Parse("2025-04-01T13:00:00+00:00"), DateTime.Parse("2025-03-07T23:30:59+00:00"))
                     .Build();
        return fixture;
    }

    public static SportEventBuilder GetSportEvent()
    {
        return new SportEventBuilder()
              .WithId(UrnCreate.MatchId(58717423))
              .WithEventStatus("not_started")
              .WithLiveOdds("booked")
              .WithScheduledTime(DateTime.Parse("2025-04-08T17:30:00+00:00"))
              .WithStartTimeTbd(false)
              .WithNextLiveTime(DateTime.Parse("2025-04-08T17:30:00+00:00"))
              .WithTournamentRound(GetMatchRoundBuilder)
              .WithSeason(GetSeasonBuilder)
              .WithTournament(GetTournamentBuilder)
              .AddTeamCompetitor(GetHomeCompetitorBuilder)
              .AddTeamCompetitor(GetAwayCompetitorBuilder)
              .WithVenue(_ => GetVenueBuilder());
    }

    private static MatchRoundBuilder GetMatchRoundBuilder()
    {
        return new MatchRoundBuilder()
              .WithType("cup")
              .WithName("Semifinal")
              .WithCupRoundMatches(7)
              .WithCupRoundMatchNumber(4)
              .WithBetradarId(3048)
              .WithBetradarName("DEL, Playoffs");
    }

    private static SeasonBuilder GetSeasonBuilder()
    {
        return new SeasonBuilder()
              .WithId(UrnCreate.SeasonId(118991))
              .WithName("DEL 24/25")
              .WithTournamentId(UrnCreate.TournamentId(225))
              .WithStartDate(new DateOnly(2024, 9, 19))
              .WithEndDate(new DateOnly(2025, 4, 29))
              .WithYear("24/25");
    }

    private static TournamentBuilder GetTournamentBuilder()
    {
        return new TournamentBuilder()
              .WithId(UrnCreate.TournamentId(225))
              .WithName("DEL")
              .WithSport(UrnCreate.SportId(4), "Ice Hockey")
              .WithCategory(UrnCreate.CategoryId(41), "Germany", "DEU");
    }

    private static CompetitorBuilder GetHomeCompetitorBuilder()
    {
        return new CompetitorBuilder()
              .WithId(UrnCreate.CompetitorId(3853))
              .WithName("Adler Mannheim")
              .WithAbbreviation("MAN")
              .WithShortName("Mannheim")
              .WithCountry("Germany")
              .WithCountryCode("DEU")
              .IsMale(true)
              .IsHome(true)
              .WithReferences(new ReferencesBuilder().WithBetradar(8882));
    }

    private static CompetitorBuilder GetAwayCompetitorBuilder()
    {
        return new CompetitorBuilder()
              .WithId(UrnCreate.CompetitorId(3856))
              .WithName("Eisbaren Berlin")
              .WithAbbreviation("BER")
              .WithShortName("Berlin")
              .WithCountry("Germany")
              .WithCountryCode("DEU")
              .IsMale(true)
              .IsHome(false)
              .WithReferences(new ReferencesBuilder().WithBetradar(7362));
    }

    private static VenueBuilder GetVenueBuilder()
    {
        return new VenueBuilder()
              .WithId(UrnCreate.VenueId(2010))
              .WithName("SAP Arena")
              .WithCapacity(13200)
              .WithCityName("Mannheim")
              .WithCountryName("Germany")
              .WithCountryCode("DEU")
              .WithMapCoordinates(49.46373, 8.51757);
    }

    private static CoverageInfoBuilder GetCoverageInfoBuilder()
    {
        return new CoverageInfoBuilder()
              .WithLevel("silver")
              .WithLiveCoverage(true)
              .WithCoveredFrom("venue")
              .AddCoverage("basic_score")
              .AddCoverage("key_events");
    }

    private static ProductInfoBuilder GetProductInfoBuilder()
    {
        return new ProductInfoBuilder()
              .IsInLiveScore()
              .IsInHostedStatistics()
              .IsInLiveMatchTracker()
              .AddLink("live_match_tracker", "https://widgets.sir.sportradar.com/sportradar/en/standalone/match.lmtPlus#matchId=58717423");
    }

    private static restSportEventStatus GetSportEventStatus()
    {
        return SportEventStatusBuilder.Rest()
                                      .WithStatusCode(0)
                                      .WithStatus("not_started")
                                      .WithMatchStatusCode(0)
                                      .Build();
    }

    public static sportCategoriesEndpoint GetSportCategoriesEndpoint()
    {
        var builder = new SportCategoriesEndpoint()
                     .ForSport(UrnCreate.SportId(4), "Ice Hockey")
                     .AddCategory(UrnCreate.CategoryId(37), "USA", "USA")
                     .AddCategory(UrnCreate.CategoryId(38), "Norway", "NOR")
                     .AddCategory(UrnCreate.CategoryId(39), "Sweden", "SWE")
                     .AddCategory(UrnCreate.CategoryId(40), "Finland", "FIN")
                     .AddCategory(UrnCreate.CategoryId(41), "Germany", "DEU")
                     .AddCategory(UrnCreate.CategoryId(42), "Czech Republic", "CZE")
                     .AddCategory(UrnCreate.CategoryId(54), "Switzerland", "CHE")
                     .AddCategory(UrnCreate.CategoryId(56), "International", null)
                     .AddCategory(UrnCreate.CategoryId(64), "Denmark", "DNK")
                     .AddCategory(UrnCreate.CategoryId(65), "Austria", "AUT")
                     .AddCategory(UrnCreate.CategoryId(98), "Slovakia", "SVK")
                     .AddCategory(UrnCreate.CategoryId(101), "Russia", "RUS")
                     .AddCategory(UrnCreate.CategoryId(115), "Poland", "POL")
                     .AddCategory(UrnCreate.CategoryId(127), "Italy", "ITA")
                     .AddCategory(UrnCreate.CategoryId(176), "Canada", "CAN")
                     .AddCategory(UrnCreate.CategoryId(242), "France", "FRA")
                     .AddCategory(UrnCreate.CategoryId(251), "Slovenia", "SVN")
                     .AddCategory(UrnCreate.CategoryId(260), "North America", null)
                     .AddCategory(UrnCreate.CategoryId(272), "Netherlands", "NLD")
                     .AddCategory(UrnCreate.CategoryId(275), "Belarus", "BLR")
                     .AddCategory(UrnCreate.CategoryId(306), "Latvia", "LVA")
                     .AddCategory(UrnCreate.CategoryId(307), "England", "ENG")
                     .AddCategory(UrnCreate.CategoryId(364), "Belgium", "BEL")
                     .AddCategory(UrnCreate.CategoryId(481), "Ireland", "IRL")
                     .AddCategory(UrnCreate.CategoryId(487), "Romania", "ROU")
                     .AddCategory(UrnCreate.CategoryId(495), "Spain", "ESP")
                     .AddCategory(UrnCreate.CategoryId(503), "Hungary", "HUN")
                     .AddCategory(UrnCreate.CategoryId(552), "Iceland", "ISL")
                     .AddCategory(UrnCreate.CategoryId(761), "Ukraine", "UKR")
                     .AddCategory(UrnCreate.CategoryId(809), "Bulgaria", "BGR")
                     .AddCategory(UrnCreate.CategoryId(810), "Kazakhstan", "KAZ")
                     .AddCategory(UrnCreate.CategoryId(820), "Estonia", "EST")
                     .AddCategory(UrnCreate.CategoryId(826), "Croatia", "HRV")
                     .AddCategory(UrnCreate.CategoryId(836), "Lithuania", "LTU")
                     .AddCategory(UrnCreate.CategoryId(861), "Scotland", "SCO")
                     .AddCategory(UrnCreate.CategoryId(878), "Australia", "AUS")
                     .AddCategory(UrnCreate.CategoryId(1127), "Turkiye", "TUR")
                     .AddCategory(UrnCreate.CategoryId(1513), "New Zealand", "NZL")
                     .AddCategory(UrnCreate.CategoryId(1998), "United Arab Emirates", "ARE")
                     .AddCategory(UrnCreate.CategoryId(2133), "Electronic Leagues", null)
                     .AddCategory(UrnCreate.CategoryId(2144), "Electronic Leagues CPU vs CPU", null)
                     .AddCategory(UrnCreate.CategoryId(2258), "Qatar", "QAT")
                     .AddCategory(UrnCreate.CategoryId(2269), "Korea", "COR")
                     .AddCategory(UrnCreate.CategoryId(2339), "Japan", "JPN")
                     .AddCategory(UrnCreate.CategoryId(2389), "Serbia", null)
                     .AddCategory(UrnCreate.CategoryId(2561), "China", "CHN");

        return builder.Build();
    }
}
