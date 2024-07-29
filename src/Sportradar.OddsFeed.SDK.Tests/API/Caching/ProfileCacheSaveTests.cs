// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Caching;

[SuppressMessage("ReSharper", "CoVariantArrayConversion")]
public class ProfileCacheSaveTests : ProfileCacheSetup
{
    private readonly XUnitLogger _logExec;

    public ProfileCacheSaveTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _logExec = _testLoggerFactory.GetOrCreateLogger(typeof(ProfileCache));
    }

    [Fact]
    public void CacheWhenDisposedThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Dto;
        _profileCache.Dispose();

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void MatchWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = null;
        var dto = new MatchDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void MatchWhenWithCompetitorsThenCached()
    {
        var dto = SummaryEndpoint.AsMatch.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);

        Assert.Equal(2, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void MatchWhenAddToExistingCompetitorsThenNoNewCacheItem()
    {
        var dto = SummaryEndpoint.AsMatch.Dto;
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsMatch.Raw;
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new MatchDto(api2);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        Assert.Equal(2, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task MatchWhenAddToExistingCompetitorsThenDataIsMerged()
    {
        var dto = SummaryEndpoint.AsMatch.Dto;
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsMatch.Raw;
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new MatchDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        var competitorName1 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[0], false);
        var competitorName2 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(competitorName1, competitorName2);
        Assert.Equal(dto.Competitors.First().Name, competitorName1);
        Assert.Equal(dto2.Competitors.First().Name, competitorName2);
    }

    [Fact]
    public void MatchWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.MatchSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void MatchWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.MatchSummary, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void StageWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsStage.Raw;
        api.sport_event.competitors = null;
        var dto = new StageDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.RaceSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void StageWhenWithCompetitorsThenCached()
    {
        var dto = SummaryEndpoint.AsStage.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.RaceSummary, null);

        Assert.Equal(dto.Competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void StageWhenAddToExistingCompetitorsThenNoNewCacheItem()
    {
        var dto = SummaryEndpoint.AsStage.Dto;
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsStage.Raw;
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new StageDto(api2);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        Assert.Equal(dto.Competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task StageWhenAddToExistingCompetitorsThenDataIsMerged()
    {
        var dto = SummaryEndpoint.AsStage.Dto;
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsStage.Raw;
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new StageDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        var competitorName1 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[0], false);
        var competitorName2 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(competitorName1, competitorName2);
        Assert.Equal(dto.Competitors.First().Name, competitorName1);
        Assert.Equal(dto2.Competitors.First().Name, competitorName2);
    }

    [Fact]
    public void StageWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.RaceSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void StageWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.RaceSummary, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void TournamentInfoWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        var dto = new TournamentInfoDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void TournamentInfoWhenWithCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamList(5).ToArray();
        var dto = new TournamentInfoDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(5, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void TournamentInfoWhenAddToExistingCompetitorsThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var dto = new TournamentInfoDto(api);
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamList(5).ToArray();
        api2.competitors[0].name += " 2";
        var dto2 = new TournamentInfoDto(api2);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        Assert.Equal(dto.Competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task TournamentInfoWhenAddToExistingCompetitorsThenDataIsMerged()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamList(5).ToArray();
        var dto = new TournamentInfoDto(api);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamList(5).ToArray();
        api2.competitors[0].name += " 2";
        var dto2 = new TournamentInfoDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        var competitorName1 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[0], false);
        var competitorName2 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(competitorName1, competitorName2);
        Assert.Equal(dto.Competitors.First().Name, competitorName1);
        Assert.Equal(dto2.Competitors.First().Name, competitorName2);
    }

    [Fact]
    public void TournamentInfoWhenCompetitorsHasPlayersThenCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        var dto = new TournamentInfoDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(55, _profileMemoryCache.Count());
        Assert.Equal(5, _profileMemoryCache.GetKeys().Count(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
        Assert.Equal(50, _profileMemoryCache.GetKeys().Count(a => a.Contains("player", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void TournamentInfoWhenAddToExistingCompetitorsWithPlayersThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(2, 10).ToArray();
        var dto = new TournamentInfoDto(api);
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        api2.competitors[0].name += " 2";
        api2.competitors[0].players[0].name += " 2";
        var dto2 = new TournamentInfoDto(api2);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        Assert.Equal(55, _profileMemoryCache.Count());
        Assert.Equal(5, _profileMemoryCache.GetKeys().Count(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
        Assert.Equal(50, _profileMemoryCache.GetKeys().Count(a => a.Contains("player", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task TournamentInfoWhenAddToExistingCompetitorsWithPlayersThenCompetitorDataIsMerged()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        var dto = new TournamentInfoDto(api);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        api2.competitors[0].name += " 2";
        var dto2 = new TournamentInfoDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        var competitorName1 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[0], false);
        var competitorName2 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(competitorName1, competitorName2);
        Assert.Equal(dto.Competitors.First().Name, competitorName1);
        Assert.Equal(dto2.Competitors.First().Name, competitorName2);
    }

    [Fact]
    public async Task TournamentInfoWhenAddToExistingCompetitorsWithPlayersThenPlayerDataIsMerged()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        var dto = new TournamentInfoDto(api);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.MatchSummary, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamCompetitorWithPlayersList(5, 10).ToArray();
        api2.competitors[0].players[0].name += " 2";
        var dto2 = new TournamentInfoDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.MatchSummary, null);

        var playerName1 = await _profileCache.GetPlayerNameAsync(dto.Competitors.First().Players.First().Id, _cultures[0], false);
        var playerName2 = await _profileCache.GetPlayerNameAsync(dto.Competitors.First().Players.First().Id, _cultures[1], false);
        Assert.NotEqual(playerName1, playerName2);
        Assert.Equal(dto.Competitors.First().Players.First().Name, playerName1);
        Assert.Equal(dto2.Competitors.First().Players.First().Name, playerName2);
    }

    //TODO: does not save on api.tournament.competitors level - should save
    [Fact]
    public void TournamentInfoWhenWithSubTournamentCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.tournament.competitors = SummaryEndpoint.BuildTeamList(5).ToArray();
        var dto = new TournamentInfoDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void TournamentInfoWhenWithGroupCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.groups = new[] { SummaryEndpoint.BuildTournamentGroup(5), SummaryEndpoint.BuildTournamentGroup(5), SummaryEndpoint.BuildTournamentGroup(5) };
        var dto = new TournamentInfoDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(5, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void TournamentInfoWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void TournamentInfoWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.TournamentInfo, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void SportEventWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsSportEvent.Raw;
        var dto = new SportEventSummaryDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.SportEventSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void SportEventWhenWithCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var dto = new MatchDto(api);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.SportEventSummary, null);

        Assert.Equal(5, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void SportEventWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.SportEventSummary, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void SportEventWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.SportEventSummary, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void PlayerProfileThenCached()
    {
        var dto = CompetitorProfileEndpoint.AsPlayerExtended.Id(1).Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.PlayerProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public void PlayerProfileTWhenAddToExistingPlayerThenNoNewCacheItem()
    {
        var dto = CompetitorProfileEndpoint.AsPlayerExtended.Id(1).Dto;
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.PlayerProfile, null);
        var api2 = CompetitorProfileEndpoint.AsPlayerExtended.Id(1).Raw;
        api2.name += " 2";
        var dto2 = new PlayerProfileDto(api2, DateTime.Now);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.PlayerProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("player", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task PlayerProfileWhenAddToExistingPlayerThenDataIsMerged()
    {
        var dto = CompetitorProfileEndpoint.AsPlayerExtended.Id(1).Dto;
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.PlayerProfile, null);
        var api2 = CompetitorProfileEndpoint.AsPlayerExtended.Id(1).Raw;
        api2.name += " 2";
        var dto2 = new PlayerProfileDto(api2, DateTime.Now);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.PlayerProfile, null);

        var name1 = await _profileCache.GetPlayerNameAsync(dto.Id, _cultures[0], false);
        var name2 = await _profileCache.GetPlayerNameAsync(dto.Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(dto.Name, name1);
        Assert.Equal(dto2.Name, name2);
    }

    [Fact]
    public void PlayerProfileWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.PlayerProfile, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void PlayerProfileWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.PlayerProfile, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void CompetitorThenCached()
    {
        var api = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        var dto = new CompetitorDto(api.competitor);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.Competitor, null);

        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public void CompetitorTWhenAddToExistingCompetitorThenNoNewCacheItem()
    {
        var api = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        var dto = new CompetitorDto(api.competitor);
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.Competitor, null);
        var api2 = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        api2.competitor.name += " 2";
        var dto2 = new CompetitorDto(api2.competitor);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.Competitor, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task CompetitorWhenAddToExistingCompetitorThenDataIsMerged()
    {
        var api = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        var dto = new CompetitorDto(api.competitor);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.Competitor, null);
        var api2 = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        api2.competitor.name += " 2";
        var dto2 = new CompetitorDto(api2.competitor);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.Competitor, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(dto.Id, _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(dto.Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(dto.Name, name1);
        Assert.Equal(dto2.Name, name2);
    }

    [Fact]
    public void CompetitorWhenWrongDtoTypeThenNothingCached()
    {
        var api = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        var dto = new CompetitorDto(api.competitor);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void CompetitorWhenWrongDtoTypeThenLogConflict()
    {
        var api = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        var dto = new CompetitorDto(api.competitor);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void CompetitorProfileThenCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
    }

    [Fact]
    public void CompetitorProfileTWhenAddToExistingCompetitorThenNoNewCacheItem()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Dto;
        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);
        var api2 = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        api2.competitor.name += " 2";
        var dto2 = new CompetitorDto(api2.competitor);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.Competitor, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task CompetitorProfileWhenAddToExistingCompetitorThenDataIsMerged()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Dto;
        await _cacheManager.SaveDtoAsync(dto.Competitor.Id, dto, _cultures[0], DtoType.CompetitorProfile, null);
        var api2 = CompetitorProfileEndpoint.AsCompetitorProfile.Id(1).Raw;
        api2.competitor.name += " 2";
        var dto2 = new CompetitorProfileDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Competitor.Id, dto2, _cultures[1], DtoType.CompetitorProfile, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(dto.Competitor.Id, _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(dto.Competitor.Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(dto.Competitor.Name, name1);
        Assert.Equal(dto2.Competitor.Name, name2);
    }

    [Fact]
    public void CompetitorProfileWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.Competitor, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void CompetitorProfileWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.Competitor, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void SimpleTeamProfileWhenNoPlayersThenCached()
    {
        var apiSimpleTeamProfile = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().Raw;
        var dtoSimpleTeam = new SimpleTeamProfileDto(apiSimpleTeamProfile);

        _cacheManager.SaveDto(dtoSimpleTeam.Competitor.Id, dtoSimpleTeam, _cultures[0], DtoType.SimpleTeamProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.Contains(dtoSimpleTeam.Competitor.Id.ToString(), _profileMemoryCache.GetKeys());
    }

    [Fact]
    public void SimpleTeamWhenAddToExistingCompetitorAndNoPlayersThenNoNewCacheItem()
    {
        var api = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().Raw;
        var dto = new SimpleTeamProfileDto(api);
        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.SimpleTeamProfile, null);
        var api2 = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().Raw;
        api2.competitor.name += " 2";
        var dto2 = new SimpleTeamProfileDto(api2);

        _cacheManager.SaveDto(dto2.Competitor.Id, dto2, _cultures[1], DtoType.SimpleTeamProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("simpleteam", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task SimpleTeamWhenAddToExistingCompetitorAndNoPlayersThenDataIsMerged()
    {
        var api = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().Raw;
        var dto = new SimpleTeamProfileDto(api);
        await _cacheManager.SaveDtoAsync(dto.Competitor.Id, dto, _cultures[0], DtoType.SimpleTeamProfile, null);
        var api2 = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().Raw;
        api2.competitor.name += " 2";
        var dto2 = new SimpleTeamProfileDto(api2);

        await _cacheManager.SaveDtoAsync(dto2.Competitor.Id, dto2, _cultures[1], DtoType.SimpleTeamProfile, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(dto.Competitor.Id, _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(dto.Competitor.Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(dto.Competitor.Name, name1);
        Assert.Equal(dto2.Competitor.Name, name2);
    }

    //TODO: team players should be saved
    [Fact]
    public void SimpleTeamProfileWhenWithPlayersThenAllCached()
    {
        var dtoSimpleTeam = CompetitorProfileEndpoint.AsSimpleTeam.Id(1).ClearPlayers().AddCompetitorPlayers(1, 20).Dto;
        Assert.Equal(20, dtoSimpleTeam.Competitor.Players.Count());

        _cacheManager.SaveDto(dtoSimpleTeam.Competitor.Id, dtoSimpleTeam, _cultures[0], DtoType.SimpleTeamProfile, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        Assert.Contains(dtoSimpleTeam.Competitor.Id.ToString(), _profileMemoryCache.GetKeys());
    }

    //Missing other save

    [Fact]
    public void MatchTimelineWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsSportEvent.Raw;
        api.competitors = null;
        var matchTimeline = new matchTimelineEndpoint() { sport_event = api };
        var dto = new MatchTimelineDto(matchTimeline);

        _cacheManager.SaveDto(dto.SportEvent.Id, dto, _cultures[0], DtoType.MatchTimeline, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    //TODO: should it save?
    [Fact]
    public void MatchTimelineWhenWithCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsSportEvent.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var matchTimeline = new matchTimelineEndpoint() { sport_event = api };
        var dto = new MatchTimelineDto(matchTimeline);

        _cacheManager.SaveDto(dto.SportEvent.Id, dto, _cultures[0], DtoType.MatchTimeline, null);

        Assert.Equal(5, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void MatchTimelineWhenAddToExistingCompetitorThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsSportEvent.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var matchTimeline = new matchTimelineEndpoint() { sport_event = api };
        var dto = new MatchTimelineDto(matchTimeline);
        _cacheManager.SaveDto(dto.SportEvent.Id, dto, _cultures[0], DtoType.MatchTimeline, null);
        var api2 = SummaryEndpoint.AsSportEvent.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        api2.competitors[0].name += " 2";
        var matchTimeline2 = new matchTimelineEndpoint() { sport_event = api };
        var dto2 = new MatchTimelineDto(matchTimeline2);

        _cacheManager.SaveDto(dto2.SportEvent.Id, dto2, _cultures[1], DtoType.MatchTimeline, null);

        Assert.Equal(5, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task MatchTimelineWhenAddToExistingCompetitorThenDataIsMerged()
    {
        var api = SummaryEndpoint.AsSportEvent.Raw;
        api.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var matchTimeline = new matchTimelineEndpoint() { sport_event = api };
        var dto = new MatchTimelineDto(matchTimeline);
        await _cacheManager.SaveDtoAsync(dto.SportEvent.Id, dto, _cultures[0], DtoType.MatchTimeline, null);
        var api2 = SummaryEndpoint.AsSportEvent.Raw;
        api2.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        api2.competitors[0].name += " 2";
        var matchTimeline2 = new matchTimelineEndpoint() { sport_event = api2 };
        var dto2 = new MatchTimelineDto(matchTimeline2);

        await _cacheManager.SaveDtoAsync(dto2.SportEvent.Id, dto2, _cultures[1], DtoType.MatchTimeline, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(Urn.Parse(api.competitors[0].id), _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(Urn.Parse(api.competitors[0].id), _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(api.competitors[0].name, name1);
        Assert.Equal(api2.competitors[0].name, name2);
    }

    [Fact]
    public void MatchTimelineWhenWrongDtoTypeThenNothingCached()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchTimeline, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void MatchTimelineWhenWrongDtoTypeThenLogConflict()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.MatchTimeline, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void TournamentSeasonsWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = null;
        var tournamentSeasons = new tournamentSeasons() { tournament = api.tournament };
        var dto = new TournamentSeasonsDto(tournamentSeasons);

        _cacheManager.SaveDto(dto.Tournament.Id, dto, _cultures[0], DtoType.MatchTimeline, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    //TODO: should it save?
    [Fact]
    public void TournamentSeasonsWhenWithCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.tournament.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
        var tournamentSeasons = new tournamentSeasons() { tournament = api.tournament };
        var dto = new TournamentSeasonsDto(tournamentSeasons);

        _cacheManager.SaveDto(dto.Tournament.Id, dto, _cultures[0], DtoType.TournamentSeasons, null);

        Assert.Equal(0, _profileMemoryCache.Count());
        // Assert.Equal(api.tournament.competitors.Count(), _profileMemoryCache.Count());
        // Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }
    //
    // [Fact]
    // public void TournamentSeasonsWhenAddToExistingCompetitorThenNoNewCacheItem()
    // {
    //     var api = SummaryEndpoint.AsTournamentInfo.Raw;
    //     api.tournament.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
    //     var tournamentSeasons = new tournamentSeasons()
    //                             {
    //                                 tournament = api.tournament
    //                             };
    //     var dto = new TournamentSeasonsDto(tournamentSeasons);
    //     _cacheManager.SaveDto(dto.Tournament.Id, dto, _cultures[0], DtoType.TournamentSeasons, null);
    //
    //     var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
    //     api2.tournament.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
    //     api2.tournament.competitors[0].name += " 2";
    //     var tournamentSeasons2 = new tournamentSeasons()
    //                              {
    //                                  tournament = api2.tournament
    //                              };
    //     var dto2 = new TournamentSeasonsDto(tournamentSeasons2);
    //
    //     _cacheManager.SaveDto(dto2.Tournament.Id, dto2, _cultures[1], DtoType.TournamentSeasons, null);
    //
    //     Assert.Equal(5, _profileMemoryCache.Count());
    //     Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    // }
    //
    // [Fact]
    // public async Task TournamentSeasonsWhenAddToExistingCompetitorThenDataIsMerged()
    // {
    //     var api = SummaryEndpoint.AsTournamentInfo.Raw;
    //     api.tournament.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
    //     var tournamentSeasons = new tournamentSeasons()
    //                             {
    //                                 tournament = api.tournament
    //                             };
    //     var dto = new TournamentSeasonsDto(tournamentSeasons);
    //     await _cacheManager.SaveDtoAsync(dto.Tournament.Id, dto, _cultures[0], DtoType.TournamentSeasons, null);
    //
    //     var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
    //     api2.tournament.competitors = SummaryEndpoint.BuildTeamCompetitorList(5).ToArray();
    //     api2.tournament.competitors[0].name += " 2";
    //     var tournamentSeasons2 = new tournamentSeasons()
    //                              {
    //                                  tournament = api2.tournament
    //                              };
    //     var dto2 = new TournamentSeasonsDto(tournamentSeasons2);
    //
    //     await _cacheManager.SaveDtoAsync(dto2.Tournament.Id, dto2, _cultures[1], DtoType.TournamentSeasons, null);
    //
    //     var name1 = await _profileCache.GetCompetitorNameAsync(dto.Tournament.Competitors.First().Id, _cultures[0], false);
    //     var name2 = await _profileCache.GetCompetitorNameAsync(dto.Tournament.Competitors.First().Id, _cultures[1], false);
    //     Assert.NotEqual(name1, name2);
    //     Assert.Equal(dto.Tournament.Competitors.First().Name, name1);
    //     Assert.Equal(dto2.Tournament.Competitors.First().Name, name2);
    // }

    [Fact]
    public void TournamentSeasonsWhenWrongDtoTypeThenNothingCached()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentSeasons, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void TournamentSeasonsWhenWrongDtoTypeThenLogConflict()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentSeasons, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void FixtureWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsFixture.Raw;
        api.competitors = null;
        var dto = new FixtureDto(api, DateTime.Now);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.Fixture, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void FixtureWhenWithCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsFixture.Raw;
        api.competitors = new[] { CompetitorProfileEndpoint.BuildTeamCompetitor(1), CompetitorProfileEndpoint.BuildTeamCompetitor(2) };
        var dto = new FixtureDto(api, DateTime.Now);

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.Fixture, null);

        Assert.Equal(2, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void FixtureWhenAddToExistingCompetitorsThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsFixture.Raw;
        api.competitors = new[] { CompetitorProfileEndpoint.BuildTeamCompetitor(1), CompetitorProfileEndpoint.BuildTeamCompetitor(2) };
        var dto = new FixtureDto(api, DateTime.Now);
        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.Fixture, null);
        var api2 = SummaryEndpoint.AsFixture.Raw;
        api2.competitors = new[] { CompetitorProfileEndpoint.BuildTeamCompetitor(1), CompetitorProfileEndpoint.BuildTeamCompetitor(2) };
        api2.competitors[0].name += " 2";
        var dto2 = new FixtureDto(api2, DateTime.Now);

        _cacheManager.SaveDto(dto2.Id, dto2, _cultures[1], DtoType.Fixture, null);

        Assert.Equal(2, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task FixtureWhenAddToExistingCompetitorsThenDataIsMerged()
    {
        var api = SummaryEndpoint.AsFixture.Raw;
        api.competitors = new[] { CompetitorProfileEndpoint.BuildTeamCompetitor(1), CompetitorProfileEndpoint.BuildTeamCompetitor(2) };
        var dto = new FixtureDto(api, DateTime.Now);
        await _cacheManager.SaveDtoAsync(dto.Id, dto, _cultures[0], DtoType.Fixture, null);
        var api2 = SummaryEndpoint.AsFixture.Raw;
        api2.competitors = new[] { CompetitorProfileEndpoint.BuildTeamCompetitor(1), CompetitorProfileEndpoint.BuildTeamCompetitor(2) };
        api2.competitors[0].name += " 2";
        var dto2 = new FixtureDto(api2, DateTime.Now);

        await _cacheManager.SaveDtoAsync(dto2.Id, dto2, _cultures[1], DtoType.Fixture, null);

        var competitorName1 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[0], false);
        var competitorName2 = await _profileCache.GetCompetitorNameAsync(dto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(competitorName1, competitorName2);
        Assert.Equal(dto.Competitors.First().Name, competitorName1);
        Assert.Equal(dto2.Competitors.First().Name, competitorName2);
    }

    [Fact]
    public void FixtureWhenWrongDtoTypeThenNothingCached()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.Fixture, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void FixtureWhenWrongDtoTypeThenLogConflict()
    {
        var dto = CompetitorProfileEndpoint.AsCompetitorProfile.Dto;

        _cacheManager.SaveDto(dto.Competitor.Id, dto, _cultures[0], DtoType.Fixture, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void SportEventSummaryListWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = null;
        var dto = new EntityList<SportEventSummaryDto>(new[] { new MatchDto(api) });

        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    //TODO: should it save?
    [Fact]
    public void SportEventSummaryListWhenWithCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 2).ToArray();
        var dto = new EntityList<SportEventSummaryDto>(new[] { new MatchDto(api) });

        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);

        Assert.Equal(api.sport_event.competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void SportEventSummaryListWhenAddToExistingCompetitorThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 2).ToArray();
        var dto = new EntityList<SportEventSummaryDto>(new[] { new MatchDto(api) });
        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);
        var api2 = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 2).ToArray();
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new EntityList<SportEventSummaryDto>(new[] { new MatchDto(api) });

        _cacheManager.SaveDto(dto2.Items.First().Id, dto2, _cultures[1], DtoType.SportEventSummaryList, null);

        Assert.Equal(api.sport_event.competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task SportEventSummaryListWhenAddToExistingCompetitorThenDataIsMerged()
    {
        var api = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 2).ToArray();
        var matchDto = new MatchDto(api);
        var dto = new EntityList<SportEventSummaryDto>(new[] { matchDto });
        await _cacheManager.SaveDtoAsync(dto.Items.First().Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);
        var api2 = SummaryEndpoint.AsMatch.Raw;
        api.sport_event.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 2).ToArray();
        api2.sport_event.competitors[0].name += " 2";
        var dto2 = new EntityList<SportEventSummaryDto>(new[] { new MatchDto(api2) });

        await _cacheManager.SaveDtoAsync(dto2.Items.First().Id, dto2, _cultures[1], DtoType.SportEventSummaryList, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(matchDto.Competitors.First().Id, _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(matchDto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(api.sport_event.competitors[0].name, name1);
        Assert.Equal(api2.sport_event.competitors[0].name, name2);
    }

    [Fact]
    public void SportEventSummaryListWhenWrongDtoTypeThenNothingCached()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void SportEventSummaryListWhenWrongDtoTypeThenLogConflict()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.SportEventSummaryList, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public void TournamentInfoListWhenNoCompetitorsThenNothingCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = null;
        var dto = new EntityList<TournamentInfoDto>(new[] { new TournamentInfoDto(api) });

        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.TournamentInfoList, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    //TODO: should it save?
    [Fact]
    public void TournamentInfoListWhenWithCompetitorsThenCached()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 20).ToArray();
        var dto = new EntityList<TournamentInfoDto>(new[] { new TournamentInfoDto(api) });

        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.TournamentInfoList, null);

        Assert.Equal(api.competitors.Length, _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public void TournamentInfoListWhenAddToExistingCompetitorThenNoNewCacheItem()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 20).ToArray();
        var dto = new EntityList<TournamentInfoDto>(new[] { new TournamentInfoDto(api) });
        _cacheManager.SaveDto(dto.Items.First().Id, dto, _cultures[0], DtoType.TournamentInfoList, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 20).ToArray();
        api2.competitors[0].name += " 2";
        var dto2 = new EntityList<TournamentInfoDto>(new[] { new TournamentInfoDto(api) });

        _cacheManager.SaveDto(dto2.Items.First().Id, dto2, _cultures[1], DtoType.TournamentInfoList, null);

        Assert.Equal(api.competitors.Count(), _profileMemoryCache.Count());
        Assert.True(_profileMemoryCache.GetKeys().All(a => a.Contains("competitor", StringComparison.InvariantCultureIgnoreCase)));
    }

    [Fact]
    public async Task TournamentInfoListWhenAddToExistingCompetitorThenDataIsMerged()
    {
        var api = SummaryEndpoint.AsTournamentInfo.Raw;
        api.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 25).ToArray();
        var tournamentInfoDto = new TournamentInfoDto(api);
        var dto = new EntityList<TournamentInfoDto>(new[] { tournamentInfoDto });
        await _cacheManager.SaveDtoAsync(dto.Items.First().Id, dto, _cultures[0], DtoType.TournamentInfoList, null);
        var api2 = SummaryEndpoint.AsTournamentInfo.Raw;
        api2.competitors = CompetitorProfileEndpoint.BuildTeamCompetitorList(1, 25).ToArray();
        api2.competitors[0].name += " 2";
        var dto2 = new EntityList<TournamentInfoDto>(new[] { new TournamentInfoDto(api2) });

        await _cacheManager.SaveDtoAsync(dto2.Items.First().Id, dto2, _cultures[1], DtoType.TournamentInfoList, null);

        var name1 = await _profileCache.GetCompetitorNameAsync(tournamentInfoDto.Competitors.First().Id, _cultures[0], false);
        var name2 = await _profileCache.GetCompetitorNameAsync(tournamentInfoDto.Competitors.First().Id, _cultures[1], false);
        Assert.NotEqual(name1, name2);
        Assert.Equal(api.competitors[0].name, name1);
        Assert.Equal(api2.competitors[0].name, name2);
    }

    [Fact]
    public void TournamentInfoListWhenWrongDtoTypeThenNothingCached()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfoList, null);

        Assert.Equal(0, _profileMemoryCache.Count());
    }

    [Fact]
    public void TournamentInfoListWhenWrongDtoTypeThenLogConflict()
    {
        var dto = SummaryEndpoint.AsSportEvent.Dto;

        _cacheManager.SaveDto(dto.Id, dto, _cultures[0], DtoType.TournamentInfoList, null);

        Assert.Equal(1, _logExec.CountByLevel(LogLevel.Warning));
        Assert.Equal(1, _logExec.CountBySearchTerm("Invalid data for item"));
    }

    [Fact]
    public async Task SimpleTeamIsCachedWithoutReferenceIds()
    {
        Assert.Equal(0, _profileMemoryCache.Count());
        var simpleTeamDto = CacheSimpleTeam(1, null);

        var competitorNamesCacheItem = await _profileCache.GetCompetitorProfileAsync(simpleTeamDto.Competitor.Id, ScheduleData.Cultures3, true);
        Assert.NotNull(competitorNamesCacheItem);
        Assert.Equal(simpleTeamDto.Competitor.Id, competitorNamesCacheItem.Id);
        Assert.NotNull(competitorNamesCacheItem.ReferenceId);
        Assert.NotNull(competitorNamesCacheItem.ReferenceId.ReferenceIds);
        Assert.True(competitorNamesCacheItem.ReferenceId.ReferenceIds.Any());
        Assert.Single(competitorNamesCacheItem.ReferenceId.ReferenceIds);
        Assert.Equal(simpleTeamDto.Competitor.Id.Id, competitorNamesCacheItem.ReferenceId.BetradarId);
    }

    [Fact]
    public async Task SimpleTeamIsCachedWithBetradarId()
    {
        Assert.Equal(0, _profileMemoryCache.Count());
        var competitorNamesCacheItem = await _profileCache.GetCompetitorProfileAsync(CreateSimpleTeamUrn(2), ScheduleData.Cultures3, true);
        Assert.NotNull(competitorNamesCacheItem);
        Assert.NotNull(competitorNamesCacheItem.ReferenceId);
        Assert.NotNull(competitorNamesCacheItem.ReferenceId.ReferenceIds);
        Assert.True(competitorNamesCacheItem.ReferenceId.ReferenceIds.Any());
        Assert.Equal(2, competitorNamesCacheItem.ReferenceId.ReferenceIds.Count);
        Assert.Equal("555", competitorNamesCacheItem.ReferenceId.BetradarId.ToString());
    }

    [Fact]
    public void SimpleTeamCanBeRemovedFromCache()
    {
        Assert.Equal(0, _profileMemoryCache.Count());
        var simpleTeamDto = CacheSimpleTeam(654321, null);

        Assert.Equal(1, _profileMemoryCache.Count());
        var cacheItem = (CompetitorCacheItem)_profileMemoryCache.Get(simpleTeamDto.Competitor.Id.ToString());
        Assert.NotNull(cacheItem);
        Assert.Equal(simpleTeamDto.Competitor.Id, cacheItem.Id);

        _cacheManager.RemoveCacheItem(simpleTeamDto.Competitor.Id, CacheItemType.Competitor, "Test");
        Assert.Equal(0, _profileMemoryCache.Count());
        cacheItem = (CompetitorCacheItem)_profileMemoryCache.Get(simpleTeamDto.Competitor.Id.ToString());
        Assert.Null(cacheItem);
    }

    [Fact]
    public async Task CachePlayerDataFromPlayerProfile()
    {
        await _profileCache.GetPlayerProfileAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures1, true);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
        Assert.Equal(1, _profileMemoryCache.Count());

        // var playerNames = _profileCache.GetPlayerNamesAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
        // Assert.NotNull(playerNames);
        // Assert.Equal(ScheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.First()), playerNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(string.Empty, playerNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(string.Empty, playerNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(1, _profileMemoryCache.Count());
        // Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));

        // playerNames = _profileCache.GetPlayerNamesAsync(ScheduleData.MatchCompetitor1PlayerId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
        // Assert.NotNull(playerNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.First()), playerNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.Skip(1).First()), playerNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1Player1.GetName(ScheduleData.Cultures3.Skip(2).First()), playerNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(1, _profileMemoryCache.Count());
        // Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointPlayerProfile));
    }

    [Fact]
    public async Task CacheCompetitorDataFromCompetitorProfile()
    {
        await _profileCache.GetCompetitorProfileAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures1, true);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + 1, _profileMemoryCache.Count());

        // var competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + 1, _profileMemoryCache.Count());
        // Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
        //
        // competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchCompetitor1PlayerCount + 1, _profileMemoryCache.Count());
        // Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointCompetitor));
    }

    [Fact]
    public async Task CacheCompetitorDataFromMatchSummary()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchId, ScheduleData.CultureEn, null);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(2, _profileMemoryCache.Count());

        // var competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(2, _profileMemoryCache.Count());
        // Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        //
        // competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(2, _profileMemoryCache.Count());
        // Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CacheCompetitorDataFromTournamentSummary()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchTournamentId, ScheduleData.CultureEn, null);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _profileMemoryCache.Count());

        // var competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _profileMemoryCache.Count());
        // Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        //
        // competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchTournamentCompetitorCount, _profileMemoryCache.Count());
        // Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    [Fact]
    public async Task CacheCompetitorDataFromSeasonSummary()
    {
        await _dataRouterManager.GetSportEventSummaryAsync(ScheduleData.MatchSeasonId, ScheduleData.CultureEn, null);
        Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _profileMemoryCache.Count());

        // var competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, false).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(string.Empty, competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _profileMemoryCache.Count());
        // Assert.Equal(1, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        //
        // competitorNames = _profileCache.GetCompetitorNamesAsync(ScheduleData.MatchCompetitorId1, ScheduleData.Cultures3, true).GetAwaiter().GetResult();
        // Assert.NotNull(competitorNames);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.First()), competitorNames[ScheduleData.Cultures3.First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(1).First()), competitorNames[ScheduleData.Cultures3.Skip(1).First()]);
        // Assert.Equal(_scheduleData.MatchCompetitor1.GetName(ScheduleData.Cultures3.Skip(2).First()), competitorNames[ScheduleData.Cultures3.Skip(2).First()]);
        // Assert.Equal(ScheduleData.MatchSeasonCompetitorCount, _profileMemoryCache.Count());
        // Assert.Equal(3, _dataRouterManager.TotalRestCalls);
        // Assert.Equal(3, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
    }

    private SimpleTeamProfileDto CacheSimpleTeam(int id, IDictionary<string, string> referenceIds)
    {
        var simpleTeam = MessageFactoryRest.GetSimpleTeamProfileEndpoint(id, referenceIds);
        var simpleTeamDto = new SimpleTeamProfileDto(simpleTeam);
        _cacheManager.SaveDto(simpleTeamDto.Competitor.Id, simpleTeamDto, CultureInfo.CurrentCulture, DtoType.SimpleTeamProfile, null);
        return simpleTeamDto;
    }

    private static Urn CreateSimpleTeamUrn(int competitorId)
    {
        return new Urn("sr", "simple_team", competitorId);
    }
}
