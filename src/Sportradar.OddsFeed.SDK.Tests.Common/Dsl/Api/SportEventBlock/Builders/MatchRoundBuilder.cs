// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

/// <summary>
/// Match round builder (in xml called tournament_round)
/// <example>
/// <tournament_round type="cup" name="Semifinal" cup_round_matches="7" cup_round_match_number="4" betradar_id="3048" betradar_name="DEL, Playoffs"/>
/// </example>
/// </summary>
public class MatchRoundBuilder
{
    private readonly matchRound _round = new matchRound
    {
        betradar_idSpecified = false,
        cup_round_match_numberSpecified = false,
        cup_round_matchesSpecified = false,
        numberSpecified = false
    };

    public MatchRoundBuilder WithName(string name)
    {
        _round.name = name;
        return this;
    }

    public MatchRoundBuilder WithType(string type)
    {
        _round.type = type;
        return this;
    }

    public MatchRoundBuilder WithCupRoundMatches(int cupRoundMatches)
    {
        _round.cup_round_matches = cupRoundMatches;
        _round.cup_round_matchesSpecified = true;
        return this;
    }

    public MatchRoundBuilder WithCupRoundMatchNumber(int cupRoundMatchNumber)
    {
        _round.cup_round_match_number = cupRoundMatchNumber;
        _round.cup_round_match_numberSpecified = true;
        return this;
    }

    public MatchRoundBuilder WithBetradarId(int betradarId)
    {
        _round.betradar_id = betradarId;
        _round.betradar_idSpecified = true;
        return this;
    }

    public MatchRoundBuilder WithBetradarName(string betradarName)
    {
        _round.betradar_name = betradarName;
        return this;
    }

    public MatchRoundBuilder WithNumber(int number)
    {
        _round.number = number;
        _round.numberSpecified = true;
        return this;
    }

    public MatchRoundBuilder WithGroupId(Urn groupId)
    {
        _round.group_id = groupId.ToString();
        return this;
    }

    public MatchRoundBuilder WithGroup(string group)
    {
        _round.group = group;
        return this;
    }

    public MatchRoundBuilder WithGroupName(string name)
    {
        _round.name = name;
        return this;
    }

    public MatchRoundBuilder WithGroupLongName(string groupLongName)
    {
        _round.group_long_name = groupLongName;
        return this;
    }

    public MatchRoundBuilder WithOtherMatchId(Urn otherMatchId)
    {
        _round.other_match_id = otherMatchId.ToString();
        return this;
    }

    public MatchRoundBuilder WithPhase(string phase)
    {
        _round.phase = phase;
        return this;
    }

    public matchRound Build()
    {
        return _round;
    }
}
