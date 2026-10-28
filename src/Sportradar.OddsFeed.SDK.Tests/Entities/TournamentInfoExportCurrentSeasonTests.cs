// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;
using Sportradar.OddsFeed.SDK.Tests.Common.TestUnits.EntityUnit;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class TournamentInfoExportCurrentSeasonTests
{
    private readonly currentSeason _apiCurrentSeason;
    private readonly CultureInfo _language;
    private readonly ISportDataProvider _sportDataProvider;
    private readonly ITournament _tournament;

    public TournamentInfoExportCurrentSeasonTests(ITestOutputHelper outputHelper)
    {
        var apiTournamentInfo = SoccerTournament.TournamentInfo();
        _apiCurrentSeason = apiTournamentInfo.tournament.current_season;
        var configuration = UofConfigurations.SingleLanguage;
        _language = configuration.DefaultLanguage;
        _sportDataProvider = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(outputHelper)
                                                  .WithConfiguration(configuration)
                                                  .WithTournamentInfoFetcherReturning(apiTournamentInfo)
                                                  .Build()
                                                  .SportDataProvider;
        _tournament = (ITournament)_sportDataProvider.GetSportEvent(apiTournamentInfo.GetTournamentId());
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenIdIsPreserved()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        currentSeason.Id.ToString().ShouldBe(_apiCurrentSeason.id);
        exportedCurrentSeason.Id.ShouldBe(currentSeason.Id.ToString());
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenNameIsPreserved()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        currentSeason.GetName(_language).ShouldBe(_apiCurrentSeason.name);
        exportedCurrentSeason.Names[_language].ShouldBe(currentSeason.GetName(_language));
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenYearIsPreserved()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        currentSeason.Year.ShouldBe(_apiCurrentSeason.year);
        exportedCurrentSeason.Year.ShouldBe(currentSeason.Year);
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenStartDateIsPreserved()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        currentSeason.StartDate.Date.ShouldBe(_apiCurrentSeason.start_date.Date);
        exportedCurrentSeason.StartDate.Date.ShouldBe(currentSeason.StartDate.Date);
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenEndDateIsPreserved()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        currentSeason.EndDate.Date.ShouldBe(_apiCurrentSeason.end_date.Date);
        exportedCurrentSeason.EndDate.Date.ShouldBe(currentSeason.EndDate.Date);
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenSeasonCoverageIsNotIncluded()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        exportedCurrentSeason.SeasonCoverage.ShouldBeNull();
        currentSeason.Coverage.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenGroupsAreNotIncluded()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        exportedCurrentSeason.Groups.ShouldBeNull();
        currentSeason.Groups.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenCurrentRoundIsNotIncluded()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        exportedCurrentSeason.CurrentRound.ShouldBeNull();
        currentSeason.CurrentRound.ShouldNotBeNull();
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenCompetitorsAreNotIncluded()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        exportedCurrentSeason.Competitors.ShouldBeNull();
        currentSeason.Competitors.Select(competitor => competitor.Id.ToString()).ShouldBe(currentSeason.Groups.SelectMany(group => group.Competitors).Select(competitor => competitor.Id.ToString()));
    }

    [Fact]
    public async Task WhenCurrentSeasonIsExportedThenScheduleIsNotIncluded()
    {
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedCurrentSeason = await ExportCurrentSeasonAsync();

        exportedCurrentSeason.Schedule.ShouldBeNull();
        currentSeason.Schedule.ShouldBeNull();
    }

    private async Task<ExportableCurrentSeasonInfo> ExportCurrentSeasonAsync()
    {
        var exportedSportEvents = await _sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        return exportedSportEvents.OfType<ExportableTournamentInfo>().First(info => info.Id == _tournament.Id.ToString()).CurrentSeasonInfo;
    }
}
