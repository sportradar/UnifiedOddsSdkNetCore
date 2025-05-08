// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <season start_date="2024-09-19" end_date="2025-04-29" year="24/25" tournament_id="sr:tournament:225" id="sr:season:118991" name="DEL 24/25"/>
public class SeasonBuilder
{
    private Urn _id;
    private string _name;
    private Urn _tournamentId;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private string _year;

    public SeasonBuilder WithId(Urn id)
    {
        _id = id;
        return this;
    }

    public SeasonBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SeasonBuilder WithTournamentId(Urn tournamentId)
    {
        _tournamentId = tournamentId;
        return this;
    }

    public SeasonBuilder WithStartDate(DateOnly startDate)
    {
        _startDate = new DateTime(startDate, new TimeOnly());
        return this;
    }

    public SeasonBuilder WithStartDate(DateTime startDate)
    {
        _startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0);
        return this;
    }

    public SeasonBuilder WithEndDate(DateOnly endDate)
    {
        _endDate = new DateTime(endDate, new TimeOnly());
        return this;
    }

    public SeasonBuilder WithEndDate(DateTime endDate)
    {
        _endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0);
        return this;
    }

    public SeasonBuilder WithStartTime(TimeOnly startTime)
    {
        _startTime = _startDate.AddTime(startTime);
        return this;
    }

    public SeasonBuilder WithEndTime(TimeOnly endTime)
    {
        _endTime = _endDate.AddTime(endTime);
        return this;
    }

    public SeasonBuilder WithStartTime(DateTime startTime)
    {
        if (_startDate != null)
        {
            _startTime = new DateTime(_startDate.Value.Year, _startDate.Value.Month, _startDate.Value.Day, startTime.Hour, startTime.Minute, startTime.Second);
        }
        return this;
    }

    public SeasonBuilder WithEndTime(DateTime endTime)
    {
        if (_endDate != null)
        {
            _endTime = new DateTime(_endDate.Value.Year, _endDate.Value.Month, _endDate.Value.Day, endTime.Hour, endTime.Minute, endTime.Second);
        }
        return this;
    }

    public SeasonBuilder WithYear(string year)
    {
        _year = year;
        return this;
    }

    public season Build()
    {
        return new season
        {
            id = _id.ToString(),
            name = _name
        };
    }

    public seasonExtended BuildExtended()
    {
        return GetSeasonAsCurrentSeason();
    }

    public currentSeason BuildCurrentSeason()
    {
        return GetSeasonAsCurrentSeason();
    }

    private currentSeason GetSeasonAsCurrentSeason()
    {
        var season = new currentSeason
        {
            id = _id.ToString(),
            name = _name,
            year = _year,
            start_timeSpecified = false,
            end_timeSpecified = false
        };
        if (_startDate != null)
        {
            season.start_date = _startDate.Value;
        }
        if (_endDate != null)
        {
            season.end_date = _endDate.Value;
        }
        if (_startTime != null)
        {
            season.start_time = _startTime.Value;
            season.start_timeSpecified = true;
        }
        if (_endTime != null)
        {
            season.end_time = _endTime.Value;
            season.end_timeSpecified = true;
        }
        if (_tournamentId != null)
        {
            season.tournament_id = _tournamentId.ToString();
        }
        return season;
    }

    public currentSeason BuildCurrent()
    {
        var season = new currentSeason
        {
            id = _id.ToString(),
            name = _name,
            year = _year,
            start_timeSpecified = false,
            end_timeSpecified = false
        };
        if (_startDate != null)
        {
            season.start_date = _startDate.Value;
        }
        if (_endDate != null)
        {
            season.end_date = _endDate.Value;
        }
        if (_startTime != null)
        {
            season.start_time = _startTime.Value;
            season.start_timeSpecified = true;
        }
        if (_endTime != null)
        {
            season.end_time = _endTime.Value;
            season.end_timeSpecified = true;
        }
        if (_tournamentId != null)
        {
            season.tournament_id = _tournamentId.ToString();
        }
        return season;
    }
}
