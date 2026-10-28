// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
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

public class DrawExportTests
{
    private readonly draw_summary _apiSummary;
    private readonly IDraw _draw;
    private readonly CultureInfo _language;
    private readonly ISportDataProvider _sportDataProvider;

    public DrawExportTests(ITestOutputHelper outputHelper)
    {
        _apiSummary = Wns.ArizonaTripleTwistSummary();
        var configuration = UofConfigurations.SingleLanguage;
        _language = configuration.DefaultLanguage;
        _sportDataProvider = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(outputHelper)
                                                  .WithConfiguration(configuration)
                                                  .WithDrawSummaryFetcherReturning(_apiSummary)
                                                  .Build()
                                                  .SportDataProvider;
        _draw = (IDraw)_sportDataProvider.GetSportEvent(Urn.Parse(_apiSummary.draw_fixture.id));
    }

    [Fact]
    public async Task GetLotteryIdAsyncWhenDrawIsExportedThenLotteryIdIsPreserved()
    {
        var lotteryId = await _draw.GetLotteryIdAsync();
        var exportedDraw = await ExportDrawAsync();

        lotteryId.ToString().ShouldBe(_apiSummary.draw_fixture.lottery.id);
        exportedDraw.LotteryId.ShouldBe(lotteryId.ToString());
    }

    [Fact]
    public async Task GetStatusAsyncWhenDrawIsExportedThenStatusIsPreserved()
    {
        var status = await _draw.GetStatusAsync();
        var exportedDraw = await ExportDrawAsync();

        status.ShouldBe(DrawStatus.Finished);
        exportedDraw.DrawStatus.ShouldBe(status);
    }

    [Fact]
    public async Task GetDisplayIdAsyncWhenDrawIsExportedThenDisplayIdIsPreserved()
    {
        var displayId = await _draw.GetDisplayIdAsync();
        var exportedDraw = await ExportDrawAsync();

        displayId.ShouldBe(_apiSummary.draw_fixture.display_id);
        exportedDraw.DisplayId.ShouldBe(displayId);
    }

    [Fact]
    public async Task GetScheduledTimeAsyncWhenDrawIsExportedThenScheduledTimeIsPreserved()
    {
        var scheduled = await _draw.GetScheduledTimeAsync();
        var exportedDraw = await ExportDrawAsync();

        scheduled.ShouldNotBeNull();
        scheduled.Value.ToUniversalTime().ShouldBe(_apiSummary.draw_fixture.draw_date.ToUniversalTime());
        exportedDraw.Scheduled.ShouldBe(scheduled);
    }

    [Fact]
    public async Task GetSportIdAsyncWhenDrawIsExportedThenSportIsPreserved()
    {
        var sportId = await _draw.GetSportIdAsync();
        var exportedDraw = await ExportDrawAsync();

        sportId.ToString().ShouldBe(_apiSummary.draw_fixture.lottery.sport.id);
        exportedDraw.SportId.ShouldBe(sportId.ToString());
    }

    [Fact]
    public async Task GetResultsAsyncWhenDrawIsExportedThenResultsArePreserved()
    {
        var results = (await _draw.GetResultsAsync()).ToList();
        var exportedDraw = await ExportDrawAsync();
        var apiDraws = _apiSummary.draw_result.draws.draw;

        results.Select(result => result.Value).ShouldBe(apiDraws.Select(draw => (int?)draw.value));
        results.Select(result => result.GetName(_language)).ShouldBe(apiDraws.Select(draw => draw.name));
        exportedDraw.ResultsChronological.ShouldBe(_apiSummary.draw_result.draws.chronological);
        exportedDraw.Results.Select(result => result.Value).ShouldBe(results.Select(result => result.Value));
        exportedDraw.Results.Select(result => result.Names[_language]).ShouldBe(results.Select(result => result.GetName(_language)));
    }

    private async Task<ExportableDraw> ExportDrawAsync()
    {
        var exportedItems = await _sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        return (ExportableDraw)exportedItems.First(item => item.Id.Equals(_draw.Id.ToString(), StringComparison.Ordinal));
    }
}
