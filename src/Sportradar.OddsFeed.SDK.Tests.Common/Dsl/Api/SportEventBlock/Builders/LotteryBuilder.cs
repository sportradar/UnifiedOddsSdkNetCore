// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <lottery id="wns:lottery:12408" name="Arizona Triple Twist 6/42">
//   <sport id="sr:sport:108" name="World Lottery"/>
//   <category id="sr:category:1039" name="USA"/>
//   <draw_info draw_type="rng" time_type="fixed" game_type="6/42"/>
//   <bonus_info bonus_balls="0"/>
// </lottery>
public class LotteryBuilder
{
    private readonly lottery _lottery = new lottery();

    public LotteryBuilder WithId(Urn id)
    {
        _lottery.id = id.ToString();
        return this;
    }

    public LotteryBuilder WithName(string name)
    {
        _lottery.name = name;
        return this;
    }

    public LotteryBuilder WithSport(Urn sportId, string name)
    {
        _lottery.sport = new sport
        {
            id = sportId.ToString(),
            name = name
        };
        return this;
    }

    public LotteryBuilder WithCategory(Urn categoryId, string name)
    {
        _lottery.category = new category
        {
            id = categoryId.ToString(),
            name = name
        };
        return this;
    }

    public LotteryBuilder WithDrawInfo(Func<DrawInfoBuilder, DrawInfoBuilder> builder)
    {
        _lottery.draw_info = builder(new DrawInfoBuilder()).Build();
        return this;
    }

    public LotteryBuilder WithBonusInfo(Func<BonusInfoBuilder, BonusInfoBuilder> builder)
    {
        _lottery.bonus_info = builder(new BonusInfoBuilder()).Build();
        return this;
    }

    public lottery Build()
    {
        return _lottery;
    }
}
