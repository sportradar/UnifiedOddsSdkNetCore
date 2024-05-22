// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestCompetition : ICompetition
{
    public TestCompetition(Urn id)
    {
        Id = id;
    }

    public Urn Id { get; }

    public Task<ICompetitionStatus> GetStatusAsync()
    {
        return Task.FromResult<ICompetitionStatus>(null);
    }

    public Task<BookingStatus?> GetBookingStatusAsync()
    {
        return Task.FromResult<BookingStatus?>(null);
    }

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

    public Task<Urn> GetReplacedByAsync()
    {
        return Task.FromResult<Urn>(null);
    }

    public Task<DateTime?> GetNextLiveTimeAsync()
    {
        return Task.FromResult<DateTime?>(null);
    }

    public Task<IEnumerable<ITeamCompetitor>> GetCompetitorsAsync()
    {
        return Task.FromResult<IEnumerable<ITeamCompetitor>>(null);
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

    public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync(CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Urn>> GetCompetitorIdsAsync()
    {
        return Task.FromResult<IEnumerable<Urn>>(null);
    }

    public Task<IGroup> GetGroupAsync()
    {
        return Task.FromResult<IGroup>(null);
    }

    public Task<ITournament> GetTournamentAsync()
    {
        return Task.FromResult<ITournament>(null);
    }

    public Task<IRound> GetTournamentRoundAsync()
    {
        return Task.FromResult<IRound>(null);
    }

    public Task<IVenue> GetVenueAsync()
    {
        return Task.FromResult<IVenue>(null);
    }

    public Task<IFixture> GetFixtureAsync()
    {
        return Task.FromResult<IFixture>(null);
    }

    public Task<ISportEventConditions> GetConditionsAsync()
    {
        return Task.FromResult<ISportEventConditions>(null);
    }

    Task<IEnumerable<ICompetitor>> ICompetition.GetCompetitorsAsync()
    {
        return Task.FromResult<IEnumerable<ICompetitor>>(null);
    }

    public Task<string> GetNameAsync(CultureInfo culture)
    {
        return Task.FromResult<string>(null);
    }
}
