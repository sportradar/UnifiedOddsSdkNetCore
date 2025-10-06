// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public static class MarketDescriptionEndpoints
{
    public static desc_market GetMarketDescription312()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(312, "{!setnr} set - {!pointnr} point")
            .WithGroups("all|score|set")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("pointnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription313()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(313, "{!setnr} set - race to {pointnr} points")
            .WithGroups("all|score|set")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("pointnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription314()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(314, "Total sets")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription189()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(189, "Total games")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription190()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(190, "{$competitor1} total games")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription208()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(208, "{!setnr} set - race to {games} games")
            .WithGroups("all|score|set")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("games", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription209()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(209, "{!setnr} set - {!gamenrX} and {!gamenrY} game winner")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenrX", "integer")
            .AddSpecifier("gamenrY", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription210()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(210, "{!setnr} set game {gamenr} - winner")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription211()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(211, "{!setnr} set game {gamenr} - exact points")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("882").WithName("4"))
            .AddOutcome(o => o.WithId("883").WithName("5"))
            .AddOutcome(o => o.WithId("884").WithName("6"))
            .AddOutcome(o => o.WithId("885").WithName("7+"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription212()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(212, "{!setnr} set game {gamenr} - to deuce")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("74").WithName("yes"))
            .AddOutcome(o => o.WithId("76").WithName("no"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription213()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(213, "{!setnr} set game {gamenr} - odd/even points")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("70").WithName("odd"))
            .AddOutcome(o => o.WithId("72").WithName("even"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription214()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(214, "{!setnr} set game {gamenr} - correct score")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("886").WithName("{$competitor1} to 0"))
            .AddOutcome(o => o.WithId("887").WithName("{$competitor1} to 15"))
            .AddOutcome(o => o.WithId("888").WithName("{$competitor1} to 30"))
            .AddOutcome(o => o.WithId("889").WithName("{$competitor1} to 40"))
            .AddOutcome(o => o.WithId("890").WithName("{$competitor2} to 0"))
            .AddOutcome(o => o.WithId("891").WithName("{$competitor2} to 15"))
            .AddOutcome(o => o.WithId("892").WithName("{$competitor2} to 30"))
            .AddOutcome(o => o.WithId("893").WithName("{$competitor2} to 40"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription215()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(215, "{!setnr} set game {gamenr} - correct score or break")
            .WithGroups("all|score|game")
            .AddOutcome(o => o.WithId("894").WithName("{%server} to 0"))
            .AddOutcome(o => o.WithId("895").WithName("{%server} to 15"))
            .AddOutcome(o => o.WithId("896").WithName("{%server} to 30"))
            .AddOutcome(o => o.WithId("897").WithName("{%server} to 40"))
            .AddOutcome(o => o.WithId("898").WithName("break"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .AddSpecifier("server", "string")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription216()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(216, "{!setnr} set game {gamenr} - race to {pointnr} points")
            .WithGroups("all|score|points")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .AddSpecifier("pointnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription217()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(217, "{!setnr} set game {gamenr} - first {pointnr} points winner")
            .WithGroups("all|score|points")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .AddSpecifier("pointnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription218()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(218, "{!setnr} set game {gamenr} - {!pointnr} point")
            .WithGroups("all|score|points")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .AddSpecifier("setnr", "integer")
            .AddSpecifier("gamenr", "integer")
            .AddSpecifier("pointnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription1()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(1, "1x2")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription7()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(7, "Which team wins the rest of the match")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .AddSpecifier("score", "string", "current score in match")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription8()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(8, "{!goalnr} goal")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("6").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("7").WithName("none"))
            .AddOutcome(o => o.WithId("8").WithName("{$competitor2}"))
            .AddSpecifier("goalnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription10()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(10, "Double chance")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("9").WithName("{$competitor1} or draw"))
            .AddOutcome(o => o.WithId("10").WithName("{$competitor1} or {$competitor2}"))
            .AddOutcome(o => o.WithId("11").WithName("draw or {$competitor2}"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription11()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(11, "Draw no bet")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("4").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("5").WithName("{$competitor2}"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription14()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(14, "Handicap {hcp}")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("1711").WithName("{$competitor1} ({hcp})"))
            .AddOutcome(o => o.WithId("1712").WithName("draw ({hcp})"))
            .AddOutcome(o => o.WithId("1713").WithName("{$competitor2} ({hcp})"))
            .AddSpecifier("hcp", "string")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription18()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(18, "Total")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription19()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(19, "{$competitor1} total")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription20()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(20, "{$competitor2} total")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription21()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(21, "Exact goals")
            .WithGroups("all|score|regular_play")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription23()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(23, "{$competitor1} exact goals")
            .WithGroups("all|score|regular_play")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription24()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(24, "{$competitor2} exact goals")
            .WithGroups("all|score|regular_play")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription26()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(26, "Odd/even")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("70").WithName("odd"))
            .AddOutcome(o => o.WithId("72").WithName("even"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription29()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(29, "Both teams to score")
            .WithGroups("all|score|regular_play")
            .AddOutcome(o => o.WithId("74").WithName("yes"))
            .AddOutcome(o => o.WithId("76").WithName("no"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription37()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(37, "1x2 & total")
            .WithGroups("all|regular_play|combo|incl_ot")
            .AddOutcome(o => o.WithId("794").WithName("{$competitor1} & under {total}"))
            .AddOutcome(o => o.WithId("796").WithName("{$competitor1} & over {total}"))
            .AddOutcome(o => o.WithId("798").WithName("draw & under {total}"))
            .AddOutcome(o => o.WithId("800").WithName("draw & over {total}"))
            .AddOutcome(o => o.WithId("802").WithName("{$competitor2} & under {total}"))
            .AddOutcome(o => o.WithId("804").WithName("{$competitor2} & over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription60()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(60, "1st half - 1x2")
            .WithGroups("all|score|1st_half")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .Build();

        marketDescription.specifiers = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription61()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(61, "1st half - which team wins the rest")
            .WithGroups("all|score|1st_half")
            .AddOutcome(o => o.WithId("1").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("2").WithName("draw"))
            .AddOutcome(o => o.WithId("3").WithName("{$competitor2}"))
            .AddSpecifier("score", "string", "current score in match")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription62()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(62, "1st half - {!goalnr} goal")
            .WithGroups("all|score|1st_half")
            .AddOutcome(o => o.WithId("6").WithName("{$competitor1}"))
            .AddOutcome(o => o.WithId("7").WithName("none"))
            .AddOutcome(o => o.WithId("8").WithName("{$competitor2}"))
            .AddSpecifier("goalnr", "integer")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription68()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(68, "1st half - total")
            .WithGroups("all|score|1st_half")
            .AddOutcome(o => o.WithId("13").WithName("under {total}"))
            .AddOutcome(o => o.WithId("12").WithName("over {total}"))
            .AddSpecifier("total", "decimal")
            .Build();

        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription71()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(71, "1st half - exact goals")
            .WithGroups("all|score|1st_half")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription72()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(72, "1st half - {$competitor1} exact goals")
            .WithGroups("all|score|1st_half")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }

    public static desc_market GetMarketDescription73()
    {
        var marketDescription = MarketDescriptionBuilder
            .Create(73, "1st half - {$competitor2} exact goals")
            .WithGroups("all|score|1st_half")
            .AddSpecifier("variant", "variable_text")
            .Build();

        marketDescription.outcomes = [];
        marketDescription.mappings = [];
        marketDescription.attributes = [];

        return marketDescription;
    }
}
