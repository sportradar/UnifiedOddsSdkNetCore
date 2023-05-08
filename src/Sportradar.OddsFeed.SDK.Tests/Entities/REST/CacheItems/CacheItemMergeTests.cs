/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using SR = Sportradar.OddsFeed.SDK.Tests.Common.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.CacheItems
{
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
            var competitorDTO1 = new CompetitorDTO(teamType1);
            var competitorDTO2 = new CompetitorDTO(teamType2);

            var competitorCI = new CompetitorCI(competitorDTO1, _cultureFirst, null);
            competitorCI.Merge(competitorDTO2, _cultureSecond);

            Assert.NotNull(competitorCI);
            Assert.Equal(competitorCI.Id.ToString(), teamType1.id);
            Assert.Equal(competitorCI.GetName(_cultureFirst), teamType1.name);
            Assert.Equal(competitorCI.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
            Assert.Equal(competitorCI.IsVirtual, teamType1.@virtual);
            Assert.Equal(competitorCI.State, teamType1.state);
            Assert.Equal(teamType1.country, competitorCI.GetCountry(_cultureFirst));
            Assert.Equal(teamType2.country, competitorCI.GetCountry(_cultureSecond));
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
            var coverageInfoDTO = new CoverageInfoDTO(coverageInfoType);
            var coverageInfo = new CoverageInfo(coverageInfoDTO.Level, coverageInfoDTO.IsLive, coverageInfoDTO.Includes, coverageInfoDTO.CoveredFrom);

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
            var groupDTO1 = new GroupDTO(groupType1);
            var groupDTO2 = new GroupDTO(groupType2);

            var groupCI = new GroupCI(groupDTO1, _cultureFirst);
            groupCI.Merge(groupDTO2, _cultureSecond);

            Assert.NotNull(groupCI);
            Assert.Equal(groupType1.name, groupCI.Name);
            Assert.Equal(groupType1.competitor.Length, groupCI.CompetitorsIds.Count());
            Assert.Equal(groupType1.competitor[0].id, groupCI.CompetitorsIds.ToList()[0].ToString());
            Assert.Equal(groupType1.competitor[1].id, groupCI.CompetitorsIds.ToList()[1].ToString());
            //Assert.NotEqual(groupCI.CompetitorsIds.ToList()[0].Id, groupCI.CompetitorsIds.ToList()[1]);
        }

        [Fact]
        public void Player()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.NotNull(playerCI);
            Assert.Equal(sportEntityDTO.Id, playerCI.Id);
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
            var productInfoDTO = new ProductInfoDTO(productInfoType);
            var productInfo = new ProductInfo(productInfoDTO);

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

            var productInfoLinkDto = new ProductInfoLinkDTO(productInfoLinkType1);
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

            var refereeDTO = new RefereeDTO(refereeType);
            var refereeCI = new RefereeCI(refereeDTO, _cultureFirst);

            Assert.NotNull(refereeCI);
            Assert.Equal(refereeType.id, refereeCI.Id.ToString());
            Assert.Equal(refereeType.name, refereeCI.Name);
            Assert.Equal(refereeType.nationality, refereeCI.GetNationality(_cultureFirst));
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

            var roundDTO1 = new RoundDTO(matchRoundTypeEn);
            var roundDTO2 = new RoundDTO(matchRoundTypeDe);
            var roundCI = new RoundCI(roundDTO1, _cultureFirst);
            roundCI.Merge(roundDTO2, _cultureSecond);

            Assert.NotNull(roundCI);
            Assert.Equal(matchRoundTypeEn.name, roundCI.GetName(_cultureFirst));
            Assert.Equal(matchRoundTypeDe.name, roundCI.GetName(_cultureSecond));
            Assert.Equal(matchRoundTypeEn.cup_round_match_number, roundCI.CupRoundMatchNumber);
            Assert.Equal(matchRoundTypeEn.cup_round_matches, roundCI.CupRoundMatches);
            Assert.Equal(matchRoundTypeEn.number, roundCI.Number);
            Assert.Equal(matchRoundTypeEn.type, roundCI.Type);
        }

        [Fact]
        public void SeasonCoverageMerge()
        {
            var coverageInfoCI = new SeasonCoverageCI(new SeasonCoverageDTO(RestMessageBuilder.BuildCoverageRecord("max", "min", 4, 1, 1, SR.Urn("season").ToString())));

            Assert.NotNull(coverageInfoCI);
            Assert.Equal("max", coverageInfoCI.MaxCoverageLevel);
            Assert.Equal("min", coverageInfoCI.MinCoverageLevel);
            Assert.Equal(4, coverageInfoCI.MaxCovered);
            Assert.Equal(1, coverageInfoCI.Played);
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
            var seasonCI = new SeasonCI(new SeasonDTO(seasonExtendedType1), _cultureFirst);

            Assert.NotNull(seasonCI);
            Assert.Equal(seasonExtendedType1.id, seasonCI.Id.ToString());
            Assert.Equal(seasonExtendedType1.name, seasonCI.GetName(_cultureFirst));
            Assert.Equal(seasonExtendedType1.start_date, seasonCI.StartDate);
            Assert.Equal(seasonExtendedType1.end_date, seasonCI.EndDate);
            //Assert.Equal(seasonExtendedType1.tournament_id, seasonCI.tournament_id);
            Assert.Equal(seasonExtendedType1.year, seasonCI.Year);
        }

        [Fact]
        public void SportEntity()
        {
            var sportEntityCI = new SportEntityDTO("sr:sport:1", "name 1");

            Assert.NotNull(sportEntityCI);
            Assert.Equal("sr:sport:1", sportEntityCI.Id.ToString());
            Assert.Equal("name 1", sportEntityCI.Name);
        }

        //TODO: sportEventConditionsType has Venue, DTO and CI is missing it
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

            var sportEventConditionsDTO = new SportEventConditionsDTO(sportEventConditionType);
            var sportEventConditionsCI = new SportEventConditionsCI(sportEventConditionsDTO, _cultureFirst);

            Assert.NotNull(sportEventConditionsCI);
            Assert.Equal(sportEventConditionType.attendance, sportEventConditionsCI.Attendance);
            Assert.Equal(sportEventConditionType.match_mode, sportEventConditionsCI.EventMode);
            Assert.Equal(sportEventConditionType.referee.id, sportEventConditionsCI.Referee.Id.ToString());
            Assert.Equal(sportEventConditionType.referee.name, sportEventConditionsCI.Referee.Name);
            Assert.Equal(sportEventConditionType.referee.nationality, sportEventConditionsCI.Referee.GetNationality(_cultureFirst));

            //Assert.Equal(sportEventConditionType.venue.id, sportEventConditionsCI.); TODO: missing Venue

            Assert.Equal(sportEventConditionType.weather_info.pitch, sportEventConditionsCI.WeatherInfo.Pitch);
            Assert.Equal(sportEventConditionType.weather_info.temperature_celsius, sportEventConditionsCI.WeatherInfo.TemperatureCelsius);
            Assert.Equal(sportEventConditionType.weather_info.weather_conditions, sportEventConditionsCI.WeatherInfo.WeatherConditions);
            Assert.Equal(sportEventConditionType.weather_info.wind, sportEventConditionsCI.WeatherInfo.Wind);
            Assert.Equal(sportEventConditionType.weather_info.wind_advantage, sportEventConditionsCI.WeatherInfo.WindAdvantage);
        }

        [Fact]
        public void StreamingChannel()
        {
            var streamingChannelType = new streamingChannel
            {
                id = 1,
                name = "name 1"
            };
            var streamingChannelDTO = new StreamingChannelDTO(streamingChannelType);
            var streamingChannelCI = new StreamingChannelCI(streamingChannelDTO);

            Assert.NotNull(streamingChannelCI);
            Assert.Equal(streamingChannelType.id, streamingChannelCI.Id);
            Assert.Equal(streamingChannelType.name, streamingChannelCI.Name);
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
            var competitorDTO1 = new TeamCompetitorDTO(teamType1);
            var competitorDTO2 = new TeamCompetitorDTO(teamType2);

            var competitorCI = new TeamCompetitorCI(competitorDTO1, _cultureFirst, new TestDataRouterManager(new CacheManager(), _outputHelper));
            competitorCI.Merge(competitorDTO2, _cultureSecond);

            Assert.NotNull(competitorCI);
            Assert.Equal(competitorCI.Id.ToString(), teamType1.id);
            Assert.Equal(competitorCI.GetName(_cultureFirst), teamType1.name);
            Assert.Equal(competitorCI.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
            Assert.Equal(competitorCI.IsVirtual, teamType1.@virtual);
            Assert.Equal(competitorCI.Qualifier, teamType1.qualifier);
            Assert.Equal(competitorCI.State, teamType1.state);
            Assert.Equal(teamType1.country, competitorCI.GetCountry(_cultureFirst));
            Assert.Equal(teamType2.country, competitorCI.GetCountry(_cultureSecond));
            Assert.NotNull(competitorCI.Division);
            Assert.Equal(competitorCI.Division.Value, teamType1.division);
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
            var venueDTO1 = new VenueDTO(venue1);
            var venueDTO2 = new VenueDTO(venue2);
            var venueCI = new VenueCI(venueDTO1, _cultureFirst);
            venueCI.Merge(venueDTO2, _cultureSecond);

            Assert.NotNull(venueCI);
            Assert.Equal(venue1.id, venueCI.Id.ToString());
            Assert.Equal(venue1.name, venueCI.GetName(_cultureFirst));
            Assert.Equal(venue1.capacity, venueCI.Capacity);
            Assert.Equal(venue1.map_coordinates, venueCI.Coordinates);
            Assert.Equal(venue1.state, venueCI.State);
            Assert.Equal(venue1.city_name, venueCI.GetCity(_cultureFirst));
            Assert.Equal(venue1.country_name, venueCI.GetCountry(_cultureFirst));
            Assert.Equal(venue2.country_name, venueCI.GetCountry(_cultureSecond));
            Assert.NotEqual(venueCI.GetCountry(_cultureFirst), venueCI.GetCountry(_cultureSecond));
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
            var weatherInfoDTO = new WeatherInfoDTO(weatherInfo);
            var weatherInfoCI = new WeatherInfoCI(weatherInfoDTO);

            Assert.NotNull(weatherInfoCI);
            Assert.Equal(weatherInfo.pitch, weatherInfoCI.Pitch);
            Assert.Equal(weatherInfo.temperature_celsius, weatherInfoCI.TemperatureCelsius);
            Assert.Equal(weatherInfo.weather_conditions, weatherInfoCI.WeatherConditions);
            Assert.Equal(weatherInfo.wind, weatherInfoCI.Wind);
            Assert.Equal(weatherInfo.wind_advantage, weatherInfoCI.WindAdvantage);
        }
    }
}
