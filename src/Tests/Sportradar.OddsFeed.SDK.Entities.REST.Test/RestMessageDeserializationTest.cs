/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class RestMessageDeserializationTest
    {
        /// <summary>
        /// Class under test
        /// </summary>
        private static readonly IDeserializer<RestMessage> Deserializer = new Deserializer<RestMessage>();

        [TestMethod]
        public void FixtureIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"fixtures.en.xml");
            var fixture = Deserializer.Deserialize<fixturesEndpoint>(stream);
            Assert.IsNotNull(fixture, "fixture cannot be a null reference");
        }

        [TestMethod]
        public void MatchSummaryIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"match_summary.xml");
            var matchSummary = Deserializer.Deserialize<matchSummaryEndpoint>(stream);
            Assert.IsNotNull(matchSummary, "MatchSummary should not be null reference");
        }

        [TestMethod]
        public void RaceSummaryIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"race_summary.xml");
            var stageSummary = Deserializer.Deserialize<stageSummaryEndpoint>(stream);
            Assert.IsNotNull(stageSummary, "race Summary should not be null reference");
        }

        [TestMethod]
        public void RaceSummaryGiroIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"race_summary_giro.xml");
            var stageSummary = Deserializer.Deserialize<stageSummaryEndpoint>(stream);
            Assert.IsNotNull(stageSummary, "race Summary Giro should not be null reference");
        }

        [TestMethod]
        public void EventsAreDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"events.xml");
            var events = Deserializer.Deserialize<scheduleEndpoint>(stream);
            Assert.IsNotNull(events, "events cannot be a null reference");
        }

        [TestMethod]
        public void LiveEventsAreDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"live_events.xml");
            var liveEvents = Deserializer.Deserialize<scheduleEndpoint>(stream);
            Assert.IsNotNull(liveEvents, "liveEvents cannot be a null reference");
        }

        [TestMethod]
        public void TournamentScheduleIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"tournament_schedule.en.xml");
            var schedule = Deserializer.Deserialize<tournamentSchedule>(stream);
            Assert.IsNotNull(schedule, "schedule cannot be a null reference");
        }

        [TestMethod]
        public void OngoingEventInfoIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"ongoing_event_info.xml");
            var timeline = Deserializer.Deserialize<matchTimelineEndpoint>(stream);
            Assert.IsNotNull(timeline, "timeline cannot be a null reference");
        }

        [TestMethod]
        public void AllTournamentsAreDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"tournaments.en.xml");
            var tournaments = Deserializer.Deserialize<tournamentsEndpoint>(stream);
            Assert.IsNotNull(tournaments, "tournaments cannot be a null reference");
        }

        [TestMethod]
        public void TournamentInfoIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"tournament_info.xml");
            var tournamentInfo = Deserializer.Deserialize<tournamentInfoEndpoint>(stream);
            Assert.IsNotNull(tournamentInfo, "tournamentInfo cannot be a null reference");
        }

        [TestMethod]
        public void PlayerProfileIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"Profiles\en.player.1.xml");
            var playerProfile = Deserializer.Deserialize<playerProfileEndpoint>(stream);
            Assert.IsNotNull(playerProfile, "playerProfile cannot be a null reference");
        }

        [TestMethod]
        public void CompetitorProfileIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"Profiles\en.competitor.1.xml");
            var competitorProfile = Deserializer.Deserialize<competitorProfileEndpoint>(stream);
            Assert.IsNotNull(competitorProfile, "competitorProfile cannot be a null reference");
        }

        [TestMethod]
        public void ProducersIsDeserialized()
        {
            var stream = FileHelper.OpenFile(TestData.RestXmlPath, @"producers.xml");
            var result = Deserializer.Deserialize<producers>(stream);
            Assert.IsNotNull(result, "Producers cannot be a null reference");
        }
    }
}
