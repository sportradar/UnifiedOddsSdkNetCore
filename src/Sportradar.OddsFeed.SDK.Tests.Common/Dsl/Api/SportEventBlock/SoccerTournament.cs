// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;

public static class SoccerTournament
{
    public static tournamentInfoEndpoint TournamentInfo()
    {
        return new TournamentInfoBuilder()
              .WithGeneratedAt(ParseDate("2026-10-26T11:10:24+00:00"))
              .WithTournament(GetTournament)
              .WithSeason(GetSeason)
              .WithRound(GetRound)
              .WithSeasonCoverageInfo(GetSeasonCoverageInfo().Build())
              .WithLiveCoverageInfo(true)
              .AddGroup(GetCompetitors())
              .Build();
    }

    private static TournamentBuilder GetTournament()
    {
        return new TournamentBuilder()
              .WithId(UrnCreate.TournamentId(7))
              .WithName("UEFA Champions League")
              .WithSport(UrnCreate.SportId(1), "Soccer")
              .WithCategory(UrnCreate.CategoryId(393), "International Clubs", null)
              .WithCurrentSeason(GetCurrentSeason().BuildCurrent());
    }

    private static SeasonBuilder GetCurrentSeason()
    {
        return new SeasonBuilder()
              .WithId(UrnCreate.SeasonId(140786))
              .WithName("UEFA Champions League 26/27")
              .WithStartDate(new DateOnly(2026, 7, 7))
              .WithEndDate(new DateOnly(2027, 6, 5))
              .WithYear("26/27");
    }

    private static SeasonBuilder GetSeason()
    {
        return GetCurrentSeason().WithTournamentId(UrnCreate.TournamentId(7));
    }

    private static MatchRoundBuilder GetRound()
    {
        return new MatchRoundBuilder()
              .WithType("group")
              .WithNumber(3);
    }

    private static SeasonCoverageInfoBuilder GetSeasonCoverageInfo()
    {
        return new SeasonCoverageInfoBuilder()
              .WithSeasonId(UrnCreate.SeasonId(140786))
              .WithScheduled(234)
              .WithPlayed(291)
              .WithMaxCoverageLevel("gold")
              .WithMaxCovered(150)
              .WithMinCoverageLevel("bronze");
    }

    private static Func<CompetitorBuilder>[] GetCompetitors()
    {
        return
        [
            Competitor(3395, "AC Omonia Nicosia", "OMO", "Omonia Nicosia", "Cyprus", "CYP", 43424),
            Competitor(3250, "AEK Athens", "AEK", null, "Greece", "GRC", 122668),
            Competitor(1291, "AGF Aarhus", "AGF", "Aarhus", "Denmark", "DNK", 18248464),
            Competitor(2702, "AS Roma", "ROM", "Roma", "Italy", "ITA", 32154),
            Competitor(42, "Arsenal FC", "ARS", "Arsenal", "England", "ENG", 6482),
            Competitor(40, "Aston Villa", "AVL", null, "England", "ENG", 13440953),
            Competitor(38989, "Atert Bissen", "BIS", "Bissen", "Luxembourg", "LUX", 34367548),
            Competitor(2836, "Atletico Madrid", "ATM", "Atletico", "Spain", "ESP", 1272672),
            Competitor(2672, "Bayern Munich", "BMU", null, "Germany", "DEU", 6631),
            Competitor(656, "Bodoe/Glimt", "BOG", null, "Norway", "NOR", 17863576),
            Competitor(2673, "Borussia Dortmund", "BVB", "Dortmund", "Germany", "DEU", 32138),
            Competitor(116223, "CS Universitatea Craiova", "CSU", "Uni Craiova", "Romania", "ROU", 21952583),
            Competitor(2352, "Celtic Glasgow", "CEL", "Celtic", "Scotland", "SCO", 46089),
            Competitor(2888, "Club Brugge", "BRU", null, "Belgium", "BEL", 119997),
            Competitor(2704, "Como 1907", "COM", "Como", "Italy", "ITA", 30754277),
            Competitor(364472, "FC Ararat-Armenia", "ARA", "Ararat-Armenia", "Armenia", "ARM", 12339642),
            Competitor(2817, "FC Barcelona", "BAR", "Barcelona", "Spain", "ESP", 6497),
            Competitor(277835, "FC Drita", "DRI", "Drita", "Kosovo", "KOS", 10924942),
            Competitor(116312, "FC Iberia 1999", "IBE", "Iberia", "Georgia", "GEO", 12339643),
            Competitor(5172, "FC Kairat Almaty", "ALM", "Kairat", "Kazakhstan", "KAZ", 554421),
            Competitor(3002, "FC Porto", "FCP", "Porto", "Portugal", "PRT", 45749),
            Competitor(3313, "FC Shakhtar Donetsk", "SHA", "Shakhtar", "Ukraine", "UKR", 6637),
            Competitor(2454, "FC Thun", "THU", "Thun", "Switzerland", "CHE", 542008),
            Competitor(5235, "FK Borac Banja Luka", "BBL", "Banja Luka", "Bosnia & Herzegovina", "BIH", 5756736),
            Competitor(5149, "FK Crvena Zvezda Belgrade", "CZV", "Crvena Zvezda", "Serbia", "SRB", 35652),
            Competitor(80741, "FK Kauno Zalgiris", "KAZ", "Kauno", "Lithuania", "LTU", 18229550),
            Competitor(6224, "FK Sutjeska Niksic", "SUN", "Sutjeska Niksic", "Montenegro", "MNE", 6672575),
            Competitor(5403, "FK Vardar Skopje", "VAR", "Vardar Skopje", "North Macedonia", "MKD", 116854),
            Competitor(3052, "Fenerbahce Istanbul", "FEN", "Fenerbahce", "Turkiye", "TUR", 32163),
            Competitor(2959, "Feyenoord Rotterdam", "FEY", "Feyenoord", "Netherlands", "NLD", 47227),
            Competitor(33584, "Floriana FC", "FLO", "Floriana", "Malta", "MLT", 15241919),
            Competitor(2032, "GNK Dinamo Zagreb", "DIN", "Dinamo Zagreb", "Croatia", "HRV", 224079),
            Competitor(3061, "Galatasaray Istanbul", "GAL", "Galatasaray", "Turkiye", "TUR", 6485),
            Competitor(3110, "Gornik Zabrze", "GOR", null, "Poland", "POL", 17248588),
            Competitor(5202, "Hapoel Be`er Sheva FC", "HBS", "Be`er Sheva", "Israel", "ISR", 7334522),
            Competitor(2353, "Heart of Midlothian FC", "HOM", "Heart of Midlothian", "Scotland", "SCO", 797738),
            Competitor(37946, "Inter Club de Escaldes", "ESC", "IC Escaldes", "Andorra", "AND", 15241843),
            Competitor(2697, "Inter Milano", "INT", "Inter", "Italy", "ITA", 122671),
            Competitor(372056, "KF Egnatia Rrogozhine", "EGR", "Egnatia Rrogozhine", "Albania", "ALB", 25573959),
            Competitor(5194, "KI Klaksvik", "KI", "Klaksvik", "Faroe Islands", "FRO", 15241891),
            Competitor(3121, "KKS Lech Poznan", "LPO", "Lech Poznan", "Poland", "POL", 5431242),
            Competitor(2244, "Kuopion Palloseura", "KUPS", null, "Finland", "FIN", 15241895),
            Competitor(2058, "LASK Linz", "ASK", "LASK", "Austria", "AUT", 12339638),
            Competitor(5184, "Larne FC", "LAR", "Larne", "Northern Ireland", "NIR", 18229130),
            Competitor(1643, "Lille OSC", "LIL", "Lille", "France", "FRA", 45752),
            Competitor(126304, "Lincoln Red Imps", "LIN", null, "Gibraltar", "GIB", 7294147),
            Competitor(44, "Liverpool FC", "LFC", "Liverpool", "England", "ENG", 45745),
            Competitor(17, "Manchester City", "MCI", null, "England", "ENG", 5567723),
            Competitor(35, "Manchester United", "MUN", null, "England", "ENG", 6632),
            Competitor(791232, "Maxline Vitebsk", "MAX", null, "Belarus", "BLR", 34397000),
            Competitor(1783, "Mjallby AIF", "MJA", "Mjallby", "Sweden", "SWE", 18559454),
            Competitor(2962, "NEC Nijmegen", "NIJ", "Nijmegen", "Netherlands", "NLD", 18802110),
            Competitor(2413, "NK Celje", "CEL", null, "Slovenia", "SVN", 15241893),
            Competitor(3245, "Olympiacos Piraeus", "OLY", "Olympiacos", "Greece", "GRC", 6496),
            Competitor(1649, "Olympique Lyon", "OL", "Lyon", "France", "FRA", 6599),
            Competitor(3273, "PFC Levski Sofia", "LEV", "Levski Sofia", "Bulgaria", "BGR", 33081),
            Competitor(2952, "PSV Eindhoven", "PSV", "Eindhoven", "Netherlands", "NLD", 6868),
            Competitor(1644, "Paris Saint-Germain", "PSG", "PSG", "France", "FRA", 6866),
            Competitor(167232, "Petrocub Hincesti", "PEH", null, "Moldova", "MDA", 18232934),
            Competitor(36360, "RB Leipzig", "RBL", "Leipzig", "Germany", "DEU", 9796666),
            Competitor(1648, "Racing Club De Lens", "RCL", "Lens", "France", "FRA", 127322),
            Competitor(2816, "Real Betis Seville", "RBB", "Real Betis", "Spain", "ESP", 542809),
            Competitor(2829, "Real Madrid", "RMA", null, "Spain", "ESP", 6494),
            Competitor(162368, "Riga FC", "RFC", null, "Latvia", "LVA", 12339641),
            Competitor(2404, "SK Slovan Bratislava", "SLO", "Slovan Bratislava", "Slovakia", "SVK", 1664178),
            Competitor(2051, "SK Sturm Graz", "STU", "Sturm Graz", "Austria", "AUT", 6484),
            Competitor(36195, "SP Tre Fiori", "TRF", "Tre Fiori", "San Marino", "SMR", 1666056),
            Competitor(2714, "SSC Napoli", "NAP", "Napoli", "Italy", "ITA", 5756773),
            Competitor(382568, "Sabah Masazir", "SBH", null, "Azerbaijan", "AZE", 32067607),
            Competitor(3177, "Shamrock Rovers", "SHA", "Shamrock", "Ireland", "IRL", 5756731),
            Competitor(2216, "Slavia Prague", "SLA", null, "Czechia", "CZE", 32602),
            Competitor(2218, "Sparta Prague", "SPA", null, "Czechia", "CZE", 6864),
            Competitor(3001, "Sporting CP", "SPO", null, "Portugal", "PRT", 6598),
            Competitor(3346, "Tallinna FC Flora", "TAF", "Tallinna", "Estonia", "EST", 116856),
            Competitor(4921, "The New Saints FC", "TNS", "New Saints", "Wales", "WAL", 554419),
            Competitor(4860, "Union Saint-Gilloise", "USG", "Union Gilloise", "Belgium", "BEL", 21489027),
            Competitor(2677, "VfB Stuttgart", "VFB", "Stuttgart", "Germany", "DEU", 224034),
            Competitor(1164, "Viking FK", "VIK", "Viking", "Norway", "NOR", 21763269),
            Competitor(1908, "Vikingur Reykjavik", "REY", null, "Iceland", "ISL", 21519383),
            Competitor(2819, "Villarreal CF", "VIL", "Villarreal", "Spain", "ESP", 322518),
            Competitor(1923, "WKW ETO FC Gyor", "GYO", "ETO Gyor", "Hungary", "HUN", 6661259),
        ];
    }

    private static Func<CompetitorBuilder> Competitor(int id, string name, string abbreviation, string shortName, string country, string countryCode, int betradarId)
    {
        return () =>
        {
            var competitor = new CompetitorBuilder()
                            .WithId(UrnCreate.CompetitorId(id))
                            .WithName(name)
                            .WithAbbreviation(abbreviation)
                            .WithCountry(country)
                            .WithCountryCode(countryCode)
                            .IsMale()
                            .WithReferences(new ReferencesBuilder().WithBetradar(betradarId));
            if (shortName != null)
            {
                competitor.WithShortName(shortName);
            }

            return competitor;
        };
    }

    private static DateTime ParseDate(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
