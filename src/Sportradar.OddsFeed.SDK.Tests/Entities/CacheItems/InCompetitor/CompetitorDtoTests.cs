// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorDtoTests : CompetitorSetup
{
    [Fact]
    public void PropertiesAreSet()
    {
        var apiTeam = GetApiTeamFull();

        var competitorDto = new CompetitorDto(apiTeam);

        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutAbbreviation()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.abbreviation = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.Abbreviation);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutCountry()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.country = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.CountryName);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutCountryCode()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.country_code = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.CountryCode);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutState()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.state = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.State);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutDivision()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.divisionSpecified = false;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.Division);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutShortName()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.short_name = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.ShortName);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutGender()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.gender = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.Gender);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutVirtual()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.virtualSpecified = false;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.IsVirtual);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutVirtualButNotSpecified()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.@virtual = true;
        apiTeam.virtualSpecified = false;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.IsVirtual);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithVirtual()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.virtualSpecified = true;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.False(competitorDto.IsVirtual);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithVirtualAndTrue()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.@virtual = true;
        apiTeam.virtualSpecified = true;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.True(competitorDto.IsVirtual);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutReferenceIds()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.reference_ids = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.ReferenceIds);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void WithoutPlayers()
    {
        var apiTeam = GetApiTeamFull();
        apiTeam.players = null;

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Null(competitorDto.Players);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void ConstructorWithExtendedTeam()
    {
        var apiTeam = new teamExtended
        {
            id = Urn.Parse("sr:competitor:123").ToString(),
            name = "Competitor 123",
            sport = new sport { id = "sr:sport:1", name = "Soccer" },
            category = new category { id = "sr:category:1", name = "Category 1" }
        };

        var competitorDto = new CompetitorDto(apiTeam);

        Assert.Equal(Urn.Parse(apiTeam.sport.id), competitorDto.SportId);
        Assert.Equal(Urn.Parse(apiTeam.category.id), competitorDto.CategoryId);
        ValidateTeamWithCompetitor(apiTeam, competitorDto);
    }

    [Fact]
    public void ConstructorWhenTeamIsNullThenThrowArgumentNullException()
    {
        Assert.Throws<NullReferenceException>(() => new CompetitorDto(null));
    }

    [Fact]
    public void TeamCompetitorWhenPropertiesAreSet()
    {
        var apiTeamCompetitor = GetApiTeamCompetitorFull();

        var teamCompetitorDto = new TeamCompetitorDto(apiTeamCompetitor);

        ValidateTeamWithCompetitor(apiTeamCompetitor, teamCompetitorDto);
        Assert.NotNull(teamCompetitorDto.Qualifier);
        Assert.Equal(apiTeamCompetitor.qualifier, teamCompetitorDto.Qualifier);
    }

    [Fact]
    public void TeamCompetitorWhenWithoutQualifier()
    {
        var apiTeamCompetitor = GetApiTeamCompetitorFull();
        apiTeamCompetitor.qualifier = null;

        var teamCompetitorDto = new TeamCompetitorDto(apiTeamCompetitor);

        ValidateTeamWithCompetitor(apiTeamCompetitor, teamCompetitorDto);
        Assert.Null(teamCompetitorDto.Qualifier);
    }
}
