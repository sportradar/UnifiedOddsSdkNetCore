// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

/// <summary>
/// Market descriptions definitions for an invariant market description list
/// </summary>
public static class MdForInvariantList
{
    /// <summary>
    /// Get a normal market with one specifier
    /// </summary>
    /// <example>
    /// <market id="282" name="Innings 1 to 5th top - {$competitor1} total" groups="all|score|4.5_innings">
    /// 	<outcomes>
    /// 		<outcome id="13" name="under {total}"/>
    /// 		<outcome id="12" name="over {total}"/>
    /// 	</outcomes>
    /// 	<specifiers>
    /// 		<specifier name="total" type="decimal"/>
    /// 	</specifiers>
    /// 	<mappings>
    /// 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:3" market_id="8:232" sov_template="{total}">
    /// 			<mapping_outcome outcome_id="13" product_outcome_id="2528" product_outcome_name="under"/>
    /// 			<mapping_outcome outcome_id="12" product_outcome_id="2530" product_outcome_name="over"/>
    /// 		</mapping>
    /// 	</mappings>
    /// </market>
    /// </example>
    /// <returns>Return normal market with one specifier</returns>
    public static desc_market GetMarket282WithCompetitorInMarketName()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(282)
                                       .WithName("Innings 1 to 5th top - {$competitor1} total")
                                       .WithGroups("all|score|4.5_innings")
                                       .AddOutcome(OutcomeDescriptionBuilder.Create().WithId("13").WithName("under {total}").BuildInvariant())
                                       .AddOutcome(OutcomeDescriptionBuilder.Create().WithId("12").WithName("over {total}").BuildInvariant())
                                       .AddSpecifier("total", "decimal")
                                       .AddMapping(builder => builder.WithProductId(1)
                                                                     .WithProductIds("1|4")
                                                                     .WithSportId("sr:sport:3")
                                                                     .WithMarketId("8:232")
                                                                     .WithSovTemplate("{total}")
                                                                     .AddInvariantMappingOutcome("13", "2528", "under")
                                                                     .AddInvariantMappingOutcome("12", "2530", "over")
                                                                     .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Free-text outright markets are market 534 (Championship), 535 (Short-term), 536 (Multi-winner) and 559. These markets have a variant specifier that describes the outright using the normal variant specifier logic.
    /// They do require a single variant market description to be present to get the possible outcomes.
    /// The market description has "include_outcomes_of_type" set to "pre:outcometext" to indicate that the outcomes are free-text outcomes.
    ///
    /// Outright markets are markets that have the “outcome_type” attribute set to “pre:outcometext”. This helps to identify the outright markets.
    /// </summary>
    /// <returns>Invariant market description</returns>
    public static desc_market GetPreOutcomeTextMarket534()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(534)
                                       .WithName("Championship free text market")
                                       .WithGroups("all")
                                       .WithIncludesOutcomesOfType("pre:outcometext")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("all")
                                                             .WithMarketId("30")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Free-text outright markets are market 534 (Championship), 535 (Short-term), 536 (Multi-winner) and 559. These markets have a variant specifier that describes the outright using the normal variant specifier logic.
    /// They do require a single variant market description to be present to get the possible outcomes.
    /// The market description has "include_outcomes_of_type" set to "pre:outcometext" to indicate that the outcomes are free-text outcomes.
    ///
    /// Outright markets are markets that have the “outcome_type” attribute set to “pre:outcometext”. This helps to identify the outright markets.
    /// </summary>
    /// <returns>Invariant market description</returns>
    public static desc_market GetPreOutcomeTextMarket535()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(535)
                                       .WithName("Short term free text market")
                                       .WithGroups("all")
                                       .WithIncludesOutcomesOfType("pre:outcometext")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("all")
                                                             .WithMarketId("40"))
                                       .Build();
    }

    /// <summary>
    /// Free-text outright markets are market 534 (Championship), 535 (Short-term), 536 (Multi-winner) and 559. These markets have a variant specifier that describes the outright using the normal variant specifier logic.
    /// They do require a single variant market description to be present to get the possible outcomes.
    /// The market description has "include_outcomes_of_type" set to "pre:outcometext" to indicate that the outcomes are free-text outcomes.
    ///
    /// Outright markets are markets that have the “outcome_type” attribute set to “pre:outcometext”. This helps to identify the outright markets.
    /// </summary>
    /// <returns>Invariant market description</returns>
    public static desc_market GetPreOutcomeTextMarket536()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(536)
                                       .WithName("Free text multiwinner market")
                                       .WithGroups("all")
                                       .WithIncludesOutcomesOfType("pre:outcometext")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddSpecifier("winners", "integer", "number of winners")
                                       .AddMapping(builder => builder.WithProductId(3).WithProductIds("3").WithSportId("all").WithMarketId("50").WithValidFor("winners=3").BuildInvariant())
                                       .AddMapping(builder => builder.WithProductId(3).WithProductIds("3").WithSportId("all").WithMarketId("80").BuildInvariant())
                                       .Build();
    }

    public static desc_market GetMarket701WithSpecifierInMarketName()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(701)
                                       .WithName("Any player to score {milestone}")
                                       .WithGroups("all")
                                       .AddOutcome(builder => builder.WithId("74").WithName("yes").BuildInvariant())
                                       .AddOutcome(builder => builder.WithId("76").WithName("no").BuildInvariant())
                                       .AddSpecifier("milestone", "integer")
                                       .AddSpecifier("maxovers", "integer")
                                       .AddMapping(builder => builder.WithProductId(5).WithProductIds("5").WithSportId("sr:sport:21").WithMarketId("61").BuildInvariant())
                                       .Build();
    }

    public static desc_market GetPlayerPropsMarket768()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(768)
                                       .WithName("Player points (incl. overtime)")
                                       .WithGroups("all|player_props")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:2")
                                                             .WithMarketId("594")
                                                             .BuildInvariant())
                                       .Build();
    }

    public static desc_market GetFakePlayerPropsMarket1768()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(1768)
                                       .WithName("Player points (incl. overtime)")
                                       .WithGroups("all|competitor_props")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:2")
                                                             .WithMarketId("594")
                                                             .BuildInvariant())
                                       .Build();
    }

    public static desc_market GetOrdinalMarket739()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(739)
                                       .WithName("When will the {!runnr} run be scored (incl. extra innings)")
                                       .WithGroups("all|score|incl_ei")
                                       .AddOutcome(builder => builder.WithId("1826").WithName("{!inningnr} inning").BuildInvariant())
                                       .AddOutcome(builder => builder.WithId("1828").WithName("{!(inningnr+1)} inning").BuildInvariant())
                                       .AddOutcome(builder => builder.WithId("1829").WithName("{!(inningnr+2)} inning").BuildInvariant())
                                       .AddOutcome(builder => builder.WithId("1830").WithName("other inning or no run").BuildInvariant())
                                       .AddSpecifier("inningnr", "integer")
                                       .AddSpecifier("runnr", "integer")
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:3")
                                                             .WithMarketId("8:1620")
                                                             .WithSovTemplate("{inningnr}/{runnr}")
                                                             .AddInvariantMappingOutcome("1826", "7485", "[inningNr] inning")
                                                             .AddInvariantMappingOutcome("1828", "7486", "[inningNr+1] inning")
                                                             .AddInvariantMappingOutcome("1829", "7487", "[inningNr+2] inning")
                                                             .AddInvariantMappingOutcome("1830", "7488", "other inning or no run")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:3")
                                                             .WithMarketId("809")
                                                             .WithValidFor("inningnr=1|runnr=1")
                                                             .AddInvariantMappingOutcome("1826", "871", "1st inning")
                                                             .AddInvariantMappingOutcome("1828", "872", "2nd inning")
                                                             .AddInvariantMappingOutcome("1829", "873", "3rd inning")
                                                             .AddInvariantMappingOutcome("1830", "259", "other")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Market where outcomes generated from associated players
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetLastGoalscorerMarket39()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(39)
                                       .WithName("Last goalscorer")
                                       .WithGroups("all|regular_play|scorers")
                                       .WithIncludesOutcomesOfType("sr:player")
                                       .AddOutcome(builder => builder
                                                             .WithId("1716")
                                                             .WithName("no goal")
                                                             .BuildInvariant())
                                       .AddSpecifier("type", "string")
                                       .AddMapping(builder => builder
                                           .WithProductId(3)
                                           .WithProductIds("3")
                                           .WithSportId("sr:sport:1")
                                           .WithMarketId("389")
                                           .WithValidFor("type=prematch")
                                           .AddInvariantMappingOutcome("1716", "114", "No goal")
                                           .BuildInvariant())
                                       .Build();
    }

    public static desc_market GetPlayerToScoreIncludingOvertimeMarket882()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(882)
            .WithName("{%player} to score (incl. overtime)")
            .WithGroups("all|scorers|incl_ot")
            .AddOutcome(builder => builder.WithId("74").WithName("yes").BuildInvariant())
            .AddOutcome(builder => builder.WithId("76").WithName("no").BuildInvariant())
            .AddSpecifier("player", "string")
            .AddMapping(builder => builder
                .WithProductId(1)
                .WithProductIds("1|4")
                .WithSportId("sr:sport:1")
                .WithMarketId("8:1686")
                .WithSovTemplate("{player}")
                .AddInvariantMappingOutcome("74", "7657", "yes")
                .AddInvariantMappingOutcome("76", "7658", "no")
                .BuildInvariant())
            .AddMapping(builder => builder
                .WithProductId(3)
                .WithProductIds("3")
                .WithSportId("sr:sport:1")
                .WithMarketId("724")
                .WithSovTemplate("{player}")
                .AddInvariantMappingOutcome("74", "4", "yes")
                .AddInvariantMappingOutcome("76", "5", "No")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Get base market definition which requires variant from a list of variants
    /// This market definition lacks mappings (there are a lot of them)
    /// </summary>
    /// <returns>Returns base market definition which requires a variant from a list of variants</returns>
    public static desc_market GetMarket199ForVariantList()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(199)
                                       .WithName("Correct score")
                                       .WithGroups("all|score|regular_play")
                                       .AddSpecifier("variant", "variable_text")
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market with score in name template
    /// This market definition lacks mappings (there are a lot of them)
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket41WithScoreSpecifier()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(41)
                                       .WithName("Correct score [{score}]")
                                       .WithGroups("all|score|regular_play")
                                       .AddOutcome(b => b.WithId("110").WithName("0:0"))
                                       .AddOutcome(b => b.WithId("114").WithName("1:0"))
                                       .AddOutcome(b => b.WithId("116").WithName("2:0"))
                                       .AddOutcome(b => b.WithId("118").WithName("3:0"))
                                       .AddOutcome(b => b.WithId("120").WithName("4:0"))
                                       .AddOutcome(b => b.WithId("122").WithName("5:0"))
                                       .AddOutcome(b => b.WithId("124").WithName("6:0"))
                                       .AddOutcome(b => b.WithId("126").WithName("0:1"))
                                       .AddOutcome(b => b.WithId("128").WithName("1:1"))
                                       .AddOutcome(b => b.WithId("130").WithName("2:1"))
                                       .AddOutcome(b => b.WithId("132").WithName("3:1"))
                                       .AddOutcome(b => b.WithId("134").WithName("4:1"))
                                       .AddOutcome(b => b.WithId("136").WithName("5:1"))
                                       .AddOutcome(b => b.WithId("138").WithName("0:2"))
                                       .AddOutcome(b => b.WithId("140").WithName("1:2"))
                                       .AddOutcome(b => b.WithId("142").WithName("2:2"))
                                       .AddOutcome(b => b.WithId("144").WithName("3:2"))
                                       .AddOutcome(b => b.WithId("146").WithName("4:2"))
                                       .AddOutcome(b => b.WithId("148").WithName("0:3"))
                                       .AddOutcome(b => b.WithId("150").WithName("1:3"))
                                       .AddOutcome(b => b.WithId("152").WithName("2:3"))
                                       .AddOutcome(b => b.WithId("154").WithName("3:3"))
                                       .AddOutcome(b => b.WithId("156").WithName("0:4"))
                                       .AddOutcome(b => b.WithId("158").WithName("1:4"))
                                       .AddOutcome(b => b.WithId("160").WithName("2:4"))
                                       .AddOutcome(b => b.WithId("162").WithName("0:5"))
                                       .AddOutcome(b => b.WithId("164").WithName("1:5"))
                                       .AddOutcome(b => b.WithId("166").WithName("0:6"))
                                       .AddSpecifier("score", "string", "current score in match")
                                       .AddAttribute("is_flex_score", "Outcomes should be adjusted according to score specifier")
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market with variant specifier found in the variant list
    /// This market definition lacks mappings (there are a lot of them)
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket374WithVariant()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(374)
                                       .WithName("{!setnr} set - correct score (in legs)")
                                       .WithGroups("all|score|set")
                                       .AddSpecifier("setnr", "integer")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:22")
                                                             .WithMarketId("8:427")
                                                             .WithSovTemplate("{setnr}")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market with variant specifier found in the variant list
    /// Some markets and outcomes have variable descriptions.
    /// This is, for example, the case for many outright markets and outcomes, as well as some dynamic markets – most importantly, the correct score market, but also some cricket markets.
    /// These markets have a special specifier variant, and this specifier is set in the odds_change message to an urn that looks like this: pre:markettext:1234.
    ///
    /// Note that the same market can have different market descriptions at different times if the variant-specifier changes.
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket239WithGenericNameVariant()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(239)
                                       .WithName("How many games will be decided by extra points")
                                       .WithGroups("all|score|regular_play")
                                       .AddSpecifier("variant", "variable_text")
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:20")
                                                             .WithMarketId("8:361")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:7")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:59", "3179", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:61", "3181", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:62", "3183", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:63", "3185", "3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:64", "3187", "4")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:65", "3189", "5")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:66", "3191", "6")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:7:67", "3193", "7")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:20")
                                                             .WithMarketId("8:68")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:5")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:31")
                                                             .WithMarketId("8:68")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:48", "219", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:50", "220", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:51", "221", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:52", "222", "3")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:31")
                                                             .WithMarketId("8:68")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:5")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:37")
                                                             .WithMarketId("8:68")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:48", "219", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:50", "220", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:51", "221", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:52", "222", "3")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(1)
                                                             .WithProductIds("1|4")
                                                             .WithSportId("sr:sport:37")
                                                             .WithMarketId("8:68")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:5")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:31")
                                                             .WithMarketId("556")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:48", "260", "0 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:50", "261", "1 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:51", "98", "2 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:52", "99", "3 sets")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:37")
                                                             .WithMarketId("556")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:3")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:48", "260", "0 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:50", "261", "1 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:51", "98", "2 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:3:52", "99", "3 sets")
                                                             .BuildInvariant())
                                       .AddMapping(builder => builder
                                                             .WithProductId(3)
                                                             .WithProductIds("3")
                                                             .WithSportId("sr:sport:37")
                                                             .WithMarketId("557")
                                                             .WithValidFor("variant=sr:decided_by_extra_points:bestof:5")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "260", "0 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "261", "1 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "98", "2 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "99", "3 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "100", "4 sets")
                                                             .AddInvariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "101", "5 sets")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market associated with competitor
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket1109ForCompetitorOutcomeType()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(1109)
                                       .WithName("{!lapnr} lap - fastest lap")
                                       .WithGroups("all")
                                       .WithIncludesOutcomesOfType("sr:competitor")
                                       .WithOutcomeType("competitor")
                                       .AddSpecifier("lapnr", "integer", "lap number")
                                       .AddMapping(builder => builder
                                                             .WithProductId(14)
                                                             .WithProductIds("14")
                                                             .WithSportId("sr:sport:40")
                                                             .WithMarketId("1109")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market associated with player
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket676ForPlayerOutcomeType()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(679)
                                       .WithName("{!inningnr} innings - {$competitor2} last player standing")
                                       .WithGroups("all")
                                       .WithIncludesOutcomesOfType("sr:player")
                                       .AddSpecifier("inningnr", "integer", "inning number")
                                       .AddSpecifier("maxovers", "integer", "max overs")
                                       .AddMapping(builder => builder
                                                             .WithProductId(5)
                                                             .WithProductIds("5")
                                                             .WithSportId("sr:sport:21")
                                                             .WithMarketId("38")
                                                             .BuildInvariant())
                                       .Build();
    }

    /// <summary>
    /// Get market definition for market with competitor in name template
    /// This market definition lacks mappings (there are a lot of them)
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket303WithCompetitorReferenceInOutcome()
    {
        return MarketDescriptionBuilder.Create()
                                       .WithId(303)
                                       .WithName("{!quarternr} quarter - handicap")
                                       .WithGroups("all|score|quarter")
                                       .AddOutcome(b => b.WithId("1714").WithName("{$competitor1} ({+hcp})").BuildInvariant())
                                       .AddOutcome(b => b.WithId("1715").WithName("{$competitor2} ({-hcp})").BuildInvariant())
                                       .AddSpecifier("quarternr", "integer")
                                       .AddSpecifier("hcp", "decimal")
                                       .Build();
    }

    /// <summary>
    /// Get the flex market with id 118
    /// These are only offered live and are an alternative to correct score markets
    /// These markets are displayed in the market description endpoint by having an extra attribute element with the name is_flex_score.
    /// For these markets, the proper outcome names are created by taking the current score and “adding” the listed outcome name to that score.
    /// For example,
    /// If the current score is 5:3 and the outcome name is 1:1.
    /// Betting on that outcome means betting on an outcome of 6:4.
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetFlexScoreMarket118()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(118)
            .WithName("Overtime - correct score [{score}]")
            .WithGroups("all|score|ot")
            .AddOutcome(b => b.WithId("442").WithName("0:0").BuildInvariant())
            .AddOutcome(b => b.WithId("444").WithName("1:0").BuildInvariant())
            .AddOutcome(b => b.WithId("446").WithName("2:0").BuildInvariant())
            .AddOutcome(b => b.WithId("448").WithName("3:0").BuildInvariant())
            .AddOutcome(b => b.WithId("450").WithName("0:1").BuildInvariant())
            .AddOutcome(b => b.WithId("452").WithName("1:1").BuildInvariant())
            .AddOutcome(b => b.WithId("454").WithName("2:1").BuildInvariant())
            .AddOutcome(b => b.WithId("456").WithName("0:2").BuildInvariant())
            .AddOutcome(b => b.WithId("458").WithName("1:2").BuildInvariant())
            .AddOutcome(b => b.WithId("460").WithName("0:3").BuildInvariant())
            .AddSpecifier("score", "string", "current score in match")
            .AddMapping(builder => builder
                .WithProductId(1)
                .WithProductIds("1|4")
                .WithSportId("sr:sport:1")
                .WithMarketId("8:116")
                .WithSovTemplate("{score}")
                .AddInvariantMappingOutcome("442", "339", "0:0")
                .AddInvariantMappingOutcome("444", "340", "1:0")
                .AddInvariantMappingOutcome("446", "341", "2:0")
                .AddInvariantMappingOutcome("448", "342", "3:0")
                .AddInvariantMappingOutcome("450", "343", "0:1")
                .AddInvariantMappingOutcome("452", "344", "1:1")
                .AddInvariantMappingOutcome("454", "345", "2:1")
                .AddInvariantMappingOutcome("456", "346", "0:2")
                .AddInvariantMappingOutcome("458", "347", "1:2")
                .AddInvariantMappingOutcome("460", "348", "0:3")
                .BuildInvariant())
            .AddAttribute("is_flex_score", "Outcomes should be adjusted according to score specifier")
            .Build();
    }

    /// <summary>
    /// Goalscorer markets refer to markets that specifically deal with goal scoring.
    /// Market versions for the first and last goalscorer markets are available.
    /// These will work the same way as outcome versions for the outright markets.
    /// When a new version is generated, the old version will be invalidated.
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetGoalScorerMarket892WithVersion()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(892)
            .WithName("{!goalnr} goalscorer")
            .WithGroups("all|regular_play|scorers")
            .WithIncludesOutcomesOfType("sr:player")
            .AddSpecifier("variant", "variable_text")
            .AddSpecifier("goalnr", "integer")
            .AddSpecifier("version", "string")
            .AddMapping(builder => builder
                .WithProductId(1)
                .WithProductIds("1|4")
                .WithSportId("sr:sport:1")
                .WithMarketId("8:1685")
                .WithSovTemplate("{goalnr}/{version}")
                .WithValidFor("variant=sr:goalscorer:fieldplayers_nogoal_owngoal_other")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1333", "7707", "none")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1334", "7708", "own goal")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1335", "7709", "other")
                .BuildInvariant())
            .AddMapping(builder => builder
                .WithProductId(3)
                .WithProductIds("3")
                .WithSportId("sr:sport:1")
                .WithMarketId("734")
                .WithValidFor("variant=sr:goalscorer:fieldplayers_nogoal_owngoal_other|goalnr=1")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1333", "-1", "None")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1334", "-3", "Own goal")
                .AddInvariantMappingOutcome("sr:goalscorer:fieldplayers_nogoal_owngoal_other:1335", "-2", "Others")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Each three-ball is 3-way market with the outcomes being the different players/competitors. Each outcome’s id is the golf-player/competitor id for that outcome.
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetGolfThreeBallsMarket1022()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(1022)
            .WithName("Hole {holenr} - 3 ball")
            .WithGroups("all")
            .WithIncludesOutcomesOfType("sr:competitor")
            .WithOutcomeType("competitor")
            .AddSpecifier("holenr", "integer")
            .AddMapping(builder => builder
                .WithProductId(14)
                .WithProductIds("14")
                .WithSportId("sr:sport:9")
                .WithMarketId("1022")
                .BuildInvariant())
            .AddAttribute("is_golf_stroke_play_market", "This market is applicable to Golf stroke play")
            .Build();
    }

    /// <summary>
    /// Head2Head markets are ordinary markets, with the two head2head competitors being the two outcomes. Each outcome’s id is the competitor id for that outcome.
    /// Head2head markets are currently associated with a broad sr:simple_tournament that is used for all head2head offers for the same sport no matter what actual race it is (just like the LCoO feed works today).
    /// Note: This will change, and the head2head markets will be associated directly with the exact race-event.
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetHead2HeadMarket1103()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(1103)
            .WithName("Head2head (teams)")
            .WithGroups("all")
            .WithIncludesOutcomesOfType("sr:competitor")
            .WithOutcomeType("competitor")
            .AddSpecifier("id", "integer")
            .AddMapping(builder => builder
                .WithProductId(14)
                .WithProductIds("14")
                .WithSportId("sr:sport:40")
                .WithMarketId("1103")
                .WithSovTemplate("{id}")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Market with player reference in name from specifier
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket915WithPlayerInNameFromSpecifier()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(915)
            .WithName("{%player} total pass completions (incl. overtime)")
            .WithGroups("all")
            .AddOutcome(b => b.WithId("13").WithName("under {total}").BuildInvariant())
            .AddOutcome(b => b.WithId("12").WithName("over {total}").BuildInvariant())
            .AddSpecifier("player", "string")
            .AddSpecifier("total", "decimal")
            .AddMapping(builder => builder
                .WithProductId(3)
                .WithProductIds("3")
                .WithSportId("sr:sport:16")
                .WithMarketId("664")
                .WithSovTemplate("{player}/{total}")
                .AddInvariantMappingOutcome("13", "7", "Under")
                .AddInvariantMappingOutcome("12", "6", "Over")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Market with player reference in outcome name from specifier
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket717WithPlayerInOutcomeNameFromSpecifier()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(717)
            .WithName("Batter head2head (handicap)")
            .WithGroups("all")
            .AddOutcome(b => b.WithId("1816").WithName("{%player1} ({+hcp})").BuildInvariant())
            .AddOutcome(b => b.WithId("1817").WithName("{%player2} ({-hcp})").BuildInvariant())
            .AddSpecifier("player1", "string")
            .AddSpecifier("player2", "string")
            .AddSpecifier("hcp", "decimal")
            .AddMapping(builder => builder
                .WithProductId(5)
                .WithProductIds("5")
                .WithSportId("sr:sport:21")
                .WithMarketId("83")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Market with player in name without a player id reference in specifier
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetMarket1140WithPlayerInNameWithoutSpecifier()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(1140)
            .WithName("{!inningnr} innings - {%player1} or {%player2} dismissal method")
            .WithGroups("all")
            .AddOutcome(b => b.WithId("1806").WithName("fielder catch").BuildInvariant())
            .AddOutcome(b => b.WithId("1807").WithName("bowled").BuildInvariant())
            .AddOutcome(b => b.WithId("1808").WithName("keeper catch").BuildInvariant())
            .AddOutcome(b => b.WithId("1809").WithName("lbw").BuildInvariant())
            .AddOutcome(b => b.WithId("1810").WithName("run out").BuildInvariant())
            .AddOutcome(b => b.WithId("1811").WithName("stumped").BuildInvariant())
            .AddOutcome(b => b.WithId("1812").WithName("other").BuildInvariant())
            .AddMapping(builder => builder
                .WithProductId(5)
                .WithProductIds("5")
                .WithSportId("sr:sport:21")
                .WithMarketId("142")
                .BuildInvariant())
            .Build();
    }

    /// <summary>
    /// Market where outcomes generated from associated players
    /// </summary>
    /// <returns>Returns market definition</returns>
    public static desc_market GetGoalScorerMarket40()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(40)
            .WithName("Anytime goalscorer")
            .WithGroups("all|regular_play|scorers")
            .WithIncludesOutcomesOfType("sr:player")
            .AddOutcome(b => b.WithId("1716").WithName("no goal").BuildInvariant())
            .AddSpecifier("type", "string")
            .AddMapping(builder => builder
                .WithProductId(1)
                .WithProductIds("1|4")
                .WithSportId("sr:sport:1")
                .WithMarketId("8:146")
                .WithValidFor("type=live")
                .AddInvariantMappingOutcome("1716", "467", "none")
                .BuildInvariant())
            .AddMapping(builder => builder
                .WithProductId(3)
                .WithProductIds("3")
                .WithSportId("sr:sport:1")
                .WithMarketId("235")
                .WithValidFor("type=prematch")
                .AddInvariantMappingOutcome("1716", "114", "No goal")
                .BuildInvariant())
            .Build();
    }
}
