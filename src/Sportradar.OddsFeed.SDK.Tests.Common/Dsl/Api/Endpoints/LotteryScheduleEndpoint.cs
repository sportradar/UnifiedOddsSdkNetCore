// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class LotteryScheduleEndpoint
{
    private readonly List<draw_event> _drawEvents = new List<draw_event>();
    private lottery _lottery;
    private DateTime? _generatedAt;

    public LotteryScheduleEndpoint WithLottery(Func<LotteryBuilder, LotteryBuilder> builder)
    {
        _lottery = builder(new LotteryBuilder()).Build();
        return this;
    }

    public LotteryScheduleEndpoint AddDrawEvent(Func<DrawEventBuilder, DrawEventBuilder> builder)
    {
        _drawEvents.Add(builder(new DrawEventBuilder()).Build());
        return this;
    }

    public LotteryScheduleEndpoint WithGeneratedAt(DateTime generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public lottery_schedule Build()
    {
        var schedule = new lottery_schedule
        {
            lottery = _lottery,
            draw_events = _drawEvents.ToArray()
        };
        if (_generatedAt.HasValue)
        {
            schedule.generated_at = _generatedAt.Value;
            schedule.generated_atSpecified = true;
        }

        return schedule;
    }
}
