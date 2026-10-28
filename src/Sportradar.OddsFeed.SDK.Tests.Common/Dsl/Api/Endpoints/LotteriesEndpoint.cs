// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class LotteriesEndpoint
{
    private readonly List<lottery> _lotteries = new List<lottery>();
    private DateTime? _generatedAt;

    public LotteriesEndpoint AddLottery(Func<LotteryBuilder, LotteryBuilder> builder)
    {
        _lotteries.Add(builder(new LotteryBuilder()).Build());
        return this;
    }

    public LotteriesEndpoint WithGeneratedAt(DateTime generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public lotteries Build()
    {
        var lotteries = new lotteries
        {
            lottery = _lotteries.ToArray()
        };
        if (_generatedAt.HasValue)
        {
            lotteries.generated_at = _generatedAt.Value;
            lotteries.generated_atSpecified = true;
        }

        return lotteries;
    }
}
