/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    //TODO The following tests should be updated / fixed to have separate tests for each of ISportEvent implementation
    [TestClass]
    public class SportEntityFactoryTest
    {
        private TestSportEntityFactoryBuilder _sef;

        [TestInitialize]
        public void Init()
        {
            SdkLoggerFactory.Configure(new FileInfo("log4net.sdk.config"));

            _sef = new TestSportEntityFactoryBuilder();
            _sef.InitializeSportEntities();
            _sef.LoadTournamentMissingValues();
            _sef.LoadSeasonMissingValues();
        }

        private void InitializeSportEntities()
        {
            _sef.Competition = _sef.SportEntityFactory.BuildSportEvent<ICompetition>(TestData.EventId, URN.Parse("sr:sport:3"), TestData.Cultures, TestData.ThrowingStrategy);
            _sef.Sport = _sef.SportEntityFactory.BuildSportAsync(TestData.SportId, TestData.Cultures, TestData.ThrowingStrategy).Result;
            _sef.Sports = _sef.SportEntityFactory.BuildSportsAsync(TestData.Cultures, TestData.ThrowingStrategy).Result?.ToList();
            _sef.Tournament = _sef.SportEntityFactory.BuildSportEvent<ITournament>(TestData.TournamentId, URN.Parse("sr:sport:3"), TestData.Cultures, TestData.ThrowingStrategy);
        }

        [TestMethod]
        public void SportEventTest()
        {
            InitializeSportEntities();
            Assert.IsNotNull(_sef.Competition);

            ValidateSportEvent(_sef.Competition);
        }

        [TestMethod]
        public void SportTest()
        {
            InitializeSportEntities();
            ValidateSport(_sef.Sport, TestData.SportId, 122);
            Assert.AreNotEqual(_sef.Sport.Names[TestData.Culture], _sef.Sport.Names[new CultureInfo("hu")]);
        }

        [TestMethod]
        public void SportsTest()
        {
            InitializeSportEntities();
            Assert.IsNotNull(_sef.Sports);

            var sameSportNameCount = 0;
            foreach (var s in _sef.Sports)
            {
                var result = ValidateSport(s, s.Id, 0);
                if (!result)
                {
                    sameSportNameCount++;
                }
                //Debug.Assert(!result, $"Sport {s.Id} has all categories translations the same.");
            }
            Assert.AreNotEqual(_sef.Sports.Count, sameSportNameCount, "All sports have the same categories name translations.");
        }

        [TestMethod]
        public void TournamentTest()
        {
            Assert.IsNotNull(_sef.Tournament);
            Assert.AreEqual(TestData.TournamentId, _sef.Tournament.Id, "Id not correct.");

            var category = _sef.Tournament.GetCategoryAsync().Result;
            var sport = _sef.Tournament.GetSportAsync().Result;

            var coverage = _sef.Tournament.GetTournamentCoverage().Result;
            var season = _sef.Tournament.GetCurrentSeasonAsync().Result;
            var seasons = _sef.Tournament.GetSeasonsAsync().Result;

            Assert.IsNotNull(category);
            Assert.IsNotNull(sport);
            Assert.IsNull(coverage);
            Assert.IsNotNull(season);
            Assert.IsNotNull(seasons);

            Assert.AreEqual(11, seasons.Count());
        }

        [TestMethod]
        public void SeasonTest()
        {
            Assert.IsNotNull(_sef.Season);
            Assert.AreEqual(TestData.SeasonId, _sef.Season.Id, "Id not correct.");

            //var category = _sef.Season.GetCategoryAsync().Result;
            var sport = _sef.Season.GetSportAsync().Result;

            var tournamentCoverage = _sef.Season.GetTournamentCoverage().Result;
            var round = _sef.Season.GetCurrentRoundAsync().Result;
            var competitors = _sef.Season.GetCompetitorsAsync().Result;
            var groups = _sef.Season.GetGroupsAsync().Result;
            var schedule = _sef.Season.GetScheduleAsync().Result;
            var seasonCoverage = _sef.Season.GetSeasonCoverageAsync().Result;
            var tourInfo = _sef.Season.GetTournamentInfoAsync().Result;
            var year = _sef.Season.GetYearAsync().Result;

            Assert.IsNotNull(sport);
            Assert.IsNotNull(tournamentCoverage);
            Assert.IsNull(competitors);
            Assert.IsNotNull(round);
            Assert.IsNotNull(groups);
            var enumerable = groups.ToList();
            Assert.IsTrue(enumerable.Any());
            Assert.AreEqual(20, enumerable.First().Competitors.Count());
            Assert.IsNotNull(schedule);
            Assert.AreEqual(241, schedule.Count());
            Assert.IsNull(seasonCoverage);
            Assert.IsNotNull(tourInfo);
            Assert.IsNotNull(year);
        }

        [TestMethod]
        public void CheckIfTournamentDataIsTransferedToSportDataCache()
        {
            var tournamentId = URN.Parse("sr:tournament:1030");

            InitializeSportEntities();
            var tourCount = _sef.SportEventCache.Cache.Count(c => c.Key.Contains("season") || c.Key.Contains("tournament"));
            Assert.IsNotNull(_sef.Tournament);

            var tournamentCI = (TournamentInfoCI) _sef.SportEventCache.GetEventCacheItem(tournamentId);
            Assert.AreEqual(tourCount, _sef.SportEventCache.Cache.Count(c => c.Key.Contains("season") || c.Key.Contains("tournament")));

            _sef.SportEventCache.CacheDeleteItem(tournamentId, CacheItemType.Tournament);
            Assert.IsFalse(_sef.SportEventCache.Cache.Contains(tournamentId.ToString()));

            var tour = _sef.SportEventCache.GetEventCacheItem(tournamentId);
            Assert.AreEqual(tournamentId, tour.Id);
            Assert.IsTrue(_sef.SportEventCache.Cache.Contains(tournamentId.ToString()));

            tour = _sef.SportEventCache.GetEventCacheItem(tournamentId);

            Assert.IsNotNull(tournamentCI);
            Assert.IsNotNull(tour);
        }

        [TestMethod]
        public void MultiThreadInvokesEventOnTournamentReceivedTest()
        {
            // we do it several times, because not every time race condition was met
            for (var j = 0; j < 3; j++)
            {
                _sef.SportEventCache.Cache.ToList().ForEach(a => _sef.SportEventCache.Cache.Remove(a.Key));
                IEnumerable<Tuple<URN, URN>> events = null;
                var tasks = new List<Task<IEnumerable<Tuple<URN, URN>>>>();

                for (var i = -1; i < 3; i++)
                {
                    tasks.Add(_sef.SportEventCache.GetEventIdsAsync(DateTime.Now.AddDays(i), TestData.Culture));
                }

                IEnumerable<IEnumerable<Tuple<URN, URN>>> results = null;
                Task.Run(async () =>
                {
                    results = await Task.WhenAll(tasks);
                    events = results.First();
                }).GetAwaiter().GetResult();

                Assert.IsNotNull(events);
                Assert.IsNotNull(results);
                var listEvents = events as IList<Tuple<URN, URN>> ?? events.ToList();
                Assert.AreEqual(listEvents.Count, _sef.SportEventCache.Cache.Count(c=>c.Key.Contains("match")));
                Assert.IsTrue(listEvents.Any());
            }
        }

        private static void ValidateSportEvent(ICompetition item, bool ignoreDate = false)
        {
            Assert.IsNotNull(item, "Sport event not found.");
            Assert.AreEqual(TestData.EventId, item.Id, "Id not correct.");

            var date = item.GetScheduledTimeAsync().Result;
            var competitors = item.GetCompetitorsAsync().Result?.ToList();
            var comp = competitors?.FirstOrDefault();
            var venue = item.GetVenueAsync().Result;
            var status = item.GetStatusAsync().Result;

            Assert.IsTrue(date != null, "date == null");
            if (!ignoreDate)
            {
                Assert.AreEqual(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));
            }

            Assert.AreEqual(2, competitors?.Count);
            if (comp != null)
            {
                Assert.AreEqual("sr:competitor:66390", comp.Id.ToString());
                Assert.AreEqual(@"Pericos de Puebla", comp.GetName(TestData.Culture));
                Assert.AreEqual(3, comp.Countries.Count);
                Assert.AreEqual("Mexico", comp.Countries[TestData.Culture]);
                Assert.AreNotEqual(comp.Countries[TestData.Culture], comp.Countries[new CultureInfo("de")]);
            }
            Assert.AreEqual(3, venue.Names.Count);
            Assert.IsTrue(!string.IsNullOrEmpty(venue.GetName(TestData.Culture)));

            Assert.IsNotNull(status);
        }

        private static bool ValidateSport(ISport item, URN sportId, int categoryCount)
        {
            Assert.IsNotNull(item);
            Assert.AreEqual(sportId.ToString(), item.Id.ToString());
            Assert.IsNotNull(item.Names);
            Assert.AreEqual(TestData.Cultures.Count, item.Names.Count);

            if (categoryCount > 0)
            {
                if (item.Categories == null)
                {
                    Assert.Fail($"Number of categories must be {categoryCount} but item.Categories is a null reference");
                }
                Assert.AreEqual(categoryCount, item.Categories.Count());
            }

            if (item.Categories != null)
            {
                var duffs = item.Categories.Where(c => c.Names[TestData.Culture] != c.Names[new CultureInfo("hu")]);
                return duffs.Any();
            }
            return true;
        }
    }
}
