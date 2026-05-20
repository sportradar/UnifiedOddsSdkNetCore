// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.CustomBet;

public static class CalculationEndpoint
{
    public static CalculationResponseType GetValidCalculationResponse()
    {
        return CalculationResponseBuilder.Create()
                                         .WithOdds(14.654619322466335)
                                         .WithProbability(0.03827598152270673)
                                         .WithEventId(TestConsts.AnyMatchId)
                                         .Build();
    }
}
