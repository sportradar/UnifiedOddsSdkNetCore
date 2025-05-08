// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;

public static class Soccer
{
    public static seasonExtended SeasonFull => GetSeasonBuilderExtended().BuildExtended();

    public static venue VenueFull => GetVenueBuilderExtended().Build();

    public static matchRound MatchRoundFull => GetMatchRoundBuilderExtended().Build();

    public static matchSummaryEndpoint Summary()
    {
        var match = GetSportEvent().Build();

        return new Endpoints.SummaryEndpoint()
              .WithSportEvent(_ => GetSportEvent())
              .WithSportEventConditions(_ => new SportEventConditionsBuilder()
                                            .WithVenue(match.venue)
                                            .WithReferee(Urn.Parse("sr:referee:69579"), "Kovacs, Istvan", "Romania"))
              .WithRestSportEventStatus(GetSportEventStatus())
              .WithCoverageInfo(_ => GetSummaryCoverageInfoBuilder())
              .WithMatchStatistics(_ => GetMatchStatisticsBuilder())
              .BuildMatchSummary();
    }

    public static fixturesEndpoint Fixture()
    {
        var fixture = new FixtureBuilder()
                     .WithSportEvent(_ => GetSportEvent()
                                         .WithStartTimeTbd()
                                         .WithEventStatus("closed")
                                         .WithLiveOdds("not_available")
                                         .WithNextLiveTime(DateTime.Parse("2025-03-11T20:00:00+00:00")))
                     .WithStartTime(DateTime.Parse("2025-03-11T20:00:00+00:00"))
                     .AddTvChannel("SKY Sport 253 HD - Hot Bird 1/2/3/4/6 (13.0E)")
                     .AddTvChannel("MTV Urheilu 1 HD - Thor 2/3 (1.0W)")
                     .AddTvChannel("V Sport Live 1 HD - Thor 2/3 (1.0W)")
                     .AddTvChannel("Sky Austria 3 HD - Astra 1C-1H / 2C (19.2E)")
                     .AddTvChannel("Paramount+")
                     .AddTvChannel("ViX+")
                     .AddTvChannel("CANAL+ Foot FR - Astra 1C-1H / 2C (19.2E)")
                     .AddExtraInfo("RTS", "not_available")
                     .AddExtraInfo("coverage_source", "venue")
                     .AddExtraInfo("extended_live_markets_offered", "true")
                     .AddExtraInfo("streaming", "false")
                     .AddExtraInfo("auto_traded", "false")
                     .AddExtraInfo("neutral_ground", "false")
                     .AddExtraInfo("period_length", "45")
                     .AddExtraInfo("early_ctrl_settlements", "true")
                     .WithCoverageInfo(_ => GetSummaryCoverageInfoBuilder().AddCoverage("extended_markets"))
                     .WithProductInfo(_ => GetProductInfoBuilder())
                     .WithReferences(b => b.WithBetradarCtrl(141176206).WithAams(4059916))
                     .Build();
        fixture.fixture.competitors[1].reference_ids =
            [
                new competitorReferenceIdsReference_id { name = "betradar", value = "6866" },
                new competitorReferenceIdsReference_id { name = "aams", value = "1644" }
            ];
        return fixture;
    }

    public static fixturesEndpoint FixtureFull()
    {
        var fixture = new FixtureBuilder()
                     .WithSportEvent(_ => GetSportEventFull()
                                         .WithStartTimeTbd()
                                         .WithEventStatus("closed")
                                         .WithLiveOdds("not_available")
                                         .WithNextLiveTime(DateTime.Parse("2025-03-11T20:00:00+00:00")))
                     .WithStartTime(DateTime.Parse("2025-03-11T20:00:00+00:00"))
                     .AddTvChannel("SKY Sport 253 HD - Hot Bird 1/2/3/4/6 (13.0E)")
                     .AddTvChannel("MTV Urheilu 1 HD - Thor 2/3 (1.0W)")
                     .AddTvChannel("V Sport Live 1 HD - Thor 2/3 (1.0W)")
                     .AddTvChannel("Sky Austria 3 HD - Astra 1C-1H / 2C (19.2E)")
                     .AddTvChannel("Paramount+")
                     .AddTvChannel("ViX+")
                     .AddTvChannel("CANAL+ Foot FR - Astra 1C-1H / 2C (19.2E)")
                     .AddExtraInfo("RTS", "not_available")
                     .AddExtraInfo("coverage_source", "venue")
                     .AddExtraInfo("extended_live_markets_offered", "true")
                     .AddExtraInfo("streaming", "false")
                     .AddExtraInfo("auto_traded", "false")
                     .AddExtraInfo("neutral_ground", "false")
                     .AddExtraInfo("period_length", "45")
                     .AddExtraInfo("early_ctrl_settlements", "true")
                     .WithCoverageInfo(_ => GetSummaryCoverageInfoBuilder().AddCoverage("extended_markets"))
                     .WithProductInfo(_ => GetProductInfoBuilder())
                     .WithReferences(b => b.WithBetradarCtrl(141176206).WithAams(4059916))
                     .Build();
        fixture.fixture.competitors[1].reference_ids =
            [
                new competitorReferenceIdsReference_id { name = "betradar", value = "6866" },
                new competitorReferenceIdsReference_id { name = "aams", value = "1644" }
            ];
        return fixture;
    }

    public static matchSummaryEndpoint SummaryFull()
    {
        return new Endpoints.SummaryEndpoint()
              .WithSportEvent(_ => GetSportEventFull())
              .WithSportEventConditions(_ => new SportEventConditionsBuilder()
                                            .WithVenue(GetVenueBuilderExtended().Build())
                                            .WithReferee(Urn.Parse("sr:referee:69579"), "Kovacs, Istvan", "Romania"))
              .WithRestSportEventStatus(GetSportEventStatus())
              .WithCoverageInfo(_ => GetSummaryCoverageInfoBuilder())
              .WithMatchStatistics(_ => GetMatchStatisticsBuilder())
              .BuildMatchSummary();
    }

    private static SportEventBuilder GetSportEvent()
    {
        return new SportEventBuilder()
              .WithId(UrnCreate.MatchId(58262005))
              .WithScheduledTime(DateTime.Parse("2025-03-11T20:00:00+00:00"))
              .WithStartTimeTbd(false)
              .WithTournamentRound(GetMatchRoundBuilder)
              .WithSeason(GetSeasonBuilder)
              .WithTournament(GetTournamentBuilder)
              .AddTeamCompetitor(GetHomeCompetitorBuilder)
              .AddTeamCompetitor(GetAwayCompetitorBuilder)
              .WithVenue(_ => GetVenueBuilder());
    }

    private static SportEventBuilder GetSportEventFull()
    {
        return new SportEventBuilder()
              .WithId(UrnCreate.MatchId(58262005))
              .WithScheduledTime(DateTime.Parse("2025-03-11T20:00:00+00:00"))
              .WithStartTimeTbd(false)
              .WithEventStatus("closed")
              .WithLiveOdds("not_available")
              .WithNextLiveTime(DateTime.Parse("2025-03-11T20:00:00+00:00"))
              .WithTournamentRound(GetMatchRoundBuilderExtended)
              .WithSeason(GetSeasonBuilderExtended)
              .WithTournament(() => GetTournamentBuilder()
                                 .WithCategory(UrnCreate.CategoryId(393), "International Clubs", "INT"))
              .AddTeamCompetitor(GetHomeCompetitorBuilder)
              .AddTeamCompetitor(() => GetAwayCompetitorBuilder()
                                    .WithReferences(new ReferencesBuilder()
                                                   .WithBetradar(6866)
                                                   .WithAams(1644)
                                                   .WithBetfair(123)
                                                   .WithLugas("any-lugas-id")
                                                   .WithRotationNumber(10)))
              .WithVenue(_ => GetVenueBuilderExtended())
              .WithSportEventConditions(_ => new SportEventConditionsBuilder()
                                            .WithVenue(GetVenueBuilderExtended().Build())
                                            .WithReferee(Urn.Parse("sr:referee:69579"), "Kovacs, Istvan", "Romania"))
              .WithName("Liverpool FC versus Paris Saint-Germain")
              .WithScheduledEndTime(DateTime.Parse("2025-03-11T22:00:00+00:00"))
              .WithReplacedBy(UrnCreate.MatchId(58262006))
              .WithType("parent");
    }

    private static MatchRoundBuilder GetMatchRoundBuilder()
    {
        return new MatchRoundBuilder()
              .WithType("cup")
              .WithName("Round of 16")
              .WithCupRoundMatches(2)
              .WithCupRoundMatchNumber(2)
              .WithOtherMatchId(UrnCreate.MatchId(58262007))
              .WithBetradarId(23)
              .WithBetradarName("UEFA Champions League")
              .WithPhase("playoffs");
    }

    private static MatchRoundBuilder GetMatchRoundBuilderExtended()
    {
        return GetMatchRoundBuilder()
              .WithNumber(11)
              .WithGroupId(UrnCreate.GroupId(333))
              .WithGroup("Relegation / Promotion Round")
              .WithGroupName("Relegation/Promotion Round")
              .WithGroupLongName("Elite Division, Men, Rel. / Prom. Round")
              .WithOtherMatchId(UrnCreate.MatchId(123));
    }

    private static SeasonBuilder GetSeasonBuilder()
    {
        return new SeasonBuilder()
              .WithId(UrnCreate.SeasonId(111))
              .WithName("UEFA Champions League 24/25")
              .WithTournamentId(UrnCreate.TournamentId(7))
              .WithStartDate(new DateOnly(2024, 7, 9))
              .WithEndDate(new DateOnly(2025, 5, 31))
              .WithYear("24/25");
    }

    private static SeasonBuilder GetSeasonBuilderExtended()
    {
        return GetSeasonBuilder()
              .WithStartTime(DateTime.Parse("00:14:15.000Z", null, DateTimeStyles.AdjustToUniversal))
              .WithEndTime(DateTime.Parse("22:59:59.000Z", null, DateTimeStyles.AdjustToUniversal));
    }

    private static TournamentBuilder GetTournamentBuilder()
    {
        return new TournamentBuilder()
              .WithId(UrnCreate.TournamentId(7))
              .WithName("UEFA Champions League")
              .WithSport(UrnCreate.SportId(1), "Soccer")
              .WithCategory(UrnCreate.CategoryId(393), "International Clubs", null);
    }

    private static CompetitorBuilder GetHomeCompetitorBuilder()
    {
        return new CompetitorBuilder()
              .WithId(UrnCreate.CompetitorId(44))
              .WithName("Liverpool FC")
              .WithAbbreviation("LFC")
              .WithShortName("Liverpool")
              .WithCountry("England")
              .WithCountryCode("ENG")
              .IsMale(true)
              .IsHome(true)
              .WithReferences(new ReferencesBuilder().WithBetradar(45745));
    }

    private static CompetitorBuilder GetAwayCompetitorBuilder()
    {
        // in fixture, it has additional reference_id with the name "aams" and value "1644"
        return new CompetitorBuilder()
              .WithId(UrnCreate.CompetitorId(1644))
              .WithName("Paris Saint-Germain")
              .WithAbbreviation("PSG")
              .WithShortName("PSG")
              .WithCountry("France")
              .WithCountryCode("FRA")
              .IsMale(true)
              .IsHome(false)
              .WithReferences(new ReferencesBuilder().WithBetradar(6866));
    }

    private static VenueBuilder GetVenueBuilder()
    {
        return new VenueBuilder()
              .WithId(UrnCreate.VenueId(579))
              .WithName("Anfield")
              .WithCapacity(61276)
              .WithCityName("Liverpool")
              .WithCountryName("England")
              .WithCountryCode("ENG")
              .WithMapCoordinates(53.430622, -2.960919);
    }

    private static VenueBuilder GetVenueBuilderExtended()
    {
        return GetVenueBuilder()
           .WithCourses(CourseBuilder.Create(Urn.Parse("sr:course:1"), "First Course")
                                     .AddHole(1, 4)
                                     .AddHole(2, 3)
                                     .Build(),
                        CourseBuilder.Create(Urn.Parse("sr:course:2"), "Second Course")
                                     .AddHole(1, 5)
                                     .AddHole(2, 4)
                                     .Build());
    }

    private static CoverageInfoBuilder GetSummaryCoverageInfoBuilder()
    {
        return new CoverageInfoBuilder()
              .WithLevel("platinum")
              .WithLiveCoverage(true)
              .WithCoveredFrom("venue")
              .AddCoverage("basic_score")
              .AddCoverage("key_events")
              .AddCoverage("detailed_events")
              .AddCoverage("lineups")
              .AddCoverage("passes_and_duels")
              .AddCoverage("commentary");
    }

    private static ProductInfoBuilder GetProductInfoBuilder()
    {
        return new ProductInfoBuilder()
              .IsInLiveScore()
              .IsInHostedStatistics()
              .IsInLiveCenterSoccer()
              .IsInLiveMatchTracker()
              .AddLink("live_match_tracker", "https://widgets.sir.sportradar.com/sportradar/en/standalone/match.lmtPlus#matchId=58262005");
    }

    private static restSportEventStatus GetSportEventStatus()
    {
        return SportEventStatusBuilder.Rest()
                                      .WithStatusCode(4)
                                      .WithStatus("closed")
                                      .WithMatchStatusCode(120)
                                      .WithMatchStatus("ap")
                                      .WithScore("1", "5")
                                      .WithAggregateScore("2", "5")
                                      .WithWinner(UrnCreate.CompetitorId(1644))
                                      .WithAggregateWinner(UrnCreate.VenueId(1644))
                                      .AddPeriodScore(1, "0", "1", 6, "regular_period")
                                      .AddPeriodScore(2, "0", "0", 7, "regular_period")
                                      .AddPeriodScore(3, "0", "0", 40, "overtime")
                                      .AddPeriodScore(4, "1", "4", 50, "penalties")
                                      .AddResult("0", "1", 100)
                                      .AddResult("0", "1", 110)
                                      .AddResult("1", "5", 120)
                                      .Build();
    }

    private static MatchStatisticsBuilder GetMatchStatisticsBuilder()
    {
        var competitorId1 = UrnCreate.CompetitorId(44);
        var competitorId2 = UrnCreate.CompetitorId(1644);
        return new MatchStatisticsBuilder()
              .WithTotal(new MatchStatisticsBuilder.TeamStatisticsBuilder()
                        .AddTeamStatistics(competitorId1, "Liverpool FC", "1", "12", null, "1", null)
                        .AddTeamStatistics(competitorId2, "Paris Saint-Germain", "1", "6", null, "1", null))
              .AddPeriod("1st half",
                         new MatchStatisticsBuilder.TeamStatisticsBuilder()
                            .AddTeamStatistics(competitorId1, "Liverpool FC", "0", "4", null, null, null)
                            .AddTeamStatistics(competitorId2, "Paris Saint-Germain", "0", "3", null, null, null))
              .AddPeriod("2nd half",
                         new MatchStatisticsBuilder.TeamStatisticsBuilder()
                            .AddTeamStatistics(competitorId1, "Liverpool FC", "1", "6", null, "1", null)
                            .AddTeamStatistics(competitorId2, "Paris Saint-Germain", "1", null, null, "1", null))
              .AddPeriod("1st extra",
                         new MatchStatisticsBuilder.TeamStatisticsBuilder()
                            .AddTeamStatistics(competitorId1, "Liverpool FC", "0", "2", null, null, null)
                            .AddTeamStatistics(competitorId2, "Paris Saint-Germain", "0", "1", null, null, null))
              .AddPeriod("2nd extra",
                         new MatchStatisticsBuilder.TeamStatisticsBuilder()
                            .AddTeamStatistics(competitorId1, "Liverpool FC", "0", null, null, null, null)
                            .AddTeamStatistics(competitorId2, "Paris Saint-Germain", "0", "2", null, null, null));
    }

    public static sportCategoriesEndpoint GetSportCategoriesEndpoint()
    {
        var builder = new SportCategoriesEndpoint()
                     .ForSport(UrnCreate.SportId(1), "Soccer")
                     .AddCategory(UrnCreate.CategoryId(1), "England", "ENG")
                     .AddCategory(UrnCreate.CategoryId(4), "International", null)
                     .AddCategory(UrnCreate.CategoryId(5), "Norway", "NOR")
                     .AddCategory(UrnCreate.CategoryId(7), "France", "FRA")
                     .AddCategory(UrnCreate.CategoryId(8), "Denmark", "DNK")
                     .AddCategory(UrnCreate.CategoryId(9), "Sweden", "SWE")
                     .AddCategory(UrnCreate.CategoryId(10), "Iceland", "ISL")
                     .AddCategory(UrnCreate.CategoryId(11), "Hungary", "HUN")
                     .AddCategory(UrnCreate.CategoryId(12), "Mexico", "MEX")
                     .AddCategory(UrnCreate.CategoryId(13), "Brazil", "BRA")
                     .AddCategory(UrnCreate.CategoryId(14), "Croatia", "HRV")
                     .AddCategory(UrnCreate.CategoryId(17), "Austria", "AUT")
                     .AddCategory(UrnCreate.CategoryId(18), "Czechia", "CZE")
                     .AddCategory(UrnCreate.CategoryId(19), "Finland", "FIN")
                     .AddCategory(UrnCreate.CategoryId(20), "Peru", "PER")
                     .AddCategory(UrnCreate.CategoryId(21), "Russia", "RUS")
                     .AddCategory(UrnCreate.CategoryId(22), "Scotland", "SCO")
                     .AddCategory(UrnCreate.CategoryId(23), "Slovakia", "SVK")
                     .AddCategory(UrnCreate.CategoryId(24), "Slovenia", "SVN")
                     .AddCategory(UrnCreate.CategoryId(25), "Switzerland", "CHE")
                     .AddCategory(UrnCreate.CategoryId(26), "USA", "USA")
                     .AddCategory(UrnCreate.CategoryId(30), "Germany", "DEU")
                     .AddCategory(UrnCreate.CategoryId(31), "Italy", "ITA")
                     .AddCategory(UrnCreate.CategoryId(32), "Spain", "ESP")
                     .AddCategory(UrnCreate.CategoryId(33), "Belgium", "BEL")
                     .AddCategory(UrnCreate.CategoryId(34), "Australia", "AUS")
                     .AddCategory(UrnCreate.CategoryId(35), "Netherlands", "NLD")
                     .AddCategory(UrnCreate.CategoryId(44), "Portugal", "PRT")
                     .AddCategory(UrnCreate.CategoryId(45), "Singapore", "SGP")
                     .AddCategory(UrnCreate.CategoryId(46), "Turkiye", "TUR")
                     .AddCategory(UrnCreate.CategoryId(47), "Poland", "POL")
                     .AddCategory(UrnCreate.CategoryId(48), "Argentina", "ARG")
                     .AddCategory(UrnCreate.CategoryId(49), "Chile", "CHL")
                     .AddCategory(UrnCreate.CategoryId(51), "Ireland", "IRL")
                     .AddCategory(UrnCreate.CategoryId(52), "Japan", "JPN")
                     .AddCategory(UrnCreate.CategoryId(57), "Uruguay", "URY")
                     .AddCategory(UrnCreate.CategoryId(66), "Israel", "ISR")
                     .AddCategory(UrnCreate.CategoryId(67), "Greece", "GRC")
                     .AddCategory(UrnCreate.CategoryId(77), "Romania", "ROU")
                     .AddCategory(UrnCreate.CategoryId(78), "Bulgaria", "BGR")
                     .AddCategory(UrnCreate.CategoryId(85), "Malaysia", "MYS")
                     .AddCategory(UrnCreate.CategoryId(86), "Ukraine", "UKR")
                     .AddCategory(UrnCreate.CategoryId(91), "Belarus", "BLR")
                     .AddCategory(UrnCreate.CategoryId(92), "Estonia", "EST")
                     .AddCategory(UrnCreate.CategoryId(94), "Norway Amateur", "NOR")
                     .AddCategory(UrnCreate.CategoryId(95), "Denmark Amateur", "DNK")
                     .AddCategory(UrnCreate.CategoryId(97), "Austria Amateur", "AUT")
                     .AddCategory(UrnCreate.CategoryId(99), "China", "CHN")
                     .AddCategory(UrnCreate.CategoryId(100), "Yugoslavia_NOT_IN_USE", null)
                     .AddCategory(UrnCreate.CategoryId(102), "Cyprus", "CYP")
                     .AddCategory(UrnCreate.CategoryId(122), "Germany Amateur", "DEU")
                     .AddCategory(UrnCreate.CategoryId(130), "Northern Ireland", "NIR")
                     .AddCategory(UrnCreate.CategoryId(131), "Wales", "WAL")
                     .AddCategory(UrnCreate.CategoryId(134), "Malta", "MLT")
                     .AddCategory(UrnCreate.CategoryId(148), "New Zealand", "NZL")
                     .AddCategory(UrnCreate.CategoryId(152), "Serbia", "SRB")
                     .AddCategory(UrnCreate.CategoryId(155), "Sweden Amateur", "SWE")
                     .AddCategory(UrnCreate.CategoryId(158), "Bosnia & Herzegovina", "BIH")
                     .AddCategory(UrnCreate.CategoryId(159), "North Macedonia", "MKD")
                     .AddCategory(UrnCreate.CategoryId(160), "Lithuania", "LTU")
                     .AddCategory(UrnCreate.CategoryId(163), "Latvia", "LVA")
                     .AddCategory(UrnCreate.CategoryId(165), "Ecuador", "ECU")
                     .AddCategory(UrnCreate.CategoryId(197), "Luxembourg", "LUX")
                     .AddCategory(UrnCreate.CategoryId(201), "Faroe Islands", "FRO")
                     .AddCategory(UrnCreate.CategoryId(207), "Finland Amateur", "FIN")
                     .AddCategory(UrnCreate.CategoryId(252), "England Amateur", "ENG")
                     .AddCategory(UrnCreate.CategoryId(254), "Turkiye Amateur", "TUR")
                     .AddCategory(UrnCreate.CategoryId(257), "Albania", "ALB")
                     .AddCategory(UrnCreate.CategoryId(270), "Georgia", "GEO")
                     .AddCategory(UrnCreate.CategoryId(274), "Colombia", "COL")
                     .AddCategory(UrnCreate.CategoryId(278), "Kazakhstan", "KAZ")
                     .AddCategory(UrnCreate.CategoryId(279), "Moldova", "MDA")
                     .AddCategory(UrnCreate.CategoryId(280), "Paraguay", "PRY")
                     .AddCategory(UrnCreate.CategoryId(281), "Venezuela", "VEN")
                     .AddCategory(UrnCreate.CategoryId(289), "Costa Rica", "CRI")
                     .AddCategory(UrnCreate.CategoryId(291), "Republic of Korea", "KOR")
                     .AddCategory(UrnCreate.CategoryId(296), "Armenia", "ARM")
                     .AddCategory(UrnCreate.CategoryId(297), "Azerbaijan", "AZE")
                     .AddCategory(UrnCreate.CategoryId(299), "United Arab Emirates", "ARE")
                     .AddCategory(UrnCreate.CategoryId(301), "Iran", "IRN")
                     .AddCategory(UrnCreate.CategoryId(303), "Morocco", "MAR")
                     .AddCategory(UrnCreate.CategoryId(304), "Algeria", "DZA")
                     .AddCategory(UrnCreate.CategoryId(305), "Egypt", "EGY")
                     .AddCategory(UrnCreate.CategoryId(310), "Saudi Arabia", "SAU")
                     .AddCategory(UrnCreate.CategoryId(322), "South Africa", "ZAF")
                     .AddCategory(UrnCreate.CategoryId(329), "Jordan", "JOR")
                     .AddCategory(UrnCreate.CategoryId(331), "Kuwait", "KWT")
                     .AddCategory(UrnCreate.CategoryId(339), "Hong Kong, China", "HKG")
                     .AddCategory(UrnCreate.CategoryId(351), "Bahrain", "BHR")
                     .AddCategory(UrnCreate.CategoryId(352), "India", "IND")
                     .AddCategory(UrnCreate.CategoryId(353), "Qatar", "QAT")
                     .AddCategory(UrnCreate.CategoryId(365), "Guatemala", "GTM")
                     .AddCategory(UrnCreate.CategoryId(366), "Vietnam", "VNM")
                     .AddCategory(UrnCreate.CategoryId(367), "El Salvador", "SLV")
                     .AddCategory(UrnCreate.CategoryId(368), "Indonesia", "IDN")
                     .AddCategory(UrnCreate.CategoryId(376), "Andorra", "AND")
                     .AddCategory(UrnCreate.CategoryId(378), "Tunisia", "TUN")
                     .AddCategory(UrnCreate.CategoryId(379), "Bolivia", "BOL")
                     .AddCategory(UrnCreate.CategoryId(380), "Syria", "SYR")
                     .AddCategory(UrnCreate.CategoryId(385), "Uzbekistan", "UZB")
                     .AddCategory(UrnCreate.CategoryId(386), "Montenegro", "MNE")
                     .AddCategory(UrnCreate.CategoryId(387), "San Marino", "SMR")
                     .AddCategory(UrnCreate.CategoryId(388), "Canada", "CAN")
                     .AddCategory(UrnCreate.CategoryId(389), "Nicaragua", "NIC")
                     .AddCategory(UrnCreate.CategoryId(390), "Ivory Coast", "CIV")
                     .AddCategory(UrnCreate.CategoryId(391), "Cameroon", "CMR")
                     .AddCategory(UrnCreate.CategoryId(392), "International Youth", null)
                     .AddCategory(UrnCreate.CategoryId(393), "International Clubs", null)
                     .AddCategory(UrnCreate.CategoryId(415), "Oman", "OMN")
                     .AddCategory(UrnCreate.CategoryId(417), "Libya", "LBY")
                     .AddCategory(UrnCreate.CategoryId(421), "Spain Amateur", "ESP")
                     .AddCategory(UrnCreate.CategoryId(428), "Lebanon", "LBN")
                     .AddCategory(UrnCreate.CategoryId(437), "Honduras", "HND")
                     .AddCategory(UrnCreate.CategoryId(440), "Yemen", "YEM")
                     .AddCategory(UrnCreate.CategoryId(469), "Bangladesh", "BGD")
                     .AddCategory(UrnCreate.CategoryId(476), "Sudan", "SDN")
                     .AddCategory(UrnCreate.CategoryId(485), "Thailand", "THA")
                     .AddCategory(UrnCreate.CategoryId(490), "Iraq", "IRQ")
                     .AddCategory(UrnCreate.CategoryId(500), "Angola", "AGO")
                     .AddCategory(UrnCreate.CategoryId(502), "Jamaica", "JAM")
                     .AddCategory(UrnCreate.CategoryId(512), "Liechtenstein", "LIE")
                     .AddCategory(UrnCreate.CategoryId(525), "Belize", "BLZ")
                     .AddCategory(UrnCreate.CategoryId(526), "Panama", "PAN")
                     .AddCategory(UrnCreate.CategoryId(527), "Trinidad and Tobago", "TTO")
                     .AddCategory(UrnCreate.CategoryId(532), "Nigeria", "NGA")
                     .AddCategory(UrnCreate.CategoryId(540), "Palestine", "PSE")
                     .AddCategory(UrnCreate.CategoryId(542), "Ghana", "GHA")
                     .AddCategory(UrnCreate.CategoryId(564), "Namibia", "NAM")
                     .AddCategory(UrnCreate.CategoryId(565), "Kosovo", "KOS")
                     .AddCategory(UrnCreate.CategoryId(567), "Mauritania", "MRT")
                     .AddCategory(UrnCreate.CategoryId(570), "Puerto Rico", "PRI")
                     .AddCategory(UrnCreate.CategoryId(582), "DR Congo", "COD")
                     .AddCategory(UrnCreate.CategoryId(600), "Maldives", "MDV")
                     .AddCategory(UrnCreate.CategoryId(608), "Aruba", "ABW")
                     .AddCategory(UrnCreate.CategoryId(711), "Korea DPR", "PRK")
                     .AddCategory(UrnCreate.CategoryId(746), "Papua New Guinea", "PNG")
                     .AddCategory(UrnCreate.CategoryId(755), "Pakistan", "PAK")
                     .AddCategory(UrnCreate.CategoryId(773), "Macao", "MAC")
                     .AddCategory(UrnCreate.CategoryId(787), "Kyrgyzstan", "KGZ")
                     .AddCategory(UrnCreate.CategoryId(790), "Myanmar", "MMR")
                     .AddCategory(UrnCreate.CategoryId(791), "Burkina Faso", "BFA")
                     .AddCategory(UrnCreate.CategoryId(792), "Barbados", "BRB")
                     .AddCategory(UrnCreate.CategoryId(793), "Chinese Taipei", "TPE")
                     .AddCategory(UrnCreate.CategoryId(794), "Guyana", "GUY")
                     .AddCategory(UrnCreate.CategoryId(795), "Haiti", "HTI")
                     .AddCategory(UrnCreate.CategoryId(800), "Virtual Leagues", null)
                     .AddCategory(UrnCreate.CategoryId(803), "Tanzania", "TZA")
                     .AddCategory(UrnCreate.CategoryId(804), "Botswana", "BWA")
                     .AddCategory(UrnCreate.CategoryId(805), "Kenya", "KEN")
                     .AddCategory(UrnCreate.CategoryId(806), "Mozambique", "MOZ")
                     .AddCategory(UrnCreate.CategoryId(807), "Fiji", "FJI")
                     .AddCategory(UrnCreate.CategoryId(812), "Zambia", "ZMB")
                     .AddCategory(UrnCreate.CategoryId(815), "Zimbabwe", "ZWE")
                     .AddCategory(UrnCreate.CategoryId(824), "Uganda", "UGA")
                     .AddCategory(UrnCreate.CategoryId(825), "Tajikistan", "TJK")
                     .AddCategory(UrnCreate.CategoryId(827), "Curacao", "CUW")
                     .AddCategory(UrnCreate.CategoryId(829), "Malawi", "MWI")
                     .AddCategory(UrnCreate.CategoryId(832), "Mali", "MLI")
                     .AddCategory(UrnCreate.CategoryId(840), "Tahiti", "PYF")
                     .AddCategory(UrnCreate.CategoryId(847), "Philippines", "PHL")
                     .AddCategory(UrnCreate.CategoryId(849), "Laos", "LAO")
                     .AddCategory(UrnCreate.CategoryId(852), "Cambodia", "KHM")
                     .AddCategory(UrnCreate.CategoryId(866), "Togo", "TGO")
                     .AddCategory(UrnCreate.CategoryId(867), "Turkmenistan", "TKM")
                     .AddCategory(UrnCreate.CategoryId(869), "Afghanistan", "AFG")
                     .AddCategory(UrnCreate.CategoryId(882), "New Caledonia", "NCL")
                     .AddCategory(UrnCreate.CategoryId(883), "Vanuatu", "VUT")
                     .AddCategory(UrnCreate.CategoryId(884), "Solomon Islands", "SLB")
                     .AddCategory(UrnCreate.CategoryId(886), "Senegal", "SEN")
                     .AddCategory(UrnCreate.CategoryId(898), "Congo", "COG")
                     .AddCategory(UrnCreate.CategoryId(904), "Suriname", "SUR")
                     .AddCategory(UrnCreate.CategoryId(905), "Nepal", "NPL")
                     .AddCategory(UrnCreate.CategoryId(906), "Djibouti", "DJI")
                     .AddCategory(UrnCreate.CategoryId(907), "Gabon", "GAB")
                     .AddCategory(UrnCreate.CategoryId(908), "Guam", "GUM")
                     .AddCategory(UrnCreate.CategoryId(909), "Bahamas", "BHS")
                     .AddCategory(UrnCreate.CategoryId(910), "Cayman Islands", "CYM")
                     .AddCategory(UrnCreate.CategoryId(911), "Bermuda", "BMU")
                     .AddCategory(UrnCreate.CategoryId(912), "Mauritius", "MUS")
                     .AddCategory(UrnCreate.CategoryId(914), "Ethiopia", "ETH")
                     .AddCategory(UrnCreate.CategoryId(915), "Eswatini", "SWZ")
                     .AddCategory(UrnCreate.CategoryId(938), "Gibraltar", "GIB")
                     .AddCategory(UrnCreate.CategoryId(943), "Somalia", "SOM")
                     .AddCategory(UrnCreate.CategoryId(944), "Antigua and Barbuda", "ATG")
                     .AddCategory(UrnCreate.CategoryId(951), "Rwanda", "RWA")
                     .AddCategory(UrnCreate.CategoryId(952), "Chad", "TCD")
                     .AddCategory(UrnCreate.CategoryId(953), "Liberia", "LBR")
                     .AddCategory(UrnCreate.CategoryId(954), "South Sudan", "SSD")
                     .AddCategory(UrnCreate.CategoryId(955), "Zanzibar", null)
                     .AddCategory(UrnCreate.CategoryId(956), "Burundi", "BDI")
                     .AddCategory(UrnCreate.CategoryId(957), "Gambia", "GMB")
                     .AddCategory(UrnCreate.CategoryId(958), "Guinea", "GIN")
                     .AddCategory(UrnCreate.CategoryId(959), "Seychelles", "SYC")
                     .AddCategory(UrnCreate.CategoryId(960), "Equatorial Guinea", "GNQ")
                     .AddCategory(UrnCreate.CategoryId(961), "Madagascar", "MDG")
                     .AddCategory(UrnCreate.CategoryId(962), "Lesotho", "LSO")
                     .AddCategory(UrnCreate.CategoryId(963), "Sierra Leone", "SLE")
                     .AddCategory(UrnCreate.CategoryId(964), "Guinea-bissau", "GNB")
                     .AddCategory(UrnCreate.CategoryId(965), "Comoros", "COM")
                     .AddCategory(UrnCreate.CategoryId(970), "Sao Tome and Principe", "STP")
                     .AddCategory(UrnCreate.CategoryId(971), "Niger", "NER")
                     .AddCategory(UrnCreate.CategoryId(978), "Samoa", "WSM")
                     .AddCategory(UrnCreate.CategoryId(1009), "Sri Lanka", "LKA")
                     .AddCategory(UrnCreate.CategoryId(1020), "Republika Srpska", null)
                     .AddCategory(UrnCreate.CategoryId(1031), "Dominican Republic", "DOM")
                     .AddCategory(UrnCreate.CategoryId(1033), "Electronic Leagues", null)
                     .AddCategory(UrnCreate.CategoryId(1042), "Mongolia", "MNG")
                     .AddCategory(UrnCreate.CategoryId(1043), "Bhutan", "BTN")
                     .AddCategory(UrnCreate.CategoryId(1057), "Crimea", null)
                     .AddCategory(UrnCreate.CategoryId(1111), "Virtual Football", null)
                     .AddCategory(UrnCreate.CategoryId(1360), "Brunei Darussalam", "BRN")
                     .AddCategory(UrnCreate.CategoryId(1380), "Cuba", "CUB")
                     .AddCategory(UrnCreate.CategoryId(1435), "Grenada", "GRD")
                     .AddCategory(UrnCreate.CategoryId(1519), "Benin", "BEN")
                     .AddCategory(UrnCreate.CategoryId(1533), "Dominica", "DMA")
                     .AddCategory(UrnCreate.CategoryId(1539), "Guadeloupe", "GLP")
                     .AddCategory(UrnCreate.CategoryId(1674), "Saint Kitts and Nevis", "KNA")
                     .AddCategory(UrnCreate.CategoryId(1677), "Northern Cyprus", null)
                     .AddCategory(UrnCreate.CategoryId(1748), "Timor-Leste", "TLS")
                     .AddCategory(UrnCreate.CategoryId(2123), "Simulated Reality League", null)
                     .AddCategory(UrnCreate.CategoryId(2134), "Indoor Soccer", null)
                     .AddCategory(UrnCreate.CategoryId(2246), "Simulated Reality Women", null)
                     .AddCategory(UrnCreate.CategoryId(2350), "Martinique", null)
                     .AddCategory(UrnCreate.CategoryId(2354), "Saint Lucia", "LCA");
        return builder.Build();
    }
}
