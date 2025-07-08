// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <sport_event id="sr:match:58717423" scheduled="2025-04-08T17:30:00+00:00" start_time_tbd="false">
//      <tournament_round type="cup" name="Semifinal" cup_round_matches="7" cup_round_match_number="4" betradar_id="3048" betradar_name="DEL, Playoffs"/>
//     <season start_date="2024-09-19" end_date="2025-04-29" year="24/25" tournament_id="sr:tournament:225" id="sr:season:118991" name="DEL 24/25"/>
//     <tournament id="sr:tournament:225" name="DEL">
//          <sport id="sr:sport:4" name="Ice Hockey"/>
//          <category id="sr:category:41" name="Germany" country_code="DEU"/>
//     </tournament>
//     <competitors>
//          <competitor qualifier="home" id="sr:competitor:3853" name="Adler Mannheim" abbreviation="MAN" short_name="Mannheim" country="Germany" country_code="DEU" gender="male">
//              <reference_ids>
//                  <reference_id name="betradar" value="8882"/>
//              </reference_ids>
//          </competitor>
//          <competitor qualifier="away" id="sr:competitor:3856" name="Eisbaren Berlin" abbreviation="BER" short_name="Berlin" country="Germany" country_code="DEU" gender="male">
//              <reference_ids>
//                  <reference_id name="betradar" value="7362"/>
//              </reference_ids>
//          </competitor>
//     </competitors>
//     <venue id="sr:venue:2010" name="SAP Arena" capacity="13200" city_name="Mannheim" country_name="Germany" country_code="DEU" map_coordinates="49.46373,8.51757"/>
// </sport_event>
public class SportEventBuilder
{
    private readonly sportEvent _sportEvent = new sportEvent();

    public SportEventBuilder WithId(Urn id)
    {
        _sportEvent.id = id.ToString();
        return this;
    }

    public SportEventBuilder WithName(string name)
    {
        _sportEvent.name = name;
        return this;
    }

    public SportEventBuilder WithEventStatus(string status)
    {
        _sportEvent.status = status;
        return this;
    }

    public SportEventBuilder WithType(string type)
    {
        _sportEvent.type = type;
        return this;
    }

    public SportEventBuilder WithLiveOdds(string liveodds)
    {
        _sportEvent.liveodds = liveodds;
        return this;
    }

    public SportEventBuilder WithScheduledTime(DateTime? scheduledTime)
    {
        _sportEvent.scheduled = scheduledTime ?? DateTime.MinValue;
        _sportEvent.scheduledSpecified = scheduledTime != null;
        return this;
    }

    public SportEventBuilder WithScheduledEndTime(DateTime? scheduledEndTime)
    {
        _sportEvent.scheduled_end = scheduledEndTime ?? DateTime.MinValue;
        _sportEvent.scheduled_endSpecified = scheduledEndTime != null;
        return this;
    }

    public SportEventBuilder WithStartTimeTbd(bool? tbd = true)
    {
        _sportEvent.start_time_tbdSpecified = false;
        if (!tbd.HasValue)
        {
            return this;
        }
        _sportEvent.start_time_tbd = tbd.Value;
        _sportEvent.start_time_tbdSpecified = true;

        return this;
    }

    public SportEventBuilder WithNextLiveTime(DateTime nextLiveTime)
    {
        _sportEvent.next_live_time = nextLiveTime.ToString(CultureInfo.InvariantCulture);
        return this;
    }

    public SportEventBuilder WithSeason(Func<SeasonBuilder> builder)
    {
        _sportEvent.season = builder().BuildExtended();
        return this;
    }

    public SportEventBuilder WithTournament(Func<TournamentBuilder> builder)
    {
        _sportEvent.tournament = builder().Build();
        return this;
    }

    public SportEventBuilder WithTournamentRound(Func<MatchRoundBuilder> builder)
    {
        _sportEvent.tournament_round = builder().Build();
        return this;
    }

    public SportEventBuilder AddTeamCompetitor(Func<CompetitorBuilder> builder)
    {
        _sportEvent.competitors ??= [];
        var list = _sportEvent.competitors.ToList();
        list.Add(builder().BuildTeamCompetitor());
        _sportEvent.competitors = list.ToArray();
        return this;
    }

    public SportEventBuilder WithSportEventConditions(Func<SportEventConditionsBuilder, SportEventConditionsBuilder> builderFunc)
    {
        _sportEvent.sport_event_conditions = builderFunc(new SportEventConditionsBuilder()).Build();
        return this;
    }

    public SportEventBuilder WithVenue(Func<VenueBuilder, VenueBuilder> builderFunc)
    {
        _sportEvent.venue = builderFunc(new VenueBuilder()).Build();
        return this;
    }

    public SportEventBuilder WithStageType(string stageType)
    {
        _sportEvent.stage_type = stageType;
        return this;
    }

    public SportEventBuilder WithReplacedBy(Urn replacedBy)
    {
        _sportEvent.replaced_by = replacedBy.ToString();
        return this;
    }

    public SportEventBuilder WithParent(Func<ParentStageBuilder, ParentStageBuilder> builderFunc)
    {
        _sportEvent.parent = builderFunc(new ParentStageBuilder()).Build();
        return this;
    }

    public SportEventBuilder AddRace(Func<SportEventChildrenBuilder> builder)
    {
        _sportEvent.races ??= [];
        var list = _sportEvent.races.ToList();
        list.Add(builder().Build());
        _sportEvent.races = list.ToArray();
        return this;
    }

    public sportEvent Build()
    {
        return _sportEvent;
    }

    public fixture BuildFixture()
    {
        var fixture = new fixture
        {
            id = _sportEvent.id,
            name = _sportEvent.name,
            status = _sportEvent.status,
            type = _sportEvent.type,
            liveodds = _sportEvent.liveodds,
            scheduled = _sportEvent.scheduled,
            scheduledSpecified = _sportEvent.scheduledSpecified,
            scheduled_end = _sportEvent.scheduled_end,
            scheduled_endSpecified = _sportEvent.scheduled_endSpecified,
            start_time_tbd = _sportEvent.start_time_tbd,
            start_time_tbdSpecified = _sportEvent.start_time_tbdSpecified,
            next_live_time = _sportEvent.next_live_time,
            season = _sportEvent.season,
            tournament = _sportEvent.tournament,
            tournament_round = _sportEvent.tournament_round,
            competitors = _sportEvent.competitors,
            sport_event_conditions = _sportEvent.sport_event_conditions,
            venue = _sportEvent.venue,
            additional_parents = _sportEvent.additional_parents,
            stage_type = _sportEvent.stage_type,
            parent = _sportEvent.parent,
            races = _sportEvent.races,
            replaced_by = _sportEvent.replaced_by
        };
        return fixture;
    }
}
