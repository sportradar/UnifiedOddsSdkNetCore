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
    public class TestStage : IStage
    {
        private List<ITeamCompetitor> _competitors;

        public TestStage(URN id)
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

        public Task<ICompetitionStatus> GetStatusAsync()
        {
            return Task.FromResult<ICompetitionStatus>(null);
        }

        public Task<BookingStatus?> GetBookingStatusAsync()
        {
            return Task.FromResult<BookingStatus?>(null);
        }

        public Task<ITournament> GetTournamentAsync()
        {
            return Task.FromResult<ITournament>(null);
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

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult($"Stage {Id} name {culture.TwoLetterISOLanguageName}");
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

        public Task<ISportSummary> GetSportAsync()
        {
            return Task.FromResult<ISportSummary>(null);
        }

        public Task<ICategorySummary> GetCategoryAsync()
        {
            return Task.FromResult<ICategorySummary>(null);
        }

        public Task<IStage> GetParentStageAsync()
        {
            return Task.FromResult<IStage>(null);
        }

        public Task<IEnumerable<IStage>> GetStagesAsync()
        {
            return Task.FromResult<IEnumerable<IStage>>(null);
        }

        public Task<StageType> GetStageTypeAsync()
        {
            return Task.FromResult(StageType.Parent);
        }

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        Task<StageType?> IStage.GetStageTypeAsync()
        {
            return Task.FromResult<StageType?>(null);
        }
    }
}
