/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;
// ReSharper disable PossibleNullReferenceException

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class SportListMapperTest
    {
        private const string InputXml = "tournaments.en.xml";

        private static EntityList<SportDTO> _entity;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var deserializer = new Deserializer<tournamentsEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentsMapperFactory();

            var dataProvider = new DataProvider<tournamentsEndpoint, EntityList<SportDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            _entity = dataProvider.GetDataAsync("", "en").Result;
        }

        private static XName GetXName(string localName)
        {
            return XName.Get(localName, @"http://schemas.sportradar.com/sportsapi/v1/unified");
        }

        private static void AssertValuesAreCorrect(TournamentDTO tournament, XElement element)
        {
            const string shortDateFormat = "yyyy-MM-dd";
            Assert.IsNotNull(element);

            //tournament
            Assert.AreEqual(element.Attribute("id").Value, tournament.Id.ToString());
            Assert.AreEqual(element.Attribute("name").Value, tournament.Name);
            if (element.Attribute("scheduled") != null)
            {
                Assert.AreEqual(element.Attribute("scheduled").Value.Substring(0, 10), tournament.Scheduled?.ToString(shortDateFormat));
            }
            else
            {
                Assert.IsNull(tournament.Scheduled);
            }
            if (element.Attribute("scheduled_end") != null)
            {
                Assert.AreEqual(element.Attribute("scheduled_end").Value.Substring(0, 10), tournament.ScheduledEnd?.ToString(shortDateFormat));
            }
            else
            {
                Assert.IsNull(tournament.ScheduledEnd);
            }

            //sport
            var sportElement = element.Element(GetXName("sport"));
            Assert.IsNotNull(sportElement);
            Assert.AreEqual(sportElement.Attribute("id").Value, tournament.Sport.Id.ToString());
            Assert.AreEqual(sportElement.Attribute("name").Value, tournament.Sport.Name);

            //category
            var categoryElement = element.Element(GetXName("category"));
            Assert.IsNotNull(categoryElement);
            Assert.AreEqual(categoryElement.Attribute("id").Value, tournament.Category.Id.ToString());
            Assert.AreEqual(categoryElement.Attribute("name").Value, tournament.Category.Name);

            //season
            var currentSeason = element.Element(GetXName("current_season"));
            if (currentSeason != null)
            {
                Assert.AreEqual(currentSeason.Attribute("id").Value, tournament.CurrentSeason.Id.ToString());
                Assert.AreEqual(currentSeason.Attribute("name").Value, tournament.CurrentSeason.Name);
                if (currentSeason.Attribute("start_date") != null)
                {
                    Assert.AreEqual(currentSeason.Attribute("start_date").Value, tournament.CurrentSeason.StartDate.ToString(shortDateFormat));
                }
                else
                {
                    Assert.AreEqual(DateTime.MinValue, tournament.CurrentSeason.StartDate);
                }
                if (currentSeason.Attribute("end_date") != null)
                {
                    Assert.AreEqual(currentSeason.Attribute("end_date").Value, tournament.CurrentSeason.EndDate.ToString(shortDateFormat));
                }
                else
                {
                    Assert.AreEqual(DateTime.MinValue, tournament.CurrentSeason.EndDate);
                }
                if (currentSeason.Attribute("year") != null)
                {
                    Assert.AreEqual(currentSeason.Attribute("year").Value, tournament.CurrentSeason.Year);
                }
                else
                {
                    Assert.IsTrue(string.IsNullOrEmpty(tournament.CurrentSeason.Year));
                }
                if (currentSeason.Attribute("tournament_id") != null)
                {
                    Assert.AreEqual(currentSeason.Attribute("tournament_id").Value, tournament.CurrentSeason.Id.ToString());
                }
                else
                {
                    //Assert.IsNull(tournament.Season.TournamentId);
                }
            }
            else
            {
                Assert.IsNull(tournament.CurrentSeason);
            }

            //season coverage info
            var coverage = element.Element(GetXName("season_coverage_info"));
            if (coverage != null)
            {
                Assert.AreEqual(coverage.Attribute("season_id").Value, tournament.SeasonCoverage.SeasonId.ToString());
                if (coverage.Attribute("scheduled") != null)
                {
                    Assert.AreEqual(coverage.Attribute("scheduled").Value, tournament.SeasonCoverage.Scheduled.ToString());
                }
                else
                {
                    Assert.AreEqual(0, tournament.SeasonCoverage.Scheduled);
                }
                if (coverage.Attribute("played") != null)
                {
                    Assert.AreEqual(coverage.Attribute("played").Value, tournament.SeasonCoverage.Played.ToString());
                }
                else
                {
                    Assert.AreEqual(0, tournament.SeasonCoverage.Played);
                }
                if (coverage.Attribute("max_coverage_level") != null)
                {
                    Assert.AreEqual(coverage.Attribute("max_coverage_level").Value, tournament.SeasonCoverage.MaxCoverageLevel);
                }
                else
                {
                    Assert.IsTrue(string.IsNullOrEmpty(tournament.SeasonCoverage.MaxCoverageLevel));
                }
                if (coverage.Attribute("min_coverage_level") != null)
                {
                    Assert.AreEqual(coverage.Attribute("min_coverage_level").Value, tournament.SeasonCoverage.MinCoverageLevel);
                }
                else
                {
                    Assert.IsTrue(string.IsNullOrEmpty(tournament.SeasonCoverage.MinCoverageLevel));
                }
                if (coverage.Attribute("max_covered") != null)
                {
                    Assert.AreEqual(coverage.Attribute("max_covered").Value, tournament.SeasonCoverage.MaxCovered.ToString());
                }
                else
                {
                    Assert.IsNull(tournament.SeasonCoverage.MaxCovered);
                }
            }
            else
            {
                Assert.IsNull(tournament.SeasonCoverage);
            }
        }

        private int GetNumberOfDistinctSportCategories(XDocument doc, URN sportId)
        {
            var count = 0;
            var distinctCategories = new List<string>();
            foreach (var tournamentElement in doc.Root?.Elements(GetXName("tournament")))
            {
                var categoryElement = tournamentElement.Element(GetXName("category"));
                if (categoryElement == null)
                {
                    continue;
                }

                var categoryIdAttribute = categoryElement.Attribute("id");
                if (categoryIdAttribute == null || distinctCategories.Contains(categoryIdAttribute.Value))
                {
                    continue;
                }

                distinctCategories.Add(categoryIdAttribute.Value);
                var sportElement = tournamentElement.Element(GetXName("sport"));
                if (sportElement == null)
                {
                    continue;
                }
                var attribute = sportElement.Attribute("id");
                if (attribute != null && attribute.Value == sportId.ToString())
                {
                    count ++;
                }
            }
            return count;
        }

        private int GetNumberOfDistinctTournaments(XDocument doc, URN categoryId)
        {
            var count = 0;
            var distinctTournaments = new List<string>();
            foreach (var tournamentElement in doc.Root?.Elements(GetXName("tournament")))
            {
                var categoryElement = tournamentElement.Element(GetXName("category"));
                if (categoryElement == null)
                {
                    continue;
                }

                var categoryIdAttribute = categoryElement.Attribute("id");
                if (categoryIdAttribute == null || categoryIdAttribute.Value != categoryId.ToString())
                {
                    continue;
                }

                var tournamentIdAttribute = tournamentElement.Attribute("id");
                if (tournamentIdAttribute == null || distinctTournaments.Contains(tournamentIdAttribute.Value))
                {
                    continue;
                }

                distinctTournaments.Add(tournamentIdAttribute.Value);
                count ++;
            }
            return count;
        }

        [TestMethod]
        public void TestInstanceIsNotNull()
        {
            Assert.IsNotNull(_entity, "EntityList instance cannot be a null reference");
            Assert.IsNotNull(_entity.Items, "EntityList.Items cannot be a null reference");
        }

        [TestMethod]
        public void TestNumberOfSportsIsCorrect()
        {
            var doc = XDocument.Load(TestData.RestXmlPath + InputXml);
            var distinctSportCount = doc.Root?.Elements(GetXName("tournament")).Distinct(new XElementBySportComparer()).Count();
            Assert.AreEqual(distinctSportCount, _entity.Items.Count(), "The number of retrieved sports is correct");
        }

        [TestMethod]
        public void TestNumberOfCategoriesPerSportIsCorrect()
        {
            var doc = XDocument.Load(TestData.RestXmlPath + InputXml);
            foreach (var sport in _entity.Items)
            {
                Assert.AreEqual(GetNumberOfDistinctSportCategories(doc, sport.Id), sport.Categories.Count(), $"Sport with id={sport.Id} has incorrect number of categories");
            }
        }

        [TestMethod]
        public void TestNumberOfTournamentsPerCategoryIsCorrect()
        {
            var doc = XDocument.Load(TestData.RestXmlPath + InputXml);
            foreach (var sport in _entity.Items)
            {
                foreach (var category in sport.Categories)
                {
                    Assert.AreEqual(GetNumberOfDistinctTournaments(doc, category.Id), category.Tournaments.Count(), $"Category with id={category.Id} has incorrect number of tournaments");
                }
            }
        }

        [TestMethod]
        public void TestSportListData()
        {
            var doc = XDocument.Load(TestData.RestXmlPath + InputXml);

            foreach (var sport in _entity.Items)
            {
                foreach (var category in sport.Categories)
                {
                    foreach (var tournament in category.Tournaments)
                    {
                        var tournamentElement = doc.Root?.Elements(GetXName("tournament")).First(t => t.Attribute("id").Value == tournament.Id.ToString());
                        AssertValuesAreCorrect(tournament, tournamentElement);
                    }
                }
            }
        }

        private class XElementBySportComparer : IEqualityComparer<XElement>
        {
            public bool Equals(XElement x, XElement y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                var sportElement1 = x.Element(GetXName("sport"));
                var sportElement2 = y.Element(GetXName("sport"));
                return sportElement1.Attribute("id").Value == sportElement2.Attribute("id").Value;
            }

            public int GetHashCode(XElement obj)
            {
                var sportElement = obj.Element(GetXName("sport"));
                return sportElement.Attribute("id").Value.GetHashCode();
            }
        }
    }
}
