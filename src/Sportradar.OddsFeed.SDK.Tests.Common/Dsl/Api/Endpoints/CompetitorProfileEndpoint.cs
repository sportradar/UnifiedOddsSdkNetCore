// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class CompetitorProfileEndpoint
{
    private teamExtended _competitor;
    private venue _venue;
    private jersey[] _jerseys;
    private manager _manager;
    private playerExtended[] _players;
    private raceDriverProfile _raceDriverProfile;

    public CompetitorProfileEndpoint WithCompetitor(Func<CompetitorBuilder, CompetitorBuilder> builderFunc)
    {
        _competitor = builderFunc(new CompetitorBuilder()).BuildTeamExtended();
        return this;
    }

    public CompetitorProfileEndpoint WithVenue(Func<VenueBuilder, VenueBuilder> builderFunc)
    {
        _venue = builderFunc(new VenueBuilder()).Build();
        return this;
    }

    public CompetitorProfileEndpoint AddJersey(Func<JerseyBuilder, JerseyBuilder> builderFunc)
    {
        _jerseys ??= [];
        var list = _jerseys.ToList();
        list.Add(builderFunc(new JerseyBuilder()).Build());
        _jerseys = list.ToArray();
        return this;
    }

    public CompetitorProfileEndpoint WithManager(Urn managerId, string name, string countryCode, string nationality)
    {
        _manager = new manager
        {
            id = managerId.ToString(),
            name = name,
            country_code = countryCode,
            nationality = nationality
        };
        return this;
    }

    public CompetitorProfileEndpoint AddPlayer(Func<PlayerBuilder, PlayerBuilder> builderFunc)
    {
        _players ??= [];
        var list = _players.ToList();
        list.Add(builderFunc(new PlayerBuilder()).BuildExtended());
        _players = list.ToArray();
        return this;
    }

    public CompetitorProfileEndpoint WithRaceDriver(Func<RaceDriverProfileBuilder, RaceDriverProfileBuilder> builderFunc)
    {
        _raceDriverProfile = builderFunc(new RaceDriverProfileBuilder()).Build();
        return this;
    }

    public competitorProfileEndpoint Build()
    {
        return new competitorProfileEndpoint
        {
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            competitor = _competitor,
            jerseys = _jerseys,
            manager = _manager,
            players = _players,
            race_driver_profile = _raceDriverProfile,
            venue = _venue
        };
    }

    public static competitorProfileEndpoint BuildCompetitor(teamCompetitor team)
    {
        var teamExtendedCompetitor = new teamExtended
        {
            id = team.id,
            name = team.name,
            country = team.country,
            country_code = team.country_code,
            state = team.state,
            abbreviation = team.abbreviation,
            short_name = team.short_name,
            division = team.division,
            division_name = team.division_name,
            divisionSpecified = team.divisionSpecified,
            @virtual = team.@virtual,
            virtualSpecified = team.virtualSpecified,
            age_group = team.age_group,
            gender = team.gender,
            reference_ids = team.reference_ids,
            players = team.players
        };

        return new competitorProfileEndpoint
        {
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            competitor = teamExtendedCompetitor
        };
    }
}
