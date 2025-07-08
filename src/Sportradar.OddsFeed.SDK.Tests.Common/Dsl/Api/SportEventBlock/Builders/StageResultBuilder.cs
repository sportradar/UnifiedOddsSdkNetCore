// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <results>
// <competitor id="sr:competitor:953189">
//   <result value="1:30:55.026" type="time"/>
//   <result value="1" type="position"/>
//   <result value="1" type="no_pitstops"/>
//   <result value="1:35.520" type="fastest_lap"/>
//   <result value="56" type="finished_laps"/>
// </competitor>
// </results>
public class StageResultBuilder
{
    private readonly stageResult _stageResult = new stageResult();

    public StageResultBuilder WithCoverage(string coverage)
    {
        _stageResult.coverage = coverage;
        return this;
    }

    public StageResultBuilder AddCompetitor(Urn competitorId, int? position = null, List<KeyValuePair<string, string>> results = null)
    {
        _stageResult.competitor ??= [];
        var list = _stageResult.competitor.ToList();
        var result = new stageResultCompetitor
        {
            id = competitorId.ToString(),
            position = position ?? 0,
            positionSpecified = position.HasValue
        };
        if (results != null)
        {
            result.result = results.Select(r => new stageResultCompetitorResult
            {
                type = r.Key,
                value = r.Value
            }).ToArray();
        }
        list.Add(result);
        _stageResult.competitor = list.ToArray();
        return this;
    }

    public stageResult Build()
    {
        return _stageResult;
    }
}
