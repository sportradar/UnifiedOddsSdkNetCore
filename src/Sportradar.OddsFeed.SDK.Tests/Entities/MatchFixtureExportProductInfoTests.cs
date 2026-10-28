// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class MatchFixtureExportProductInfoTests
{
    private readonly fixturesEndpoint _apiFixture;
    private readonly IMatch _match;
    private readonly ISportDataProvider _sportDataProvider;

    public MatchFixtureExportProductInfoTests(ITestOutputHelper outputHelper)
    {
        _apiFixture = Soccer.Fixture();
        _sportDataProvider = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(outputHelper)
                                                  .WithConfiguration(UofConfigurations.SingleLanguage)
                                                  .WithFixtureFetcherReturning(_apiFixture)
                                                  .Build()
                                                  .SportDataProvider;
        _match = (IMatch)_sportDataProvider.GetSportEvent(_apiFixture.GetMatchId());
    }

    [Fact]
    public async Task ExportsLiveScore()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.IsInLiveScore.ShouldBeTrue();
        exportedProductInfo.IsInLiveScore.ShouldBe(productInfo.IsInLiveScore);
    }

    [Fact]
    public async Task ExportsHostedStatistics()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.IsInHostedStatistics.ShouldBeTrue();
        exportedProductInfo.IsInHostedStatistics.ShouldBe(productInfo.IsInHostedStatistics);
    }

    [Fact]
    public async Task ExportsLiveCenterSoccer()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.IsInLiveCenterSoccer.ShouldBeTrue();
        exportedProductInfo.IsInLiveCenterSoccer.ShouldBe(productInfo.IsInLiveCenterSoccer);
    }

    [Fact]
    public async Task ExportsLiveMatchTracker()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.IsInLiveMatchTracker.ShouldBeTrue();
        exportedProductInfo.IsInLiveMatchTracker.ShouldBe(productInfo.IsInLiveMatchTracker);
    }

    [Fact]
    public async Task LeavesAutoTradedOff()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.IsAutoTraded.ShouldBeFalse();
        exportedProductInfo.IsAutoTraded.ShouldBe(productInfo.IsAutoTraded);
    }

    [Fact]
    public async Task ExportsLiveMatchTrackerLink()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();
        var apiLink = _apiFixture.fixture.product_info.links.Single();

        var link = productInfo.Links.Single();
        link.Name.ShouldBe(apiLink.name);
        link.Reference.ShouldBe(apiLink.@ref);
        var exportedLink = exportedProductInfo.Links.Single();
        exportedLink.Name.ShouldBe(link.Name);
        exportedLink.Reference.ShouldBe(link.Reference);
    }

    [Fact]
    public async Task LeavesStreamingChannelsEmpty()
    {
        var productInfo = (await _match.GetFixtureAsync()).ProductInfo;
        var exportedProductInfo = await ExportProductInfoAsync();

        productInfo.Channels.ShouldBeNull();
        exportedProductInfo.Channels.ShouldBeNull();
    }

    private async Task<ExportableProductInfo> ExportProductInfoAsync()
    {
        var exportedItems = await _sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        var exportedMatch = (ExportableMatch)exportedItems.First(item => item.Id.Equals(_match.Id.ToString(), StringComparison.Ordinal));
        return exportedMatch.Fixture.ProductInfo;
    }
}
