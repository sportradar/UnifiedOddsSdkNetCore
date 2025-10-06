// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestMatch : IMatch
{
    private List<ITeamCompetitor> _competitors;

    public TestMatch(Urn id)
    {
        Id = id;
        _competitors = new List<ITeamCompetitor>()
                           {
                               new TestTeamCompetitor(Urn.Parse("sr:competitor:1"), "First competitor", new CultureInfo("en")),
                               new TestTeamCompetitor(Urn.Parse("sr:competitor:2"), "Second competitor", new CultureInfo("en"))
                           };
    }

    public void SetCompetitors(ICollection<ITeamCompetitor> competitors)
    {
        _competitors = competitors.ToList();
    }

    public Urn Id { get; }

    public Task<Urn> GetSportIdAsync()
    {
        return Task.FromResult<Urn>(null);
    }

    public Task<DateTime?> GetScheduledTimeAsync()
    {
        return Task.FromResult<DateTime?>(null);
    }

    public Task<DateTime?> GetScheduledEndTimeAsync()
    {
        return Task.FromResult<DateTime?>(null);
    }

    public Task<bool?> GetStartTimeTbdAsync()
    {
        return Task.FromResult<bool?>(null);
    }

    Task<ICompetitionStatus> ICompetition.GetStatusAsync()
    {
        return Task.FromResult<ICompetitionStatus>(null);
    }

    Task<IMatchStatus> IMatch.GetStatusAsync()
    {
        return Task.FromResult<IMatchStatus>(null);
    }

    public Task<BookingStatus?> GetBookingStatusAsync()
    {
        return Task.FromResult<BookingStatus?>(null);
    }

    public Task<ILongTermEvent> GetTournamentAsync()
    {
        return Task.FromResult<ILongTermEvent>(null);
    }

    public Task<IVenue> GetVenueAsync()
    {
        return Task.FromResult<IVenue>(null);
    }

    public Task<IFixture> GetFixtureAsync()
    {
        return Task.FromResult<IFixture>(null);
    }

    public Task<IEventTimeline> GetEventTimelineAsync()
    {
        return Task.FromResult<IEventTimeline>(null);
    }

    public Task<IEventTimeline> GetEventTimelineAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Task<IDelayedInfo> GetDelayedInfoAsync()
    {
        return Task.FromResult<IDelayedInfo>(null);
    }

    public Task<ICoverageInfo> GetCoverageInfoAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ISportEventConditions> GetConditionsAsync()
    {
        return Task.FromResult<ISportEventConditions>(null);
    }

    public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
    {
        return Task.FromResult<IEnumerable<ICompetitor>>(_competitors);
    }

    public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Urn>> GetCompetitorIdsAsync()
    {
        return Task.FromResult(_competitors.Select(s => s.Id));
    }

    public Task<EventStatus?> GetEventStatusAsync()
    {
        return Task.FromResult<EventStatus?>(null);
    }

    public Task<SportEventType?> GetSportEventTypeAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetLiveOddsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetNameAsync(CultureInfo culture)
    {
        return Task.FromResult($"Match {Id} name {culture.TwoLetterISOLanguageName}");
    }

    public Task<ITeamCompetitor> GetHomeCompetitorAsync()
    {
        return Task.FromResult(_competitors.First());
    }

    public Task<ITeamCompetitor> GetAwayCompetitorAsync()
    {
        return Task.FromResult(_competitors.Skip(1).First());
    }

    Task<ISeasonInfo> IMatch.GetSeasonAsync()
    {
        return Task.FromResult<ISeasonInfo>(null);
    }

    public Task<IRound> GetTournamentRoundAsync()
    {
        return Task.FromResult<IRound>(null);
    }

    public Task<Urn> GetReplacedByAsync()
    {
        return Task.FromResult<Urn>(null);
    }

    public Task<DateTime?> GetNextLiveTimeAsync()
    {
        return Task.FromResult<DateTime?>(null);
    }
}
