// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

/// <summary>
/// Market descriptions definitions for variant market description list
/// </summary>
public static class MdForVariantList
{
    public static desc_variant GetCorrectScoreBestOf12()
    {
        //<variant id="sr:correct_score:bestof:12">
        // 	<outcomes>
        // 		<outcome id="sr:correct_score:bestof:12:192" name="7:0"/>
        // 		<outcome id="sr:correct_score:bestof:12:193" name="7:1"/>
        // 		<outcome id="sr:correct_score:bestof:12:194" name="7:2"/>
        // 		<outcome id="sr:correct_score:bestof:12:195" name="7:3"/>
        // 		<outcome id="sr:correct_score:bestof:12:196" name="7:4"/>
        // 		<outcome id="sr:correct_score:bestof:12:197" name="7:5"/>
        // 		<outcome id="sr:correct_score:bestof:12:198" name="6:6"/>
        // 		<outcome id="sr:correct_score:bestof:12:199" name="5:7"/>
        // 		<outcome id="sr:correct_score:bestof:12:200" name="4:7"/>
        // 		<outcome id="sr:correct_score:bestof:12:201" name="3:7"/>
        // 		<outcome id="sr:correct_score:bestof:12:202" name="2:7"/>
        // 		<outcome id="sr:correct_score:bestof:12:203" name="1:7"/>
        // 		<outcome id="sr:correct_score:bestof:12:204" name="0:7"/>
        // 	</outcomes>
        // 	<mappings>
        // 		<mapping product_id="3" product_ids="3" sport_id="sr:sport:22" market_id="374" product_market_id="455">
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:192" product_outcome_id="33" product_outcome_name="7:0"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:193" product_outcome_id="34" product_outcome_name="7:1"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:194" product_outcome_id="318" product_outcome_name="7:2"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:195" product_outcome_id="320" product_outcome_name="7:3"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:196" product_outcome_id="322" product_outcome_name="7:4"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:197" product_outcome_id="116" product_outcome_name="7:5"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:198" product_outcome_id="602" product_outcome_name="6:6"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:199" product_outcome_id="121" product_outcome_name="5:7"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:200" product_outcome_id="323" product_outcome_name="4:7"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:201" product_outcome_id="321" product_outcome_name="3:7"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:202" product_outcome_id="319" product_outcome_name="2:7"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:203" product_outcome_id="57" product_outcome_name="1:7"/>
        // 			<mapping_outcome outcome_id="sr:correct_score:bestof:12:204" product_outcome_id="56" product_outcome_name="0:7"/>
        // 		</mapping>
        // 	</mappings>
        // </variant>
        return VariantDescriptionBuilder.Create()
            .WithId("sr:correct_score:bestof:12")
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:192").WithName("7:0"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:193").WithName("7:1"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:194").WithName("7:2"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:195").WithName("7:3"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:196").WithName("7:4"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:197").WithName("7:5"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:198").WithName("6:6"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:199").WithName("5:7"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:200").WithName("4:7"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:201").WithName("3:7"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:202").WithName("2:7"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:203").WithName("1:7"))
            .AddOutcome(builder => builder.WithId("sr:correct_score:bestof:12:204").WithName("0:7"))
            .AddMapping(builder => builder
                .WithProductId(3)
                .WithProductIds("3")
                .WithSportId("sr:sport:22")
                .WithMarketId("374")
                .WithProductMarketId("455")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:192", "33", "7:0")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:193", "34", "7:1")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:194", "318", "7:2")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:195", "320", "7:3")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:196", "322", "7:4")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:197", "116", "7:5")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:198", "602", "6:6")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:199", "121", "5:7")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:200", "323", "4:7")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:201", "321", "3:7")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:202", "319", "2:7")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:203", "57", "1:7")
                .AddVariantMappingOutcome("sr:correct_score:bestof:12:204", "56", "0:7")
                .BuildVariant()
            )
            .Build();
    }

    public static desc_variant GetDecidedByExtraPointsBestOf5()
    {
        // <variant id="sr:decided_by_extra_points:bestof:5">
        // 	<outcomes>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:53" name="0"/>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:54" name="1"/>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:55" name="2"/>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:56" name="3"/>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:57" name="4"/>
        // 		<outcome id="sr:decided_by_extra_points:bestof:5:58" name="5"/>
        // 	</outcomes>
        // 	<mappings>
        // 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:20" market_id="239" product_market_id="8:68">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
        // 		</mapping>
        // 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:37" market_id="239" product_market_id="8:68">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
        // 		</mapping>
        // 		<mapping product_id="3" product_ids="3" sport_id="sr:sport:23" market_id="306" product_market_id="338">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="260" product_outcome_name="0 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="261" product_outcome_name="1 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="98" product_outcome_name="2 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="99" product_outcome_name="3 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="100" product_outcome_name="4 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="101" product_outcome_name="5 sets"/>
        // 		</mapping>
        // 		<mapping product_id="3" product_ids="3" sport_id="sr:sport:37" market_id="239" product_market_id="557">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="260" product_outcome_name="0 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="261" product_outcome_name="1 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="98" product_outcome_name="2 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="99" product_outcome_name="3 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="100" product_outcome_name="4 sets"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="101" product_outcome_name="5 sets"/>
        // 		</mapping>
        // 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:23" market_id="306" product_market_id="8:68">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
        // 		</mapping>
        // 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:34" market_id="306" product_market_id="8:68">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
        // 		</mapping>
        // 		<mapping product_id="1" product_ids="1|4" sport_id="sr:sport:31" market_id="239" product_market_id="8:68">
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:53" product_outcome_id="219" product_outcome_name="0"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:54" product_outcome_id="220" product_outcome_name="1"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:55" product_outcome_id="221" product_outcome_name="2"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:56" product_outcome_id="222" product_outcome_name="3"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:57" product_outcome_id="223" product_outcome_name="4"/>
        // 			<mapping_outcome outcome_id="sr:decided_by_extra_points:bestof:5:58" product_outcome_id="224" product_outcome_name="5"/>
        // 		</mapping>
        // 	</mappings>
        // </variant>
        return VariantDescriptionBuilder.Create()
                                        .WithId("sr:decided_by_extra_points:bestof:5")
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:53").WithName("0"))
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:54").WithName("1"))
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:55").WithName("2"))
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:56").WithName("3"))
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:57").WithName("4"))
                                        .AddOutcome(builder => builder.WithId("sr:decided_by_extra_points:bestof:5:58").WithName("5"))
                                        .AddMapping(builder => builder
                                                              .WithProductId(1)
                                                              .WithProductIds("1|4")
                                                              .WithSportId("sr:sport:20")
                                                              .WithMarketId("239")
                                                              .WithProductMarketId("8:68")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(1)
                                                              .WithProductIds("1|4")
                                                              .WithSportId("sr:sport:37")
                                                              .WithMarketId("239")
                                                              .WithProductMarketId("8:68")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(3)
                                                              .WithProductIds("3")
                                                              .WithSportId("sr:sport:23")
                                                              .WithMarketId("306")
                                                              .WithProductMarketId("338")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "260", "0 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "261", "1 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "98", "2 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "99", "3 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "100", "4 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "101", "5 sets")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(3)
                                                              .WithProductIds("3")
                                                              .WithSportId("sr:sport:37")
                                                              .WithMarketId("239")
                                                              .WithProductMarketId("557")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "260", "0 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "261", "1 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "98", "2 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "99", "3 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "100", "4 sets")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "101", "5 sets")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(1)
                                                              .WithProductIds("1|4")
                                                              .WithSportId("sr:sport:23")
                                                              .WithMarketId("306")
                                                              .WithProductMarketId("8:68")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(1)
                                                              .WithProductIds("1|4")
                                                              .WithSportId("sr:sport:34")
                                                              .WithMarketId("306")
                                                              .WithProductMarketId("8:68")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                              .BuildVariant())
                                        .AddMapping(builder => builder
                                                              .WithProductId(1)
                                                              .WithProductIds("1|4")
                                                              .WithSportId("sr:sport:31")
                                                              .WithMarketId("239")
                                                              .WithProductMarketId("8:68")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:53", "219", "0")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:54", "220", "1")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:55", "221", "2")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:56", "222", "3")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:57", "223", "4")
                                                              .AddVariantMappingOutcome("sr:decided_by_extra_points:bestof:5:58", "224", "5")
                                                              .BuildVariant())
                                        .Build();
    }
}
