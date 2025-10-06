// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions.Assertions;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;
using static Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints.MatchTimelineEndpoints;

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventCacheExportTests
{
    private static readonly List<CultureInfo> EnglishAndTwoOtherLanguages = [TestConsts.CultureEn, TestConsts.CultureDe, TestConsts.CultureHu];
    private static readonly CultureInfo English = TestConsts.CultureEn;
    private readonly XunitLoggerFactory _loggerFactory;
    private readonly IUofConfiguration _uofConfiguration;

    public SportEventCacheExportTests(ITestOutputHelper outputHelper)
    {
        _loggerFactory = new XunitLoggerFactory(outputHelper);
        _uofConfiguration = UofConfigurations.GetUofConfiguration(English, EnglishAndTwoOtherLanguages);
    }

    [Fact]
    public async Task SportEventCacheExportedMatchTimelineShouldContainTimelineWithIdsGreaterThanMaxInteger()
    {
        var liverpoolVsPsg2025MatchTimeline = LiverpoolVsPsg2025MatchTimeline();
        var liverpoolMatchId = liverpoolVsPsg2025MatchTimeline.sport_event.id.ToUrn();
        AssignLongToFirstTimelineId(liverpoolVsPsg2025MatchTimeline);

        var sportEventCache = liverpoolVsPsg2025MatchTimeline.CreateSportEventCacheWithTimeline(_uofConfiguration, _loggerFactory);

        var match = sportEventCache.GetEventCacheItem(liverpoolMatchId) as IMatchCacheItem;
        await match!.GetEventTimelineAsync([English]);

        await ExportAndCleanReimportCacheItems(sportEventCache);

        match = sportEventCache.GetEventCacheItem(liverpoolMatchId) as IMatchCacheItem;
        var reimportedTimelineCacheItem = await match!.GetEventTimelineAsync([English]);

        reimportedTimelineCacheItem.ShouldNotBeNull();
        reimportedTimelineCacheItem.Timeline.ShouldMatchEvents(liverpoolVsPsg2025MatchTimeline.timeline);
    }

    private static async Task ExportAndCleanReimportCacheItems(ISportEventCache sportEventCache)
    {
        var exportedSportEvents = (await sportEventCache.ExportAsync()).ToList();
        PurgeCache(sportEventCache, exportedSportEvents);
        await sportEventCache.ImportAsync(exportedSportEvents);
    }

    private static void AssignLongToFirstTimelineId(matchTimelineEndpoint liverpoolVsPsg2025MatchTimeline)
    {
        liverpoolVsPsg2025MatchTimeline.timeline[0].id = int.MaxValue + 1L;
    }

    private static void PurgeCache(ISportEventCache sportEventCache, IEnumerable<ExportableBase> exportables)
    {
        foreach (var exportableBase in exportables)
        {
            sportEventCache.CacheDeleteItem(exportableBase.Id.ToUrn(), CacheItemType.All);
        }
    }
}
