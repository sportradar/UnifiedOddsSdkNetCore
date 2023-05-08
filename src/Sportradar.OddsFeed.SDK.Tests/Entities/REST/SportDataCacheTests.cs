/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    /// <summary>
    /// For testing functionality of <see cref="SportDataCache"/>
    /// <remarks>DataProvider calls GET "All available tournaments for all sports" for all requests</remarks>
    /// </summary>
    public class SportDataCacheTests
    {
        private readonly SportDataCache _sportDataCache;
        private readonly ISportEventCache _sportEventCache;
        private readonly TestTimer _timer;

        private static readonly URN EventId = TestData.EventMatchId;
        private static readonly URN PlayerId = URN.Parse("sr:player:9210275");
        private static readonly URN CompetitorId = URN.Parse("sr:competitor:9210275");
        private static readonly URN SportId = URN.Parse("sr:sport:1");
        private static readonly URN TournamentId = URN.Parse("sr:tournament:1");
        private static readonly URN TournamentIdExtra = URN.Parse("sr:simple_tournament:11111");
        private static readonly URN DrawId = URN.Parse("wns:draw:9210275");
        private static readonly URN LotteryId = URN.Parse("wns:lottery:9210275");

        private readonly CultureInfo _cultureEn = new CultureInfo("en");
        private readonly CultureInfo _cultureNl = new CultureInfo("nl");

        private readonly TestDataRouterManager _dataRouterManager;
        private readonly CacheManager _cacheManager;

        public SportDataCacheTests(ITestOutputHelper outputHelper)
        {
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager, outputHelper);
            var memoryCache = new MemoryCache("cache");
            _timer = new TestTimer(false);

            _sportEventCache = new SportEventCache(
                                                   memoryCache,
                                                   _dataRouterManager,
                                                   new SportEventCacheItemFactory(
                                                                                  _dataRouterManager,
                                                                                  new SemaphorePool(5, ExceptionHandlingStrategy.THROW),
                                                                                  _cultureEn,
                                                                                  new MemoryCache("FixtureTimestampCache")),
                                                   _timer,
                                                   TestData.Cultures,
                                                   _cacheManager);

            _sportDataCache = new SportDataCache(_dataRouterManager, _timer, TestData.Cultures, _sportEventCache, _cacheManager);
        }

        [Fact]
        public async Task CallSportEventSummaryEndpoint()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            await _dataRouterManager.GetSportEventSummaryAsync(TournamentIdExtra, _cultureEn, null);
            Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
        }

        [Fact]
        public async Task TestDataRouterManagerAllMethods()
        {
            var callType = string.Empty;
            Assert.Equal(0, _dataRouterManager.GetCallCount(callType));
            await _dataRouterManager.GetSportEventSummaryAsync(EventId, _cultureEn, null);
            await _dataRouterManager.GetSportEventFixtureAsync(EventId, _cultureEn, true, null);
            await _dataRouterManager.GetAllTournamentsForAllSportAsync(_cultureEn);
            await _dataRouterManager.GetAllSportsAsync(_cultureEn);
            await _dataRouterManager.GetLiveSportEventsAsync(_cultureEn);
            await _dataRouterManager.GetSportEventsForDateAsync(DateTime.Now, _cultureEn);
            await _dataRouterManager.GetSportEventsForTournamentAsync(TournamentIdExtra, _cultureEn, null);
            await _dataRouterManager.GetPlayerProfileAsync(PlayerId, _cultureEn, null);
            await _dataRouterManager.GetCompetitorAsync(CompetitorId, _cultureEn, null);
            await _dataRouterManager.GetSeasonsForTournamentAsync(TournamentIdExtra, _cultureEn, null);
            await _dataRouterManager.GetInformationAboutOngoingEventAsync(EventId, _cultureEn, null);
            await _dataRouterManager.GetMarketDescriptionsAsync(_cultureEn);
            await _dataRouterManager.GetVariantDescriptionsAsync(_cultureEn);
            await _dataRouterManager.GetVariantMarketDescriptionAsync(1, "variant", _cultureEn);
            await _dataRouterManager.GetDrawSummaryAsync(DrawId, _cultureEn, null);
            await _dataRouterManager.GetDrawFixtureAsync(DrawId, _cultureEn, null);
            await _dataRouterManager.GetLotteryScheduleAsync(LotteryId, _cultureEn, null);
            await _dataRouterManager.GetAllLotteriesAsync(_cultureEn, false);
            Assert.Equal(18, _dataRouterManager.GetCallCount(callType));
        }

        [Fact]
        public async Task TestDataIsCaching()
        {
            _timer.FireOnce(TimeSpan.Zero);
            await Task.Delay(500);

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            Assert.NotNull(sports);
            Assert.True(sports.Any(), "sports.Count() > 0");
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        }

        [Fact]
        public async Task TestDataIsCached()
        {
            var allSports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            Assert.NotNull(allSports);
            Assert.Equal(136, allSports.Count());
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            _timer.FireOnce(TimeSpan.Zero);

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            Assert.NotNull(sports);

            var sport = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures);
            Assert.NotNull(sport);

            var tournamentSport = await _sportDataCache.GetSportForTournamentAsync(URN.Parse("sr:tournament:146"), TestData.Cultures);
            Assert.NotNull(tournamentSport);
            Assert.Single(tournamentSport.Categories);
            Assert.Single(tournamentSport.Categories.First().Tournaments);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        }

        [Fact]
        public async Task TimerCalledMultipleTimesInvokesApiCalls()
        {
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllLotteries));

            const int nbrTimerCalls = 3;
            for (var i = 0; i < nbrTimerCalls; i++)
            {
                _timer.FireOnce(TimeSpan.Zero);
            }
            var sport = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures);
            Assert.NotNull(sport);
            Assert.Equal(TestData.Cultures.Count, sport.Names.Count);

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            Assert.NotNull(sports);
            Assert.Equal(136, sports.Count());
            Assert.Equal(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllTournamentsForAllSport));
            Assert.Equal(TestData.Cultures.Count * nbrTimerCalls, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllLotteries));
            Assert.Equal(TestData.Cultures.Count, _sportDataCache.FetchedCultures.Count);
        }

        [Fact]
        public async Task SequentialRepeatFetch()
        {
            for (var i = 0; i < 3; i++)
            {
                _sportDataCache.FetchedCultures.Clear();
                var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false);
                var sport = await _sportDataCache.GetSportAsync(TestData.SportId, TestData.Cultures).ConfigureAwait(false);
                var tournamentSport = await _sportDataCache.GetSportForTournamentAsync(URN.Parse("sr:tournament:146"), TestData.Cultures).ConfigureAwait(false);

                var tournamentEvents = await _sportEventCache.GetEventIdsAsync(TestData.TournamentId, _cultureEn).ConfigureAwait(false);
                var tournament = (TournamentInfoCI)_sportEventCache.GetEventCacheItem(TestData.TournamentId);
                var dateEvents = await _sportEventCache.GetEventIdsAsync(DateTime.Now, _cultureEn).ConfigureAwait(false);

                Assert.NotNull(sports);
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var s in sports)
                {
                    BaseSportDataValidation(s);
                }
                Assert.NotNull(sport);
                Assert.NotNull(tournament);
                Assert.NotNull(tournamentEvents);
                Assert.NotNull(dateEvents);
                Assert.Single(tournamentSport.Categories);
                Assert.Single(tournamentSport.Categories.First().Tournaments);
            }
        }

        [Fact]
        public async Task CacheFetchesOnlyOnceStartedByTimer()
        {
            _timer.FireOnce(TimeSpan.Zero);
            await _sportDataCache.GetSportsAsync(TestData.Cultures);
            await _sportDataCache.GetSportsAsync(TestData.Cultures);
            await _sportDataCache.GetSportsAsync(TestData.Cultures);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        }

        [Fact]
        public async Task CacheFetchesOnlyOnceStartedManually()
        {
            Task t = _sportDataCache.GetSportsAsync(TestData.Cultures);
            await Task.Delay(100);
            _timer.FireOnce(TimeSpan.Zero);
            await t;

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
        }

        [Fact]
        public async Task GetSportsAsyncReturnsCorrectlyForBaseLocale()
        {
            var sprts = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
            var sports = sprts.ToList();

            foreach (var s in sports)
            {
                BaseSportDataValidation(s);
            }
        }

        [Fact]
        public async Task GetSportsAsyncReturnsCorrectlyForNewLocale()
        {
            List<SportData> sports;
            List<SportData> sportsNl;

            await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load
            var sprts = await _sportDataCache.GetSportsAsync(new[] { _cultureNl });
            sportsNl = sprts.ToList();
            sprts = await _sportDataCache.GetSportsAsync(TestData.Cultures);
            sports = sprts.ToList();

            foreach (var s in sportsNl)
            {
                BaseSportDataValidation(s, 1);
            }

            foreach (var s in sports)
            {
                BaseSportDataValidation(s, TestData.Cultures.Count);
            }
        }

        [Fact]
        public async Task GetSportAsyncReturnKnownId()
        {
            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
            var sportsList = sports.ToList();
            var data = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures);

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(SportId, data.Id);
            Assert.Equal("Soccer", data.Names[_cultureEn]);
            BaseSportDataValidation(data);

            foreach (var s in sportsList)
            {
                BaseSportDataValidation(s);
            }
        }

        [Fact]
        public void GetSportAsyncReturnKnownId2()
        {
            var sportsList = _sportDataCache.GetSportsAsync(TestData.Cultures).GetAwaiter().GetResult().ToList(); // initial load
            var data = _sportDataCache.GetSportAsync(SportId, TestData.Cultures).GetAwaiter().GetResult();

            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(SportId, data.Id);
            Assert.Equal("Soccer", data.Names[_cultureEn]);
            BaseSportDataValidation(data);

            foreach (var s in sportsList)
            {
                BaseSportDataValidation(s);
            }
        }

        [Fact]
        public async Task GetSportAsyncReturnUnknownId()
        {
            var sportId = URN.Parse("sr:sport:11111111");
            var data = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures);
            Assert.Null(data);
        }

        [Fact]
        public async Task GetSportAsyncReturnNewLocale()
        {
            //loads tournaments.xml for new locale
            SportData dataNl; // with NL locale
            SportData data01; // without locale

            await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
            dataNl = await _sportDataCache.GetSportAsync(SportId, new[] { _cultureNl }); // add new locale
            data01 = await _sportDataCache.GetSportAsync(SportId, TestData.Cultures); // get all translations

            Assert.NotNull(dataNl);
            Assert.Equal(SportId, dataNl.Id);
            BaseSportDataValidation(dataNl, 1);

            Assert.NotNull(dataNl.Categories);
            Assert.NotNull(dataNl.Categories.FirstOrDefault()?.Tournaments);
            Assert.Equal(@"Voetbal", dataNl.Names[_cultureNl]);
            Assert.Equal(@"Internationaal", dataNl.Categories.FirstOrDefault()?.Names[_cultureNl]);
            Assert.Equal(URN.Parse("sr:tournament:1"), dataNl.Categories.FirstOrDefault()?.Tournaments.FirstOrDefault());

            BaseSportDataValidation(data01, 3);
        }

        [Fact]
        public async Task GetSportForTournamentAsyncReturnNewLocale()
        {
            //loads tournaments.xml for new locale

            await _sportDataCache.GetSportsAsync(TestData.Cultures).ConfigureAwait(false); // initial load
            var dataNl = await _sportDataCache.GetSportForTournamentAsync(TournamentId, new[] { _cultureNl }); //add new locale - with NL locale
            var data01 = await _sportDataCache.GetSportForTournamentAsync(TournamentId, TestData.Cultures); // get all translations - without locale
            Assert.NotNull(dataNl);
            Assert.Equal(SportId, dataNl.Id);

            BaseSportDataValidation(dataNl, 1);
            Assert.NotNull(dataNl.Categories);
            Assert.NotNull(dataNl.Categories.FirstOrDefault()?.Tournaments);
            Assert.Equal(@"Voetbal", dataNl.Names[_cultureNl]);
            Assert.Equal(@"Internationaal", dataNl.Categories.FirstOrDefault()?.Names[_cultureNl]);
            Assert.Equal(URN.Parse("sr:tournament:1"), dataNl.Categories.FirstOrDefault()?.Tournaments.FirstOrDefault());

            BaseSportDataValidation(data01, TestData.Cultures.Count);
        }

        [Fact]
        public async Task GetSportForTournamentAsyncReturnForUnknownTournamentId()
        {
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            var sports = await _sportDataCache.GetSportsAsync(TestData.Cultures); // initial load
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);

            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            var data01 = await _sportDataCache.GetSportForTournamentAsync(TournamentIdExtra, TestData.Cultures);
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            Assert.NotNull(sports);
            Assert.NotNull(data01);

            BaseSportDataValidation(data01, TestData.Cultures.Count);
        }

        [Fact]
        public async Task RetrievingNonExistingSport()
        {
            var sportId = URN.Parse("sr:sport:12345");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures); // initial load
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Null(sportData);
        }

        [Fact]
        public async Task RetrievingNonExistingCategory()
        {
            var categoryId = URN.Parse("sr:category:12345");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures); // initial load
            Assert.Equal(TestData.Cultures.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Null(categoryData);
        }

        [Fact]
        public async Task AddNewSportWithCategoryFromSummary()
        {
            var eventId = URN.Parse("sr:match:123456");
            var sportId = URN.Parse("sr:sport:1888");
            var categoryId = URN.Parse("sr:category:204");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Null(sportData);

            // adding from summary
            var sportEvent = _sportEventCache.GetEventCacheItem(eventId);
            Assert.NotNull(sportEvent);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(sportId, sportEvent.GetSportIdAsync().GetAwaiter().GetResult());
            Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.NotNull(sportData);
            Assert.Equal(sportId, sportData.Id);
            Assert.NotNull(sportData.Categories);
            Assert.Single(sportData.Categories);
            Assert.Contains(categoryId, sportData.Categories.Select(s => s.Id));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        [Fact]
        public async Task AddNewCategoryForExistingSportSummary()
        {
            var eventId = URN.Parse("sr:match:654321");
            var sportId = URN.Parse("sr:sport:18");
            var categoryId = URN.Parse("sr:category:204");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.NotNull(sportData);
            Assert.Empty(sportData.Categories);

            // adding from summary
            var sportEvent = _sportEventCache.GetEventCacheItem(eventId);
            Assert.NotNull(sportEvent);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.Equal(sportId, sportEvent.GetSportIdAsync().GetAwaiter().GetResult());
            Assert.Equal(1, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.NotNull(sportData);
            Assert.Equal(sportId, sportData.Id);
            Assert.NotNull(sportData.Categories);
            Assert.Single(sportData.Categories);
            Assert.Contains(categoryId, sportData.Categories.Select(s => s.Id));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        [Fact]
        public async Task AddNewCategoryFromCategoryDto()
        {
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Null(sportData);

            // adding from CategoryDto
            var category = MessageFactoryRest.GetCategory((int)categoryId.Id);
            var categoryDto = new CategoryDTO(category.id, category.name, category.country_code, new List<tournamentExtended>());
            await _cacheManager.SaveDtoAsync(categoryDto.Id, categoryDto, TestData.Cultures1.First(), DtoType.Category, null);

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.Null(sportData);

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        [Fact]
        public async Task AddNewSportWithCategoryFromCategoryDto()
        {
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Null(sportData);

            // adding from CategoryDto
            var category = MessageFactoryRest.GetCategory((int)categoryId.Id);
            var sport = MessageFactoryRest.GetSport((int)sportId.Id);
            var categoryDto = new CategoryDTO(category.id, category.name, category.country_code, new List<tournamentExtended>());
            var sportDto = new SportDTO(sport.id, sport.name, new List<CategoryDTO> { categoryDto });
            await _cacheManager.SaveDtoAsync(sportDto.Id, sportDto, TestData.Cultures1.First(), DtoType.Sport, null);

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.NotNull(sportData);
            Assert.Equal(sportId, sportData.Id);
            Assert.NotNull(sportData.Categories);
            Assert.Single(sportData.Categories);
            Assert.Contains(categoryId, sportData.Categories.Select(s => s.Id));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        [Fact]
        public async Task AddNewSportWithCategoryFromTournament()
        {
            var sportId = URN.Parse("sr:sport:1234");
            var categoryId = URN.Parse("sr:category:4321");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount, _sportDataCache.Categories.Count);
            Assert.Null(sportData);

            // adding 
            var tournament = MessageFactoryRest.GetTournament();
            tournament.category.id = categoryId.ToString();
            tournament.sport.id = sportId.ToString();
            var tournamentDto = new TournamentDTO(tournament);
            await _cacheManager.SaveDtoAsync(tournamentDto.Id, tournamentDto, TestData.Cultures1.First(), DtoType.Tournament, null);

            Assert.Equal(TestData.CacheSportCount + 1, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCount + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.NotNull(sportData);
            Assert.Equal(sportId, sportData.Id);
            Assert.NotNull(sportData.Categories);
            Assert.Single(sportData.Categories);
            Assert.Contains(categoryId, sportData.Categories.Select(s => s.Id));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        [Fact]
        public async Task AddNewCategoryForExistingSportFromTournament()
        {
            var sportId = URN.Parse("sr:sport:18");
            var categoryId = URN.Parse("sr:category:204");
            Assert.Equal(0, _sportDataCache.Sports.Count);
            Assert.Equal(0, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));

            // first it is not cached
            var sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1); // initial load
            Assert.Equal(TestData.Cultures1.Count, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointAllSports));
            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus, _sportDataCache.Categories.Count);
            Assert.Equal(0, _dataRouterManager.GetCallCount(TestDataRouterManager.EndpointSportEventSummary));
            Assert.NotNull(sportData);
            Assert.Empty(sportData.Categories);

            // adding
            var tournament = MessageFactoryRest.GetTournament();
            tournament.category.id = categoryId.ToString();
            tournament.sport.id = sportId.ToString();
            var tournamentDto = new TournamentDTO(tournament);
            await _cacheManager.SaveDtoAsync(tournamentDto.Id, tournamentDto, TestData.Cultures1.First(), DtoType.Tournament, null);

            Assert.Equal(TestData.CacheSportCount, _sportDataCache.Sports.Count);
            Assert.Equal(TestData.CacheCategoryCountPlus + 1, _sportDataCache.Categories.Count);

            sportData = await _sportDataCache.GetSportAsync(sportId, TestData.Cultures1);
            Assert.NotNull(sportData);
            Assert.Equal(sportId, sportData.Id);
            Assert.NotNull(sportData.Categories);
            Assert.Single(sportData.Categories);
            Assert.Contains(categoryId, sportData.Categories.Select(s => s.Id));

            var categoryData = await _sportDataCache.GetCategoryAsync(categoryId, TestData.Cultures1);
            Assert.NotNull(categoryData);
            Assert.Equal(categoryId, categoryData.Id);
            Assert.Single(categoryData.Names);
        }

        private static void BaseSportDataValidation(SportData data)
        {
            BaseSportDataValidation(data, TestData.Cultures.Count);
        }

        private static void BaseSportDataValidation(SportData data, int cultureNbr)
        {
            Assert.NotNull(data);
            Assert.NotNull(data.Names);

            Assert.Equal(cultureNbr, data.Names.Count);

            if (data.Categories != null)
            {
                foreach (var i in data.Categories)
                {
                    Assert.NotNull(i.Id);
                    Assert.Equal(cultureNbr, i.Names.Count);
                }
            }
        }
    }
}
