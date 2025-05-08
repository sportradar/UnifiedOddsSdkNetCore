// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <coverage_info level="silver" live_coverage="true" covered_from="venue">
//   <coverage includes="basic_score"/>
//   <coverage includes="key_events"/>
// </coverage_info>
public class CoverageInfoBuilder
{
    private readonly coverageInfo _coverage = new coverageInfo();

    public CoverageInfoBuilder WithLevel(string level)
    {
        _coverage.level = level;
        return this;
    }

    public CoverageInfoBuilder WithLiveCoverage(bool isCoveredLive)
    {
        _coverage.live_coverage = isCoveredLive;
        return this;
    }

    public CoverageInfoBuilder WithCoveredFrom(string coveredFrom)
    {
        _coverage.covered_from = coveredFrom;
        return this;
    }

    public CoverageInfoBuilder AddCoverage(string coverage)
    {
        _coverage.coverage ??= [];
        var list = _coverage.coverage.ToList();
        list.Add(new coverage { includes = coverage });
        _coverage.coverage = list.ToArray();
        return this;
    }

    public coverageInfo Build()
    {
        return _coverage;
    }
}
