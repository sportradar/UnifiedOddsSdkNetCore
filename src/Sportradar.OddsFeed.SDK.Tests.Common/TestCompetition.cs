using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestCompetition : ICompetition
    {
        public TestCompetition(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

        public Task<ICompetitionStatus> GetStatusAsync()
        {
            return Task.FromResult<ICompetitionStatus>(null);
        }

        public Task<BookingStatus?> GetBookingStatusAsync()
        {
            return Task.FromResult<BookingStatus?>(null);
        }

        public Task<URN> GetSportIdAsync()
        {
            return Task.FromResult<URN>(null);
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

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
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
}
