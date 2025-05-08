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
    private tournamentLiveCoverageInfo _coverageInfo;
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
        _coverageInfo = new tournamentLiveCoverageInfo { live_coverage = liveCoverage.ToString() };
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

    public tournamentInfoEndpoint Build()
    {
        return new tournamentInfoEndpoint
        {
            tournament = _tournament,
            season = _season,
            coverage_info = _coverageInfo,
            groups = _groups.Count != 0 ? _groups.ToArray() : null
        };
    }
}
