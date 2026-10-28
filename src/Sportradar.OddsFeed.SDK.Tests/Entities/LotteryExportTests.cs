// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class LotteryExportTests
{
    private readonly lottery_schedule _apiSchedule;
    private readonly CultureInfo _language;
    private readonly ILottery _lottery;
    private readonly ISportDataProvider _sportDataProvider;

    public LotteryExportTests(ITestOutputHelper outputHelper)
    {
        _apiSchedule = Wns.ArizonaTripleTwistSchedule();
        var configuration = UofConfigurations.SingleLanguage;
        _language = configuration.DefaultLanguage;
        _sportDataProvider = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(outputHelper)
                                                  .WithConfiguration(configuration)
                                                  .WithLotteriesFetcherReturning(Wns.Lotteries())
                                                  .WithLotteryScheduleFetcherReturning(_apiSchedule)
                                                  .Build()
                                                  .SportDataProvider;
        _lottery = _sportDataProvider.GetLotteriesAsync().GetAwaiter().GetResult().Single(item => item.Id.ToString() == _apiSchedule.lottery.id);
    }

    [Fact]
    public async Task GetNameAsyncWhenLotteryIsExportedThenNameIsPreserved()
    {
        var name = await _lottery.GetNameAsync(_language);
        var exportedLottery = await ExportLotteryAsync();

        name.ShouldBe(_apiSchedule.lottery.name);
        exportedLottery.Names[_language].ShouldBe(name);
    }

    [Fact]
    public async Task GetCategoryAsyncWhenLotteryIsExportedThenCategoryIsPreserved()
    {
        var category = await _lottery.GetCategoryAsync();
        var exportedLottery = await ExportLotteryAsync();

        category.ShouldNotBeNull();
        category.Id.ToString().ShouldBe(_apiSchedule.lottery.category.id);
        category.GetName(_language).ShouldBe(_apiSchedule.lottery.category.name);
        exportedLottery.CategoryId.ShouldBe(category.Id.ToString());
    }

    [Fact]
    public async Task GetBonusInfoAsyncWhenLotteryIsExportedThenBonusBallsArePreserved()
    {
        var bonusInfo = await _lottery.GetBonusInfoAsync();
        var exportedLottery = await ExportLotteryAsync();

        bonusInfo.BonusBalls.ShouldBe(_apiSchedule.lottery.bonus_info.bonus_balls);
        exportedLottery.BonusInfo.ShouldNotBeNull();
        exportedLottery.BonusInfo.BonusBalls.ShouldBe(bonusInfo.BonusBalls);
    }

    [Fact]
    public async Task GetDrawInfoAsyncWhenLotteryIsExportedThenDrawTypeIsPreserved()
    {
        var drawInfo = await _lottery.GetDrawInfoAsync();
        var exportedLottery = await ExportLotteryAsync();

        drawInfo.DrawType.ShouldBe(DrawType.Rng);
        exportedLottery.DrawInfo.ShouldNotBeNull();
        exportedLottery.DrawInfo.DrawType.ShouldBe(drawInfo.DrawType);
    }

    [Fact]
    public async Task GetDrawInfoAsyncWhenLotteryIsExportedThenTimeTypeIsPreserved()
    {
        var drawInfo = await _lottery.GetDrawInfoAsync();
        var exportedLottery = await ExportLotteryAsync();

        drawInfo.TimeType.ShouldBe(TimeType.Fixed);
        exportedLottery.DrawInfo.ShouldNotBeNull();
        exportedLottery.DrawInfo.TimeType.ShouldBe(drawInfo.TimeType);
    }

    [Fact]
    public async Task GetDrawInfoAsyncWhenLotteryIsExportedThenGameTypeIsPreserved()
    {
        var drawInfo = await _lottery.GetDrawInfoAsync();
        var exportedLottery = await ExportLotteryAsync();

        drawInfo.GameType.ShouldBe(_apiSchedule.lottery.draw_info.game_type);
        exportedLottery.DrawInfo.ShouldNotBeNull();
        exportedLottery.DrawInfo.GameType.ShouldBe(drawInfo.GameType);
    }

    [Fact]
    public async Task GetScheduledDrawsAsyncWhenLotteryIsExportedThenScheduledDrawsArePreserved()
    {
        var scheduledDraws = (await _lottery.GetScheduledDrawsAsync()).ToList();
        var exportedLottery = await ExportLotteryAsync();

        scheduledDraws.Select(draw => draw.ToString()).ShouldBe(_apiSchedule.draw_events.Select(draw => draw.id));
        exportedLottery.ScheduledDraws.ShouldBe(scheduledDraws.Select(draw => draw.ToString()));
    }

    private async Task<ExportableLottery> ExportLotteryAsync()
    {
        var exportedItems = await _sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        return (ExportableLottery)exportedItems.First(item => item.Id.Equals(_lottery.Id.ToString(), StringComparison.Ordinal));
    }
}
