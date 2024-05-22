// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.CacheItems;

public class CacheItemMergeTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly CultureInfo _cultureFirst = new CultureInfo("en");
    private readonly CultureInfo _cultureSecond = new CultureInfo("de");

    public CacheItemMergeTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public void Competitor()
    {
        var teamType1 = new team
        {
            abbreviation = "ABC",
            country = "Germany",
            id = SR.Urn("team").ToString(),
            name = "Team A",
            @virtual = true,
            virtualSpecified = true,
            state = "state"
        };
        var teamType2 = new team
        {
            abbreviation = "ABC",
            country = "Deutschland",
            id = SR.Urn("team").ToString(),
            name = "Team A",
            @virtual = true,
            virtualSpecified = true,
            state = "state"
        };
        var competitorDto1 = new CompetitorDto(teamType1);
        var competitorDto2 = new CompetitorDto(teamType2);

        var competitorCacheItem = new CompetitorCacheItem(competitorDto1, _cultureFirst, null);
        competitorCacheItem.Merge(competitorDto2, _cultureSecond);

        Assert.NotNull(competitorCacheItem);
        Assert.Equal(competitorCacheItem.Id.ToString(), teamType1.id);
        Assert.Equal(competitorCacheItem.GetName(_cultureFirst), teamType1.name);
        Assert.Equal(competitorCacheItem.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
        Assert.Equal(competitorCacheItem.IsVirtual, teamType1.@virtual);
        Assert.Equal(competitorCacheItem.State, teamType1.state);
        Assert.Equal(teamType1.country, competitorCacheItem.GetCountry(_cultureFirst));
        Assert.Equal(teamType2.country, competitorCacheItem.GetCountry(_cultureSecond));
    }

    [Fact]
    public void CoverageInfoMerge()
    {
        var coverageInfoType = new coverageInfo
        {
            level = "first",
            live_coverage = true,
            coverage = new[]
            {
                new coverage { includes = "coverage includes 1" },
                new coverage { includes = "coverage includes 2" },
                new coverage { includes = "coverage includes 3" }
            },
            covered_from = "tv"
        };
        var coverageInfoDto = new CoverageInfoDto(coverageInfoType);
        var coverageInfo = new CoverageInfo(coverageInfoDto.Level, coverageInfoDto.IsLive, coverageInfoDto.Includes, coverageInfoDto.CoveredFrom);

        Assert.NotNull(coverageInfo);
        Assert.Equal(coverageInfoType.level, coverageInfo.Level);
        Assert.Equal(coverageInfoType.live_coverage, coverageInfo.IsLive);
        Assert.Equal(CoveredFrom.Tv, coverageInfo.CoveredFrom);
        Assert.Equal(coverageInfoType.coverage.Length, coverageInfo.Includes.Count());
        Assert.NotEqual(coverageInfo.Includes.ToList()[0], coverageInfo.Includes.ToList()[2]);
    }

    [Fact]
    public void Group()
    {
        var teamType1En = new team
        {
            abbreviation = "ABC",
            country = "Germany",
            id = "sr:team:1",
            name = "Team A",
            @virtual = true,
            virtualSpecified = true
        };
        var teamType1De = new team
        {
            abbreviation = "ABC",
            country = "Deutschland",
            id = "sr:team:1",
            name = "Team A",
            @virtual = true,
            virtualSpecified = true
        };
        var teamType2En = new team
        {
            abbreviation = "ABC",
            country = "Germany",
            id = "sr:team:2",
            name = "Team B",
            @virtual = true,
            virtualSpecified = true
        };
        var teamType2De = new team
        {
            abbreviation = "ABC",
            country = "Deutschland",
            id = "sr:team:2",
            name = "Team B",
            @virtual = true,
            virtualSpecified = true
        };
        var groupType1 = new tournamentGroup
        {
            name = "Group A",
            competitor = new[] { teamType1En, teamType2En }
        };
        var groupType2 = new tournamentGroup
        {
            name = "Group A",
            competitor = new[] { teamType1De, teamType2De }
        };
        var groupDto1 = new GroupDto(groupType1);
        var groupDto2 = new GroupDto(groupType2);

        var groupCacheItem = new GroupCacheItem(groupDto1, _cultureFirst);
        groupCacheItem.Merge(groupDto2, _cultureSecond);

        Assert.NotNull(groupCacheItem);
        Assert.Equal(groupType1.name, groupCacheItem.Name);
        Assert.Equal(groupType1.competitor.Length, groupCacheItem.CompetitorsIds.Count());
        Assert.Equal(groupType1.competitor[0].id, groupCacheItem.CompetitorsIds.ToList()[0].ToString());
        Assert.Equal(groupType1.competitor[1].id, groupCacheItem.CompetitorsIds.ToList()[1].ToString());
        //Assert.NotEqual(groupCacheItem.CompetitorsIds.ToList()[0].Id, groupCacheItem.CompetitorsIds.ToList()[1]);
    }

    [Fact]
    public void Player()
    {
        var sportEntityDto = new SportEntityDto("sr:player:1", "Sport Entity Name");
        var playerCacheItem = new SportEntityCacheItem(sportEntityDto);

        Assert.NotNull(playerCacheItem);
        Assert.Equal(sportEntityDto.Id, playerCacheItem.Id);
    }

    [Fact]
    public void ProductInfo()
    {
        var productInfoType = new productInfo
        {
            is_auto_traded = new productInfoItem(),
            is_in_hosted_statistics = new productInfoItem(),
            is_in_live_center_soccer = new productInfoItem(),
            is_in_live_match_tracker = new productInfoItem(),
            is_in_live_score = new productInfoItem(),
            links = new[]
            {
                new productInfoLink {name = "info link 1", @ref = "ref 1"},
                new productInfoLink {name = "info link 2", @ref = "ref 2"},
                new productInfoLink {name = "info link 3", @ref = "ref 3"}
            },
            streaming = new[]
            {
                new streamingChannel {id = 1, name = "streaming channel 1"},
                new streamingChannel {id = 2, name = "streaming channel 2"},
                new streamingChannel {id = 3, name = "streaming channel 3"},
                new streamingChannel {id = 4, name = "streaming channel 4"}
            }
        };
        var productInfoDto = new ProductInfoDto(productInfoType);
        var productInfo = new ProductInfo(productInfoDto);

        Assert.NotNull(productInfo);
        Assert.Equal(productInfoType.is_auto_traded != null, productInfo.IsAutoTraded);
        Assert.Equal(productInfoType.is_in_hosted_statistics != null, productInfo.IsInHostedStatistics);
        Assert.Equal(productInfoType.is_in_live_center_soccer != null, productInfo.IsInLiveCenterSoccer);
        Assert.Equal(productInfoType.is_in_live_match_tracker != null, productInfo.IsInLiveMatchTracker);
        Assert.Equal(productInfoType.is_in_live_score != null, productInfo.IsInLiveScore);
        Assert.Equal(productInfoType.links.Length, productInfo.Links.Count());
        Assert.Equal(productInfoType.streaming.Length, productInfo.Channels.Count());
        Assert.NotEqual(productInfo.Links.ToList()[0].Name, productInfo.Links.ToList()[2].Name);
        Assert.NotEqual(productInfo.Channels.ToList()[0].Name, productInfo.Channels.ToList()[2].Name);
    }

    [Fact]
    public void ProductInfoLink()
    {
        var productInfoLinkType1 = new productInfoLink
        {
            @ref = "ref 1",
            name = "name 1"
        };

        var productInfoLinkDto = new ProductInfoLinkDto(productInfoLinkType1);
        var productInfoLink = new ProductInfoLink(productInfoLinkDto.Reference, productInfoLinkDto.Name);

        Assert.NotNull(productInfoLink);
        Assert.Equal(productInfoLinkType1.@ref, productInfoLink.Reference);
        Assert.Equal(productInfoLinkType1.name, productInfoLink.Name);
    }

    [Fact]
    public void Referee()
    {
        var refereeType = new referee
        {
            id = "sr:referee:1",
            name = "John Doe",
            nationality = "German",
        };

        var refereeDto = new RefereeDto(refereeType);
        var refereeCacheItem = new RefereeCacheItem(refereeDto, _cultureFirst);

        Assert.NotNull(refereeCacheItem);
        Assert.Equal(refereeType.id, refereeCacheItem.Id.ToString());
        Assert.Equal(refereeType.name, refereeCacheItem.Name);
        Assert.Equal(refereeType.nationality, refereeCacheItem.GetNationality(_cultureFirst));
    }

    [Fact]
    public void RoundMerge()
    {
        var matchRoundTypeEn = new matchRound
        {
            name = "round name in english",
            cup_round_match_number = 1,
            cup_round_match_numberSpecified = true,
            cup_round_matches = 2,
            cup_round_matchesSpecified = true,
            number = 10,
            numberSpecified = true,
            type = "match type 1",
        };

        var matchRoundTypeDe = new matchRound
        {
            name = "round name in deutsch",
            cup_round_match_number = 1,
            cup_round_match_numberSpecified = true,
            cup_round_matches = 2,
            cup_round_matchesSpecified = true,
            number = 10,
            numberSpecified = true,
            type = "match type 1",
        };

        var roundDto1 = new RoundDto(matchRoundTypeEn);
        var roundDto2 = new RoundDto(matchRoundTypeDe);
        var roundCacheItem = new RoundCacheItem(roundDto1, _cultureFirst);
        roundCacheItem.Merge(roundDto2, _cultureSecond);

        Assert.NotNull(roundCacheItem);
        Assert.Equal(matchRoundTypeEn.name, roundCacheItem.GetName(_cultureFirst));
        Assert.Equal(matchRoundTypeDe.name, roundCacheItem.GetName(_cultureSecond));
        Assert.Equal(matchRoundTypeEn.cup_round_match_number, roundCacheItem.CupRoundMatchNumber);
        Assert.Equal(matchRoundTypeEn.cup_round_matches, roundCacheItem.CupRoundMatches);
        Assert.Equal(matchRoundTypeEn.number, roundCacheItem.Number);
        Assert.Equal(matchRoundTypeEn.type, roundCacheItem.Type);
    }

    [Fact]
    public void SeasonCoverageMerge()
    {
        var coverageInfoCacheItem = new SeasonCoverageCacheItem(new SeasonCoverageDto(RestMessageBuilder.BuildCoverageRecord("max", "min", 4, 1, 1, SR.Urn("season").ToString())));

        Assert.NotNull(coverageInfoCacheItem);
        Assert.Equal("max", coverageInfoCacheItem.MaxCoverageLevel);
        Assert.Equal("min", coverageInfoCacheItem.MinCoverageLevel);
        Assert.Equal(4, coverageInfoCacheItem.MaxCovered);
        Assert.Equal(1, coverageInfoCacheItem.Played);
    }

    [Fact]
    public void Season()
    {
        var seasonExtendedType1 = new seasonExtended
        {
            end_date = DateTime.Today,
            id = "sr:season:1",
            name = "season name",
            start_date = DateTime.Now,
            tournament_id = "sr:tournament:1",
            year = "2016"
        };
        var seasonCacheItem = new SeasonCacheItem(new SeasonDto(seasonExtendedType1), _cultureFirst);

        Assert.NotNull(seasonCacheItem);
        Assert.Equal(seasonExtendedType1.id, seasonCacheItem.Id.ToString());
        Assert.Equal(seasonExtendedType1.name, seasonCacheItem.GetName(_cultureFirst));
        Assert.Equal(seasonExtendedType1.start_date, seasonCacheItem.StartDate);
        Assert.Equal(seasonExtendedType1.end_date, seasonCacheItem.EndDate);
        //Assert.Equal(seasonExtendedType1.tournament_id, seasonCacheItem.tournament_id);
        Assert.Equal(seasonExtendedType1.year, seasonCacheItem.Year);
    }

    [Fact]
    public void SportEntity()
    {
        var sportEntityCacheItem = new SportEntityDto("sr:sport:1", "name 1");

        Assert.NotNull(sportEntityCacheItem);
        Assert.Equal("sr:sport:1", sportEntityCacheItem.Id.ToString());
        Assert.Equal("name 1", sportEntityCacheItem.Name);
    }

    //TODO: sportEventConditionsType has Venue, Dto and CacheItem is missing it
    [Fact]
    public void SportEventConditions()
    {
        var venue1 = new venue
        {
            id = "sr:venue:1",
            capacity = 1,
            capacitySpecified = true,
            city_name = "my city",
            country_name = "my country",
            map_coordinates = "coordinates",
            name = "venue name",
        };

        var weatherInfo1 = new weatherInfo
        {
            pitch = "my pitch",
            temperature_celsius = 40,
            temperature_celsiusSpecified = true,
            weather_conditions = "my weather conditions",
            wind = "strong",
            wind_advantage = "none"
        };

        var sportEventConditionType = new sportEventConditions
        {
            attendance = "all",
            match_mode = "full mode",
            referee = new referee
            {
                id = "sr:referee:1",
                name = "John Doe",
                nationality = "German",
            },
            venue = venue1,
            weather_info = weatherInfo1
        };

        var sportEventConditionsDto = new SportEventConditionsDto(sportEventConditionType);
        var sportEventConditionsCacheItem = new SportEventConditionsCacheItem(sportEventConditionsDto, _cultureFirst);

        Assert.NotNull(sportEventConditionsCacheItem);
        Assert.Equal(sportEventConditionType.attendance, sportEventConditionsCacheItem.Attendance);
        Assert.Equal(sportEventConditionType.match_mode, sportEventConditionsCacheItem.EventMode);
        Assert.Equal(sportEventConditionType.referee.id, sportEventConditionsCacheItem.Referee.Id.ToString());
        Assert.Equal(sportEventConditionType.referee.name, sportEventConditionsCacheItem.Referee.Name);
        Assert.Equal(sportEventConditionType.referee.nationality, sportEventConditionsCacheItem.Referee.GetNationality(_cultureFirst));

        //Assert.Equal(sportEventConditionType.venue.id, sportEventConditionsCacheItem.); TODO: missing Venue

        Assert.Equal(sportEventConditionType.weather_info.pitch, sportEventConditionsCacheItem.WeatherInfo.Pitch);
        Assert.Equal(sportEventConditionType.weather_info.temperature_celsius, sportEventConditionsCacheItem.WeatherInfo.TemperatureCelsius);
        Assert.Equal(sportEventConditionType.weather_info.weather_conditions, sportEventConditionsCacheItem.WeatherInfo.WeatherConditions);
        Assert.Equal(sportEventConditionType.weather_info.wind, sportEventConditionsCacheItem.WeatherInfo.Wind);
        Assert.Equal(sportEventConditionType.weather_info.wind_advantage, sportEventConditionsCacheItem.WeatherInfo.WindAdvantage);
    }

    [Fact]
    public void StreamingChannel()
    {
        var streamingChannelType = new streamingChannel
        {
            id = 1,
            name = "name 1"
        };
        var streamingChannelDto = new StreamingChannelDto(streamingChannelType);
        var streamingChannelCacheItem = new StreamingChannelCacheItem(streamingChannelDto);

        Assert.NotNull(streamingChannelCacheItem);
        Assert.Equal(streamingChannelType.id, streamingChannelCacheItem.Id);
        Assert.Equal(streamingChannelType.name, streamingChannelCacheItem.Name);
    }

    [Fact]
    public void TeamCompetitorMerge()
    {
        var teamType1 = new teamCompetitor
        {
            abbreviation = "ABC",
            country = "Germany",
            id = "sr:team:1",
            name = "Team A",
            @virtual = true,
            virtualSpecified = true,
            qualifier = "qua 1",
            divisionSpecified = true,
            division = 1,
            state = "state"
        };
        var teamType2 = new teamCompetitor
        {
            abbreviation = "ABC",
            country = "Deutschland",
            id = "sr:team:1",
            name = "Team A",
            @virtual = true,
            virtualSpecified = true,
            qualifier = "qua 1",
            divisionSpecified = true,
            division = 1,
            state = "state"
        };
        var competitorDto1 = new TeamCompetitorDto(teamType1);
        var competitorDto2 = new TeamCompetitorDto(teamType2);

        var competitorCacheItem = new TeamCompetitorCacheItem(competitorDto1, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));
        competitorCacheItem.Merge(competitorDto2, _cultureSecond);

        Assert.NotNull(competitorCacheItem);
        Assert.Equal(competitorCacheItem.Id.ToString(), teamType1.id);
        Assert.Equal(competitorCacheItem.GetName(_cultureFirst), teamType1.name);
        Assert.Equal(competitorCacheItem.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
        Assert.Equal(competitorCacheItem.IsVirtual, teamType1.@virtual);
        Assert.Equal(competitorCacheItem.Qualifier, teamType1.qualifier);
        Assert.Equal(competitorCacheItem.State, teamType1.state);
        Assert.Equal(teamType1.country, competitorCacheItem.GetCountry(_cultureFirst));
        Assert.Equal(teamType2.country, competitorCacheItem.GetCountry(_cultureSecond));
        Assert.NotNull(competitorCacheItem.Division);
        Assert.Equal(competitorCacheItem.Division.Id, teamType1.division);
    }

    [Fact]
    public void VenueMerge()
    {
        var venue1 = new venue
        {
            id = SR.Urn("venue").ToString(),
            capacity = 5,
            capacitySpecified = true,
            city_name = "my city",
            country_name = "eng country name",
            state = "state",
            map_coordinates = "coordinates",
            name = "eng name"
        };
        var venue2 = new venue
        {
            id = SR.Urn("venue").ToString(),
            capacity = 5,
            capacitySpecified = true,
            city_name = "my city",
            country_name = "de country name",
            state = "state",
            map_coordinates = "coordinates",
            name = "eng name"
        };
        var venueDto1 = new VenueDto(venue1);
        var venueDto2 = new VenueDto(venue2);
        var venueCacheItem = new VenueCacheItem(venueDto1, _cultureFirst);
        venueCacheItem.Merge(venueDto2, _cultureSecond);

        Assert.NotNull(venueCacheItem);
        Assert.Equal(venue1.id, venueCacheItem.Id.ToString());
        Assert.Equal(venue1.name, venueCacheItem.GetName(_cultureFirst));
        Assert.Equal(venue1.capacity, venueCacheItem.Capacity);
        Assert.Equal(venue1.map_coordinates, venueCacheItem.Coordinates);
        Assert.Equal(venue1.state, venueCacheItem.State);
        Assert.Equal(venue1.city_name, venueCacheItem.GetCity(_cultureFirst));
        Assert.Equal(venue1.country_name, venueCacheItem.GetCountry(_cultureFirst));
        Assert.Equal(venue2.country_name, venueCacheItem.GetCountry(_cultureSecond));
        Assert.NotEqual(venueCacheItem.GetCountry(_cultureFirst), venueCacheItem.GetCountry(_cultureSecond));
    }

    [Fact]
    public void WeatherInfo()
    {
        var weatherInfo = new weatherInfo
        {
            pitch = "my pitch",
            temperature_celsius = 40,
            temperature_celsiusSpecified = true,
            weather_conditions = "my weather conditions",
            wind = "strong",
            wind_advantage = "none"
        };
        var weatherInfoDto = new WeatherInfoDto(weatherInfo);
        var weatherInfoCacheItem = new WeatherInfoCacheItem(weatherInfoDto);

        Assert.NotNull(weatherInfoCacheItem);
        Assert.Equal(weatherInfo.pitch, weatherInfoCacheItem.Pitch);
        Assert.Equal(weatherInfo.temperature_celsius, weatherInfoCacheItem.TemperatureCelsius);
        Assert.Equal(weatherInfo.weather_conditions, weatherInfoCacheItem.WeatherConditions);
        Assert.Equal(weatherInfo.wind, weatherInfoCacheItem.Wind);
        Assert.Equal(weatherInfo.wind_advantage, weatherInfoCacheItem.WindAdvantage);
    }
}
