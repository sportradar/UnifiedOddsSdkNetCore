/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
using SR = Sportradar.OddsFeed.SDK.Test.Shared.StaticRandom;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class CacheItemMergeTest
    {
        private readonly CultureInfo _cultureFirst = new CultureInfo("en");
        private readonly CultureInfo _cultureSecond = new CultureInfo("de");

        [TestMethod]
        public void CompetitorTest()
        {
            var teamType1 = new team
            {
                abbreviation = "ABC",
                country = "Germany",
                id = SR.Urn("team").ToString(),
                name = "Team A",
                @virtual = true,
                virtualSpecified = true
            };
            var teamType2 = new team
            {
                abbreviation = "ABC",
                country = "Deutschland",
                id = SR.Urn("team").ToString(),
                name = "Team A",
                @virtual = true,
                virtualSpecified = true
            };
            var competitorDTO1 = new CompetitorDTO(teamType1);
            var competitorDTO2 = new CompetitorDTO(teamType2);

            var competitorCI = new CompetitorCI(competitorDTO1, _cultureFirst, null);
            competitorCI.Merge(competitorDTO2, _cultureSecond);

            Assert.IsNotNull(competitorCI);
            Assert.AreEqual(competitorCI.Id.ToString(), teamType1.id);
            Assert.AreEqual(competitorCI.GetName(_cultureFirst), teamType1.name);
            Assert.AreEqual(competitorCI.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
            Assert.AreEqual(competitorCI.IsVirtual, teamType1.@virtual);
            Assert.AreEqual(teamType1.country, competitorCI.GetCountry(_cultureFirst));
            Assert.AreEqual(teamType2.country, competitorCI.GetCountry(_cultureSecond));
        }

        [TestMethod]
        public void CoverageInfoMergeTest()
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

            Assert.IsNotNull(coverageInfo);
            Assert.AreEqual(coverageInfoType.level, coverageInfo.Level);
            Assert.AreEqual(coverageInfoType.live_coverage, coverageInfo.IsLive);
            Assert.AreEqual(CoveredFrom.Tv, coverageInfo.CoveredFrom);
            Assert.AreEqual(coverageInfoType.coverage.Length, coverageInfo.Includes.Count());
            Assert.AreNotEqual(coverageInfo.Includes.ToList()[0], coverageInfo.Includes.ToList()[2]);
        }

        [TestMethod]
        public void GroupTest()
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
                competitor = new[] {teamType1En, teamType2En}
            };
            var groupType2 = new tournamentGroup
            {
                name = "Group A",
                competitor = new[] { teamType1De, teamType2De }
            };
            var groupDTO1 = new GroupDTO(groupType1);
            var groupDTO2 = new GroupDTO(groupType2);

            var groupCI = new GroupCI(groupDTO1, _cultureFirst, new TestDataRouterManager(new CacheManager()));
            groupCI.Merge(groupDTO2, _cultureSecond);

            Assert.IsNotNull(groupCI);
            Assert.AreEqual(groupType1.name, groupCI.Name);
            Assert.AreEqual(groupType1.competitor.Length, groupCI.Competitors.Count());
            Assert.AreEqual(groupType1.competitor[0].id, groupCI.Competitors.ToList()[0].Id.ToString());
            Assert.AreEqual(groupType1.competitor[1].id, groupCI.Competitors.ToList()[1].Id.ToString());
            Assert.AreNotEqual(groupCI.Competitors.ToList()[0].Id, groupCI.Competitors.ToList()[1].Id);
            Assert.AreEqual(groupType1.competitor[0].country, groupCI.Competitors.ToList()[0].GetCountry(_cultureFirst));
            Assert.AreEqual(groupType2.competitor[0].country, groupCI.Competitors.ToList()[0].GetCountry(_cultureSecond));
            Assert.AreEqual(groupType2.competitor[1].country, groupCI.Competitors.ToList()[1].GetCountry(_cultureSecond));
        }

        [TestMethod]
        public void PlayerTest()
        {
            var sportEntityDTO = new SportEntityDTO("sr:player:1", "Sport Entity Name");
            var playerCI = new SportEntityCI(sportEntityDTO);

            Assert.IsNotNull(playerCI);
            Assert.AreEqual(sportEntityDTO.Id, playerCI.Id);
        }

        [TestMethod]
        public void ProductInfoTest()
        {
            var productInfoType = new productInfo
            {
                is_auto_traded = true,
                is_in_hosted_statistics = true,
                is_in_live_center_soccer = true,
                is_in_live_score = true,
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
            var productInfo = new ProductInfo(productInfoDTO.IsAutoTraded,
                                              productInfoDTO.IsInHostedStatistics,
                                              productInfoDTO.IsInLiveCenterSoccer,
                                              productInfoDTO.IsInLiveScore,
                                              productInfoDTO.ProductInfoLinks?.Select(s=>new ProductInfoLink(s.Reference, s.Name)),
                                              productInfoDTO.StreamingChannels?.Select(s=>new StreamingChannel(s.Id, s.Name)));

            Assert.IsNotNull(productInfo);
            Assert.AreEqual(productInfoType.is_auto_traded, productInfo.IsAutoTraded);
            Assert.AreEqual(productInfoType.is_in_hosted_statistics, productInfo.IsInHostedStatistics);
            Assert.AreEqual(productInfoType.is_in_live_center_soccer, productInfo.IsInLiveCenterSoccer);
            Assert.AreEqual(productInfoType.is_in_live_score, productInfo.IsInLiveScore);
            Assert.AreEqual(productInfoType.links.Length, productInfo.Links.Count());
            Assert.AreEqual(productInfoType.streaming.Length, productInfo.Channels.Count());
            Assert.AreNotEqual(productInfo.Links.ToList()[0].Name, productInfo.Links.ToList()[2].Name);
            Assert.AreNotEqual(productInfo.Channels.ToList()[0].Name, productInfo.Channels.ToList()[2].Name);
        }

        [TestMethod]
        public void ProductInfoLinkTest()
        {
            var productInfoLinkType1 = new productInfoLink
            {
                @ref = "ref 1",
                name = "name 1"
            };

            var productInfoLinkDto = new ProductInfoLinkDTO(productInfoLinkType1);
            var productInfoLink = new ProductInfoLink(productInfoLinkDto.Reference, productInfoLinkDto.Name);

            Assert.IsNotNull(productInfoLink);
            Assert.AreEqual(productInfoLinkType1.@ref, productInfoLink.Reference);
            Assert.AreEqual(productInfoLinkType1.name, productInfoLink.Name);
        }

        [TestMethod]
        public void RefereeTest()
        {
            var refereeType = new referee
            {
                id = "sr:referee:1",
                name = "John Doe",
                nationality = "German",
            };

            var refereeDTO = new RefereeDTO(refereeType);
            var refereeCI = new RefereeCI(refereeDTO, _cultureFirst);

            Assert.IsNotNull(refereeCI);
            Assert.AreEqual(refereeType.id, refereeCI.Id.ToString());
            Assert.AreEqual(refereeType.name, refereeCI.Name);
            Assert.AreEqual(refereeType.nationality, refereeCI.GetNationality(_cultureFirst));
        }

        [TestMethod]
        public void RoundMergeTest()
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

            Assert.IsNotNull(roundCI);
            Assert.AreEqual(matchRoundTypeEn.name, roundCI.GetName(_cultureFirst));
            Assert.AreEqual(matchRoundTypeDe.name, roundCI.GetName(_cultureSecond));
            Assert.AreEqual(matchRoundTypeEn.cup_round_match_number, roundCI.CupRoundMatchNumber);
            Assert.AreEqual(matchRoundTypeEn.cup_round_matches, roundCI.CupRoundMatches);
            Assert.AreEqual(matchRoundTypeEn.number, roundCI.Number);
            Assert.AreEqual(matchRoundTypeEn.type, roundCI.Type);
        }

        [TestMethod]
        public void SeasonCoverageMergeTest()
        {
            var coverageInfoCI = new SeasonCoverageCI(new SeasonCoverageDTO(RestMessageBuilder.BuildCoverageRecord("max", "min", 4, 1, 1, SR.Urn("season").ToString())));

            Assert.IsNotNull(coverageInfoCI);
            Assert.AreEqual("max", coverageInfoCI.MaxCoverageLevel);
            Assert.AreEqual("min", coverageInfoCI.MinCoverageLevel);
            Assert.AreEqual(4, coverageInfoCI.MaxCovered);
            Assert.AreEqual(1, coverageInfoCI.Played);
        }

        [TestMethod]
        public void SeasonTest()
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

            Assert.IsNotNull(seasonCI);
            Assert.AreEqual(seasonExtendedType1.id, seasonCI.Id.ToString());
            Assert.AreEqual(seasonExtendedType1.name, seasonCI.GetName(_cultureFirst));
            Assert.AreEqual(seasonExtendedType1.start_date, seasonCI.StartDate);
            Assert.AreEqual(seasonExtendedType1.end_date, seasonCI.EndDate);
            //Assert.AreEqual(seasonExtendedType1.tournament_id, seasonCI.tournament_id);
            Assert.AreEqual(seasonExtendedType1.year, seasonCI.Year);
        }

        [TestMethod]
        public void SportEntityTest()
        {
            var sportEntityCI = new SportEntityDTO("sr:sport:1", "name 1");

            Assert.IsNotNull(sportEntityCI);
            Assert.AreEqual("sr:sport:1", sportEntityCI.Id.ToString());
            Assert.AreEqual("name 1", sportEntityCI.Name);
        }

        //TODO: sportEventConditionsType has Venue, DTO and CI is missing it
        [TestMethod]
        public void SportEventConditionsTest()
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

            Assert.IsNotNull(sportEventConditionsCI);
            Assert.AreEqual(sportEventConditionType.attendance, sportEventConditionsCI.Attendance);
            Assert.AreEqual(sportEventConditionType.match_mode, sportEventConditionsCI.EventMode);
            Assert.AreEqual(sportEventConditionType.referee.id, sportEventConditionsCI.Referee.Id.ToString());
            Assert.AreEqual(sportEventConditionType.referee.name, sportEventConditionsCI.Referee.Name);
            Assert.AreEqual(sportEventConditionType.referee.nationality, sportEventConditionsCI.Referee.GetNationality(_cultureFirst));

            //Assert.AreEqual(sportEventConditionType.venue.id, sportEventConditionsCI.); TODO: missing Venue

            Assert.AreEqual(sportEventConditionType.weather_info.pitch, sportEventConditionsCI.WeatherInfo.Pitch);
            Assert.AreEqual(sportEventConditionType.weather_info.temperature_celsius, sportEventConditionsCI.WeatherInfo.TemperatureCelsius);
            Assert.AreEqual(sportEventConditionType.weather_info.weather_conditions, sportEventConditionsCI.WeatherInfo.WeatherConditions);
            Assert.AreEqual(sportEventConditionType.weather_info.wind, sportEventConditionsCI.WeatherInfo.Wind);
            Assert.AreEqual(sportEventConditionType.weather_info.wind_advantage, sportEventConditionsCI.WeatherInfo.WindAdvantage);
        }

        [TestMethod]
        public void StreamingChannelTest()
        {
            var streamingChannelType = new streamingChannel
            {
                id = 1,
                name = "name 1"
            };
            var streamingChannelDTO = new StreamingChannelDTO(streamingChannelType);
            var streamingChannelCI = new StreamingChannelCI(streamingChannelDTO);

            Assert.IsNotNull(streamingChannelCI);
            Assert.AreEqual(streamingChannelType.id, streamingChannelCI.Id);
            Assert.AreEqual(streamingChannelType.name, streamingChannelCI.Name);
        }

        [TestMethod]
        public void TeamCompetitorMergeTest()
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
                division = 1
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
                division = 1
            };
            var competitorDTO1 = new TeamCompetitorDTO(teamType1);
            var competitorDTO2 = new TeamCompetitorDTO(teamType2);

            var competitorCI = new TeamCompetitorCI(competitorDTO1, _cultureFirst, new TestDataRouterManager(new CacheManager()));
            competitorCI.Merge(competitorDTO2, _cultureSecond);

            Assert.IsNotNull(competitorCI);
            Assert.AreEqual(competitorCI.Id.ToString(), teamType1.id);
            Assert.AreEqual(competitorCI.GetName(_cultureFirst), teamType1.name);
            Assert.AreEqual(competitorCI.GetAbbreviation(_cultureFirst), teamType1.abbreviation);
            Assert.AreEqual(competitorCI.IsVirtual, teamType1.@virtual);
            Assert.AreEqual(competitorCI.Qualifier, teamType1.qualifier);
            Assert.AreEqual(teamType1.country, competitorCI.GetCountry(_cultureFirst));
            Assert.AreEqual(teamType2.country, competitorCI.GetCountry(_cultureSecond));
            Assert.IsNotNull(competitorCI.Division);
            Assert.AreEqual(competitorCI.Division.Value, teamType1.division);
        }

        [TestMethod]
        public void VenueMergeTest()
        {
            var venue1 = new venue
            {
                id = SR.Urn("venue").ToString(),
                capacity = 5,
                capacitySpecified = true,
                city_name = "my city",
                country_name = "eng country name",
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
                map_coordinates = "coordinates",
                name = "eng name"
            };
            var venueDTO1 = new VenueDTO(venue1);
            var venueDTO2 = new VenueDTO(venue2);
            var venueCI = new VenueCI(venueDTO1, _cultureFirst);
            venueCI.Merge(venueDTO2, _cultureSecond);

            Assert.IsNotNull(venueCI);
            Assert.AreEqual(venue1.id, venueCI.Id.ToString());
            Assert.AreEqual(venue1.name, venueCI.GetName(_cultureFirst));
            Assert.AreEqual(venue1.capacity, venueCI.Capacity);
            Assert.AreEqual(venue1.map_coordinates, venueCI.Coordinates);
            Assert.AreEqual(venue1.city_name, venueCI.GetCity(_cultureFirst));
            Assert.AreEqual(venue1.country_name, venueCI.GetCountry(_cultureFirst));
            Assert.AreEqual(venue2.country_name, venueCI.GetCountry(_cultureSecond));
            Assert.AreNotEqual(venueCI.GetCountry(_cultureFirst), venueCI.GetCountry(_cultureSecond));
        }

        [TestMethod]
        public void WeatherInfoTest()
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

            Assert.IsNotNull(weatherInfoCI);
            Assert.AreEqual(weatherInfo.pitch, weatherInfoCI.Pitch);
            Assert.AreEqual(weatherInfo.temperature_celsius, weatherInfoCI.TemperatureCelsius);
            Assert.AreEqual(weatherInfo.weather_conditions, weatherInfoCI.WeatherConditions);
            Assert.AreEqual(weatherInfo.wind, weatherInfoCI.Wind);
            Assert.AreEqual(weatherInfo.wind_advantage, weatherInfoCI.WindAdvantage);
        }
    }
}
