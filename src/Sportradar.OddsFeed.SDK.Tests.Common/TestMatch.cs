using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestMatch : IMatch
    {
        public TestMatch(URN id)
        {
            Id = id;
        }

        public URN Id { get; }

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

        public Task<IDelayedInfo> GetDelayedInfoAsync()
        {
            return Task.FromResult<IDelayedInfo>(null);
        }

        public Task<ISportEventConditions> GetConditionsAsync()
        {
            return Task.FromResult<ISportEventConditions>(null);
        }

        public Task<IEnumerable<ICompetitor>> GetCompetitorsAsync()
        {
            var competitors = new List<ICompetitor>()
                              {
                                  new TestCompetitor(URN.Parse("sr:competitor:1"), "First competitor", new CultureInfo("en")),
                                  new TestCompetitor(URN.Parse("sr:competitor:2"), "Second competitor", new CultureInfo("en"))
                              };
            return Task.FromResult<IEnumerable<ICompetitor>>(competitors);
        }

        public Task<EventStatus?> GetEventStatusAsync()
        {
            return Task.FromResult<EventStatus?>(null);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }

        public Task<ITeamCompetitor> GetHomeCompetitorAsync()
        {
            return Task.FromResult<ITeamCompetitor>(null);
        }

        public Task<ITeamCompetitor> GetAwayCompetitorAsync()
        {
            return Task.FromResult<ITeamCompetitor>(null);
        }

        Task<ISeasonInfo> IMatch.GetSeasonAsync()
        {
            return Task.FromResult<ISeasonInfo>(null);
        }

        public Task<IRound> GetTournamentRoundAsync()
        {
            return Task.FromResult<IRound>(null);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }
    }
}
