// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;

public static class Wns
{
    public static lottery_schedule ArizonaTripleTwistSchedule()
    {
        return new LotteryScheduleEndpoint()
              .WithGeneratedAt(Parse("2026-10-23T07:46:51.857Z"))
              .WithLottery(_ => ArizonaTripleTwist())
              .AddDrawEvent(draw => draw.WithId(DrawId(4936143671)).WithScheduled(Parse("2026-10-23T01:59:00.000Z")).WithStatus(drawStatus.finished).WithDisplayId(17))
              .AddDrawEvent(draw => draw.WithId(DrawId(4938156060)).WithScheduled(Parse("2026-10-24T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(16))
              .AddDrawEvent(draw => draw.WithId(DrawId(4939894800)).WithScheduled(Parse("2026-10-25T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(17))
              .AddDrawEvent(draw => draw.WithId(DrawId(4942365564)).WithScheduled(Parse("2026-10-26T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(16))
              .AddDrawEvent(draw => draw.WithId(DrawId(4944358164)).WithScheduled(Parse("2026-10-27T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(18))
              .AddDrawEvent(draw => draw.WithId(DrawId(4946483106)).WithScheduled(Parse("2026-10-28T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(17))
              .AddDrawEvent(draw => draw.WithId(DrawId(4948475193)).WithScheduled(Parse("2026-10-29T01:59:00.000Z")).WithStatus(drawStatus.open).WithDisplayId(20))
              .Build();
    }

    public static draw_fixtures ArizonaTripleTwistFixture()
    {
        return new DrawFixtureEndpoint()
              .WithGeneratedAt(Parse("2026-10-23T07:48:33.503Z"))
              .WithDrawFixture(ArizonaTripleTwistFinishedDraw)
              .Build();
    }

    public static draw_summary ArizonaTripleTwistSummary()
    {
        return new DrawSummaryEndpoint()
              .WithGeneratedAt(Parse("2026-10-23T07:47:54.054Z"))
              .WithDrawFixture(ArizonaTripleTwistFinishedDraw)
              .WithDrawResult(result => result.WithChronological(false)
                                              .AddDraw("draw_1", 3)
                                              .AddDraw("draw_2", 5)
                                              .AddDraw("draw_3", 6)
                                              .AddDraw("draw_4", 12)
                                              .AddDraw("draw_5", 23)
                                              .AddDraw("draw_6", 41))
              .Build();
    }

    public static lotteries Lotteries()
    {
        return new LotteriesEndpoint()
              .WithGeneratedAt(Parse("2026-10-23T07:43:36.758Z"))
              .AddLottery(_ => ArizonaTripleTwist())
              .AddLottery(_ => Lottery(46575, "Lotto Ireland 6/47", 1102, "Ireland", drawType.drum, "6/47", bonus => bonus.WithBonusBalls(1).WithBonusDrum(bonusDrumType.same).WithBonusRange("1-47")))
              .AddLottery(_ => Lottery(46576, "Lotto HotPicks 6/59", 1040, "UK", drawType.drum, "6/59", bonus => bonus.WithBonusBalls(0)))
              .AddLottery(_ => Lottery(46577, "Thunderball 5/39", 1040, "UK", drawType.drum, "5/39", bonus => bonus.WithBonusBalls(1).WithBonusDrum(bonusDrumType.additional).WithBonusRange("1-14")))
              .AddLottery(_ => Lottery(46578, "Health Lottery 5/50", 1040, "UK", drawType.drum, "5/50", bonus => bonus.WithBonusBalls(1).WithBonusDrum(bonusDrumType.same).WithBonusRange("1-50")))
              .Build();
    }

    private static DrawFixtureBuilder ArizonaTripleTwistFinishedDraw(DrawFixtureBuilder builder)
    {
        return builder.WithId(DrawId(4936143671))
                      .WithDrawDate(Parse("2026-10-23T01:59:00.000Z"))
                      .WithStatus(drawStatus.finished)
                      .WithDisplayId(17)
                      .WithLottery(_ => ArizonaTripleTwist());
    }

    private static LotteryBuilder ArizonaTripleTwist()
    {
        return Lottery(12408, "Arizona Triple Twist 6/42", 1039, "USA", drawType.rng, "6/42", bonus => bonus.WithBonusBalls(0));
    }

    private static LotteryBuilder Lottery(int id, string name, int categoryId, string categoryName, drawType drawType, string gameType, Func<BonusInfoBuilder, BonusInfoBuilder> bonus)
    {
        return new LotteryBuilder()
              .WithId(Urn.Parse($"wns:lottery:{id}"))
              .WithName(name)
              .WithSport(UrnCreate.SportId(108), "World Lottery")
              .WithCategory(UrnCreate.CategoryId(categoryId), categoryName)
              .WithDrawInfo(info => info.WithDrawType(drawType).WithTimeType(timeType.@fixed).WithGameType(gameType))
              .WithBonusInfo(bonus);
    }

    private static Urn DrawId(long id)
    {
        return Urn.Parse($"wns:draw:{id}");
    }

    private static DateTime Parse(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
