using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class TestTournament : ITournament
    {
        public TestTournament(URN id)
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

        public Task<URN> GetReplacedByAsync()
        {
            return Task.FromResult<URN>(null);
        }

        public Task<DateTime?> GetNextLiveTimeAsync()
        {
            return Task.FromResult<DateTime?>(null);
        }

        public Task<string> GetNameAsync(CultureInfo culture)
        {
            return Task.FromResult<string>(null);
        }

        public Task<ISportSummary> GetSportAsync()
        {
            return Task.FromResult<ISportSummary>(null);
        }

        public Task<ITournamentCoverage> GetTournamentCoverage()
        {
            return Task.FromResult<ITournamentCoverage>(null);
        }

        public Task<ICategorySummary> GetCategoryAsync()
        {
            return Task.FromResult<ICategorySummary>(null);
        }

        public Task<ICurrentSeasonInfo> GetCurrentSeasonAsync()
        {
            return Task.FromResult<ICurrentSeasonInfo>(null);
        }

        public Task<IEnumerable<ISeason>> GetSeasonsAsync()
        {
            return Task.FromResult<IEnumerable<ISeason>>(null);
        }

        public Task<bool?> GetExhibitionGamesAsync()
        {
            return Task.FromResult<bool?>(null);
        }

        public Task<ISeasonCoverage> GetCoverageAsync()
        {
            return Task.FromResult<ISeasonCoverage>(null);
        }

        public Task<ISeason> GetSeasonAsync()
        {
            return Task.FromResult<ISeason>(null);
        }

        public Task<IEnumerable<IGroup>> GetGroupsAsync()
        {
            return Task.FromResult<IEnumerable<IGroup>>(null);
        }

        public Task<IRound> GetCurrentRoundAsync()
        {
            return Task.FromResult<IRound>(null);
        }

        public Task<IEnumerable<ICompetition>> GetScheduleAsync()
        {
            return Task.FromResult<IEnumerable<ICompetition>>(null);
        }
    }
}
