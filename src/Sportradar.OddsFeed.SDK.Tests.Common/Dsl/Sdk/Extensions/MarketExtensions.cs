// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Market;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Extensions;

public static class MarketExtensions
{
    public static IMarketDescription ToUserMarketDescription(this desc_market market, CultureInfo language)
    {
        return new Builders.MarketDescriptionBuilder()
              .WithName(market.name, language)
              .BuildWith(market, language);
    }
}
