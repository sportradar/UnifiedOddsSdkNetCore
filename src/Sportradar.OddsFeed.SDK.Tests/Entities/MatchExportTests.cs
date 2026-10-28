// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class MatchExportTests
{
    private readonly ITestOutputHelper _outputHelper;

    public MatchExportTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task GetConditionsAsyncWhenMatchIsExportedThenRefereeIsPreserved()
    {
        var apiMatchSummaryEndpoint = Soccer.Summary();
        var uofConfig = UofConfigurations.SingleLanguage;
        var sportEventUnit = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(_outputHelper)
                                                  .WithConfiguration(uofConfig)
                                                  .WithSummaryFetcherReturning(apiMatchSummaryEndpoint)
                                                  .Build();
        var sportDataProvider = sportEventUnit.SportDataProvider;

        var match = (IMatch)sportDataProvider.GetCompetition(Urn.Parse(apiMatchSummaryEndpoint.sport_event.id));
        var matchConditions = await match.GetConditionsAsync();
        var exportedItems = await sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        var exportedMatch = (ExportableMatch)exportedItems.First(f => f.Id.Equals(match.Id.ToString(), StringComparison.Ordinal));

        var exportedMatchConditions = exportedMatch.Conditions;
        exportedMatchConditions.ShouldNotBeNull();
        exportedMatchConditions.Referee.ShouldNotBeNull();
        exportedMatchConditions.Referee.Id.ShouldBe(matchConditions.Referee.Id.ToString());
        exportedMatchConditions.Referee.Name.ShouldBe(matchConditions.Referee.Name);
        exportedMatchConditions.Referee.Nationality[uofConfig.DefaultLanguage].ShouldBe(matchConditions.Referee.GetNationality(uofConfig.DefaultLanguage));
    }
}
