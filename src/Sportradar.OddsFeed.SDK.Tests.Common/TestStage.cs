using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class TestStage : IStage
{
    private List<ITeamCompetitor> _competitors;

    public TestStage(Urn id)
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

    public Task<IEnumerable<IStage>> GetAdditionalParentStagesAsync()
    {
        throw new NotImplementedException();
    }

    Task<IStageStatus> IStage.GetStatusAsync()
    {
        return Task.FromResult<IStageStatus>(null);
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

    public Task<Urn> GetReplacedByAsync()
    {
        return Task.FromResult<Urn>(null);
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
