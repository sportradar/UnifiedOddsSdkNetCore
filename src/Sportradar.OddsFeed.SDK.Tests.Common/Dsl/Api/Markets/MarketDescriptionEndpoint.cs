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
            MdForInvariantList.GetMarket41WithScoreSpecifier(),
            MdForInvariantList.GetMarket199ForVariantList(),
            MdForInvariantList.GetMarket282WithCompetitorInMarketName(),
            MdForInvariantList.GetPreOutcomeTextMarket534(),
            MdForInvariantList.GetPreOutcomeTextMarket535(),
            MdForInvariantList.GetPreOutcomeTextMarket536(),
            MdForInvariantList.GetMarket701WithSpecifierInMarketName(),
            MdForInvariantList.GetOrdinalMarket739(),
            MdForInvariantList.GetPlayerPropsMarket768(),
            MdForInvariantList.GetFakePlayerPropsMarket1768(),
            MdForInvariantList.GetMarket374WithVariant(),
            MdForInvariantList.GetMarket239WithGenericNameVariant(),
            MdForInvariantList.GetMarket1109ForCompetitorOutcomeType(),
            MdForInvariantList.GetMarket676ForPlayerOutcomeType(),
            MdForInvariantList.GetMarket303WithCompetitorReferenceInOutcome(),
            MdForInvariantList.GetFlexScoreMarket118(),
            MdForInvariantList.GetGoalScorerMarket892WithVersion(),
            MdForInvariantList.GetGolfThreeBallsMarket1022(),
            MdForInvariantList.GetHead2HeadMarket1103(),
            MdForInvariantList.GetMarket915WithPlayerInNameFromSpecifier(),
            MdForInvariantList.GetMarket717WithPlayerInOutcomeNameFromSpecifier(),
            MdForInvariantList.GetMarket1140WithPlayerInNameWithoutSpecifier(),
            MdForInvariantList.GetGoalScorerMarket40(),
            MdForInvariantList.GetLastGoalscorerMarket39(),
            MdForInvariantList.GetPlayerToScoreIncludingOvertimeMarket882()
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

    internal static MarketDescriptionDto GetSingleVariantDto(desc_market market)
    {
        return new MarketDescriptionDto(market);
    }

    public static variant_descriptions GetDefaultVariantList()
    {
        var variantList = new List<desc_variant>
        {
            MdForVariantList.GetCorrectScoreBestOf12(),
            MdForVariantList.GetDecidedByExtraPointsBestOf5()
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
