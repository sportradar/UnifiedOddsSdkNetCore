// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <tournament id="sr:tournament:225" name="DEL">
//   <sport id="sr:sport:4" name="Ice Hockey"/>
//   <category id="sr:category:41" name="Germany" country_code="DEU"/>
// </tournament>
public class TournamentBuilder
{
    private readonly tournamentExtended _tournament = new tournamentExtended();

    public TournamentBuilder WithId(Urn id)
    {
        _tournament.id = id.ToString();
        return this;
    }

    public TournamentBuilder WithName(string name)
    {
        _tournament.name = name;
        return this;
    }

    public TournamentBuilder WithScheduled(DateTime scheduled)
    {
        _tournament.scheduled = scheduled;
        _tournament.scheduledSpecified = true;
        return this;
    }

    public TournamentBuilder WithScheduledEnd(DateTime scheduled)
    {
        _tournament.scheduled_end = scheduled;
        _tournament.scheduled_endSpecified = true;
        return this;
    }

    public TournamentBuilder WithSport(Urn sportId, string name)
    {
        _tournament.sport = new sport
        {
            id = sportId.ToString(),
            name = name
        };
        return this;
    }

    public TournamentBuilder WithCategory(Urn categoryId, string name, string countryCode)
    {
        _tournament.category = new category
        {
            id = categoryId.ToString(),
            name = name,
            country_code = countryCode
        };
        return this;
    }

    public TournamentBuilder WithCurrentSeason(currentSeason currentSeason)
    {
        _tournament.current_season = currentSeason;
        return this;
    }

    public TournamentBuilder WithSeasonCoverageInfo(seasonCoverageInfo seasonCoverageInfo)
    {
        _tournament.season_coverage_info = seasonCoverageInfo;
        return this;
    }

    public TournamentBuilder AddTeam(Func<CompetitorBuilder> builder)
    {
        _tournament.competitors ??= [];
        var list = _tournament.competitors.ToList();
        list.Add(builder().Build());
        _tournament.competitors = list.ToArray();
        return this;
    }

    public tournament Build()
    {
        return _tournament;
    }
}
