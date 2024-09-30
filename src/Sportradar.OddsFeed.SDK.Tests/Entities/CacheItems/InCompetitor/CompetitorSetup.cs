// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InVenue;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems.InCompetitor;

public class CompetitorSetup
{
    protected const string DefaultDivisionName = "some-default-division-name";
    internal readonly Mock<IDataRouterManager> DataRouterManagerMock = new Mock<IDataRouterManager>();

    public static team GetApiTeamFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new team
        {
            id = Urn.Parse($"sr:competitor:{resultCompetitorId}").ToString(),
            name = $"Competitor {resultCompetitorId}",
            players = GetApiPlayerCompetitors(playerSize),
            abbreviation = $"P{resultCompetitorId}",
            divisionSpecified = true,
            division = 1,
            division_name = "Division 1",
            @virtual = false,
            virtualSpecified = false,
            country = "Country",
            country_code = "UK",
            state = "some-state",
            age_group = "18-24",
            gender = "male",
            short_name = "some-short-name",
            reference_ids = GetApiCompetitorReference(resultCompetitorId, resultCompetitorId)
        };
    }

    private static teamExtended GetApiTeamExtendedFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new teamExtended
        {
            id = Urn.Parse($"sr:competitor:{resultCompetitorId}").ToString(),
            name = $"Competitor {resultCompetitorId}",
            players = GetApiPlayerCompetitors(playerSize),
            abbreviation = $"P{resultCompetitorId}",
            divisionSpecified = true,
            division = 1,
            division_name = "Division 1",
            @virtual = false,
            virtualSpecified = false,
            country = "Country",
            country_code = "UK",
            state = "some-state",
            age_group = "18-24",
            gender = "male",
            short_name = "some-short-name",
            reference_ids = GetApiCompetitorReference(resultCompetitorId, resultCompetitorId),
            category = new category { id = "sr:category:123", name = "Category 123", country_code = "uk" },
            sport = new sport { id = "sr:sport:31", name = "Sport 123" }
        };
    }

    internal static competitorProfileEndpoint GetApiCompetitorProfileFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new competitorProfileEndpoint
        {
            competitor = GetApiTeamExtendedFull(resultCompetitorId, playerSize),
            players = GetApiPlayersExtended(playerSize),
            jerseys = GetApiJerseys(playerSize),
            generated_at = DateTime.Now,
            generated_atSpecified = true,
            manager = GetApiManager(111),
            race_driver_profile = GetApiRaceDriverProfile(222),
            venue = VenueHelper.GenerateApiVenue(courseSize: 3)
        };
    }

    protected static simpleTeamProfileEndpoint GetApiSimpleTeamProfileFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new simpleTeamProfileEndpoint
        {
            competitor = GetApiTeamExtendedFull(resultCompetitorId, playerSize),
            generated_at = DateTime.Now,
            generated_atSpecified = true
        };
    }

    private static raceTeam GetApiRaceTeamFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new raceTeam
        {
            id = Urn.Parse($"sr:competitor:{resultCompetitorId}").ToString(),
            name = $"Competitor {resultCompetitorId}",
            players = GetApiPlayerCompetitors(playerSize),
            abbreviation = $"P{resultCompetitorId}",
            divisionSpecified = true,
            division = 1,
            division_name = "Division 1",
            @virtual = false,
            virtualSpecified = false,
            country = "Country",
            country_code = "UK",
            state = "some-state",
            age_group = "18-24",
            gender = "male",
            short_name = "some-short-name",
            reference_ids = GetApiCompetitorReference(resultCompetitorId, resultCompetitorId)
        };
    }

    protected static teamCompetitor GetApiTeamCompetitorFull(int competitorId = 123, int playerSize = 15)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new teamCompetitor
        {
            id = Urn.Parse($"sr:competitor:{resultCompetitorId}").ToString(),
            name = $"Competitor {resultCompetitorId}",
            players = GetApiPlayerCompetitors(playerSize),
            abbreviation = $"P{resultCompetitorId}",
            divisionSpecified = true,
            division = 1,
            division_name = "Division 1",
            @virtual = false,
            virtualSpecified = false,
            country = "Country",
            country_code = "UK",
            state = "some-state",
            age_group = "18-24",
            gender = "male",
            short_name = "some-short-name",
            reference_ids = GetApiCompetitorReference(resultCompetitorId, resultCompetitorId),
            qualifier = "home"
        };
    }

    private static playerExtended GetApiPlayerExtended(int playerId = 1)
    {
        return new playerExtended
        {
            id = $"sr:player:{playerId}",
            name = "Doe, John",
            type = "F",
            date_of_birth = "1984-07-18",
            nationality = "England",
            country_code = "ENG",
            height = 180,
            heightSpecified = true,
            weight = 70,
            weightSpecified = true,
            jersey_number = 10,
            jersey_numberSpecified = true,
            full_name = "John Doe",
            nickname = "Johny",
            gender = "male"
        };
    }

    private static playerExtended[] GetApiPlayersExtended(int playerSize = 15)
    {
        var player = new List<playerExtended>();
        for (var i = 1; i <= playerSize; i++)
        {
            player.Add(GetApiPlayerExtended(i));
        }
        return player.ToArray();
    }

    internal static playerCompetitor[] GetApiPlayerCompetitors(int playerSize = 15)
    {
        var player = new List<playerCompetitor>();
        for (var i = 1; i <= playerSize; i++)
        {
            player.Add(GetApiPlayerCompetitor(i));
        }
        return player.ToArray();
    }

    protected static playerCompetitor GetApiPlayerCompetitor(int playerId = 0)
    {
        var resultPlayerId = playerId == 0 ? Random.Shared.Next(1, 10_000) : playerId;

        return new playerCompetitor
        {
            id = Urn.Parse($"sr:player:{resultPlayerId}").ToString(),
            name = $"Player {resultPlayerId}",
            abbreviation = $"P{resultPlayerId}",
            nationality = $"Nationality {resultPlayerId}"
        };
    }

    private static competitorReferenceIdsReference_id[] GetApiCompetitorReference(int betradarId, int rotationNumber)
    {
        var result = new List<competitorReferenceIdsReference_id>();
        if (betradarId > 0)
        {
            result.Add(new competitorReferenceIdsReference_id { name = "betradar", value = betradarId.ToString() });
        }
        if (rotationNumber > 0)
        {
            result.Add(new competitorReferenceIdsReference_id { name = "rotation_number", value = rotationNumber.ToString() });
        }

        return result.ToArray();
    }

    internal static raceDriverProfile GetApiRaceDriverProfile(int competitorId = 123)
    {
        var resultCompetitorId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new raceDriverProfile
        {
            car = new car { chassis = "some-car-chassis", engine_name = "some-engine-name", name = "some-car-name" },
            race_driver = GetApiRaceDriver(resultCompetitorId),
            race_team = GetApiRaceTeamFull(1, 20)
        };
    }

    private static raceDriver GetApiRaceDriver(int competitorId = 123)
    {
        var resultId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new raceDriver
        {
            reference_ids = GetApiCompetitorReference(resultId, 100),
            players = GetApiPlayerCompetitors(5),
            id = Urn.Parse($"sr:competitor:{resultId}").ToString(),
            name = $"Competitor {resultId}",
            abbreviation = $"P{resultId}",
            divisionSpecified = true,
            division = 1,
            division_name = "Division 1",
            @virtual = false,
            virtualSpecified = false,
            country = "Country",
            country_code = "UK",
            state = "some-state",
            age_group = "18-24",
            gender = "male",
            short_name = "some-short-name",
            date_of_birth = "2000-01-01",
            nationality = "some-nationality"
        };
    }

    internal static car GetApiCar()
    {
        return new car { chassis = "some-car-chassis", engine_name = "some-engine-name", name = "some-car-name" };
    }

    internal static manager GetApiManager(int competitorId = 123)
    {
        var resultId = competitorId == 0 ? Random.Shared.Next(1, 10_000) : competitorId;
        return new manager
        {
            id = Urn.Parse($"sr:competitor:{resultId}").ToString(),
            name = $"Competitor {resultId}",
            country_code = "UK",
            nationality = "some-nationality"
        };
    }

    internal static jersey GetApiJersey(int jerseyNumber = 1)
    {
        return new jersey
        {
            type = "some-jersey-type",
            @base = "some-jersey-base",
            sleeve = "some-sleeve",
            number = jerseyNumber.ToString(),
            stripes = true,
            stripesSpecified = true,
            stripes_color = "red",
            horizontal_stripes = true,
            horizontal_stripesSpecified = true,
            horizontal_stripes_color = "blue",
            squares = true,
            squaresSpecified = true,
            squares_color = "green",
            split = true,
            splitSpecified = true,
            split_color = "yellow",
            shirt_type = "long",
            sleeve_detail = "some-sleeve-detail"
        };
    }

    private static jersey[] GetApiJerseys(int size)
    {
        var jerseys = new List<jersey>();
        for (var i = 0; i < size; i++)
        {
            jerseys.Add(GetApiJersey(i + 1));
        }

        return jerseys.ToArray();
    }

    internal static void ValidatePlayerCompetitor(playerCompetitor apiPlayerCompetitor, PlayerCompetitorDto playerCompetitorDto)
    {
        Assert.NotNull(playerCompetitorDto);
        Assert.Equal(Urn.Parse(apiPlayerCompetitor.id), playerCompetitorDto.Id);
        Assert.Equal(apiPlayerCompetitor.name, playerCompetitorDto.Name);
        Assert.Equal(apiPlayerCompetitor.abbreviation, playerCompetitorDto.Abbreviation);
        Assert.Equal(apiPlayerCompetitor.nationality, playerCompetitorDto.Nationality);
    }

    // ReSharper disable once MethodTooLong
    internal static void ValidateTeamWithCompetitor(team apiTeam, CompetitorDto dto)
    {
        Assert.NotNull(apiTeam);
        Assert.NotNull(dto);
        Assert.Equal(Urn.Parse(apiTeam.id), dto.Id);
        Assert.Equal(apiTeam.name, dto.Name);
        Assert.Equal(apiTeam.abbreviation, dto.Abbreviation);
        Assert.Equal(apiTeam.age_group, dto.AgeGroup);
        Assert.Equal(apiTeam.country, dto.CountryName);
        Assert.Equal(apiTeam.country_code, dto.CountryCode);
        Assert.Equal(apiTeam.gender, dto.Gender);
        Assert.Equal(apiTeam.short_name, dto.ShortName);
        Assert.Equal(apiTeam.state, dto.State);
        if (apiTeam.divisionSpecified)
        {
            Assert.Equal(apiTeam.division, dto.Division.Id);
            Assert.Equal(apiTeam.division_name, dto.Division.Name);
        }

        if (!apiTeam.players.IsNullOrEmpty())
        {
            Assert.Equal(apiTeam.players.Length, dto.Players.Count());
            foreach (var apiTeamPlayer in apiTeam.players)
            {
                var competitorPlayerDto = dto.Players.FirstOrDefault(x => Equals(x.Id, Urn.Parse(apiTeamPlayer.id)));
                Assert.NotNull(competitorPlayerDto);
                ValidatePlayerCompetitor(apiTeamPlayer, competitorPlayerDto);
            }
        }

        if (!apiTeam.reference_ids.IsNullOrEmpty())
        {
            Assert.Equal(apiTeam.reference_ids.Length, dto.ReferenceIds.Count());
            foreach (var apiReferenceId in apiTeam.reference_ids)
            {
                var referenceDto = dto.ReferenceIds.FirstOrDefault(x => Equals(x.Key, apiReferenceId.name));
                Assert.Equal(apiReferenceId.value, referenceDto.Value);
            }
        }
    }

    internal static CompetitorDto GetCompetitorWithDivision(int divisionId = 1010)
    {
        var apiTeam = GetApiTeamExtendedFull(1, 0);
        apiTeam.divisionSpecified = true;
        apiTeam.division = divisionId;
        apiTeam.division_name = $"Division {divisionId}";
        return new CompetitorDto(apiTeam);
    }

    internal static CompetitorDto GetCompetitorWithoutDivision()
    {
        var apiTeamExtended = GetApiTeamExtendedFull(1, 0);
        apiTeamExtended.divisionSpecified = false;
        return new CompetitorDto(apiTeamExtended);
    }

    internal static CompetitorProfileDto GetCompetitorProfileWithDivision(int divisionId = 1010)
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1, 0);
        apiCompetitorProfile.competitor.divisionSpecified = true;
        apiCompetitorProfile.competitor.division = divisionId;
        apiCompetitorProfile.competitor.division_name = $"Division {divisionId}";
        return new CompetitorProfileDto(apiCompetitorProfile);
    }

    internal static CompetitorProfileDto GetCompetitorProfileWithoutDivision()
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1, 0);
        apiCompetitorProfile.competitor.divisionSpecified = false;
        return new CompetitorProfileDto(apiCompetitorProfile);
    }

    internal static SimpleTeamProfileDto GetSimpleTeamProfileWithDivision(int divisionId = 1010)
    {
        var apiSimpleTeamProfile = GetApiSimpleTeamProfileFull(1, 0);
        apiSimpleTeamProfile.competitor.divisionSpecified = true;
        apiSimpleTeamProfile.competitor.division = divisionId;
        apiSimpleTeamProfile.competitor.division_name = $"Division {divisionId}";
        return new SimpleTeamProfileDto(apiSimpleTeamProfile);
    }

    internal static SimpleTeamProfileDto GetSimpleTeamProfileWithoutDivision()
    {
        var apiSimpleTeamProfile = GetApiSimpleTeamProfileFull(1, 0);
        apiSimpleTeamProfile.competitor.divisionSpecified = false;
        return new SimpleTeamProfileDto(apiSimpleTeamProfile);
    }

    internal static TeamCompetitorDto GetTeamCompetitorWithDivision(int divisionId = 1010)
    {
        var apiTeamCompetitor = GetApiTeamCompetitorFull(1, 0);
        apiTeamCompetitor.divisionSpecified = true;
        apiTeamCompetitor.division = divisionId;
        apiTeamCompetitor.division_name = $"Division {divisionId}";
        return new TeamCompetitorDto(apiTeamCompetitor);
    }

    internal static TeamCompetitorDto GetTeamCompetitorWithoutDivision()
    {
        var apiTeamCompetitor = GetApiTeamCompetitorFull(1, 0);
        apiTeamCompetitor.divisionSpecified = false;
        return new TeamCompetitorDto(apiTeamCompetitor);
    }

    internal static void ValidateCompetitorDtoWithCi(CompetitorDto dto, CompetitorCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(dto.Id, ci.Id);
        Assert.Equal(dto.AgeGroup, ci.AgeGroup);
        Assert.Equal(dto.CategoryId, ci.CategoryId);
        Assert.Equal(dto.CountryCode, ci.CountryCode);
        Assert.Equal(dto.Gender, ci.Gender);
        Assert.Null(dto.IsVirtual);
        Assert.Equal(dto.ShortName, ci.ShortName);
        Assert.Equal(dto.SportId, ci.SportId);
        Assert.Equal(dto.State, ci.State);
        if (dto.Division != null)
        {
            Assert.Equal(dto.Division.Id, ci.Division.Id);
            Assert.Equal(dto.Division.Name, ci.Division.Name);
        }

        ValidateCompetitorTranslatablePropertiesDtoWithCi(dto, ci, culture);
    }

    internal static CompetitorDto GetCompetitorWithVirtualFlagSet(bool? isVirtual)
    {
        var apiTeam = GetApiTeamExtendedFull(1, 0);
        apiTeam.virtualSpecified = isVirtual.HasValue;
        apiTeam.@virtual = isVirtual ?? false;

        return new CompetitorDto(apiTeam);
    }

    private static void ValidateCompetitorTranslatablePropertiesDtoWithCi(CompetitorDto dto, CompetitorCacheItem ci, CultureInfo culture)
    {
        Assert.Equal(dto.Name, ci.Names[culture]);
        Assert.Equal(dto.Abbreviation, ci.GetAbbreviation(culture));
        Assert.Equal(dto.CountryName, ci.GetCountry(culture));
        if (!dto.Players.IsNullOrEmpty())
        {
            Assert.Equal(dto.Players.Count, ci.AssociatedPlayerIds.Count);
            Assert.True(dto.Players.All(a => ci.AssociatedPlayerIds.Contains(a.Id)));
        }
        if (!dto.ReferenceIds.IsNullOrEmpty())
        {
            Assert.Equal(dto.ReferenceIds.Count, ci.ReferenceId.ReferenceIds.Count);
            Assert.Equal(dto.ReferenceIds.Count, ci.ReferenceId.ReferenceIds.Count);
        }
    }

    internal static CompetitorProfileDto GetCompetitorProfileDtoWithPlayerJerseyNumber(bool hasJerseyNumber)
    {
        var apiCompetitorProfile = GetApiCompetitorProfileFull(1, 10);
        foreach (var player in apiCompetitorProfile.players)
        {
            if (hasJerseyNumber)
            {
                player.jersey_numberSpecified = true;
                player.jersey_number = (int)Urn.Parse(player.id).Id;
            }
            else
            {
                player.jersey_numberSpecified = false;
            }
        }
        return new CompetitorProfileDto(apiCompetitorProfile);
    }
}
