using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestMatch : IMatch
    {
        private List<ITeamCompetitor> _competitors;

        public TestMatch(URN id)
        {
            Id = id;
            _competitors = new List<ITeamCompetitor>()
            {
                new TestTeamCompetitor(URN.Parse("sr:competitor:1"), "First competitor", new CultureInfo("en")),
                new TestTeamCompetitor(URN.Parse("sr:competitor:2"), "Second competitor", new CultureInfo("en"))
            };
        }

        public void SetCompetitors(ICollection<ITeamCompetitor> competitors)
        {
            _competitors = competitors.ToList();
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
            return Task.FromResult<IEnumerable<ICompetitor>>(_competitors);
        }

        public Task<EventStatus?> GetEventStatusAsync()
        {
            return Task.FromResult<EventStatus?>(null);
        }

        /// <inheritdoc />
        public Task<IEnumerable<URN>> GetCompetitorIdsAsync()
        {
            return Task.FromResult(_competitors.Select(s => s.Id));
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
