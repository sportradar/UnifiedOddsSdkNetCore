/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

// ReSharper disable PossibleNullReferenceException

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SportListMapperTests
    {
        private const string InputXml = "tournaments_en.xml";

        private static EntityList<SportDTO> Entity;

        public SportListMapperTests()
        {
            var deserializer = new Deserializer<tournamentsEndpoint>();
            var dataFetcher = new TestDataFetcher();
            var mapperFactory = new TournamentsMapperFactory();

            var dataProvider = new DataProvider<tournamentsEndpoint, EntityList<SportDTO>>(
                TestData.RestXmlPath + InputXml,
                dataFetcher,
                deserializer,
                mapperFactory);
            Entity = dataProvider.GetDataAsync("", "en").GetAwaiter().GetResult();
        }

        [Fact]
        public void TestInstanceNotNull()
        {
            Assert.NotNull(Entity);
            Assert.NotNull(Entity.Items);
        }

        [Fact]
        public async Task TestNumberOfSportsIsCorrect()
        {
            await using var stream = FileHelper.GetResource("tournaments_en.xml");
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            var distinctSportCount = doc.Root?.Elements(GetXName("tournament")).Distinct(new XElementBySportComparer()).Count();
            Assert.Equal(distinctSportCount, Entity.Items.Count());
        }

        [Fact]
        public async Task TestNumberOfCategoriesPerSportIsCorrect()
        {
            await using var stream = FileHelper.GetResource("tournaments_en.xml");
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            foreach (var sport in Entity.Items)
            {
                Assert.Equal(GetNumberOfDistinctSportCategories(doc, sport.Id), sport.Categories.Count());
            }
        }

        [Fact]
        public async Task TestNumberOfTournamentsPerCategoryIsCorrect()
        {
            await using var stream = FileHelper.GetResource("tournaments_en.xml");
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            foreach (var sport in Entity.Items)
            {
                foreach (var category in sport.Categories)
                {
                    Assert.Equal(GetNumberOfDistinctTournaments(doc, category.Id), category.Tournaments.Count());
                }
            }
        }

        [Fact]
        public async Task TestSportListData()
        {
            await using var stream = FileHelper.GetResource("tournaments_en.xml");
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);

            foreach (var sport in Entity.Items)
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

        private static XName GetXName(string localName) => XName.Get(localName, @"http://schemas.sportradar.com/sportsapi/v1/unified");

        private static void AssertValuesAreCorrect(TournamentDTO tournament, XElement element)
        {
            Assert.NotNull(element);

            //tournament
            Assert.Equal(element.Attribute("id").Value, tournament.Id.ToString());
            Assert.Equal(element.Attribute("name").Value, tournament.Name);
            if (element.Attribute("scheduled") != null)
            {
                var elDate = SdkInfo.ParseDate(element.Attribute("scheduled").Value);
                Assert.True(elDate.Equals(tournament.Scheduled), $"Tournament {tournament.Id} expected schedule date does not match actual one.");
            }
            else
            {
                Assert.Null(tournament.Scheduled);
            }
            if (element.Attribute("scheduled_end") != null)
            {
                var elDate = SdkInfo.ParseDate(element.Attribute("scheduled_end").Value);
                Assert.Equal(elDate, tournament.ScheduledEnd);
            }
            else
            {
                Assert.Null(tournament.ScheduledEnd);
            }

            //sport
            var sportElement = element.Element(GetXName("sport"));
            Assert.NotNull(sportElement);
            Assert.Equal(sportElement.Attribute("id").Value, tournament.Sport.Id.ToString());
            Assert.Equal(sportElement.Attribute("name").Value, tournament.Sport.Name);

            //category
            var categoryElement = element.Element(GetXName("category"));
            Assert.NotNull(categoryElement);
            Assert.Equal(categoryElement.Attribute("id").Value, tournament.Category.Id.ToString());
            Assert.Equal(categoryElement.Attribute("name").Value, tournament.Category.Name);

            //season
            var currentSeason = element.Element(GetXName("current_season"));
            if (currentSeason != null)
            {
                Assert.Equal(currentSeason.Attribute("id").Value, tournament.CurrentSeason.Id.ToString());
                Assert.Equal(currentSeason.Attribute("name").Value, tournament.CurrentSeason.Name);
                if (currentSeason.Attribute("start_date") != null)
                {
                    var elDate = SdkInfo.ParseDate(currentSeason.Attribute("start_date").Value);
                    Assert.Equal(elDate, tournament.CurrentSeason.StartDate);
                }
                else
                {
                    Assert.Equal(DateTime.MinValue, tournament.CurrentSeason.StartDate);
                }
                if (currentSeason.Attribute("end_date") != null)
                {
                    var elDate = SdkInfo.ParseDate(currentSeason.Attribute("end_date").Value);
                    Assert.Equal(elDate, tournament.CurrentSeason.EndDate);
                }
                else
                {
                    Assert.Equal(DateTime.MinValue, tournament.CurrentSeason.EndDate);
                }
                if (currentSeason.Attribute("year") != null)
                {
                    Assert.Equal(currentSeason.Attribute("year").Value, tournament.CurrentSeason.Year);
                }
                else
                {
                    Assert.True(string.IsNullOrEmpty(tournament.CurrentSeason.Year));
                }
                if (currentSeason.Attribute("tournament_id") != null)
                {
                    Assert.Equal(currentSeason.Attribute("tournament_id").Value, tournament.CurrentSeason.Id.ToString());
                }
                else
                {
                    //Assert.Null(tournament.Season.TournamentId);
                }
            }
            else
            {
                Assert.Null(tournament.CurrentSeason);
            }

            //season coverage info
            var coverage = element.Element(GetXName("season_coverage_info"));
            if (coverage != null)
            {
                Assert.Equal(coverage.Attribute("season_id").Value, tournament.SeasonCoverage.SeasonId.ToString());
                if (coverage.Attribute("scheduled") != null)
                {
                    Assert.Equal(coverage.Attribute("scheduled").Value, tournament.SeasonCoverage.Scheduled.ToString());
                }
                else
                {
                    Assert.Equal(0, tournament.SeasonCoverage.Scheduled);
                }
                if (coverage.Attribute("played") != null)
                {
                    Assert.Equal(coverage.Attribute("played").Value, tournament.SeasonCoverage.Played.ToString());
                }
                else
                {
                    Assert.Equal(0, tournament.SeasonCoverage.Played);
                }
                if (coverage.Attribute("max_coverage_level") != null)
                {
                    Assert.Equal(coverage.Attribute("max_coverage_level").Value, tournament.SeasonCoverage.MaxCoverageLevel);
                }
                else
                {
                    Assert.True(string.IsNullOrEmpty(tournament.SeasonCoverage.MaxCoverageLevel));
                }
                if (coverage.Attribute("min_coverage_level") != null)
                {
                    Assert.Equal(coverage.Attribute("min_coverage_level").Value, tournament.SeasonCoverage.MinCoverageLevel);
                }
                else
                {
                    Assert.True(string.IsNullOrEmpty(tournament.SeasonCoverage.MinCoverageLevel));
                }
                if (coverage.Attribute("max_covered") != null)
                {
                    Assert.Equal(coverage.Attribute("max_covered").Value, tournament.SeasonCoverage.MaxCovered.ToString());
                }
                else
                {
                    Assert.Null(tournament.SeasonCoverage.MaxCovered);
                }
            }
            else
            {
                Assert.Null(tournament.SeasonCoverage);
            }
        }

        private static int GetNumberOfDistinctSportCategories(XDocument doc, URN sportId)
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
                    count++;
                }
            }
            return count;
        }

        private static int GetNumberOfDistinctTournaments(XDocument doc, URN categoryId)
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
                count++;
            }
            return count;
        }

        // BW: removed this test as it fails, but the original version does not work either?

        // [TestMethod]
        // public void TestSportListData()
        // {
        //     var doc = XDocument.Load(TestData.RestXmlPath + InputXml);
        //
        //     foreach (var sport in _entity.Items)
        //     {
        //         foreach (var category in sport.Categories)
        //         {
        //             foreach (var tournament in category.Tournaments)
        //             {
        //                 var tournamentElement = doc.Root?.Elements(GetXName("tournament")).First(t => t.Attribute("id").Value == tournament.Id.ToString());
        //                 AssertValuesAreCorrect(tournament, tournamentElement);
        //             }
        //         }
        //     }
        // }

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
