// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <bonus_info bonus_balls="1" bonus_drum="same" bonus_range="1-47"/>
public class BonusInfoBuilder
{
    private readonly lotteryBonus_info _bonusInfo = new lotteryBonus_info();

    public BonusInfoBuilder WithBonusBalls(int bonusBalls)
    {
        _bonusInfo.bonus_balls = bonusBalls;
        _bonusInfo.bonus_ballsSpecified = true;
        return this;
    }

    public BonusInfoBuilder WithBonusDrum(bonusDrumType bonusDrum)
    {
        _bonusInfo.bonus_drum = bonusDrum;
        _bonusInfo.bonus_drumSpecified = true;
        return this;
    }

    public BonusInfoBuilder WithBonusRange(string bonusRange)
    {
        _bonusInfo.bonus_range = bonusRange;
        return this;
    }

    public lotteryBonus_info Build()
    {
        return _bonusInfo;
    }
}
