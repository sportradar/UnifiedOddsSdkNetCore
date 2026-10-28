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

public class TournamentInfoExportTests
{
    private readonly tournamentInfoEndpoint _apiTournamentInfo;
    private readonly CultureInfo _language;
    private readonly ISportDataProvider _sportDataProvider;
    private readonly ITournament _tournament;

    public TournamentInfoExportTests(ITestOutputHelper outputHelper)
    {
        _apiTournamentInfo = SoccerTournament.TournamentInfo();
        var configuration = UofConfigurations.SingleLanguage;
        _language = configuration.DefaultLanguage;
        _sportDataProvider = SportEventUnitBuilder.Create()
                                                  .WithLoggerFactory(outputHelper)
                                                  .WithConfiguration(configuration)
                                                  .WithTournamentInfoFetcherReturning(_apiTournamentInfo)
                                                  .Build()
                                                  .SportDataProvider;
        _tournament = (ITournament)_sportDataProvider.GetSportEvent(_apiTournamentInfo.GetTournamentId());
    }

    [Fact]
    public async Task GetNameAsyncWhenTournamentIsExportedThenNameIsPreserved()
    {
        var name = await _tournament.GetNameAsync(_language);
        var exportedTournament = await ExportTournamentAsync();

        name.ShouldBe(_apiTournamentInfo.tournament.name);
        exportedTournament.Names[_language].ShouldBe(name);
    }

    [Fact]
    public async Task GetSportIdAsyncWhenTournamentIsExportedThenSportIsPreserved()
    {
        var sportId = await _tournament.GetSportIdAsync();
        var sport = await _sportDataProvider.GetSportAsync(sportId, _language);
        var exportedTournament = await ExportTournamentAsync();

        sport.Id.ShouldBe(_apiTournamentInfo.GetSportId());
        sport.GetName(_language).ShouldBe(_apiTournamentInfo.tournament.sport.name);
        exportedTournament.SportId.ShouldBe(sport.Id.ToString());
    }

    [Fact]
    public async Task GetCategoryAsyncWhenTournamentIsExportedThenCategoryIsPreserved()
    {
        var category = await _tournament.GetCategoryAsync();
        var exportedTournament = await ExportTournamentAsync();

        category.ShouldNotBeNull();
        category.Id.ToString().ShouldBe(_apiTournamentInfo.tournament.category.id);
        category.GetName(_language).ShouldBe(_apiTournamentInfo.tournament.category.name);
        exportedTournament.CategoryId.ShouldBe(category.Id.ToString());
    }

    [Fact]
    public async Task GetScheduledTimeAsyncWhenTournamentIsExportedThenScheduledTimeIsPreserved()
    {
        var scheduled = await _tournament.GetScheduledTimeAsync();
        var exportedTournament = await ExportTournamentAsync();

        scheduled.ShouldNotBeNull();
        scheduled.Value.Date.ShouldBe(_apiTournamentInfo.tournament.current_season.start_date.Date);
        exportedTournament.Scheduled.ShouldBe(scheduled);
    }

    [Fact]
    public async Task GetScheduledEndTimeAsyncWhenTournamentIsExportedThenScheduledEndTimeIsPreserved()
    {
        var scheduledEnd = await _tournament.GetScheduledEndTimeAsync();
        var exportedTournament = await ExportTournamentAsync();

        scheduledEnd.ShouldNotBeNull();
        scheduledEnd.Value.Date.ShouldBe(_apiTournamentInfo.tournament.current_season.end_date.Date);
        exportedTournament.ScheduledEnd.ShouldBe(scheduledEnd);
    }

    [Fact]
    public async Task GetTournamentCoverageWhenTournamentIsExportedThenLiveCoverageIsPreserved()
    {
        var coverage = await _tournament.GetTournamentCoverage();
        var exportedTournament = await ExportTournamentAsync();

        coverage.ShouldNotBeNull();
        coverage.LiveCoverage.ShouldBeTrue();
        exportedTournament.TournamentCoverage.LiveCoverage.ShouldBe(coverage.LiveCoverage);
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenGroupsArePreserved()
    {
        var groups = (await _tournament.GetCurrentSeasonAsync()).Groups.ToList();
        var exportedTournament = await ExportTournamentAsync();
        var exportedGroups = exportedTournament.Groups.ToList();

        exportedGroups.Count.ShouldBe(groups.Count);
        exportedGroups[0].Id.ShouldBe(groups[0].Id);
        exportedGroups[0].Name.ShouldBe(groups[0].Name);
        exportedGroups[0].Competitors.ShouldBe(groups[0].Competitors.Select(competitor => competitor.Id.ToString()));
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenRoundIsPreserved()
    {
        var round = (await _tournament.GetCurrentSeasonAsync()).CurrentRound;
        var exportedTournament = await ExportTournamentAsync();

        round.ShouldNotBeNull();
        round.Type.ShouldBe(_apiTournamentInfo.round.type);
        round.Number.ShouldBe(_apiTournamentInfo.round.number);
        exportedTournament.Round.Type.ShouldBe(round.Type);
        exportedTournament.Round.Number.ShouldBe(round.Number);
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenYearIsPreserved()
    {
        var year = (await _tournament.GetCurrentSeasonAsync()).Year;
        var exportedTournament = await ExportTournamentAsync();

        year.ShouldBe(_apiTournamentInfo.tournament.current_season.year);
        exportedTournament.Year.ShouldBe(year);
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenSeasonCoverageIsPreserved()
    {
        var apiCoverage = _apiTournamentInfo.season_coverage_info;

        var coverage = (await _tournament.GetCurrentSeasonAsync()).Coverage;
        var exportedTournament = await ExportTournamentAsync();

        coverage.ShouldNotBeNull();
        coverage.SeasonId.ToString().ShouldBe(apiCoverage.season_id);
        coverage.Scheduled.ShouldBe(apiCoverage.scheduled);
        coverage.Played.ShouldBe(apiCoverage.played);
        coverage.MinCoverageLevel.ShouldBe(apiCoverage.min_coverage_level);
        coverage.MaxCovered.ShouldBe(apiCoverage.max_covered);
        exportedTournament.SeasonCoverage.SeasonId.ShouldBe(coverage.SeasonId.ToString());
        exportedTournament.SeasonCoverage.Scheduled.ShouldBe(coverage.Scheduled);
        exportedTournament.SeasonCoverage.Played.ShouldBe(coverage.Played);
        exportedTournament.SeasonCoverage.MaxCoverageLevel.ShouldBe(apiCoverage.max_coverage_level);
        exportedTournament.SeasonCoverage.MinCoverageLevel.ShouldBe(coverage.MinCoverageLevel);
        exportedTournament.SeasonCoverage.MaxCovered.ShouldBe(coverage.MaxCovered);
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenTournamentInfoBasicIsPreserved()
    {
        var category = await _tournament.GetCategoryAsync();
        var currentSeason = await _tournament.GetCurrentSeasonAsync();
        var exportedTournament = await ExportTournamentAsync();

        exportedTournament.TournamentInfoBasic.ShouldNotBeNull();
        exportedTournament.TournamentInfoBasic.Id.ShouldBe(_tournament.Id.ToString());
        exportedTournament.TournamentInfoBasic.Names[_language].ShouldBe(await _tournament.GetNameAsync(_language));
        exportedTournament.TournamentInfoBasic.Category.ShouldBe(category.Id.ToString());
        exportedTournament.TournamentInfoBasic.CurrentSeason.Id.ShouldBe(currentSeason.Id.ToString());
        exportedTournament.TournamentInfoBasic.CurrentSeason.Names[_language].ShouldBe(currentSeason.GetName(_language));
    }

    [Fact]
    public async Task GetCurrentSeasonAsyncWhenTournamentIsExportedThenCompetitorReferencesArePreserved()
    {
        var competitor = (await _tournament.GetCurrentSeasonAsync()).Groups.First().Competitors.First();
        var exportedTournament = await ExportTournamentAsync();

        exportedTournament.CompetitorsReferences[competitor.Id.ToString()]["betradar"].ShouldBe(competitor.References.BetradarId.ToString());
    }

    private async Task<ExportableTournamentInfo> ExportTournamentAsync()
    {
        var exportedSportEvents = await _sportDataProvider.CacheExportAsync(CacheType.SportEvent);
        return exportedSportEvents.OfType<ExportableTournamentInfo>().First(info => info.Id == _tournament.Id.ToString());
    }
}
