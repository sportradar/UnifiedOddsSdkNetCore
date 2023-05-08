/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SportEventDetailsMapperTests
    {
        private readonly IDeserializer<matchSummaryEndpoint> _deserializer = new Deserializer<matchSummaryEndpoint>();
        private matchSummaryEndpoint _record;
        private matchSummaryEndpoint _recordFull;
        private MatchDTO _entity;
        private MatchDTO _entityFull;

        private async Task Initialise()
        {
            if (_record is null)
            {
                await using var eventInfoStream = FileHelper.GetResource("event_info.xml");
                await using var eventInfoFullStream = FileHelper.GetResource("event_info_full.xml");
                _record = _deserializer.Deserialize(eventInfoStream);
                _entity = new MatchSummaryMapperFactory().CreateMapper(_record).Map();
                _recordFull = _deserializer.Deserialize(eventInfoFullStream);
                _entityFull = new MatchSummaryMapperFactory().CreateMapper(_recordFull).Map();
            }
        }

        [Fact]
        public async Task MappedRecordIsNotNull()
        {
            await Initialise();

            Assert.NotNull(_entity);
            Assert.Equal(2, _entity.Competitors.ToList().Count);

            Assert.NotNull(_entityFull);
            Assert.Equal(2, _entityFull.Competitors.ToList().Count);
        }

        [Fact]
        public async Task Mapping()
        {
            await Initialise();

            Assert.Equal(_entity.Id.ToString(), _record.sport_event.id);
            Assert.Equal(_entity.Scheduled, _record.sport_event.scheduledSpecified ? (DateTime?)_record.sport_event.scheduled : null);
            Assert.Equal(_entity.Season.Id.ToString(), _record.sport_event.season.id);
            Assert.Equal(_entity.Season.Name, _record.sport_event.season.name);

            MappingCompetitorsTest(_entity, _record);

            MappingConditionsTest(_entity, _record);

            MappingTournamentTest(_entity, _record);

            MappingTournamentRoundTest(_entity, _record);

            MappingVenueTest(_entity, _record);
        }

        [Fact]
        public async Task MappingTestFull()
        {
            await Initialise();

            Assert.Equal(_entityFull.Id.ToString(), _recordFull.sport_event.id);
            Assert.Equal(_entityFull.Scheduled, _recordFull.sport_event.scheduledSpecified ? (DateTime?)_recordFull.sport_event.scheduled : null);
            Assert.Equal(_entityFull.Season.Id.ToString(), _recordFull.sport_event.season.id);
            Assert.Equal(_entityFull.Season.Name, _recordFull.sport_event.season.name);

            MappingCompetitorsTest(_entityFull, _recordFull);

            MappingConditionsTest(_entityFull, _recordFull);

            MappingTournamentTest(_entityFull, _recordFull);

            MappingTournamentRoundTest(_entityFull, _recordFull);

            MappingVenueTest(_entityFull, _recordFull);
        }

        private static void MappingTournamentTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            Assert.Equal(dto.Tournament.Id.ToString(), record.sport_event.tournament.id);
            Assert.Equal(dto.Tournament.Name, record.sport_event.tournament.name);
            Assert.Equal(dto.Tournament.Sport.Id.ToString(), record.sport_event.tournament.sport.id);
            Assert.Equal(dto.Tournament.Sport.Name, record.sport_event.tournament.sport.name);
            Assert.Equal(dto.Tournament.Category.Id.ToString(), record.sport_event.tournament.category.id);
            Assert.Equal(dto.Tournament.Category.Name, record.sport_event.tournament.category.name);
        }

        private static void MappingTournamentRoundTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            Assert.Equal(dto.Round.Name, record.sport_event.tournament_round.name);
            Assert.Equal(dto.Round.Type, record.sport_event.tournament_round.type);
            Assert.Equal(dto.Round.CupRoundMatchNumber, record.sport_event.tournament_round.cup_round_match_numberSpecified ? (int?)record.sport_event.tournament_round.cup_round_match_number : null);
            Assert.Equal(dto.Round.CupRoundMatches, record.sport_event.tournament_round.cup_round_matchesSpecified ? (int?)record.sport_event.tournament_round.cup_round_matches : null);
            Assert.Equal(dto.Round.Number, record.sport_event.tournament_round.numberSpecified ? (int?)record.sport_event.tournament_round.number : null);
        }

        private static void MappingVenueTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            if (record.sport_event_conditions?.venue == null)
            {
                return;
            }

            Assert.Equal(dto.Venue.Id.ToString(), record.sport_event_conditions.venue.id);
            Assert.Equal(dto.Venue.Name, record.sport_event_conditions.venue.name);
            Assert.Equal(dto.Venue.City, record.sport_event_conditions.venue.city_name);
            Assert.Equal(dto.Venue.Coordinates, record.sport_event_conditions.venue.map_coordinates);
            Assert.Equal(dto.Venue.Country, record.sport_event_conditions.venue.country_name);
            Assert.Equal(dto.Venue.State, record.sport_event_conditions.venue.state);
            Assert.Equal(dto.Venue.Capacity, record.sport_event_conditions.venue.capacitySpecified ? (int?)record.sport_event_conditions.venue.capacity : null);
        }

        private static void MappingConditionsTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            if (record.sport_event_conditions == null)
            {
                return;
            }

            Assert.Equal(dto.Conditions.Attendance, record.sport_event_conditions.attendance);
            Assert.Equal(dto.Conditions.EventMode, record.sport_event_conditions.match_mode);

            Assert.Equal(dto.Conditions.Referee.Id.ToString(), record.sport_event_conditions.referee.id);
            Assert.Equal(dto.Conditions.Referee.Name, record.sport_event_conditions.referee.name);
            Assert.Equal(dto.Conditions.Referee.Nationality, record.sport_event_conditions.referee.nationality);
            Assert.Equal(dto.Conditions.WeatherInfo.Pitch, record.sport_event_conditions.weather_info.pitch);
            Assert.Equal(dto.Conditions.WeatherInfo.WeatherConditions, record.sport_event_conditions.weather_info.weather_conditions);
            Assert.Equal(dto.Conditions.WeatherInfo.Wind, record.sport_event_conditions.weather_info.wind);
            Assert.Equal(dto.Conditions.WeatherInfo.WindAdvantage, record.sport_event_conditions.weather_info.wind_advantage);
            Assert.Equal(dto.Conditions.WeatherInfo.TemperatureCelsius,
                                   record.sport_event_conditions.weather_info.temperature_celsiusSpecified
                                       ? (int?)record.sport_event_conditions.weather_info.temperature_celsius
                                       : null);

            MappingVenueTest(dto, record);
        }

        private static void MappingCompetitorsTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            Assert.Equal(dto.Competitors.Count(), record.sport_event.competitors.Length);
            foreach (var c in record.sport_event.competitors)
            {
                var m = dto.Competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
                Assert.NotNull(m);
                Assert.Equal(m.Qualifier, c.qualifier);
                if (c.divisionSpecified)
                {
                    Assert.NotNull(m.Division);
                    Assert.Equal(m.Division.Value, c.division);
                }
                else
                {
                    Assert.False(m.Division.HasValue);
                }
            }

            TestCompetitors(dto.Competitors.ToList(), record.sport_event.competitors);
        }

        private static void TestCompetitors(IReadOnlyList<CompetitorDTO> competitors, IReadOnlyCollection<team> comps)
        {
            Assert.Equal(competitors.Count, comps.Count);
            foreach (var c in comps)
            {
                var m = competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
                Assert.NotNull(m);
                Assert.Equal(m.Id.ToString(), c.id);
                Assert.Equal(m.Name, c.name);
                Assert.Equal(m.Abbreviation, c.abbreviation);
                Assert.Equal(m.CountryName, c.country);
                Assert.Equal(m.IsVirtual, c.virtualSpecified && c.@virtual);
            }
        }
    }
}
