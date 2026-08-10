// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet;

public static class FilteredCalculationEndpoint
{
    public static FilteredCalculationResponseType GetValidFilteredCalculationResponse()
    {
        return FilteredCalculationResponseBuilder.Create()
                                                .WithOdds(3.8567583406625268)
                                                .WithProbability(0.21421042720764835)
                                                .WithEventId(TestConsts.AnyMatchId)
                                                .AddMarket(10, null, true,
                                                    ("9", true), ("10", true), ("11", true))
                                                .Build();
    }
}
