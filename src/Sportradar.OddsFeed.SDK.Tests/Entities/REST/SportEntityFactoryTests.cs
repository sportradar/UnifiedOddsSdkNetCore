/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    //TODO The following tests should be updated / fixed to have separate tests for each of ISportEvent implementation
    public class SportEntityFactoryTests
    {
        private readonly TestSportEntityFactoryBuilder _sef;

        public SportEntityFactoryTests(ITestOutputHelper outputHelper)
        {
            _sef = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3);
        }

        private async Task InitializeSportEntities()
        {
            await _sef.InitializeSportEntities();
            await _sef.LoadTournamentMissingValues();
            await _sef.LoadSeasonMissingValues();

            _sef.Competition = _sef.SportEntityFactory.BuildSportEvent<ICompetition>(TestData.EventMatchId, URN.Parse("sr:sport:3"), TestData.Cultures.ToList(), TestData.ThrowingStrategy);
            _sef.Sport = await _sef.SportEntityFactory.BuildSportAsync(TestData.SportId, TestData.Cultures, TestData.ThrowingStrategy);
            _sef.Sports = (await _sef.SportEntityFactory.BuildSportsAsync(TestData.Cultures, TestData.ThrowingStrategy)).ToList();
            _sef.Tournament = _sef.SportEntityFactory.BuildSportEvent<ITournament>(TestData.TournamentId, URN.Parse("sr:sport:3"), TestData.Cultures.ToList(), TestData.ThrowingStrategy);
        }

        [Fact]
        public async Task SportEvent()
        {
            await InitializeSportEntities();
            Assert.NotNull(_sef.Competition);

            await ValidateSportEvent(_sef.Competition);
        }

        [Fact]
        public async Task Sport()
        {
            await InitializeSportEntities();
            ValidateSport(_sef.Sport, TestData.SportId, 122);
            Assert.NotEqual(_sef.Sport.Names[TestData.Culture], _sef.Sport.Names[new CultureInfo("hu")]);
        }

        [Fact]
        public async Task Sports()
        {
            await InitializeSportEntities();
            Assert.NotNull(_sef.Sports);

            var sameSportNameCount = 0;
            foreach (var s in _sef.Sports)
            {
                var result = ValidateSport(s, s.Id, 0);
                if (!result)
                {
                    sameSportNameCount++;
                }
            }
            Assert.NotEqual(_sef.Sports.Count, sameSportNameCount);
        }

        [Fact]
        public async Task Tournament()
        {
            await InitializeSportEntities();

            Assert.NotNull(_sef.Tournament);
            Assert.Equal(TestData.TournamentId, _sef.Tournament.Id);

            var category = await _sef.Tournament.GetCategoryAsync();
            var sport = await _sef.Tournament.GetSportAsync();

            var coverage = await _sef.Tournament.GetTournamentCoverage();
            var season = await _sef.Tournament.GetCurrentSeasonAsync();
            var seasons = await _sef.Tournament.GetSeasonsAsync();

            Assert.NotNull(category);
            Assert.NotNull(sport);
            Assert.Null(coverage);
            //Assert.NotNull(coverage);
            //Assert.True(coverage.LiveCoverage);
            Assert.NotNull(season);
            Assert.NotNull(seasons);

            Assert.Equal(11, seasons.Count());
        }

        [Fact]
        public async Task Season()
        {
            await InitializeSportEntities();

            Assert.NotNull(_sef.Season);
            Assert.Equal(TestData.SeasonId, _sef.Season.Id);

            var sport = await _sef.Season.GetSportAsync();
            var tournamentCoverage = await _sef.Season.GetTournamentCoverage();
            var round = await _sef.Season.GetCurrentRoundAsync();
            var competitors = await _sef.Season.GetCompetitorsAsync();
            var groups = await _sef.Season.GetGroupsAsync();
            var schedule = await _sef.Season.GetScheduleAsync();
            var seasonCoverage = await _sef.Season.GetSeasonCoverageAsync();
            var tourInfo = await _sef.Season.GetTournamentInfoAsync();
            var year = await _sef.Season.GetYearAsync();

            Assert.NotNull(sport);
            Assert.NotNull(tournamentCoverage);
            Assert.NotNull(competitors);
            Assert.NotNull(round);
            Assert.NotNull(groups);
            var enumerable = groups.ToList();
            Assert.True(enumerable.Any());
            Assert.Equal(20, enumerable.First().Competitors.Count());
            Assert.Equal(20, competitors.Count());
            Assert.NotNull(schedule);
            Assert.Equal(241, schedule.Count());
            Assert.Null(seasonCoverage);
            Assert.NotNull(tourInfo);
            Assert.NotNull(year);
        }

        [Fact]
        public async Task CheckIfTournamentDataIsTransferredToSportDataCache()
        {
            var tournamentId = URN.Parse("sr:tournament:1030");

            await InitializeSportEntities();
            var tourCount = _sef.SportEventCache.Cache.Count(c => c.Key.Contains("season") || c.Key.Contains("tournament"));
            Assert.NotNull(_sef.Tournament);

            var tournamentCI = (TournamentInfoCI)_sef.SportEventCache.GetEventCacheItem(tournamentId);
            Assert.Equal(tourCount, _sef.SportEventCache.Cache.Count(c => c.Key.Contains("season") || c.Key.Contains("tournament")));

            _sef.SportEventCache.CacheDeleteItem(tournamentId, CacheItemType.Tournament);
            Assert.False(_sef.SportEventCache.Cache.Contains(tournamentId.ToString()));

            var tour = _sef.SportEventCache.GetEventCacheItem(tournamentId);
            Assert.Equal(tournamentId, tour.Id);
            Assert.True(_sef.SportEventCache.Cache.Contains(tournamentId.ToString()));

            tour = _sef.SportEventCache.GetEventCacheItem(tournamentId);

            Assert.NotNull(tournamentCI);
            Assert.NotNull(tour);
        }

        //TODO: maybe strangly behaves in pipeline - to be checked
        //[Fact]
        public async Task MultiThreadInvokesEventOnTournamentReceived()
        {
            await InitializeSportEntities();

            // we do it several times, because not every time race condition was met
            for (var j = 0; j < 3; j++)
            {
                _sef.SportEventCache.Cache.ToList().ForEach(a => _sef.SportEventCache.Cache.Remove(a.Key));
                var tasks = new List<Task<IEnumerable<Tuple<URN, URN>>>>();

                for (var i = -1; i < 3; i++)
                {
                    tasks.Add(_sef.SportEventCache.GetEventIdsAsync(DateTime.Now.AddDays(i), TestData.Culture));
                }

                IEnumerable<IEnumerable<Tuple<URN, URN>>> results = await Task.WhenAll(tasks);
                var events = results.First();

                Assert.NotNull(events);
                Assert.NotNull(results);
                var listEvents = events as IList<Tuple<URN, URN>> ?? events.ToList();
                Assert.Equal(listEvents.Count, _sef.SportEventCache.Cache.Count(c => c.Key.Contains("match")));
                Assert.True(listEvents.Any());
            }
        }

        private static async Task ValidateSportEvent(ICompetition item, bool ignoreDate = false)
        {
            Assert.NotNull(item);
            Assert.Equal(TestData.EventMatchId, item.Id);

            var date = await item.GetScheduledTimeAsync();
            var competitors = (await item.GetCompetitorsAsync()).ToList();
            var comp = competitors.FirstOrDefault();
            var venue = await item.GetVenueAsync();
            var status = await item.GetStatusAsync();

            Assert.True(date != null, "date == null");
            if (!ignoreDate)
            {
                Assert.Equal(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));
            }

            Assert.Equal(2, competitors.Count);

            if (comp != null)
            {
                Assert.Equal("sr:competitor:66390", comp.Id.ToString());
                Assert.Equal(@"Pericos de Puebla", comp.GetName(TestData.Culture));
                Assert.Equal(3, comp.Countries.Count);
                Assert.Equal("Mexico", comp.Countries[TestData.Culture]);
                Assert.NotEqual(comp.Countries[TestData.Culture], comp.Countries[new CultureInfo("de")]);
            }
            Assert.Equal(3, venue.Names.Count);
            Assert.True(!string.IsNullOrEmpty(venue.GetName(TestData.Culture)));

            Assert.NotNull(status);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static bool ValidateSport(ISport item, URN sportId, int categoryCount)
        {
            Assert.NotNull(item);
            Assert.NotNull(sportId);
            Assert.Equal(sportId.ToString(), item.Id.ToString());
            Assert.NotNull(item.Names);
            Assert.Equal(TestData.Cultures.Count, item.Names.Count);

            if (categoryCount > 0)
            {
                Assert.Equal(categoryCount, item.Categories.Count());
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
