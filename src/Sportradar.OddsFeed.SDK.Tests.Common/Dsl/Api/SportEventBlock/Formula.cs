// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;

public static class Formula
{
    public static stageSummaryEndpoint Race()
    {
        return new Endpoints.SummaryEndpoint()
            .WithSportEvent(_ => GetSportEventForRace())
            .WithStageSportEventStatus(GetSportEventStatusForRace())
            .BuildStageSummary();
    }

    public static stageSummaryEndpoint GrandPrix()
    {
        return new Endpoints.SummaryEndpoint()
            .WithSportEvent(_ => GetSportEventForGrandPrix())
            .WithStageSportEventStatus(GetSportEventStatusForGrandPrix())
              .BuildStageSummary();
    }

    public static tournamentInfoEndpoint RaceTournament()
    {
        return new TournamentInfoBuilder()
            .WithTournament(() => new TournamentBuilder()
                .WithId(UrnCreate.StageId(1189123))
                .WithName("Formula 1 2025")
                .WithSport(UrnCreate.SportId(40), "Formula 1")
                .WithCategory(UrnCreate.CategoryId(36), "Formula 1", null)
                .WithScheduled(DateTime.Parse("2025-03-14T01:35:00+00:00"))
                .WithScheduledEnd(DateTime.Parse("2025-12-07T15:00:00+00:00"))
                .AddTeam(() => BuildCompetitor(4521, "Alonso, Fernando", "ALO").WithCountry("Spain").WithCountryCode("ESP").IsMale())
                .AddTeam(() => BuildCompetitor(7135, "Hamilton, Lewis", "HAM").WithCountry("Great Britain").WithCountryCode("GBR").IsMale())
                .AddTeam(() => BuildCompetitor(39412, "Hulkenberg, Nico", "HUL").WithCountry("Germany").WithCountryCode("DEU").IsMale())
                .AddTeam(() => BuildCompetitor(178318, "Verstappen, Max", "VER").WithCountry("Netherlands").WithCountryCode("NLD").IsMale())
                .AddTeam(() => BuildCompetitor(184751, "Ocon, Esteban", "OCO").WithCountry("France").WithCountryCode("FRA").IsMale())
                .AddTeam(() => BuildCompetitor(189029, "Sainz Jr., Carlos", "SAI").WithCountry("Spain").WithCountryCode("ESP").IsMale())
                .AddTeam(() => BuildCompetitor(269471, "Leclerc, Charles", "LEC").WithCountry("Monaco").WithCountryCode("MCO").IsMale())
                .AddTeam(() => BuildCompetitor(302866, "Stroll, Lance", "STR").WithCountry("Canada").WithCountryCode("CAN").IsMale())
                .AddTeam(() => BuildCompetitor(381362, "Gasly, Pierre", "GAS").WithCountry("France").WithCountryCode("FRA").IsMale())
                .AddTeam(() => BuildCompetitor(391432, "Russell, George", "RUS").WithCountry("Great Britain").WithCountryCode("GBR").IsMale())
                .AddTeam(() => BuildCompetitor(495898, "Norris, Lando", "NOR").WithCountry("Great Britain").WithCountryCode("GBR").IsMale())
                .AddTeam(() => BuildCompetitor(522994, "Albon, Alexander", "ALB").WithCountry("Thailand").WithCountryCode("THA").IsMale())
                .AddTeam(() => BuildCompetitor(644036, "Hirakawa, Ryo", "HIR").WithCountry("Japan").WithCountryCode("JPN").IsMale())
                .AddTeam(() => BuildCompetitor(764646, "Tsunoda, Yuki", "TSU").WithCountry("Japan").WithCountryCode("JPN").IsMale())
                .AddTeam(() => BuildCompetitor(792380, "Lawson, Liam", "LAW").WithCountry("New Zealand").WithCountryCode("NZL").IsMale())
                .AddTeam(() => BuildCompetitor(953189, "Piastri, Oscar", "PIA").WithCountry("Australia").WithCountryCode("AUS").IsMale())
                .AddTeam(() => BuildCompetitor(959061, "Doohan, Jack", "DOO").WithCountry("Australia").WithCountryCode("AUS").IsMale())
                .AddTeam(() => BuildCompetitor(966967, "Drugovich, Felipe", "DRU").WithCountry("Brazil").WithCountryCode("BRA").IsMale())
                .AddTeam(() => BuildCompetitor(1075732, "Vesti, Frederik", "VES").WithCountry("Denmark").WithCountryCode("DNK").IsMale())
                .AddTeam(() => BuildCompetitor(1075734, "Hadjar, Isack", "HAD").WithCountry("France").WithCountryCode("FRA").IsMale())
                .AddTeam(() => BuildCompetitor(1075736, "Bearman, Oliver", "BEA").WithCountry("Great Britain").WithCountryCode("GBR").IsMale())
                .AddTeam(() => BuildCompetitor(1112567, "Bortoleto, Gabriel", "BOR").WithCountry("Portugal").WithCountryCode("PRT").IsMale())
                .AddTeam(() => BuildCompetitor(1112573, "Colapinto, Franco", "COL").WithCountry("Argentina").WithCountryCode("ARG").IsMale())
                .AddTeam(() => BuildCompetitor(1119923, "Iwasa, Ayumu", "IWA").WithCountry("Japan").WithCountryCode("JPN").IsMale())
                .AddTeam(() => BuildCompetitor(1180107, "Antonelli, Andrea Kimi", "ANT").WithCountry("Italy").WithCountryCode("ITA").IsMale())
                .AddTeam(() => BuildCompetitor(1214501, "Browning, Luke", "BRO").WithCountry("Great Britain").WithCountryCode("GBR").IsMale())
                .AddTeam(() => BuildCompetitor(1244962, "Beganovic, Dino", "BEG").WithCountry("Sweden").WithCountryCode("SWE").IsMale())
                .AddTeam(() => BuildCompetitor(1257175, "Martins, Victor", "MAR").WithCountry("France").WithCountryCode("FRA").IsMale()))
            .WithLiveCoverageInfo(true)
            .Build();
    }

    private static SportEventBuilder GetSportEventForRace()
    {
        var builder = new SportEventBuilder()
            .WithId(UrnCreate.StageId(1190143))
            .WithName("Race")
            .WithType("child")
            .WithStageType("race")
            .WithScheduledTime(DateTime.Parse("2025-03-23T07:00:00+00:00"))
            .WithScheduledEndTime(DateTime.Parse("2025-03-23T09:00:00+00:00"))
            .WithParent(builder => builder.WithId(UrnCreate.StageId(1190057)).WithName("Chinese Grand Prix 2025").WithType("parent").WithStageType("event"))
            .WithTournament(GetTournamentBuilder);
        builder.AddTeamCompetitors();
        return builder;
    }

    private static SportEventBuilder GetSportEventForGrandPrix()
    {
        var builder = new SportEventBuilder()
              .WithId(UrnCreate.StageId(1190057))
              .WithName("Chinese Grand Prix 2025")
              .WithType("parent")
              .WithStageType("event")
              .WithScheduledTime(DateTime.Parse("2025-03-21T03:30:00+00:00"))
              .WithScheduledEndTime(DateTime.Parse("2025-03-23T09:00:00+00:00"))
              .WithParent(builder => builder.WithId(UrnCreate.StageId(1189123)).WithName("Formula 1 2025").WithType("parent").WithStageType("season"))
              .WithTournament(GetTournamentBuilder)
              .AddRace(() => BuildChildRace(1190059, "practice", "2025-03-21T03:30:00+00:00"))
              .AddRace(() => BuildChildRace(1190065, "qualifying", "2025-03-22T07:00:00+00:00"))
              .AddRace(() => BuildChildRace(1190073, "qualifying", "2025-03-21T07:30:00+00:00"))
              .AddRace(() => BuildChildRace(1190081, "sprint_race", "2025-03-22T03:00:00+00:00"))
              .AddRace(() => BuildChildRace(1190143, "race", "2025-03-23T07:00:00+00:00"));
        builder.AddTeamCompetitors();
        return builder;
    }

    private static void AddTeamCompetitors(this SportEventBuilder builder)
    {
        builder.AddTeamCompetitor(() => BuildCompetitor(4521, "Alonso, Fernando", "ALO"))
            .AddTeamCompetitor(() => BuildCompetitor(7135, "Hamilton, Lewis", "HAM"))
            .AddTeamCompetitor(() => BuildCompetitor(39412, "Hulkenberg, Nico", "HUL"))
            .AddTeamCompetitor(() => BuildCompetitor(178318, "Verstappen, Max", "VER"))
            .AddTeamCompetitor(() => BuildCompetitor(184751, "Ocon, Esteban", "OCO"))
            .AddTeamCompetitor(() => BuildCompetitor(189029, "Sainz Jr., Carlos", "SAI"))
            .AddTeamCompetitor(() => BuildCompetitor(269471, "Leclerc, Charles", "LEC"))
            .AddTeamCompetitor(() => BuildCompetitor(302866, "Stroll, Lance", "STR"))
            .AddTeamCompetitor(() => BuildCompetitor(381362, "Gasly, Pierre", "GAS"))
            .AddTeamCompetitor(() => BuildCompetitor(391432, "Russell, George", "RUS"))
            .AddTeamCompetitor(() => BuildCompetitor(495898, "Norris, Lando", "NOR"))
            .AddTeamCompetitor(() => BuildCompetitor(522994, "Albon, Alexander", "ALB"))
            .AddTeamCompetitor(() => BuildCompetitor(764646, "Tsunoda, Yuki", "TSU"))
            .AddTeamCompetitor(() => BuildCompetitor(792380, "Lawson, Liam", "LAW"))
            .AddTeamCompetitor(() => BuildCompetitor(953189, "Piastri, Oscar", "PIA"))
            .AddTeamCompetitor(() => BuildCompetitor(959061, "Doohan, Jack", "DOO"))
            .AddTeamCompetitor(() => BuildCompetitor(1075734, "Hadjar, Isack", "HAD"))
            .AddTeamCompetitor(() => BuildCompetitor(1075736, "Bearman, Oliver", "BEA"))
            .AddTeamCompetitor(() => BuildCompetitor(1112567, "Bortoleto, Gabriel", "BOR"))
            .AddTeamCompetitor(() => BuildCompetitor(1180107, "Antonelli, Andrea Kimi", "ANt"));
    }

    private static CompetitorBuilder BuildCompetitor(int competitorId, string name, string abbreviation)
    {
        return new CompetitorBuilder()
              .WithId(UrnCreate.CompetitorId(competitorId))
              .WithName(name)
              .WithAbbreviation(abbreviation)
              .IsMale();
    }

    private static SportEventChildrenBuilder BuildChildRace(int stageId, string stageType, string scheduled)
    {
        return new SportEventChildrenBuilder()
              .WithId(UrnCreate.StageId(stageId))
              .WithType("child")
              .WithStageType(stageType)
              .WithScheduledTime(DateTime.Parse(scheduled));
    }

    private static TournamentBuilder GetTournamentBuilder()
    {
        return new TournamentBuilder()
              .WithId(UrnCreate.StageId(1189123))
              .WithName("Formula 1 2025")
              .WithScheduled(DateTime.Parse("2025-03-14T01:35:00+00:00"))
              .WithScheduledEnd(DateTime.Parse("2025-12-07T15:00:00+00:00"))
              .WithSport(UrnCreate.SportId(40), "Formula 1")
              .WithCategory(UrnCreate.CategoryId(36), "Formula 1", null);
    }

    private static stageSportEventStatus GetSportEventStatusForRace()
    {
        return SportEventStatusBuilder.Stage()
            .WithStatus("ended")
            .WithWinner(UrnCreate.CompetitorId(953189))
            .AddResult(new StageResultBuilder()
                .AddCompetitor(UrnCreate.CompetitorId(4521), null, [
                    new KeyValuePair<string, string>("time", "1:30:55.026"),
                    new KeyValuePair<string, string>("position", "1"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.520"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(495898), null, [
                    new KeyValuePair<string, string>("time", "+9.748"),
                    new KeyValuePair<string, string>("position", "2"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.454"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(391432), null, [
                    new KeyValuePair<string, string>("time", "+11.097"),
                    new KeyValuePair<string, string>("position", "3"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.816"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(178318), null, [
                    new KeyValuePair<string, string>("time", "+16.656"),
                    new KeyValuePair<string, string>("position", "4"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.488"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(184751), null, [
                    new KeyValuePair<string, string>("time", "+49.969"),
                    new KeyValuePair<string, string>("position", "5"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.740"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(1180107), null, [
                    new KeyValuePair<string, string>("time", "+53.748"),
                    new KeyValuePair<string, string>("position", "6"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.046"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(522994), null, [
                    new KeyValuePair<string, string>("time", "+56.321"),
                    new KeyValuePair<string, string>("position", "7"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.254"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(1075736), null, [
                    new KeyValuePair<string, string>("time", "+1:01.303"),
                    new KeyValuePair<string, string>("position", "8"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.363"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(302866), null, [
                    new KeyValuePair<string, string>("time", "+1:10.204"),
                    new KeyValuePair<string, string>("position", "9"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.044"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(189029), null, [
                    new KeyValuePair<string, string>("time", "+1:16.387"),
                    new KeyValuePair<string, string>("position", "10"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.779"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(1075734), null, [
                    new KeyValuePair<string, string>("time", "+1:18.875"),
                    new KeyValuePair<string, string>("position", "11"),
                    new KeyValuePair<string, string>("no_pitstops", "2"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.868"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(792380), null, [
                    new KeyValuePair<string, string>("time", "+1:21.147"),
                    new KeyValuePair<string, string>("position", "12"),
                    new KeyValuePair<string, string>("no_pitstops", "2"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.985"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(959061), null, [
                    new KeyValuePair<string, string>("time", "+1:28.401"),
                    new KeyValuePair<string, string>("position", "13"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:36.424"),
                    new KeyValuePair<string, string>("finished_laps", "56")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(1112567), null, [
                    new KeyValuePair<string, string>("time", "1l"),
                    new KeyValuePair<string, string>("position", "14"),
                    new KeyValuePair<string, string>("no_pitstops", "2"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.874"),
                    new KeyValuePair<string, string>("finished_laps", "55")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(39412), null, [
                    new KeyValuePair<string, string>("time", "1l"),
                    new KeyValuePair<string, string>("position", "15"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:37.275"),
                    new KeyValuePair<string, string>("finished_laps", "55")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(764646), null, [
                    new KeyValuePair<string, string>("time", "1l"),
                    new KeyValuePair<string, string>("position", "16"),
                    new KeyValuePair<string, string>("no_pitstops", "3"),
                    new KeyValuePair<string, string>("fastest_lap", "1:35.871"),
                    new KeyValuePair<string, string>("finished_laps", "55")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(4521), null, [
                    new KeyValuePair<string, string>("retired_in_lap", "NA"),
                    new KeyValuePair<string, string>("position", "17"),
                    new KeyValuePair<string, string>("no_pitstops", "0"),
                    new KeyValuePair<string, string>("fastest_lap", "1:39.256"),
                    new KeyValuePair<string, string>("finished_laps", "4")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(269471), null, [
                    new KeyValuePair<string, string>("position", "18"),
                    new KeyValuePair<string, string>("no_pitstops", "0"),
                    new KeyValuePair<string, string>("fastest_lap", "1:37.648"),
                    new KeyValuePair<string, string>("finished_laps", "5")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(7135), null, [
                    new KeyValuePair<string, string>("position", "19"),
                    new KeyValuePair<string, string>("no_pitstops", "0"),
                    new KeyValuePair<string, string>("fastest_lap", "1:37.802"),
                    new KeyValuePair<string, string>("finished_laps", "5")
                ])
                .AddCompetitor(UrnCreate.CompetitorId(381362), null, [
                    new KeyValuePair<string, string>("position", "20"),
                    new KeyValuePair<string, string>("no_pitstops", "1"),
                    new KeyValuePair<string, string>("fastest_lap", "1:37.366"),
                    new KeyValuePair<string, string>("finished_laps", "1")
                ])
            ).Build();
    }

    private static stageSportEventStatus GetSportEventStatusForGrandPrix()
    {
        return SportEventStatusBuilder.Stage()
                                      .WithStatus("ended")
                                      .WithWinner(UrnCreate.CompetitorId(953189))
                                      .AddResult(UrnCreate.CompetitorId(4521))
                                      .AddResult(UrnCreate.CompetitorId(381362))
                                      .AddResult(UrnCreate.CompetitorId(495898), 3)
                                      .AddResult(UrnCreate.CompetitorId(269471), 10)
                                      .AddResult(UrnCreate.CompetitorId(391432), 2)
                                      .AddResult(UrnCreate.CompetitorId(302866), 12)
                                      .AddResult(UrnCreate.CompetitorId(959061))
                                      .AddResult(UrnCreate.CompetitorId(1180107), 6)
                                      .AddResult(UrnCreate.CompetitorId(7135), 7)
                                      .AddResult(UrnCreate.CompetitorId(792380))
                                      .AddResult(UrnCreate.CompetitorId(1075734))
                                      .AddResult(UrnCreate.CompetitorId(522994), 8)
                                      .AddResult(UrnCreate.CompetitorId(1075736), 9)
                                      .AddResult(UrnCreate.CompetitorId(1112567))
                                      .AddResult(UrnCreate.CompetitorId(39412))
                                      .AddResult(UrnCreate.CompetitorId(178318), 4)
                                      .AddResult(UrnCreate.CompetitorId(189029), 13)
                                      .AddResult(UrnCreate.CompetitorId(764646), 11)
                                      .AddResult(UrnCreate.CompetitorId(184751), 5)
                                      .Build();
    }

    public static sportCategoriesEndpoint GetSportCategoriesEndpoint()
    {
        var builder = new SportCategoriesEndpoint()
                     .ForSport(UrnCreate.SportId(40), "Formula 1")
                     .AddCategory(UrnCreate.CategoryId(36), "Formula 1", null);
        return builder.Build();
    }
}
