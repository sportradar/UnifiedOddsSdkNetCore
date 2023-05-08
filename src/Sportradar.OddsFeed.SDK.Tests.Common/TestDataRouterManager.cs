/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Local

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    internal class TestDataRouterManager : IDataRouterManager
    {
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        public const string EndpointSportEventSummary = "GetSportEventSummaryAsync";
        public const string EndpointSportEventFixture = "GetSportEventFixtureAsync";
        public const string EndpointAllTournamentsForAllSport = "GetAllTournamentsForAllSportAsync";
        public const string EndpointSportCategories = "GetSportCategoriesAsync";
        public const string EndpointAllSports = "GetAllSportsAsync";
        public const string EndpointLiveSportEvents = "GetLiveSportEventsAsync";
        public const string EndpointSportEventsForDate = "GetSportEventsForDateAsync";
        public const string EndpointSportEventsForTournament = "GetSportEventsForTournamentAsync";
        public const string EndpointPlayerProfile = "GetPlayerProfileAsync";
        public const string EndpointCompetitor = "GetCompetitorAsync";
        public const string EndpointSeasonsForTournament = "GetSeasonsForTournamentAsync";
        public const string EndpointInformationAboutOngoingEvent = "GetInformationAboutOngoingEventAsync";
        public const string EndpointMarketDescriptions = "GetMarketDescriptionsAsync";
        public const string EndpointVariantMarketDescription = "GetVariantMarketDescriptionAsync";
        public const string EndpointVariantDescriptions = "GetVariantDescriptionsAsync";
        public const string EndpointDrawSummary = "GetDrawSummaryAsync";
        public const string EndpointDrawFixture = "GetDrawFixtureAsync";
        public const string EndpointLotterySchedule = "GetLotteryScheduleAsync";
        public const string EndpointAllLotteries = "GetAllLotteriesAsync";
        public const string EndpointAvailableSelections = "GetAvailableSelectionsAsync";
        public const string EndpointCalculateProbability = "CalculateProbabilityAsync";
        public const string EndpointCalculateProbabilityFiltered = "CalculateProbabilityFilteredAsync";

        private const string FixtureXml = "fixtures_{culture}.xml";
        private const string SportCategoriesXml = "sport_categories_{culture}.xml";
        private const string ScheduleXml = "schedule_{culture}.xml";
        private const string TourScheduleXml = "tournament_schedule_{culture}.xml";
        private const string SportsXml = "sports_{culture}.xml";
        private const string MatchDetailsXml = "event_details_{culture}.xml";
        private const string TournamentScheduleXml = "tournaments_{culture}.xml";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Allowed for future reference")]
        private const string PlayerProfileXml = "player_1_{culture}.xml";
        private const string SimpleTeamProfileXml = "simpleteam_1_{culture}.xml";

        private readonly ICacheManager _cacheManager;
        private readonly IDeserializer<RestMessage> _restDeserializer = new Deserializer<RestMessage>();
        private TimeSpan _delay = TimeSpan.Zero;
        private bool _delayVariable;
        private int _delayPercent = 10;

        /// <summary>
        /// The list of URI replacements (to get wanted response when specific url is called)
        /// </summary>
        public readonly List<Tuple<string, string>> UriReplacements;
        /// <summary>
        /// The list of URI exceptions (to get wanted response when specific url is called)
        /// </summary>
        public readonly List<Tuple<string, Exception>> UriExceptions;

        public int TotalRestCalls => RestMethodCalls.Sum(s => s.Value);

        public readonly Dictionary<string, int> RestMethodCalls;

        public readonly List<string> RestUrlCalls;

        private readonly object _lock = new object();

        private readonly ITestOutputHelper _outputHelper;

        /// <summary>
        /// The exception handling strategy
        /// </summary>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; internal set; }

        internal TestDataRouterManager(ICacheManager cacheManager, ITestOutputHelper outputHelper, ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.THROW)
        {
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _outputHelper = outputHelper;
            UriReplacements = new List<Tuple<string, string>>();
            UriExceptions = new List<Tuple<string, Exception>>();
            RestMethodCalls = new Dictionary<string, int>();
            RestUrlCalls = new List<string>();
            ExceptionHandlingStrategy = exceptionHandlingStrategy;
        }

        private string FindUriReplacement(string path, string cultureIso, string defaultPath = null)
        {
            if (string.IsNullOrEmpty(defaultPath))
            {
                defaultPath = path;
            }
            var replacement = UriReplacements.Where(w => w.Item1.Equals(path)).ToList();
            return replacement.IsNullOrEmpty() ? defaultPath : replacement[0].Item2.Replace("culture", cultureIso);
        }

        private void FindUriException(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var replacement = UriExceptions.Where(w => w.Item1.Equals(path)).ToList();
            if (!replacement.IsNullOrEmpty())
            {
                throw replacement.First().Item2;
            }
        }

        private static string GetFile(string template, CultureInfo culture)
        {
            var filePath = FileHelper.FindFile(template.Replace("{culture}", culture.TwoLetterISOLanguageName));
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = FileHelper.FindFile(template.Replace("{culture}", TestData.Culture.TwoLetterISOLanguageName));
            }
            var fi = new FileInfo(filePath);
            if (fi.Exists)
            {
                return fi.FullName;
            }
            return string.Empty;
        }

        private void RecordMethodCall(string callType)
        {
            lock (_lock)
            {
                if (RestMethodCalls.ContainsKey(callType))
                {
                    RestMethodCalls.TryGetValue(callType, out var value);
                    RestMethodCalls[callType] = value + 1;
                }
                else
                {
                    RestMethodCalls.Add(callType, 1);
                }
            }
        }

        public void ResetMethodCall(string callType = null)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(callType))
                {
                    RestMethodCalls.Clear();
                    return;
                }
                if (RestMethodCalls.ContainsKey(callType))
                {
                    RestMethodCalls[callType] = 0;
                }
            }
        }

        /// <summary>
        /// Gets the count of the calls (per specific method or all together if not type provided)
        /// </summary>
        /// <param name="callType">Type of the call</param>
        /// <returns>The count calls</returns>
        public int GetCallCount(string callType)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(callType))
                {
                    return TotalRestCalls;
                }
                if (!RestMethodCalls.ContainsKey(callType))
                {
                    return 0;
                }
                RestMethodCalls.TryGetValue(callType, out var value);
                return value;
            }
        }

        public void AddDelay(TimeSpan delay, bool variable = false, int percentOfRequests = 20)
        {
            _delay = delay;
            _delayVariable = variable;
            _delayPercent = percentOfRequests;
        }

        private async Task ExecuteDelayAsync(URN id, CultureInfo culture)
        {
            await ExecuteDelayAsync(id.ToString(), culture).ConfigureAwait(false);
        }

        private async Task ExecuteDelayAsync(string id, CultureInfo culture)
        {
            if (_delay != TimeSpan.Zero)
            {
                if (_delayPercent < 1)
                {
                    return;
                }
                if (_delayPercent < 100)
                {
                    var percent = StaticRandom.I100;
                    if (percent > _delayPercent)
                    {
                        return;
                    }
                }
                var delayMs = (int)_delay.TotalMilliseconds;
                if (_delayVariable)
                {
                    delayMs = StaticRandom.I(delayMs);
                }
                _outputHelper.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms START");
                await Task.Delay(delayMs).ConfigureAwait(false);
                _outputHelper.WriteLine($"DRM - executing delay for {id} and {culture.TwoLetterISOLanguageName}: {delayMs} ms END");
            }
        }

        public async Task GetSportEventSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointSportEventSummary);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var mapper = new SportEventSummaryMapperFactory();

            var strId = id?.ToString().Replace(":", "_") ?? string.Empty;
            var resourceName = $"summary_{strId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);
            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(_restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.SportEventSummary, requester).ConfigureAwait(false);
                }

                return;
            }

            var filePath = GetFile($"summary_{strId}.{culture.TwoLetterISOLanguageName}.xml", culture);
            if (string.IsNullOrEmpty(filePath) &&
                id?.TypeGroup != ResourceTypeGroup.BASIC_TOURNAMENT &&
                id?.TypeGroup != ResourceTypeGroup.TOURNAMENT &&
                id?.TypeGroup != ResourceTypeGroup.SEASON)
            {
                filePath = GetFile(MatchDetailsXml, culture);
                var stream2 = FileHelper.OpenFile(filePath);
                var feedMsg = _restDeserializer.Deserialize(stream2);
                RawApiDataReceived?.Invoke(this, new RawApiDataEventArgs(new Uri(resourceName), feedMsg, $"{resourceName}", TimeSpan.Zero, culture.TwoLetterISOLanguageName));
                var result = mapper.CreateMapper(feedMsg).Map();
                if (result != null)
                {
                    RestUrlCalls.Add(filePath);
                    await LogSaveDtoAsync(id, result, culture, DtoType.SportEventSummary, requester).ConfigureAwait(false);
                }
            }
        }

        public async Task GetSportEventFixtureAsync(URN id, CultureInfo culture, bool useCachedProvider, ISportEventCI requester)
        {
            RecordMethodCall(EndpointSportEventFixture);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<fixturesEndpoint>();
            var mapper = new FixtureMapperFactory();

            var strId = id?.ToString().Replace(":", "_") ?? string.Empty;
            var resourceName = $"fixture_{strId}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);
            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.Fixture, requester).ConfigureAwait(false);
                }

                return;
            }

            var filePath = GetFile($"fixture_{strId}.{culture.TwoLetterISOLanguageName}.xml", culture);
            if (string.IsNullOrEmpty(filePath) &&
                id?.TypeGroup != ResourceTypeGroup.BASIC_TOURNAMENT &&
                id?.TypeGroup != ResourceTypeGroup.TOURNAMENT &&
                id?.TypeGroup != ResourceTypeGroup.SEASON)
            {
                filePath = GetFile(FixtureXml, culture);
                var stream2 = FileHelper.OpenFile(filePath);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
                if (result != null)
                {
                    RestUrlCalls.Add(filePath);
                    await LogSaveDtoAsync(id, result, culture, DtoType.Fixture, requester).ConfigureAwait(false);
                }
            }
        }

        public async Task GetAllTournamentsForAllSportAsync(CultureInfo culture)
        {
            RecordMethodCall(EndpointAllTournamentsForAllSport);

            await ExecuteDelayAsync("alltournamentsallsports", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<tournamentsEndpoint>();
            var mapper = new TournamentsMapperFactory();

            var resourceName = $"tournaments_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);
            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:sports:{result.Items.Count()}"), result, culture, DtoType.SportList, null).ConfigureAwait(false);
                }

                return;
            }

            var filePath = GetFile(TournamentScheduleXml, culture);
            var stream2 = FileHelper.OpenFile(filePath);
            var result2 = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
            if (result2 != null)
            {
                RestUrlCalls.Add(filePath);
                await LogSaveDtoAsync(URN.Parse($"sr:sports:{result2.Items.Count()}"), result2, culture, DtoType.SportList, null).ConfigureAwait(false);
            }
        }

        public async Task GetSportCategoriesAsync(URN id, CultureInfo culture)
        {
            RecordMethodCall(EndpointSportCategories);

            await ExecuteDelayAsync("sportcategories", culture).ConfigureAwait(false);

            var filePath = GetFile(SportCategoriesXml, culture);
            var restDeserializer = new Deserializer<sportCategoriesEndpoint>();
            var mapper = new SportCategoriesMapperFactory();
            var stream = FileHelper.OpenFile(filePath);
            RestUrlCalls.Add(filePath);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();

            if (result?.Categories != null)
            {
                foreach (var category in result.Categories)
                {
                    await LogSaveDtoAsync(id, category, culture, DtoType.Category, null).ConfigureAwait(false);
                }
            }
        }

        public async Task GetAllSportsAsync(CultureInfo culture)
        {
            RecordMethodCall(EndpointAllSports);

            await ExecuteDelayAsync("allsports", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<sportsEndpoint>();
            var mapper = new SportsMapperFactory();

            var resourceName = $"sports_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:sports:{result.Items.Count()}"), result, culture, DtoType.SportList, null).ConfigureAwait(false);
                }

                return;
            }

            var filePath = GetFile(SportsXml, culture);
            var stream2 = FileHelper.OpenFile(filePath);
            var result2 = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
            if (result2 != null)
            {
                RestUrlCalls.Add(filePath);
                await LogSaveDtoAsync(URN.Parse($"sr:sports:{result2.Items.Count()}"), result2, culture, DtoType.SportList, null).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetLiveSportEventsAsync(CultureInfo culture)
        {
            RecordMethodCall(EndpointLiveSportEvents);

            await ExecuteDelayAsync("getlivesports", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<scheduleEndpoint>();
            var mapper = new DateScheduleMapperFactory();

            await using var stream = FileHelper.GetResource("live_events.xml");

            if (stream != null)
            {
                RestUrlCalls.Add("live_events.xml");
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                    var urns = new List<Tuple<URN, URN>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                    }
                    return urns.AsEnumerable();
                }
            }

            return null;
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture)
        {
            RecordMethodCall(EndpointSportEventsForDate);

            await ExecuteDelayAsync(date.ToString("yyyy-M-d"), culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<scheduleEndpoint>();
            var mapper = new DateScheduleMapperFactory();

            var resourceName = $"schedule_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                    var urns = new List<Tuple<URN, URN>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                    }
                    return urns.AsEnumerable();
                }
            }

            var filePath = GetFile(ScheduleXml, culture);
            var stream2 = FileHelper.OpenFile(filePath);
            var result2 = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
            if (result2 != null)
            {
                RestUrlCalls.Add(filePath);
                await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result2.Items.Count()}"), result2, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result2.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task<IEnumerable<Tuple<URN, URN>>> GetSportEventsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointSportEventsForTournament);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<tournamentSchedule>();
            var mapper = new TournamentScheduleMapperFactory();

            var resourceName = $"tournament_schedule_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result.Items.Count()}"), result, culture, DtoType.SportEventSummaryList, requester).ConfigureAwait(false);
                    var urns = new List<Tuple<URN, URN>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                    }

                    return urns.AsEnumerable();
                }
            }

            var filePath = GetFile(TourScheduleXml, culture);
            var stream2 = FileHelper.OpenFile(filePath);
            var result2 = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
            if (result2 != null)
            {
                RestUrlCalls.Add(filePath);
                await LogSaveDtoAsync(URN.Parse($"sr:sport_events:{result2.Items.Count()}"), result2, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<URN, URN>>();
                foreach (var item in result2.Items)
                {
                    urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                }
                return urns.AsEnumerable();
            }

            return null;
        }

        public async Task GetPlayerProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointPlayerProfile);

            var resourceName = $"player_{id?.Id ?? 1}_{culture.TwoLetterISOLanguageName}.xml";
            if (!FileHelper.ResourceExists(resourceName))
            {
                resourceName = "player_1_de.xml";
            }
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

                var restDeserializer = new Deserializer<playerProfileEndpoint>();
                var mapper = new PlayerProfileMapperFactory();
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.PlayerProfile, requester).ConfigureAwait(false);
                }
            }
        }

        public Task GetCompetitorAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointCompetitor);
            return id.IsSimpleTeam()
                ? GetSimpleTeamProfileAsync(id, culture, requester)
                : GetCompetitorProfileAsync(id, culture, requester);
        }

        private async Task GetCompetitorProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            CompetitorProfileDTO dto;
            _outputHelper.WriteLine($"DRM-GetCompetitorProfileAsync for {id} and culture {culture.TwoLetterISOLanguageName} - START");

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var resourceName = $"competitor_{id?.Id ?? 1}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);
            RestUrlCalls.Add(resourceName);

            if (stream is null)
            {
                dto = new CompetitorProfileDTO(MessageFactoryRest.GetCompetitorProfileEndpoint(id == null ? 1 : (int)id.Id, StaticRandom.I(15)));
            }
            else
            {
                var restDeserializer = new Deserializer<competitorProfileEndpoint>();
                var mapper = new CompetitorProfileMapperFactory();
                dto = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            }

            if (dto != null)
            {
                await LogSaveDtoAsync(id, dto, culture, DtoType.CompetitorProfile, requester).ConfigureAwait(false);
            }
            _outputHelper.WriteLine($"DRM-GetCompetitorProfileAsync for {id} and culture {culture.TwoLetterISOLanguageName} - END");
        }

        private async Task GetSimpleTeamProfileAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<simpleTeamProfileEndpoint>();
            var mapper = new SimpleTeamProfileMapperFactory();

            var resourceName = $"simpleteam_{id?.Id ?? 1}_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.SimpleTeamProfile, requester).ConfigureAwait(false);
                }

                return;
            }

            var filePath = GetFile(SimpleTeamProfileXml, culture);
            var stream2 = FileHelper.OpenFile(filePath);
            var result2 = mapper.CreateMapper(restDeserializer.Deserialize(stream2)).Map();
            if (result2 != null)
            {
                RestUrlCalls.Add(filePath);
                await LogSaveDtoAsync(id, result2, culture, DtoType.SimpleTeamProfile, requester).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<URN>> GetSeasonsForTournamentAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointSeasonsForTournament);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<tournamentSeasons>();
            var mapper = new TournamentSeasonsMapperFactory();

            var resourceName = $"tournament_seasons_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.TournamentSeasons, requester).ConfigureAwait(false);
                    var urns = new List<URN>();
                    foreach (var item in result.Seasons)
                    {
                        urns.Add(item.Id);
                    }
                    return urns.AsEnumerable();
                }
            }

            return null;
        }

        public async Task<MatchTimelineDTO> GetInformationAboutOngoingEventAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointInformationAboutOngoingEvent);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<matchTimelineEndpoint>();
            var mapper = new MatchTimelineMapperFactory();

            var resourceName = $"match_timeline_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(id, result, culture, DtoType.MatchTimeline, requester).ConfigureAwait(false);
                }
            }

            return null;
        }

        public async Task GetMarketDescriptionsAsync(CultureInfo culture)
        {
            RecordMethodCall(EndpointMarketDescriptions);

            await ExecuteDelayAsync("market_description", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<market_descriptions>();
            var mapper = new MarketDescriptionsMapperFactory();

            var resourceName = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse("sr:markets:" + result.Items?.Count()), result, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
                }
            }
        }

        public async Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture)
        {
            RecordMethodCall(EndpointVariantMarketDescription);

            await ExecuteDelayAsync($"sr:variant:{id}", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<market_descriptions>();
            var mapper = new MarketDescriptionMapperFactory();

            var marketSelector = $"{id}_{variant}";
            var resourceName = FindUriReplacement(marketSelector, culture.TwoLetterISOLanguageName, $"variant_market_description_{culture.TwoLetterISOLanguageName}.xml");
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse("sr:variant:" + result.Id), result, culture, DtoType.MarketDescription, null).ConfigureAwait(false);
                }
            }
        }

        public async Task GetVariantDescriptionsAsync(CultureInfo culture)
        {
            RecordMethodCall(EndpointVariantDescriptions);

            await ExecuteDelayAsync("variant_description", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<variant_descriptions>();
            var mapper = new VariantDescriptionsMapperFactory();

            var resourceName = $"variant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse("sr:variants:" + result.Items?.Count()), result, culture, DtoType.VariantDescriptionList, null).ConfigureAwait(false);
                }
            }
        }

        public async Task GetDrawSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointDrawSummary);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<draw_summary>();
            var mapper = new DrawSummaryMapperFactory();

            var resourceName = $"draw_summary_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
                }
            }
        }

        public async Task GetDrawFixtureAsync(URN id, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointDrawFixture);

            await ExecuteDelayAsync(id, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<draw_fixtures>();
            var mapper = new DrawFixtureMapperFactory();

            var resourceName = $"draw_fixture_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
                }
            }
        }

        public async Task GetLotteryScheduleAsync(URN lotteryId, CultureInfo culture, ISportEventCI requester)
        {
            RecordMethodCall(EndpointLotterySchedule);

            await ExecuteDelayAsync(lotteryId, culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<lottery_schedule>();
            var mapper = new LotteryScheduleMapperFactory();

            var resourceName = $"lottery_schedule_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(result.Id, result, culture, DtoType.Lottery, requester).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="ignoreFail">if the fail should be ignored - when user does not have access</param>
        /// <returns>The list of combination of id of the lottery and associated sport id</returns>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task<IEnumerable<Tuple<URN, URN>>> GetAllLotteriesAsync(CultureInfo culture, bool ignoreFail)
        {
            RecordMethodCall(EndpointAllLotteries);

            await ExecuteDelayAsync("all_lotteries", culture).ConfigureAwait(false);

            var restDeserializer = new Deserializer<lotteries>();
            var mapper = new LotteriesMapperFactory();

            var resourceName = $"lotteries_{culture.TwoLetterISOLanguageName}.xml";
            await using var stream = FileHelper.GetResource(resourceName);

            if (stream != null)
            {
                RestUrlCalls.Add(resourceName);
                var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
                if (result != null)
                {
                    await LogSaveDtoAsync(URN.Parse($"sr:lotteries:{result.Items.Count()}"), result, culture, DtoType.LotteryList, null).ConfigureAwait(false);
                    var urns = new List<Tuple<URN, URN>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<URN, URN>(item.Id, item.SportId));
                    }
                    return urns.AsEnumerable();
                }
            }

            return null;
        }

        public async Task<IAvailableSelections> GetAvailableSelectionsAsync(URN id)
        {
            RecordMethodCall(EndpointAvailableSelections);

            await ExecuteDelayAsync("available_selections", CultureInfo.CurrentCulture).ConfigureAwait(false);

            FindUriException("available_selections.xml");

            var restDeserializer = new Deserializer<AvailableSelectionsType>();
            var mapper = new AvailableSelectionsMapperFactory();

            var resourceName = FindUriReplacement("available_selections.xml", ScheduleData.CultureEn.TwoLetterISOLanguageName);
            var stream = FileHelper.GetResource(resourceName);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            RestUrlCalls.Add(resourceName);

            if (id.Id == 0)
            {
                result = null;
            }
            else if (id.Id != 31561675)
            {
                result = new AvailableSelectionsDto(MessageFactoryRest.GetAvailableSelections(id));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:sels:{result.Markets.Count}"), result, CultureInfo.CurrentCulture, DtoType.AvailableSelections, null).ConfigureAwait(false);
                return new AvailableSelections(result);
            }

            return null;
        }

        public async Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            RecordMethodCall(EndpointCalculateProbability);

            await ExecuteDelayAsync("calculate", CultureInfo.CurrentCulture).ConfigureAwait(false);

            FindUriException("calculate_response.xml");

            var restDeserializer = new Deserializer<CalculationResponseType>();
            var mapper = new CalculationMapperFactory();

            var resourceName = FindUriReplacement("calculate_response.xml", ScheduleData.CultureEn.TwoLetterISOLanguageName);
            var stream = FileHelper.GetResource(resourceName);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            RestUrlCalls.Add(resourceName);

            if (selections == null)
            {
                result = null;
            }
            else if (selections.IsNullOrEmpty())
            {
                result = new CalculationDto(MessageFactoryRest.GetCalculationResponse(StaticRandom.U1000, StaticRandom.I100));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:calc:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null).ConfigureAwait(false);
                return new Calculation(result);
            }

            return null;
        }

        public async Task<ICalculationFilter> CalculateProbabilityFilteredAsync(IEnumerable<ISelection> selections)
        {
            RecordMethodCall(EndpointCalculateProbabilityFiltered);

            await ExecuteDelayAsync("calculate_filter", CultureInfo.CurrentCulture).ConfigureAwait(false);

            FindUriException("calculate_filter_response.xml");

            var restDeserializer = new Deserializer<FilteredCalculationResponseType>();
            var mapper = new CalculationFilteredMapperFactory();

            var resourceName = FindUriReplacement("calculate_filter_response.xml", ScheduleData.CultureEn.TwoLetterISOLanguageName);
            var stream = FileHelper.GetResource(resourceName);
            var result = mapper.CreateMapper(restDeserializer.Deserialize(stream)).Map();
            RestUrlCalls.Add(resourceName);

            if (selections == null)
            {
                result = null;
            }
            else if (selections.IsNullOrEmpty())
            {
                result = new FilteredCalculationDto(MessageFactoryRest.GetFilteredCalculationResponse(StaticRandom.U1000, StaticRandom.I100));
            }

            if (result != null)
            {
                await LogSaveDtoAsync(URN.Parse($"sr:calcfilt:{result.AvailableSelections.Count}"), result, CultureInfo.CurrentCulture, DtoType.Calculation, null).ConfigureAwait(false);
                return new CalculationFilter(result);
            }

            return null;
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="System.DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all fixtures that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of almost all events we are offering prematch odds for.
        /// </summary>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        public Task<IEnumerable<Tuple<URN, URN>>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of all the available tournaments for a specific sport
        /// </summary>
        /// <param name="sportId">The specific sport id</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the available tournament ids with the sportId it belongs to</returns>
        public Task<IEnumerable<Tuple<URN, URN>>> GetSportAvailableTournamentsAsync(URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="System.DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="URN"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all results that have changed in the last 24 hours</returns>
        public Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, URN sportId, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get stage event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>The periods summary or null if not found</returns>
        public Task<PeriodSummaryDTO> GetPeriodSummaryAsync(URN id, CultureInfo culture, ISportEventCI requester, ICollection<URN> competitorIds = null, ICollection<int> periods = null)
        {
            throw new NotImplementedException();
        }

        private async Task LogSaveDtoAsync(URN id, object item, CultureInfo culture, DtoType dtoType, ISportEventCI requester)
        {
            if (item != null)
            {
                var stopWatch = Stopwatch.StartNew();
                await _cacheManager.SaveDtoAsync(id, item, culture, dtoType, requester).ConfigureAwait(false);
                stopWatch.Stop();
                if (stopWatch.ElapsedMilliseconds > 100)
                {
                    _outputHelper.WriteLine($"Saving took {stopWatch.ElapsedMilliseconds} ms. For id={id}, culture={culture.TwoLetterISOLanguageName} and dtoType={dtoType}.");
                }
            }
        }
    }
}
