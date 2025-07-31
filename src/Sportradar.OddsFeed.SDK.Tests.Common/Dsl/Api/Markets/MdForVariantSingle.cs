// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

/// <summary>
/// Market descriptions definitions for single variant market description
/// </summary>
public static class MdForVariantSingle
{
    /// <summary>
    /// Get variant market description for market 534
    /// </summary>
    /// <example>
    /// <market id="534" name="World Cup - Group Points Portugal" variant="pre:markettext:168883">
    /// 	<outcomes>
    /// 		<outcome id="pre:outcometext:108719" name="7 Points"/>
    /// 		<outcome id="pre:outcometext:108720" name="4 Points"/>
    /// 		<outcome id="pre:outcometext:108721" name="6 Points"/>
    /// 		<outcome id="pre:outcometext:108722" name="9 Points"/>
    /// 		<outcome id="pre:outcometext:108723" name="5 Points"/>
    /// 		<outcome id="pre:outcometext:108724" name="3 Points"/>
    /// 		<outcome id="pre:outcometext:108725" name="2 Points"/>
    /// 		<outcome id="pre:outcometext:108726" name="1 Point"/>
    /// 		<outcome id="pre:outcometext:108727" name="0 Points"/>
    /// 	</outcomes>
    /// </market>
    /// </example>
    /// <returns></returns>
    public static desc_market GetPreOutcomeMarket534()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(534)
            .WithName("World Cup - Group Points Portugal")
            .WithVariant("pre:markettext:168883")
            .AddOutcome(builder => builder.WithId("pre:outcometext:108719").WithName("7 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108720").WithName("4 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108721").WithName("6 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108722").WithName("9 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108723").WithName("5 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108724").WithName("3 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108725").WithName("2 Points"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108726").WithName("1 Point"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:108727").WithName("0 Points"))
            .Build();
    }

    /// <summary>
    /// Market description for pre:markettext market 535
    /// </summary>
    /// <example>
    /// <market_descriptions response_code="OK">
    ///   <market id="535" name="Magical Kenya Open 2025 - Round 1 - 18 Hole Match Bet - H Li vs A Saddier" variant="pre:markettext:289339">
    ///     <outcomes>
    ///       <outcome id="pre:outcometext:6388892" name="Saddier, Adrien"/>
    ///       <outcome id="pre:outcometext:5725004" name="Li, Haotong"/>
    ///     </outcomes>
    ///   </market>
    /// </market_descriptions>
    /// </example>
    /// <returns></returns>
    public static desc_market GetPreMarketTextMarket535()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(535)
            .WithName("Magical Kenya Open 2025 - Round 1 - 18 Hole Match Bet - H Li vs A Saddier")
            .WithVariant("pre:markettext:289339")
            .AddOutcome(builder => builder.WithId("pre:outcometext:6388892").WithName("Saddier, Adrien"))
            .AddOutcome(builder => builder.WithId("pre:outcometext:5725004").WithName("Li, Haotong"))
            .Build();
    }

    public static desc_market GetPlayerPropsMarket768()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(768)
            .WithName("Player points (incl. overtime)")
            .WithVariant("pre:playerprops:35432179:608000")
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:22").WithName("Kyle Anderson 22+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:23").WithName("Kyle Anderson 23+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:24").WithName("Kyle Anderson 24+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:10").WithName("Kyle Anderson 10+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:11").WithName("Kyle Anderson 11+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:12").WithName("Kyle Anderson 12+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:13").WithName("Kyle Anderson 13+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:14").WithName("Kyle Anderson 14+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:15").WithName("Kyle Anderson 15+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:16").WithName("Kyle Anderson 16+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:17").WithName("Kyle Anderson 17+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:18").WithName("Kyle Anderson 18+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:19").WithName("Kyle Anderson 19+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:7").WithName("Kyle Anderson 7+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:8").WithName("Kyle Anderson 8+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:9").WithName("Kyle Anderson 9+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:20").WithName("Kyle Anderson 20+"))
            .AddOutcome(builder => builder.WithId("pre:playerprops:35432179:608000:21").WithName("Kyle Anderson 21+"))
            .Build();
    }

    public static desc_market GetCompetitorPropsMarket1768()
    {
        return MarketDescriptionBuilder.Create()
            .WithId(1768)
            .WithName("Competitor points (incl. overtime)")
            .WithVariant("pre:competitorprops:35432179:608000")
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:22").WithName("Kyle Anderson 22+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:23").WithName("Kyle Anderson 23+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:24").WithName("Kyle Anderson 24+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:10").WithName("Kyle Anderson 10+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:11").WithName("Kyle Anderson 11+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:12").WithName("Kyle Anderson 12+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:13").WithName("Kyle Anderson 13+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:14").WithName("Kyle Anderson 14+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:15").WithName("Kyle Anderson 15+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:16").WithName("Kyle Anderson 16+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:17").WithName("Kyle Anderson 17+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:18").WithName("Kyle Anderson 18+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:19").WithName("Kyle Anderson 19+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:7").WithName("Kyle Anderson 7+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:8").WithName("Kyle Anderson 8+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:9").WithName("Kyle Anderson 9+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:20").WithName("Kyle Anderson 20+"))
            .AddOutcome(builder => builder.WithId("pre:competitorprops:35432179:608000:21").WithName("Kyle Anderson 21+"))
            .Build();
    }

    /// <summary>
    /// Calling a single variant market description with unknown marketId or variantId can result in NOT_FOUND response
    /// This can also be result for valid market, but just not available at the moment
    /// </summary>
    /// <example>
    /// <market_descriptions response_code="NOT_FOUND">
    ///   <market/>
    /// </market_descriptions>
    /// </example>
    /// <returns></returns>
    public static market_descriptions GetNotFound()
    {
        return new market_descriptions
        {
            response_code = response_code.NOT_FOUND,
            response_codeSpecified = true
        };
    }
}
