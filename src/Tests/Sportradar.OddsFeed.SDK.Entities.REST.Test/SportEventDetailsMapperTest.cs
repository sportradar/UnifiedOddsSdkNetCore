/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportEventDetailsMapperTest
    {
        private static readonly IDeserializer<matchSummaryEndpoint> Deserializer = new Deserializer<matchSummaryEndpoint>();

        private const string InputXml = "event_info.xml";
        private const string InputFullXml = "event_info_full.xml";

        private matchSummaryEndpoint _record;
        private matchSummaryEndpoint _recordFull;

        private MatchDTO _entity;
        private MatchDTO _entityFull;

        private AssertHelper _assertHelper;

        [TestInitialize]
        public void Init()
        {
            _record = Deserializer.Deserialize(FileHelper.OpenFile(TestData.RestXmlPath, InputXml));
            _entity = new MatchSummaryMapperFactory().CreateMapper(_record).Map();

            _recordFull = Deserializer.Deserialize(FileHelper.OpenFile(TestData.RestXmlPath, InputFullXml));
            _entityFull = new MatchSummaryMapperFactory().CreateMapper(_recordFull).Map();

            _assertHelper = new AssertHelper(_entity);
        }

        [TestMethod]
        public void MappedRecordIsNotNull()
        {
            Assert.IsNotNull(_entity);
            Assert.AreEqual(_entity.Competitors.ToList().Count, 2, "Value Competitors.ToList().Count is not correct.");

            Assert.IsNotNull(_entityFull);
            Assert.AreEqual(_entityFull.Competitors.ToList().Count, 2, "Value Competitors.ToList().Count is not correct.");
        }

        [TestMethod]
        public void MappingTest()
        {
            _assertHelper.AreEqual(() => _entity.Id.ToString(), _record.sport_event.id);

            _assertHelper.AreEqual(() => _entity.Scheduled, _record.sport_event.scheduledSpecified ? (DateTime?)_record.sport_event.scheduled : null);

            _assertHelper.AreEqual(() => _entity.Season.Id.ToString(), _record.sport_event.season.id);
            _assertHelper.AreEqual(() => _entity.Season.Name, _record.sport_event.season.name);

            MappingCompetitorsTest(_entity, _record);

            MappingConditionsTest(_entity, _record);

            MappingTournamentTest(_entity, _record);

            MappingTournamentRoundTest(_entity, _record);

            MappingVenueTest(_entity, _record);
        }

        [TestMethod]
        public void MappingTestFull()
        {
            _assertHelper.AreEqual(() => _entityFull.Id.ToString(), _recordFull.sport_event.id);

            _assertHelper.AreEqual(() => _entityFull.Scheduled, _recordFull.sport_event.scheduledSpecified ? (DateTime?)_recordFull.sport_event.scheduled : null);

            _assertHelper.AreEqual(() => _entityFull.Season.Id.ToString(), _recordFull.sport_event.season.id);
            _assertHelper.AreEqual(() => _entityFull.Season.Name, _recordFull.sport_event.season.name);

            MappingCompetitorsTest(_entityFull, _recordFull);

            MappingConditionsTest(_entityFull, _recordFull);

            MappingTournamentTest(_entityFull, _recordFull);

            MappingTournamentRoundTest(_entityFull, _recordFull);

            MappingVenueTest(_entityFull, _recordFull);
        }

        private void MappingTournamentTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            _assertHelper.AreEqual(() => dto.Tournament.Id.ToString(), record.sport_event.tournament.id);
            _assertHelper.AreEqual(() => dto.Tournament.Name, record.sport_event.tournament.name);
            _assertHelper.AreEqual(() => dto.Tournament.Sport.Id.ToString(), record.sport_event.tournament.sport.id);
            _assertHelper.AreEqual(() => dto.Tournament.Sport.Name, record.sport_event.tournament.sport.name);
            _assertHelper.AreEqual(() => dto.Tournament.Category.Id.ToString(), record.sport_event.tournament.category.id);
            _assertHelper.AreEqual(() => dto.Tournament.Category.Name, record.sport_event.tournament.category.name);
        }

        private void MappingTournamentRoundTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            _assertHelper.AreEqual(() => dto.Round.Name, record.sport_event.tournament_round.name);
            _assertHelper.AreEqual(() => dto.Round.Type, record.sport_event.tournament_round.type);
            _assertHelper.AreEqual(() => dto.Round.CupRoundMatchNumber, record.sport_event.tournament_round.cup_round_match_numberSpecified ? (int?)record.sport_event.tournament_round.cup_round_match_number : null);
            _assertHelper.AreEqual(() => dto.Round.CupRoundMatches, record.sport_event.tournament_round.cup_round_matchesSpecified ? (int?)record.sport_event.tournament_round.cup_round_matches : null);
            _assertHelper.AreEqual(() => dto.Round.Number, record.sport_event.tournament_round.numberSpecified ? (int?)record.sport_event.tournament_round.number : null);
        }

        private void MappingVenueTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            if (record.sport_event_conditions?.venue == null) return;

            _assertHelper.AreEqual(() => dto.Venue.Id.ToString(), record.sport_event_conditions.venue.id);
            _assertHelper.AreEqual(() => dto.Venue.Name, record.sport_event_conditions.venue.name);
            _assertHelper.AreEqual(() => dto.Venue.City, record.sport_event_conditions.venue.city_name);
            _assertHelper.AreEqual(() => dto.Venue.Coordinates, record.sport_event_conditions.venue.map_coordinates);
            _assertHelper.AreEqual(() => dto.Venue.Country, record.sport_event_conditions.venue.country_name);
            _assertHelper.AreEqual(() => dto.Venue.Capacity, record.sport_event_conditions.venue.capacitySpecified ? (int?)record.sport_event_conditions.venue.capacity : null);
        }

        private void MappingConditionsTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            if (record.sport_event_conditions == null) return;

            _assertHelper.AreEqual(() => dto.Conditions.Attendance, record.sport_event_conditions.attendance);
            _assertHelper.AreEqual(() => dto.Conditions.EventMode, record.sport_event_conditions.match_mode);

            _assertHelper.AreEqual(() => dto.Conditions.Referee.Id.ToString(), record.sport_event_conditions.referee.id);
            _assertHelper.AreEqual(() => dto.Conditions.Referee.Name, record.sport_event_conditions.referee.name);
            _assertHelper.AreEqual(() => dto.Conditions.Referee.Nationality, record.sport_event_conditions.referee.nationality);
            //_assertHelper.AreEqual(() => dto.Conditions.Referee.Value, record.sport_event_conditions.referee.Value);

            //_assertHelper.AreEqual(() => null, record.sport_event_conditions.weather_info.Value);
            _assertHelper.AreEqual(() => dto.Conditions.WeatherInfo.Pitch, record.sport_event_conditions.weather_info.pitch);
            _assertHelper.AreEqual(() => dto.Conditions.WeatherInfo.WeatherConditions, record.sport_event_conditions.weather_info.weather_conditions);
            _assertHelper.AreEqual(() => dto.Conditions.WeatherInfo.Wind, record.sport_event_conditions.weather_info.wind);
            _assertHelper.AreEqual(() => dto.Conditions.WeatherInfo.WindAdvantage, record.sport_event_conditions.weather_info.wind_advantage);
            _assertHelper.AreEqual(() => dto.Conditions.WeatherInfo.TemperatureCelsius,
                                   record.sport_event_conditions.weather_info.temperature_celsiusSpecified
                                       ? (int?) record.sport_event_conditions.weather_info.temperature_celsius
                                       : null);

            MappingVenueTest(dto, record);
        }

        private void MappingCompetitorsTest(MatchDTO dto, matchSummaryEndpoint record)
        {
            //_assertHelper.AreEqual(() => dto.Competitors.Count().ToString(), record.sport_event.competitors.Length.ToString());
            Assert.AreEqual(dto.Competitors.Count(), record.sport_event.competitors.Length);
            foreach (var c in record.sport_event.competitors)
            {
                var m = dto.Competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
                Assert.IsNotNull(m, $"Missing ITeamCompetitor with id: {c.id}.");
                _assertHelper.AreEqual(() => m.Qualifier, c.qualifier);
                if (c.divisionSpecified)
                {
                    _assertHelper.AreEqual(() => m.Division.Value, c.division);
                }
                else
                {
                    _assertHelper.AreEqual(() => m.Division.HasValue, false);
                }
            }

            TestCompetitors(dto.Competitors.ToList(), record.sport_event.competitors, _assertHelper);
        }

        private static void TestCompetitors(IReadOnlyList<CompetitorDTO> competitors, IReadOnlyCollection<team> comps, AssertHelper assertHelper)
        {
            assertHelper.AreEqual(() => competitors.Count, comps.Count);
            foreach (var c in comps)
            {
                var m = competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
                Assert.IsNotNull(m, $"Missing ICompetitor with id: {c.id}.");
                assertHelper.AreEqual(() => m.Id.ToString(), c.id);
                assertHelper.AreEqual(() => m.Name, c.name);
                assertHelper.AreEqual(() => m.Abbreviation, c.abbreviation);
                assertHelper.AreEqual(() => m.CountryName, c.country);
                assertHelper.AreEqual(() => m.IsVirtual, c.virtualSpecified && c.@virtual);
            }
        }
    }
}
