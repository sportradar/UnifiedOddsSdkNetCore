/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Linq;
using System.Xml.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class TournamentScheduleMapperTests
    {
        private const string InputXml = "tournament_schedule_{1}.xml";

        private readonly EntityList<SportEventSummaryDTO> _entity;

        public TournamentScheduleMapperTests()
        {
            var deserializer = new Deserializer<tournamentSchedule>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentScheduleMapperFactory();

            var dataProvider = new DataProvider<tournamentSchedule, EntityList<SportEventSummaryDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", "en").GetAwaiter().GetResult();

        }

        private static XName GetXName(string localName)
        {
            return XName.Get(localName, "http://schemas.sportradar.com/sportsapi/v1/unified");
        }

        [Fact]
        public void TestDataEquality()
        {
            Assert.NotNull(_entity);
            Assert.NotNull(_entity.Items);

            var doc = XDocument.Load(string.Format(TestData.RestXmlPath + InputXml, "1", "en"));

            var allXmlSportEvents = doc.Descendants(GetXName("sport_event")).ToList();
            Assert.Equal(_entity.Items.Count(), allXmlSportEvents.Count);
            foreach (var sportEvent in allXmlSportEvents)
            {
                var xmlId = sportEvent.Attribute(XName.Get("id"))?.Value;
                var entityEvent = _entity.Items.ToList().Find(t => t.Id.ToString() == xmlId);

                CheckEventEquality(entityEvent, sportEvent);
            }
        }

        private static void CheckEventEquality(SportEventSummaryDTO eventSummary, XElement element)
        {
            Assert.NotNull(eventSummary);
            Assert.NotNull(element);
            Assert.Equal(eventSummary.Id.ToString(), element.Attribute(XName.Get("id"))?.Value);
            var scheduled = element.Attribute(XName.Get("scheduled")) == null
                ? null
                : element.Attribute(XName.Get("scheduled"))?.Value.Substring(0, 10);

            Assert.Equal(eventSummary.Scheduled?.ToString("yyyy-MM-dd"), scheduled);

            var match = eventSummary as MatchDTO;
            if (match != null)
            {
                CompareRound(match.Round, element.Element(GetXName("tournament_round")));
                CompareEntity(match.Season, element.Element(GetXName("season")));
                CompareTournament(match.Tournament, element.Element(GetXName("tournament")));

                var allXmlCompetitors = element.Descendants(GetXName("competitor")).ToList();
                Assert.Equal(match.Competitors.Count(), allXmlCompetitors.Count);
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
            Assert.NotNull(round);
            Assert.NotNull(element);

            Assert.Equal(round.Type, element.Attribute("type")?.Value);
            var number = element.Attribute("number")?.Value;
            Assert.Equal(round.Number, number == null ? (int?)null : int.Parse(number));
        }

        private static void CompareTournament(TournamentDTO tournament, XElement element)
        {
            CompareEntity(tournament, element);

            CompareEntity(tournament.Sport, element.Element(GetXName("sport")));
            CompareEntity(tournament.Category, element.Element(GetXName("category")));
        }

        private static void CompareEntity(SportEntityDTO entity, XElement element)
        {
            Assert.NotNull(entity);
            Assert.NotNull(element);

            Assert.Equal(entity.Id.ToString(), element.Attribute("id")?.Value);
            Assert.Equal(entity.Name, element.Attribute("name")?.Value);
        }

        private static void CompareCompetitor(CompetitorDTO competitor, XElement element)
        {
            Assert.NotNull(competitor);
            Assert.NotNull(element);

            Assert.Equal(competitor.Id.ToString(), element.Attribute("id")?.Value);
            Assert.Equal(competitor.Name, element.Attribute("name")?.Value);
            Assert.Equal(competitor.CountryName, element.Attribute("country")?.Value);
            Assert.Equal(competitor.Abbreviation, element.Attribute("abbreviation")?.Value);
            Assert.Equal(competitor.CountryCode, element.Attribute("country_code")?.Value);
            Assert.Equal(competitor.State, element.Attribute("state")?.Value);
            var refs = element.Element(GetXName("reference_ids"));
            if (refs == null)
            {
                Assert.True(!competitor.ReferenceIds.Any());
                return;
            }

            refs = refs.Descendants(GetXName("reference_id")).FirstOrDefault(f => (string)f.Attribute("name") == "betradar");
            if (refs != null)
            {
                Assert.Equal(competitor.ReferenceIds["betradar"], refs.Attribute("value")?.Value);
            }
            else
            {
                Assert.True(!competitor.ReferenceIds.ContainsKey("betradar"));
            }
        }
    }
}
