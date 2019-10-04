/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class TournamentScheduleMapperTest
    {
        private const string InputXml = "tournament_schedule.{1}.xml";

        private static EntityList<SportEventSummaryDTO> _entity;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var deserializer = new Deserializer<tournamentSchedule>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentScheduleMapperFactory();

            var dataProvider = new DataProvider<tournamentSchedule, EntityList<SportEventSummaryDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", "en").Result;
        }

        private static XName GetXName(string localName)
        {
            return XName.Get(localName, "http://schemas.sportradar.com/sportsapi/v1/unified");
        }

        [TestMethod]
        public void TestDataEquality()
        {
            Assert.IsNotNull(_entity);
            Assert.IsNotNull(_entity.Items);

            var doc = XDocument.Load(string.Format(TestData.RestXmlPath + InputXml, "1", "en"));

            var allXmlSportEvents = doc.Descendants(GetXName("sport_event")).ToList();
            Assert.AreEqual(_entity.Items.Count(), allXmlSportEvents.Count);
            foreach(var sportEvent in allXmlSportEvents)
            {
                var xmlId = sportEvent.Attribute(XName.Get("id"))?.Value;
                var entityEvent = _entity.Items.ToList().Find(t => t.Id.ToString() == xmlId);

                CheckEventEquality(entityEvent, sportEvent);
            }
        }

        private static void CheckEventEquality(SportEventSummaryDTO eventSummary, XElement element)
        {
            Assert.IsNotNull(eventSummary);
            Assert.IsNotNull(element);
            Assert.AreEqual(eventSummary.Id.ToString(), element.Attribute(XName.Get("id"))?.Value);
            string scheduled = element.Attribute(XName.Get("scheduled")) == null
                ? null
                : element.Attribute(XName.Get("scheduled"))?.Value.Substring(0, 10);

            Assert.AreEqual(eventSummary.Scheduled?.ToString("yyyy-MM-dd"), scheduled);

            var match = eventSummary as MatchDTO;
            if (match != null)
            {
                CompareRound(match.Round, element.Element(GetXName("tournament_round")));
                CompareEntity(match.Season, element.Element(GetXName("season")));
                CompareTournament(match.Tournament, element.Element(GetXName("tournament")));

                var allXmlCompetitors = element.Descendants(GetXName("competitor")).ToList();
                Assert.AreEqual(match.Competitors.Count(), allXmlCompetitors.Count);
                foreach (var xmlCompetitor in allXmlCompetitors)
                {
                    var xmlId = xmlCompetitor.Attribute(XName.Get("id"))?.Value;
                    var competitor = match.Competitors.ToList().Find(t => t.Id.ToString() == xmlId);
                    CompareCompetitor(competitor, xmlCompetitor);
                }
            }
        }

        private static void CompareRound(RoundDTO round, XElement element)
        {
            Assert.IsNotNull(round);
            Assert.IsNotNull(element);

            Assert.AreEqual(round.Type, element.Attribute("type")?.Value);
            var number = element.Attribute("number")?.Value;
            Assert.AreEqual(round.Number, number == null ? (int?)null : int.Parse(number));
        }

        private static void CompareTournament(TournamentDTO tournament, XElement element)
        {
            CompareEntity(tournament, element);

            CompareEntity(tournament.Sport, element.Element(GetXName("sport")));
            CompareEntity(tournament.Category, element.Element(GetXName("category")));
        }

        private static void CompareEntity(SportEntityDTO entity, XElement element)
        {
            Assert.IsNotNull(entity);
            Assert.IsNotNull(element);

            Assert.AreEqual(entity.Id.ToString(), element.Attribute("id")?.Value);
            Assert.AreEqual(entity.Name, element.Attribute("name")?.Value);
        }

        private static void CompareCompetitor(CompetitorDTO competitor, XElement element)
        {
            Assert.IsNotNull(competitor);
            Assert.IsNotNull(element);

            Assert.AreEqual(competitor.Id.ToString(), element.Attribute("id")?.Value);
            Assert.AreEqual(competitor.Name, element.Attribute("name")?.Value);
            Assert.AreEqual(competitor.CountryName, element.Attribute("country")?.Value);
            Assert.AreEqual(competitor.Abbreviation, element.Attribute("abbreviation")?.Value);
            Assert.AreEqual(competitor.CountryCode, element.Attribute("country_code")?.Value);
            var refs = element.Element(GetXName("reference_ids"));
            if (refs == null)
            {
                Assert.IsTrue(!competitor.ReferenceIds.Any());
                return;
            }

            refs = refs.Descendants(GetXName("reference_id")).FirstOrDefault(f => (string)f.Attribute("name") == "betradar");
            if (refs != null)
            {
                Assert.AreEqual(competitor.ReferenceIds["betradar"], refs.Attribute("value")?.Value);
            }
            else
            {
                Assert.IsTrue(!competitor.ReferenceIds.ContainsKey("betradar"));
            }
        }
    }
}
