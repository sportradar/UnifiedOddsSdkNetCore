// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class TournamentInfoBuilder
{
    private tournamentExtended _tournament;
    private seasonExtended _season;
    private matchRound _round;
    private seasonCoverageInfo _seasonCoverageInfo;
    private tournamentLiveCoverageInfo _coverageInfo;
    private DateTime? _generatedAt;
    private readonly List<tournamentGroup> _groups = [];

    public TournamentInfoBuilder WithTournament(tournamentExtended tournament)
    {
        _tournament = tournament;
        return this;
    }

    public TournamentInfoBuilder WithTournament(Func<TournamentBuilder> tournamentBuilder)
    {
        _tournament = tournamentBuilder().Build() as tournamentExtended;
        return this;
    }

    public TournamentInfoBuilder WithSeason(seasonExtended season)
    {
        _season = season;
        return this;
    }

    public TournamentInfoBuilder WithSeason(Func<SeasonBuilder> seasonBuilder)
    {
        _season = seasonBuilder().BuildExtended();
        return this;
    }

    public TournamentInfoBuilder WithLiveCoverageInfo(bool liveCoverage)
    {
        _coverageInfo = new tournamentLiveCoverageInfo { live_coverage = liveCoverage ? "true" : "false" };
        return this;
    }

    public TournamentInfoBuilder WithGeneratedAt(DateTime generatedAt)
    {
        _generatedAt = generatedAt;
        return this;
    }

    public TournamentInfoBuilder WithRound(matchRound round)
    {
        _round = round;
        return this;
    }

    public TournamentInfoBuilder WithRound(Func<MatchRoundBuilder> roundBuilder)
    {
        _round = roundBuilder().Build();
        return this;
    }

    public TournamentInfoBuilder WithSeasonCoverageInfo(seasonCoverageInfo seasonCoverageInfo)
    {
        _seasonCoverageInfo = seasonCoverageInfo;
        return this;
    }

    public TournamentInfoBuilder WithGroups(tournamentGroup[] tournamentGroups)
    {
        _groups.Clear();
        _groups.AddRange(tournamentGroups);
        return this;
    }

    public TournamentInfoBuilder AddGroup(string name, string id, params Func<CompetitorBuilder>[] competitors)
    {
        var group = new tournamentGroup
        {
            id = id,
            name = name,
            competitor = competitors.Select(b => b().Build()).ToArray()
        };
        _groups.Add(group);
        return this;
    }

    public TournamentInfoBuilder AddGroup(params Func<CompetitorBuilder>[] competitors)
    {
        var group = new tournamentGroup
        {
            competitor = competitors.Select(b => b().Build()).ToArray()
        };
        _groups.Add(group);
        return this;
    }

    public tournamentInfoEndpoint Build()
    {
        return new tournamentInfoEndpoint
        {
            tournament = _tournament,
            season = _season,
            round = _round,
            season_coverage_info = _seasonCoverageInfo,
            coverage_info = _coverageInfo,
            groups = _groups.Count != 0 ? _groups.ToArray() : null,
            generated_at = _generatedAt ?? default,
            generated_atSpecified = _generatedAt.HasValue
        };
    }
}
