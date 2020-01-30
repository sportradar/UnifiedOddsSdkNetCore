/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public class FixtureInternalMapperTest
    {
        private static readonly IDeserializer<fixturesEndpoint> Deserializer = new Deserializer<fixturesEndpoint>();

        private const string InputXml = "fixtures.en.xml";

        private fixturesEndpoint _record;

        private FixtureDTO _entity;

        private AssertHelper _assertHelper;

        [TestInitialize]
        public void Init()
        {
            _record = Deserializer.Deserialize(FileHelper.OpenFile(TestData.RestXmlPath, InputXml));
            _entity = new FixtureMapperFactory().CreateMapper(_record).Map();

            _assertHelper = new AssertHelper(_entity);
        }

        [TestMethod]
        public void MappedRecordIsNotNull()
        {
            Assert.IsNotNull(_entity);
            Assert.AreEqual(_entity.Competitors.ToList().Count, 2, "Value fixtures.Competitors.ToList().Count is not correct.");
        }

        [TestMethod]
        public void MappingTest()
        {
            _assertHelper.AreEqual(() => _entity.Id.ToString(), _record.fixture.id);

            _assertHelper.AreEqual(() => _entity.Scheduled, _record.fixture.scheduledSpecified ? (DateTime?)_record.fixture.scheduled : null);
            _assertHelper.AreEqual(() => _entity.StartTime, _record.fixture.start_time);
            _assertHelper.AreEqual(() => _entity.NextLiveTime, SdkInfo.ParseDate(_record.fixture.next_live_time));
            _assertHelper.AreEqual(() => _entity.StartTimeTBD, _record.fixture.start_time_tbdSpecified ? (bool?) _record.fixture.start_time_tbd : null);
            _assertHelper.AreEqual(() => _entity.StartTimeConfirmed, _record.fixture.start_time_confirmed);

            _assertHelper.AreEqual(() => _entity.Season.Id.ToString(), _record.fixture.season.id);
            _assertHelper.AreEqual(() => _entity.Season.Name, _record.fixture.season.name);

            TestCompetitors(_entity.Competitors.ToList(), _record.fixture.competitors, _assertHelper);

            MappingExtraInfoTest();

            MappingProductInfoTest();

            MappingTvChannelsTest();

            MappingTournamentTest();

            MappingTournamentRoundTest();

            MappingVenueTest();
        }

        [TestMethod]
        public void MappingTournamentTest()
        {
            _assertHelper.AreEqual(() => _entity.Tournament.Id.ToString(), _record.fixture.tournament.id);
            _assertHelper.AreEqual(() => _entity.Tournament.Name, _record.fixture.tournament.name);

            _assertHelper.AreEqual(() => _entity.Tournament.Sport.Id.ToString(), _record.fixture.tournament.sport.id);
            _assertHelper.AreEqual(() => _entity.Tournament.Sport.Name, _record.fixture.tournament.sport.name);

            _assertHelper.AreEqual(() => _entity.Tournament.Category.Id.ToString(), _record.fixture.tournament.category.id);
            _assertHelper.AreEqual(() => _entity.Tournament.Category.Name, _record.fixture.tournament.category.name);
        }

        [TestMethod]
        public void MappingTournamentRoundTest()
        {
            _assertHelper.AreEqual(() => _entity.Round.Name, _record.fixture.tournament_round.name);
            _assertHelper.AreEqual(() => _entity.Round.Type, _record.fixture.tournament_round.type);
            _assertHelper.AreEqual(() => _entity.Round.CupRoundMatchNumber, _record.fixture.tournament_round.cup_round_match_numberSpecified ? (int?)_record.fixture.tournament_round.cup_round_match_number : null);
            _assertHelper.AreEqual(() => _entity.Round.CupRoundMatches, _record.fixture.tournament_round.cup_round_matchesSpecified ? (int?)_record.fixture.tournament_round.cup_round_matches : null);
            _assertHelper.AreEqual(() => _entity.Round.Number, _record.fixture.tournament_round.numberSpecified ? (int?)_record.fixture.tournament_round.number : null);
        }

        [TestMethod]
        public void MappingVenueTest()
        {
            if (_record.fixture.venue == null) return;


            _assertHelper.AreEqual(() => _entity.Venue.Id.ToString(), _record.fixture.venue.id);
            _assertHelper.AreEqual(() => _entity.Venue.Name, _record.fixture.venue.name);
            _assertHelper.AreEqual(() => _entity.Venue.City, _record.fixture.venue.city_name);
            _assertHelper.AreEqual(() => _entity.Venue.Coordinates, _record.fixture.venue.map_coordinates);
            _assertHelper.AreEqual(() => _entity.Venue.Country, _record.fixture.venue.country_name);
            _assertHelper.AreEqual(() => _entity.Venue.State, _record.fixture.venue.state);
            _assertHelper.AreEqual(() => _entity.Venue.Capacity, _record.fixture.venue.capacitySpecified ? (int?)_record.fixture.venue.capacity : null);
        }


        [TestMethod]
        public void MappingTvChannelsTest()
        {
            if (_record.fixture.tv_channels == null) return;

            foreach (var i in _record.fixture.tv_channels)
            {
                var j = _entity.TvChannels.FirstOrDefault(x => x.Name == i.name);
                _assertHelper.AreEqual(() => j.Name, i.name);
                _assertHelper.AreEqual(() => j.StartTime, i.start_timeSpecified ? (DateTime?)i.start_time : null);
            }
        }

        [TestMethod]
        public void MappingProductInfoTest()
        {
            if (_record.fixture.product_info == null) return;

            if (_record.fixture.product_info.links != null)
            {
                foreach (var i in _record.fixture.product_info.links)
                {
                    var j = _entity.ProductInfo.ProductInfoLinks.FirstOrDefault(x => x.Name == i.name);
                    _assertHelper.AreEqual(() => j.Reference, i.@ref);
                }
            }
            if (_record.fixture.product_info.streaming != null)
            {
                foreach (var i in _record.fixture.product_info.streaming)
                {
                    var j = _entity.ProductInfo.StreamingChannels.FirstOrDefault(x => x.Id == i.id);
                    _assertHelper.AreEqual(() => j.Name, i.name);
                }
            }
        }

        [TestMethod]
        [SuppressMessage("ReSharper", "RedundantToStringCall")]
        public void MappingExtraInfoTest()
        {
            _assertHelper.AreEqual(() => _entity.ExtraInfo.Count, _record.fixture.extra_info.Length);
            foreach (var ei in _record.fixture.extra_info)
            {
                var eci = _entity.ExtraInfo[ei.key];
                _assertHelper.AreEqual(() => eci.ToString(), ei.value.ToString());
            }
        }

        private static void TestCompetitors(IReadOnlyList<TeamCompetitorDTO> competitors, IReadOnlyCollection<teamCompetitor> comps, AssertHelper assertHelper)
        {
            assertHelper.AreEqual(() => competitors.Count, comps.Count);
            foreach (var c in comps)
            {
                var m = competitors.FirstOrDefault(x => x.Id.ToString() == c.id);
                Assert.IsNotNull(m, $"Missing ITeamCompetitor with id: {c.id}.");
                assertHelper.AreEqual(() => m.Id.ToString(), c.id);
                assertHelper.AreEqual(() => m.Qualifier, c.qualifier);
                assertHelper.AreEqual(() => m.Name, c.name);
                assertHelper.AreEqual(() => m.Abbreviation, c.abbreviation);
                assertHelper.AreEqual(() => m.CountryName, c.country);
                assertHelper.AreEqual(() => m.IsVirtual, c.virtualSpecified && c.@virtual);

                if (c.divisionSpecified)
                {
                    assertHelper.AreEqual(() => m.Division.Value, c.division);
                }
                else
                {
                    assertHelper.AreEqual(() => m.Division.HasValue, false);
                }
            }
        }
    }
}
