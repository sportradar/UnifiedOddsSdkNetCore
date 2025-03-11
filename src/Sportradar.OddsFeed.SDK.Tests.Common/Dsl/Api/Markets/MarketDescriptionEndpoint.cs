// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Markets;

public class MarketDescriptionEndpoint
{
    public MarketDescriptionBuilder MarketDescriptionBuilder = MarketDescriptionBuilder.Create();
    public VariantDescriptionBuilder VariantDescriptionBuilder = VariantDescriptionBuilder.Create();

    public static market_descriptions GetDefaultInvariantList()
    {
        var marketList = new List<desc_market>
        {
            MdForInvariantList.GetMarketForVariantList199(),
            MdForInvariantList.GetMarketWithCompetitor282(),
            MdForInvariantList.GetPreOutcomeTextMarket534(),
            MdForInvariantList.GetPreOutcomeTextMarket535(),
            MdForInvariantList.GetPreOutcomeTextMarket536(),
            MdForInvariantList.GetMarketWithSpecifier701(),
            MdForInvariantList.GetOrdinalMarket739(),
            MdForInvariantList.GetPlayerPropsMarket768()
        };
        return new market_descriptions
        {
            market = marketList.ToArray(),
            response_codeSpecified = false
        };
    }

    public static market_descriptions GetDefaultInvariantList(CultureInfo culture)
    {
        var marketDescriptions = GetDefaultInvariantList();
        foreach (var market in marketDescriptions.market)
        {
            market.AddSuffix($" [{culture.TwoLetterISOLanguageName.ToUpperInvariant()}]");
        }
        return marketDescriptions;
    }

    public static market_descriptions GetInvariantList(params desc_market[] markets)
    {
        return new market_descriptions
        {
            market = markets,
            response_codeSpecified = false
        };
    }

    internal static EntityList<MarketDescriptionDto> GetInvariantDto(params desc_market[] markets)
    {
        return new EntityList<MarketDescriptionDto>(markets.Select(m => new MarketDescriptionDto(m)).ToList());
    }

    public static variant_descriptions GetDefaultVariantList()
    {
        var variantList = new List<desc_variant>
        {
            MdForVariantList.GetCorrectScoreBestOf12()
        };
        return new variant_descriptions
        {
            variant = variantList.ToArray(),
            response_codeSpecified = false
        };
    }

    public static variant_descriptions GetDefaultVariantList(CultureInfo culture)
    {
        var variantDescriptions = GetDefaultVariantList();
        foreach (var market in variantDescriptions.variant)
        {
            market.AddSuffix($" [{culture.TwoLetterISOLanguageName.ToUpperInvariant()}]");
        }
        return variantDescriptions;
    }

    internal static EntityList<VariantDescriptionDto> GetVariantDto(params desc_variant[] markets)
    {
        return new EntityList<VariantDescriptionDto>(markets.Select(m => new VariantDescriptionDto(m)).ToList());
    }
}
