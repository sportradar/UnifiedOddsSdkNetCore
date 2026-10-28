// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <season_coverage_info season_id="sr:season:140786" scheduled="234" played="291" max_coverage_level="gold" max_covered="150" min_coverage_level="bronze"/>
public class SeasonCoverageInfoBuilder
{
    private readonly seasonCoverageInfo _coverageInfo = new seasonCoverageInfo();

    public SeasonCoverageInfoBuilder WithSeasonId(Urn seasonId)
    {
        _coverageInfo.season_id = seasonId.ToString();
        return this;
    }

    public SeasonCoverageInfoBuilder WithScheduled(int scheduled)
    {
        _coverageInfo.scheduled = scheduled;
        return this;
    }

    public SeasonCoverageInfoBuilder WithPlayed(int played)
    {
        _coverageInfo.played = played;
        return this;
    }

    public SeasonCoverageInfoBuilder WithMaxCoverageLevel(string maxCoverageLevel)
    {
        _coverageInfo.max_coverage_level = maxCoverageLevel;
        return this;
    }

    public SeasonCoverageInfoBuilder WithMaxCovered(int maxCovered)
    {
        _coverageInfo.max_covered = maxCovered;
        _coverageInfo.max_coveredSpecified = true;
        return this;
    }

    public SeasonCoverageInfoBuilder WithMinCoverageLevel(string minCoverageLevel)
    {
        _coverageInfo.min_coverage_level = minCoverageLevel;
        return this;
    }

    public seasonCoverageInfo Build()
    {
        return _coverageInfo;
    }
}
