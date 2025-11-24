// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ConvertToPrimaryConstructor

namespace Sportradar.OddsFeed.SDK.Tests.API.Caching.SportEventCacheTests;

public class SportEventCacheAddTournamentTests : SportEventCacheSetup
{
    public SportEventCacheAddTournamentTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public async Task WhenAddingTournamentInfoMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoDto = new SummaryEndpoint.TournamentInfoSummaryEndpoint().Dto;

        var tournamentId = tournamentInfoDto.Id;
        var tournament = SportEventCacheItemFactory.Build(tournamentId);

        tournament.Scheduled.Should().BeNull();

        await SportEventCache.CacheAddDtoAsync(tournamentId, tournamentInfoDto, CultureInfo.CurrentCulture, DtoType.TournamentInfo, tournament);

        tournament.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoAsSportEventSummaryMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoDto = new SummaryEndpoint.TournamentInfoSummaryEndpoint().Dto;

        var tournamentId = tournamentInfoDto.Id;
        var tournament = SportEventCacheItemFactory.Build(tournamentId);

        tournament.Scheduled.Should().BeNull();

        await SportEventCache.CacheAddDtoAsync(tournamentId, tournamentInfoDto, CultureInfo.CurrentCulture, DtoType.SportEventSummary, tournament);

        tournament.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoAsSportEventSummaryListMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoDto = new SummaryEndpoint.TournamentInfoSummaryEndpoint().Dto;

        var tournamentId = tournamentInfoDto.Id;
        var tournament = SportEventCacheItemFactory.Build(tournamentId);

        tournament.Scheduled.Should().BeNull();

        var summaries = new EntityList<SportEventSummaryDto>(new[] { tournamentInfoDto });
        await SportEventCache.CacheAddDtoAsync(tournamentId, summaries, CultureInfo.CurrentCulture, DtoType.SportEventSummaryList, tournament);

        tournament.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoMergeIsInvokedOnTheRequesterAndSeasonIsUpdated()
    {
        var tournamentInfoDto = new SummaryEndpoint.TournamentInfoSummaryEndpoint().Dto;

        var seasonId = tournamentInfoDto.Season.Id;
        var season = SportEventCacheItemFactory.Build(seasonId);

        season.Scheduled.Should().BeNull();

        await SportEventCache.CacheAddDtoAsync(tournamentInfoDto.Id, tournamentInfoDto, CultureInfo.CurrentCulture, DtoType.SportEventSummary, season);

        season.Scheduled.Should().Be(tournamentInfoDto.Season.StartDate);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoAsSportEventSummaryListMergeIsInvokedOnTheRequesterAndSeasonIsUpdated()
    {
        var tournamentInfoDto = new SummaryEndpoint.TournamentInfoSummaryEndpoint().Dto;

        var seasonId = tournamentInfoDto.Season.Id;
        var season = SportEventCacheItemFactory.Build(seasonId);

        season.Scheduled.Should().BeNull();

        var summaries = new EntityList<SportEventSummaryDto>(new[] { tournamentInfoDto });
        await SportEventCache.CacheAddDtoAsync(tournamentInfoDto.Id, summaries, CultureInfo.CurrentCulture, DtoType.SportEventSummaryList, season);

        season.Scheduled.Should().Be(tournamentInfoDto.Season.StartDate);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoWithoutSeasonAsSportEventSummaryMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoSummaryEndpoint = new SummaryEndpoint.TournamentInfoSummaryEndpoint();
        tournamentInfoSummaryEndpoint.Raw.season = null;

        var tournamentInfoDto = tournamentInfoSummaryEndpoint.Dto;

        var tournamentId = tournamentInfoDto.Id;
        var tournament = SportEventCacheItemFactory.Build(tournamentId);

        tournament.Scheduled.Should().BeNull();

        await SportEventCache.CacheAddDtoAsync(tournamentId, tournamentInfoDto, CultureInfo.CurrentCulture, DtoType.SportEventSummary, tournament);

        tournament.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoWithoutSeasonAsSportEventSummaryListMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoSummaryEndpoint = new SummaryEndpoint.TournamentInfoSummaryEndpoint();
        tournamentInfoSummaryEndpoint.Raw.season = null;

        var tournamentInfoDto = tournamentInfoSummaryEndpoint.Dto;

        var sportEvent = SportEventCacheItemFactory.Build(tournamentInfoDto.Id);

        sportEvent.Scheduled.Should().BeNull();

        var summaries = new EntityList<SportEventSummaryDto>(new[] { tournamentInfoDto });
        await SportEventCache.CacheAddDtoAsync(tournamentInfoDto.Id, summaries, CultureInfo.CurrentCulture, DtoType.SportEventSummaryList, sportEvent);

        sportEvent.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoWithoutCurrentSeasonAsSportEventSummaryMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoSummaryEndpoint = new SummaryEndpoint.TournamentInfoSummaryEndpoint();
        tournamentInfoSummaryEndpoint.Raw.tournament.current_season = null;

        var tournamentInfoDto = tournamentInfoSummaryEndpoint.Dto;

        var tournamentId = tournamentInfoDto.Id;
        var tournament = SportEventCacheItemFactory.Build(tournamentId);

        tournament.Scheduled.Should().BeNull();

        await SportEventCache.CacheAddDtoAsync(tournamentId, tournamentInfoDto, CultureInfo.CurrentCulture, DtoType.SportEventSummary, tournament);

        tournament.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }

    [Fact]
    public async Task WhenAddingTournamentInfoWithoutCurrentSeasonAsSportEventSummaryListMergeIsInvokedOnTheRequesterAndTournamentIsUpdated()
    {
        var tournamentInfoSummaryEndpoint = new SummaryEndpoint.TournamentInfoSummaryEndpoint();
        tournamentInfoSummaryEndpoint.Raw.tournament.current_season = null;

        var tournamentInfoDto = tournamentInfoSummaryEndpoint.Dto;

        var sportEvent = SportEventCacheItemFactory.Build(tournamentInfoDto.Id);

        sportEvent.Scheduled.Should().BeNull();

        var summaries = new EntityList<SportEventSummaryDto>(new[] { tournamentInfoDto });
        await SportEventCache.CacheAddDtoAsync(tournamentInfoDto.Id, summaries, CultureInfo.CurrentCulture, DtoType.SportEventSummaryList, sportEvent);

        sportEvent.Scheduled.Should().Be(tournamentInfoDto.Scheduled);
    }
}
