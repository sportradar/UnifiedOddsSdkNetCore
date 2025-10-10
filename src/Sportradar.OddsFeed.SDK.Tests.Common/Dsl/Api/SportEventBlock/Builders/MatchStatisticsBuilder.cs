// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class MatchStatisticsBuilder
{
    private List<teamStatistics> _teamStatistics = [];
    private readonly List<matchPeriod> _matchPeriod = [];

    public MatchStatisticsBuilder WithTotal(TeamStatisticsBuilder builder)
    {
        _teamStatistics = builder.Build().ToList();
        return this;
    }

    public MatchStatisticsBuilder AddPeriod(string periodName, TeamStatisticsBuilder builder)
    {
        var matchPeriod = new matchPeriod
        {
            name = periodName,
            teams = builder.Build()
        };
        _matchPeriod.Add(matchPeriod);
        return this;
    }

    public matchStatistics Build()
    {
        var matchStatistics = new matchStatistics();
        if (!_teamStatistics.IsNullOrEmpty())
        {
            matchStatistics.totals = [_teamStatistics.ToArray()];
        }
        if (!_matchPeriod.IsNullOrEmpty())
        {
            matchStatistics.periods = _matchPeriod.ToArray();
        }
        return matchStatistics;
    }

    public class TeamStatisticsBuilder
    {
        private readonly List<teamStatistics> _teamStatistics = [];

        public TeamStatisticsBuilder AddTeamStatistics(Urn teamId, string name, string cards, string cornerKicks, string redCards, string yellowCards, string yellowRedCards)
        {
            var innerStatistics = new teamStatisticsStatistics
            {
                cards = cards,
                corner_kicks = cornerKicks,
                red_cards = redCards,
                yellow_cards = yellowCards,
                yellow_red_cards = yellowRedCards
            };
            var teamStatistics = new teamStatistics
            {
                id = teamId.ToString(),
                name = name,
                statistics = innerStatistics
            };
            _teamStatistics.Add(teamStatistics);
            return this;
        }

        public teamStatistics[] Build()
        {
            return _teamStatistics.ToArray();
        }
    }
}
