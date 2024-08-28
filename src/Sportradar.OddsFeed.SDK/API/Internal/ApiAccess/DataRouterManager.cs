// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// The implementation of data router manager
    /// </summary>
    /// <seealso cref="IDataRouterManager" />
    internal class DataRouterManager : IDataRouterManager
    {
        /// <summary>
        /// Occurs when data from Sports API arrives
        /// </summary>
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;
        /// <summary>
        /// The execution log
        /// </summary>
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(DataRouterManager));
        /// <summary>
        /// The sport event summary provider
        /// </summary>
        private readonly IDataProvider<SportEventSummaryDto> _sportEventSummaryProvider;
        /// <summary>
        /// The sport event fixture provider
        /// </summary>
        private readonly IDataProvider<FixtureDto> _sportEventFixtureProvider;
        /// <summary>
        /// The sport event fixture provider without cache
        /// </summary>
        private readonly IDataProvider<FixtureDto> _sportEventFixtureChangeFixtureProvider;
        /// <summary>
        /// All tournaments for all sports provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportDto>> _allTournamentsForAllSportsProvider;
        /// <summary>
        /// All sports provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportDto>> _allSportsProvider;
        /// <summary>
        /// The sport events for date provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForDateProvider;
        /// <summary>
        /// The sport events for tournament provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForTournamentProvider;
        /// <summary>
        /// The sport events for race schedule tournament provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportEventSummaryDto>> _sportEventsForRaceTournamentProvider;
        /// <summary>
        /// The player profile provider
        /// </summary>
        private readonly IDataProvider<PlayerProfileDto> _playerProfileProvider;
        /// <summary>
        /// The competitor provider
        /// </summary>
        private readonly IDataProvider<CompetitorProfileDto> _competitorProvider;
        /// <summary>
        /// The simple team profile provider
        /// </summary>
        private readonly IDataProvider<SimpleTeamProfileDto> _simpleTeamProvider;
        /// <summary>
        /// The tournament seasons provider
        /// </summary>
        private readonly IDataProvider<TournamentSeasonsDto> _tournamentSeasonsProvider;
        /// <summary>
        /// The ongoing sport event provider
        /// </summary>
        private readonly IDataProvider<MatchTimelineDto> _ongoingSportEventProvider;
        /// <summary>
        /// The ongoing sport event provider
        /// </summary>
        private readonly IDataProvider<SportCategoriesDto> _sportCategoriesProvider;
        /// <summary>
        /// The invariant market descriptions provider
        /// </summary>
        private readonly IDataProvider<EntityList<MarketDescriptionDto>> _invariantMarketDescriptionsProvider;
        /// <summary>
        /// The variant market description provider
        /// </summary>
        private readonly IDataProvider<MarketDescriptionDto> _variantMarketDescriptionProvider;
        /// <summary>
        /// The variant descriptions provider
        /// </summary>
        private readonly IDataProvider<EntityList<VariantDescriptionDto>> _variantDescriptionsProvider;
        /// <summary>
        /// The lottery draw summary provider
        /// </summary>
        private readonly IDataProvider<DrawDto> _lotteryDrawSummaryProvider;
        /// <summary>
        /// The lottery draw fixture provider
        /// </summary>
        private readonly IDataProvider<DrawDto> _lotteryDrawFixtureProvider;
        /// <summary>
        /// The lottery schedule provider
        /// </summary>
        private readonly IDataProvider<LotteryDto> _lotteryScheduleProvider;
        /// <summary>
        /// The lottery list provider
        /// </summary>
        private readonly IDataProvider<EntityList<LotteryDto>> _lotteryListProvider;
        /// <summary>
        /// The available selections provider
        /// </summary>
        private readonly IDataProvider<AvailableSelectionsDto> _availableSelectionsProvider;
        /// <summary>
        /// The calculate probability provider
        /// </summary>
        private readonly ICalculateProbabilityProvider _calculateProbabilityProvider;
        /// <summary>
        /// The calculate probability provider (filtered)
        /// </summary>
        private readonly ICalculateProbabilityFilteredProvider _calculateProbabilityFilteredProvider;
        /// <summary>
        /// The fixture changes provider
        /// </summary>
        private readonly IDataProvider<EntityList<FixtureChangeDto>> _fixtureChangesProvider;
        /// <summary>
        /// The result changes provider
        /// </summary>
        private readonly IDataProvider<EntityList<ResultChangeDto>> _resultChangesProvider;
        /// <summary>
        /// The list sport events provider
        /// </summary>
        private readonly IDataProvider<EntityList<SportEventSummaryDto>> _listSportEventProvider;
        /// <summary>
        /// The list sport available tournaments provider
        /// </summary>
        private readonly IDataProvider<EntityList<TournamentInfoDto>> _availableSportTournamentsProvider;
        /// <summary>
        /// The sport event fixture provider for when tournamentInfo is returned
        /// </summary>
        private readonly IDataProvider<TournamentInfoDto> _sportEventFixtureForTournamentProvider;
        /// <summary>
        /// The sport event fixture provider without cache for when tournamentInfo is returned
        /// </summary>
        private readonly IDataProvider<TournamentInfoDto> _sportEventFixtureChangeFixtureForTournamentProvider;
        /// <summary>
        /// The stage event period summary provider
        /// </summary>
        private readonly IDataProvider<PeriodSummaryDto> _stagePeriodSummaryProvider;

        /// <summary>
        /// The cache manager
        /// </summary>
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// The is WNS available
        /// </summary>
        private readonly bool _isWnsAvailable;

        /// <summary>
        /// The exception handling strategy
        /// </summary>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// The exception handling strategy
        /// </summary>
        private readonly CultureInfo _defaultLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataRouterManager"/> class
        /// </summary>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to interact among caches</param>
        /// <param name="producerManager">A <see cref="IProducerManager"/> used to get WNS producer</param>
        /// <param name="configuration">An <see cref="IUofConfiguration"/> used to configured instance</param>
        /// <param name="sportEventSummaryProvider">The sport event summary provider</param>
        /// <param name="sportEventFixtureProvider">The sport event fixture provider</param>
        /// <param name="sportEventFixtureChangeFixtureProvider">The sport event fixture provider without cache</param>
        /// <param name="allTournamentsForAllSportsProvider">All tournaments for all sports provider</param>
        /// <param name="allSportsProvider">All sports provider</param>
        /// <param name="sportEventsForDateProvider">The sport events for date provider</param>
        /// <param name="sportEventsForTournamentProvider">The sport events for tournament provider</param>
        /// <param name="playerProfileProvider">The player profile provider</param>
        /// <param name="competitorProvider">The competitor provider</param>
        /// <param name="simpleTeamProvider">The simple team provider</param>
        /// <param name="tournamentSeasonsProvider">The tournament seasons provider</param>
        /// <param name="ongoingSportEventProvider">The ongoing sport event provider</param>
        /// <param name="sportCategoriesProvider">The sport categories provider</param>
        /// <param name="invariantMarketDescriptionsProvider">The invariant market description provider</param>
        /// <param name="variantMarketDescriptionProvider">The variant market description provider</param>
        /// <param name="variantDescriptionsProvider">The variant descriptions provider</param>
        /// <param name="drawSummaryProvider">Lottery draw summary provider</param>
        /// <param name="drawFixtureProvider">Lottery draw fixture provider</param>
        /// <param name="lotteryScheduleProvider">Lottery schedule provider (single lottery with schedule)</param>
        /// <param name="lotteryListProvider">Lottery list provider</param>
        /// <param name="availableSelectionsProvider">Available selections provider</param>
        /// <param name="calculateProbabilityProvider">The probability calculation provider</param>
        /// <param name="calculateProbabilityFilteredProvider">The probability calculation provider (filtered)</param>
        /// <param name="fixtureChangesProvider">Fixture changes provider</param>
        /// <param name="resultChangesProvider">Result changes provider</param>
        /// <param name="listSportEventProvider">List sport events provider</param>
        /// <param name="availableSportTournamentsProvider">The sports available tournaments provider</param>
        /// <param name="sportEventFixtureForTournamentProvider">The sport event fixture provider for when tournamentInfo is returned</param>
        /// <param name="sportEventFixtureChangeFixtureForTournamentProvider">The sport event fixture provider without cache for when tournamentInfo is returned</param>
        /// <param name="stagePeriodSummaryProvider">Stage period summary provider</param>
        /// <param name="sportEventsForRaceTournamentProvider">The sport events for race schedule tournament provider</param>
        /// <remarks>Ignored _availableSelectionsProvider.RawApiDataReceived += OnRawApiDataReceived; _calculateProbabilityProvider.RawApiDataReceived += OnRawApiDataReceived;</remarks>
        [SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        public DataRouterManager(ICacheManager cacheManager,
                                 IProducerManager producerManager,
                                 IUofConfiguration configuration,
                                 IDataProvider<SportEventSummaryDto> sportEventSummaryProvider,
                                 IDataProvider<FixtureDto> sportEventFixtureProvider,
                                 IDataProvider<FixtureDto> sportEventFixtureChangeFixtureProvider,
                                 IDataProvider<EntityList<SportDto>> allTournamentsForAllSportsProvider,
                                 IDataProvider<EntityList<SportDto>> allSportsProvider,
                                 IDataProvider<EntityList<SportEventSummaryDto>> sportEventsForDateProvider,
                                 IDataProvider<EntityList<SportEventSummaryDto>> sportEventsForTournamentProvider,
                                 IDataProvider<PlayerProfileDto> playerProfileProvider,
                                 IDataProvider<CompetitorProfileDto> competitorProvider,
                                 IDataProvider<SimpleTeamProfileDto> simpleTeamProvider,
                                 IDataProvider<TournamentSeasonsDto> tournamentSeasonsProvider,
                                 IDataProvider<MatchTimelineDto> ongoingSportEventProvider,
                                 IDataProvider<SportCategoriesDto> sportCategoriesProvider,
                                 IDataProvider<EntityList<MarketDescriptionDto>> invariantMarketDescriptionsProvider,
                                 IDataProvider<MarketDescriptionDto> variantMarketDescriptionProvider,
                                 IDataProvider<EntityList<VariantDescriptionDto>> variantDescriptionsProvider,
                                 IDataProvider<DrawDto> drawSummaryProvider,
                                 IDataProvider<DrawDto> drawFixtureProvider,
                                 IDataProvider<LotteryDto> lotteryScheduleProvider,
                                 IDataProvider<EntityList<LotteryDto>> lotteryListProvider,
                                 IDataProvider<AvailableSelectionsDto> availableSelectionsProvider,
                                 ICalculateProbabilityProvider calculateProbabilityProvider,
                                 ICalculateProbabilityFilteredProvider calculateProbabilityFilteredProvider,
                                 IDataProvider<EntityList<FixtureChangeDto>> fixtureChangesProvider,
                                 IDataProvider<EntityList<ResultChangeDto>> resultChangesProvider,
                                 IDataProvider<EntityList<SportEventSummaryDto>> listSportEventProvider,
                                 IDataProvider<EntityList<TournamentInfoDto>> availableSportTournamentsProvider,
                                 IDataProvider<TournamentInfoDto> sportEventFixtureForTournamentProvider,
                                 IDataProvider<TournamentInfoDto> sportEventFixtureChangeFixtureForTournamentProvider,
                                 IDataProvider<PeriodSummaryDto> stagePeriodSummaryProvider,
                                 IDataProvider<EntityList<SportEventSummaryDto>> sportEventsForRaceTournamentProvider)
        {
            Guard.Argument(cacheManager, nameof(cacheManager)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();
            Guard.Argument(configuration, nameof(configuration)).NotNull();
            Guard.Argument(sportEventSummaryProvider, nameof(sportEventSummaryProvider)).NotNull();
            Guard.Argument(sportEventFixtureProvider, nameof(sportEventFixtureProvider)).NotNull();
            Guard.Argument(sportEventFixtureChangeFixtureProvider, nameof(sportEventFixtureChangeFixtureProvider)).NotNull();
            Guard.Argument(allTournamentsForAllSportsProvider, nameof(allTournamentsForAllSportsProvider)).NotNull();
            Guard.Argument(allSportsProvider, nameof(allSportsProvider)).NotNull();
            Guard.Argument(sportEventsForDateProvider, nameof(sportEventsForDateProvider)).NotNull();
            Guard.Argument(sportEventsForTournamentProvider, nameof(sportEventsForTournamentProvider)).NotNull();
            Guard.Argument(playerProfileProvider, nameof(playerProfileProvider)).NotNull();
            Guard.Argument(competitorProvider, nameof(competitorProvider)).NotNull();
            Guard.Argument(simpleTeamProvider, nameof(simpleTeamProvider)).NotNull();
            Guard.Argument(tournamentSeasonsProvider, nameof(tournamentSeasonsProvider)).NotNull();
            Guard.Argument(ongoingSportEventProvider, nameof(ongoingSportEventProvider)).NotNull();
            Guard.Argument(sportCategoriesProvider, nameof(sportCategoriesProvider)).NotNull();
            Guard.Argument(invariantMarketDescriptionsProvider, nameof(invariantMarketDescriptionsProvider)).NotNull();
            Guard.Argument(variantMarketDescriptionProvider, nameof(variantMarketDescriptionProvider)).NotNull();
            Guard.Argument(variantDescriptionsProvider, nameof(variantDescriptionsProvider)).NotNull();
            Guard.Argument(drawSummaryProvider, nameof(drawSummaryProvider)).NotNull();
            Guard.Argument(drawFixtureProvider, nameof(drawFixtureProvider)).NotNull();
            Guard.Argument(lotteryScheduleProvider, nameof(lotteryScheduleProvider)).NotNull();
            Guard.Argument(lotteryListProvider, nameof(lotteryListProvider)).NotNull();
            Guard.Argument(availableSelectionsProvider, nameof(availableSelectionsProvider)).NotNull();
            Guard.Argument(calculateProbabilityProvider, nameof(calculateProbabilityProvider)).NotNull();
            Guard.Argument(calculateProbabilityFilteredProvider, nameof(calculateProbabilityFilteredProvider)).NotNull();
            Guard.Argument(fixtureChangesProvider, nameof(fixtureChangesProvider)).NotNull();
            Guard.Argument(resultChangesProvider, nameof(resultChangesProvider)).NotNull();
            Guard.Argument(listSportEventProvider, nameof(listSportEventProvider)).NotNull();
            Guard.Argument(availableSportTournamentsProvider, nameof(availableSportTournamentsProvider)).NotNull();
            Guard.Argument(sportEventFixtureForTournamentProvider, nameof(sportEventFixtureForTournamentProvider)).NotNull();
            Guard.Argument(sportEventFixtureChangeFixtureForTournamentProvider, nameof(sportEventFixtureChangeFixtureForTournamentProvider)).NotNull();
            Guard.Argument(stagePeriodSummaryProvider, nameof(stagePeriodSummaryProvider)).NotNull();
            Guard.Argument(sportEventsForRaceTournamentProvider, nameof(sportEventsForRaceTournamentProvider)).NotNull();

            _cacheManager = cacheManager;
            var wnsProducer = producerManager.GetProducer(7);
            _isWnsAvailable = wnsProducer.IsAvailable && !wnsProducer.IsDisabled;
            ExceptionHandlingStrategy = configuration.ExceptionHandlingStrategy;
            _defaultLocale = configuration.DefaultLanguage;
            _sportEventSummaryProvider = sportEventSummaryProvider;
            _sportEventFixtureProvider = sportEventFixtureProvider;
            _sportEventFixtureChangeFixtureProvider = sportEventFixtureChangeFixtureProvider;
            _allTournamentsForAllSportsProvider = allTournamentsForAllSportsProvider;
            _allSportsProvider = allSportsProvider;
            _sportEventsForDateProvider = sportEventsForDateProvider;
            _sportEventsForTournamentProvider = sportEventsForTournamentProvider;
            _playerProfileProvider = playerProfileProvider;
            _competitorProvider = competitorProvider;
            _simpleTeamProvider = simpleTeamProvider;
            _tournamentSeasonsProvider = tournamentSeasonsProvider;
            _ongoingSportEventProvider = ongoingSportEventProvider;
            _sportCategoriesProvider = sportCategoriesProvider;
            _invariantMarketDescriptionsProvider = invariantMarketDescriptionsProvider;
            _variantMarketDescriptionProvider = variantMarketDescriptionProvider;
            _variantDescriptionsProvider = variantDescriptionsProvider;
            _lotteryDrawSummaryProvider = drawSummaryProvider;
            _lotteryDrawFixtureProvider = drawFixtureProvider;
            _lotteryScheduleProvider = lotteryScheduleProvider;
            _lotteryListProvider = lotteryListProvider;
            _availableSelectionsProvider = availableSelectionsProvider;
            _calculateProbabilityProvider = calculateProbabilityProvider;
            _calculateProbabilityFilteredProvider = calculateProbabilityFilteredProvider;
            _fixtureChangesProvider = fixtureChangesProvider;
            _resultChangesProvider = resultChangesProvider;
            _listSportEventProvider = listSportEventProvider;
            _availableSportTournamentsProvider = availableSportTournamentsProvider;
            _sportEventFixtureForTournamentProvider = sportEventFixtureForTournamentProvider;
            _sportEventFixtureChangeFixtureForTournamentProvider = sportEventFixtureChangeFixtureForTournamentProvider;
            _stagePeriodSummaryProvider = stagePeriodSummaryProvider;
            _sportEventsForRaceTournamentProvider = sportEventsForRaceTournamentProvider;

            _sportEventSummaryProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventFixtureProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventFixtureChangeFixtureProvider.RawApiDataReceived += OnRawApiDataReceived;
            _allTournamentsForAllSportsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _allSportsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventsForDateProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventsForTournamentProvider.RawApiDataReceived += OnRawApiDataReceived;
            _playerProfileProvider.RawApiDataReceived += OnRawApiDataReceived;
            _competitorProvider.RawApiDataReceived += OnRawApiDataReceived;
            _simpleTeamProvider.RawApiDataReceived += OnRawApiDataReceived;
            _tournamentSeasonsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _ongoingSportEventProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportCategoriesProvider.RawApiDataReceived += OnRawApiDataReceived;
            _invariantMarketDescriptionsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _variantMarketDescriptionProvider.RawApiDataReceived += OnRawApiDataReceived;
            _variantDescriptionsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _lotteryDrawSummaryProvider.RawApiDataReceived += OnRawApiDataReceived;
            _lotteryDrawFixtureProvider.RawApiDataReceived += OnRawApiDataReceived;
            _lotteryScheduleProvider.RawApiDataReceived += OnRawApiDataReceived;
            _lotteryListProvider.RawApiDataReceived += OnRawApiDataReceived;
            _fixtureChangesProvider.RawApiDataReceived += OnRawApiDataReceived;
            _resultChangesProvider.RawApiDataReceived += OnRawApiDataReceived;
            _listSportEventProvider.RawApiDataReceived += OnRawApiDataReceived;
            _availableSportTournamentsProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventFixtureForTournamentProvider.RawApiDataReceived += OnRawApiDataReceived;
            _sportEventFixtureChangeFixtureForTournamentProvider.RawApiDataReceived += OnRawApiDataReceived;
            _stagePeriodSummaryProvider.RawApiDataReceived += OnRawApiDataReceived;
            sportEventsForRaceTournamentProvider.RawApiDataReceived += OnRawApiDataReceived;
        }

        private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            RawApiDataReceived?.Invoke(sender, e);
        }

        /// <summary>
        /// Get sport event summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        public async Task GetSportEventSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportEventSummary"))
            {
                WriteLog($"Executing GetSportEventSummaryAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                SportEventSummaryDto result = null;
                int restCallTime;
                try
                {
                    result = await _sportEventSummaryProvider
                        .GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e, "Error getting sport event summary for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.SportEventSummary, requester)
                        .ConfigureAwait(false);
                }

                WriteLog($"Executing GetSportEventSummaryAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Get sport event fixture as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="useCachedProvider">Should the cached provider be used</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        public async Task GetSportEventFixtureAsync(Urn id, CultureInfo culture, bool useCachedProvider, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportEventFixture"))
            {
                WriteLog($"Executing {(useCachedProvider ? "cached" : "non-cached")} GetSportEventFixtureAsync for id={id} and language={culture.TwoLetterISOLanguageName}", true);

                FixtureDto result = null;
                int restCallTime;
                try
                {
                    var provider = useCachedProvider
                        ? _sportEventFixtureProvider // cached endpoint
                        : _sportEventFixtureChangeFixtureProvider; // not cached endpoint
                    result = await provider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    if (!e.Message.IsNullOrEmpty() && e.Message.Contains("Unable to cast object",
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            // instead of fixture there is probably tournamentInfo returned
                            var isTournamentFixtureFetched = await GetSportEventFixtureForTournamentAsync(id, culture, useCachedProvider, requester).ConfigureAwait(false);
                            if (isTournamentFixtureFetched)
                            {
                                restCallTime = (int)t.Elapsed.TotalMilliseconds;
                                WriteLog(
                                    $"Executing GetSportEventFixtureAsync (via tournament) for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                                return;
                            }
                        }
                        catch (Exception exception)
                        {
                            var innerMessage = exception.InnerException?.Message ?? exception.Message;
                            _executionLog.LogError(exception.InnerException ?? exception, "Error getting sport event fixture for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, innerMessage);
                        }
                    }

                    if (!useCachedProvider
                        && !e.Message.IsNullOrEmpty()
                        && e.Message.Contains("InternalServerError", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //sometimes on non-cached endpoint (fixture_change_fixture.xml) there can be error 500. In such case try also cached endpoint
                        try
                        {
                            result = await _sportEventFixtureProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                            restCallTime = (int)t.Elapsed.TotalMilliseconds;
                            WriteLog($"Executing GetSportEventFixtureAsync (via cached endpoint) for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                            return;
                        }
                        catch (Exception exception)
                        {
                            var innerMessage = exception.InnerException?.Message ?? exception.Message;
                            _executionLog.LogError(exception.InnerException ?? exception, "Error getting sport event fixture for id={EventId} and language:[{Language}]. Message (cached endpoint)={ErrorMessage}", id, culture.TwoLetterISOLanguageName, innerMessage);
                        }
                    }

                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e, "Error getting sport event fixture for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.Fixture, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetSportEventFixtureAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        private async Task<bool> GetSportEventFixtureForTournamentAsync(Urn id, CultureInfo culture, bool useCachedProvider, ISportEventCacheItem requester)
        {
            var provider = useCachedProvider
                    ? _sportEventFixtureForTournamentProvider // cached endpoint
                    : _sportEventFixtureChangeFixtureForTournamentProvider; // not cached endpoint
            var result = await provider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);

            if (result != null)
            {
                await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.TournamentInfo, requester).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all tournaments for sport as an asynchronous operation.
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>Task</returns>
        public async Task GetAllTournamentsForAllSportAsync(CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "AllTournamentsForAllSport"))
            {
                WriteLog($"Executing GetAllTournamentsForAllSportAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportDto> result = null;
                int restCallTime;
                try
                {
                    result = await _allTournamentsForAllSportsProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = HandleExceptionForSportList("all tournaments for all sports", culture, (int)t.Elapsed.TotalMilliseconds, e);
                }

                await DistributeSportList(result, culture).ConfigureAwait(false);
                WriteLog($"Executing GetAllTournamentsForAllSportAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets all categories for sport endpoint
        /// </summary>
        /// <param name="id">The id of the sport to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        public async Task GetSportCategoriesAsync(Urn id, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportCategories"))
            {
                WriteLog($"Executing GetSportCategoriesAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                SportCategoriesDto result = null;
                int restCallTime;
                try
                {
                    result = await _sportCategoriesProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting sport categories for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result?.Categories != null)
                {
                    await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.SportCategories, null).ConfigureAwait(false);
                }

                WriteLog($"Executing GetSportCategoriesAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Get all sports as an asynchronous operation.
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>Task</returns>
        public async Task GetAllSportsAsync(CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "AllSports"))
            {
                WriteLog($"Executing GetAllSportsAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportDto> result = null;
                int restCallTime;
                try
                {
                    result = await _allSportsProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = HandleExceptionForSportList("all sports", culture, (int)t.Elapsed.TotalMilliseconds, e);
                }

                await DistributeSportList(result, culture).ConfigureAwait(false);
                WriteLog($"Executing GetAllSportsAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the currently live sport events
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId each belongs to</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetLiveSportEventsAsync(CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "LiveSportEvents"))
            {
                WriteLog($"Executing GetLiveSportEventsAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportEventSummaryDto> apiData = null;
                int restCallTime;
                try
                {
                    apiData = await _sportEventsForDateProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting live sport events and language:[{Language}]. Message={ErrorMessage}", culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                var logMsg = $"Executing GetLiveSportEventsAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}";
                var result = await FilterEventSportFromSportEventSummaryList(apiData, culture, logMsg).ConfigureAwait(false);

                return result;
            }
        }

        /// <summary>
        /// Gets the sport events for specific date
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetSportEventsForDateAsync(DateTime date, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportEventsForDate"))
            {
                WriteLog($"Executing GetSportEventsForDateAsync for date={date.ToShortDateString()} and language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportEventSummaryDto> apiData = null;
                var dateId = date.ToString("yyyy-MM-dd");
                int restCallTime;
                try
                {
                    apiData = await _sportEventsForDateProvider.GetDataAsync(dateId, culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("No events scheduled", StringComparison.InvariantCultureIgnoreCase))
                    {
                        restCallTime = (int)t.Elapsed.TotalMilliseconds;
                        WriteLog($"Executing GetSportEventsForDateAsync for date={date.ToShortDateString()} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms. No results.");
                        return new List<Tuple<Urn, Urn>>();
                    }

                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting sport events for date {Date} and language:[{Language}]. Message={ErrorMessage}", dateId, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                var logMsg = $"Executing GetSportEventsForDateAsync for date={date.ToShortDateString()} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}";
                var result = await FilterEventSportFromSportEventSummaryList(apiData, culture, logMsg).ConfigureAwait(false);

                return result;
            }
        }

        /// <summary>
        /// Gets the sport events for specific tournament (tournament schedule)
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the sport events with the sportId belonging to specified tournament</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetSportEventsForTournamentAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportEventsForTournament"))
            {
                WriteLog($"Executing GetSportEventsForTournamentAsync for tournament id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportEventSummaryDto> result = null;
                int restCallTime;
                try
                {
                    result = await _sportEventsForTournamentProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    if (e.Message.Contains("raceScheduleEndpoint") && id.TypeGroup.Equals(ResourceTypeGroup.Stage))
                    {
                        try
                        {
                            result = await _sportEventsForRaceTournamentProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                            restCallTime = (int)t.Elapsed.TotalMilliseconds;
                        }
                        catch (Exception ex)
                        {
                            _executionLog.LogDebug(ex.InnerException ?? ex, "Error getting sport events for tournament for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                        }
                    }
                    else if (!e.Message.Contains("No schedule for this tournament") && !e.Message.Contains("This is a place-holder tournament."))
                    {
                        _executionLog.LogError(e.InnerException ?? e, "Error getting sport events for tournament for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        _executionLog.LogDebug(e.InnerException ?? e, "Error getting sport events for tournament for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    }
                }

                if (result != null && result.Items.Any())
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse($"sr:sportevents:{result.Items.Count()}", true), result, culture, DtoType.SportEventSummaryList, requester).ConfigureAwait(false);
                    var urns = new List<Tuple<Urn, Urn>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<Urn, Urn>(item.Id, item.SportId));
                    }
                    WriteLog($"Executing GetSportEventsForTournamentAsync for tournament id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                    return urns;
                }
            }
            return null;
        }

        /// <summary>
        /// Get player profile as an asynchronous operation.
        /// </summary>
        /// <param name="id">The id of the player</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        public async Task GetPlayerProfileAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "PlayerProfile"))
            {
                WriteLog($"Executing GetPlayerProfileAsync for id={id} and language={culture.TwoLetterISOLanguageName}.");

                PlayerProfileDto result = null;
                int restCallTime;
                try
                {
                    result = await _playerProfileProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting player profile for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null && result.Id.Equals(id))
                {
                    await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.PlayerProfile, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetPlayerProfileAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Get competitor as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the competitor</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        public async Task GetCompetitorAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "CompetitorProfile"))
            {
                WriteLog($"Executing GetCompetitorAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                CompetitorProfileDto competitorResult = null;
                SimpleTeamProfileDto simpleTeamResult = null;
                int restCallTime;
                try
                {
                    if (id.IsSimpleTeam() || id.ToString().StartsWith(SdkInfo.OutcomeTextVariantValue,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        simpleTeamResult = await _simpleTeamProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    }
                    else
                    {
                        competitorResult = await _competitorProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    }

                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting competitor profile for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (simpleTeamResult != null)
                {
                    await _cacheManager.SaveDtoAsync(id, simpleTeamResult, culture, DtoType.SimpleTeamProfile, requester).ConfigureAwait(false);
                    if (!simpleTeamResult.Competitor.Id.Equals(id))
                    {
                        await _cacheManager.SaveDtoAsync(simpleTeamResult.Competitor.Id, simpleTeamResult, culture, DtoType.SimpleTeamProfile, requester).ConfigureAwait(false);
                    }
                }

                if (competitorResult != null && competitorResult.Competitor.Id.Equals(id))
                {
                    await _cacheManager.SaveDtoAsync(id, competitorResult, culture, DtoType.CompetitorProfile, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetCompetitorAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Get seasons for tournament as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the tournament</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The list of ids of the seasons for specified tournament</returns>
        public async Task<IEnumerable<Urn>> GetSeasonsForTournamentAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SeasonsForTournament"))
            {
                WriteLog($"Executing GetSeasonsForTournamentAsync for tournament id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                TournamentSeasonsDto result = null;
                int restCallTime;
                try
                {
                    result = await _tournamentSeasonsProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;

                    if (e.Message.Contains("No seasons for tournament") || e.Message.Contains("This is a place-holder tournament.") || e.Message.Contains("NotFound"))
                    {
                        message = message.Contains(".")
                            ? message.Substring(0, message.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)
                            : message;
                        _executionLog.LogDebug("Error getting seasons for tournament id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    }
                    else
                    {
                        _executionLog.LogError(e.InnerException ?? e, "Error getting seasons for tournament id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                }

                if (result?.Tournament != null)
                {
                    await _cacheManager.SaveDtoAsync(result.Tournament.Id, result, culture, DtoType.TournamentSeasons, requester).ConfigureAwait(false);
                    if (result.Seasons != null)
                    {
                        var urns = new List<Urn>();
                        foreach (var item in result.Seasons)
                        {
                            urns.Add(item.Id);
                        }
                        WriteLog($"Executing GetSeasonsForTournamentAsync for tournament id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                        return urns;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get information about ongoing event as an asynchronous operation (match timeline)
        /// </summary>
        /// <param name="id">The id of the sport event</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The match timeline data object</returns>
        public async Task<MatchTimelineDto> GetInformationAboutOngoingEventAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "InformationAboutOngoingEvent"))
            {
                WriteLog($"Executing GetInformationAboutOngoingEventAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                MatchTimelineDto result = null;
                int restCallTime;
                try
                {
                    result = await _ongoingSportEventProvider
                        .GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;

                    if (e.Message.Contains("NotFound"))
                    {
                        message = message.Contains(".")
                            ? message.Substring(0,
                                message.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)
                            : message;
                        _executionLog.LogDebug("Error getting match timeline for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    }
                    else
                    {
                        _executionLog.LogError(e.InnerException ?? e,
                            "Error getting match timeline for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(result.SportEvent.Id, result, culture, DtoType.MatchTimeline, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetInformationAboutOngoingEventAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");

                return result;
            }
        }

        /// <summary>
        /// Gets the market descriptions (static)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>Task</returns>
        public async Task GetMarketDescriptionsAsync(CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "MarketDescriptions"))
            {
                WriteLog($"Executing GetMarketDescriptionsAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<MarketDescriptionDto> result = null;
                int restCallTime;
                try
                {
                    result = await _invariantMarketDescriptionsProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting market descriptions for language:[{Language}]. Message={ErrorMessage}", culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse("sr:markets:" + result.Items?.Count(), true), result, culture, DtoType.MarketDescriptionList, null).ConfigureAwait(false);
                }

                WriteLog($"Executing GetMarketDescriptionsAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the variant market description (dynamic, single)
        /// </summary>
        /// <param name="id">The id of the market</param>
        /// <param name="variant">The variant urn</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task GetVariantMarketDescriptionAsync(int id, string variant, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "VariantMarketDescription"))
            {
                WriteLog($"Executing GetVariantMarketDescriptionAsync for id={id}, variant={variant} and language={culture.TwoLetterISOLanguageName}.", true);

                MarketDescriptionDto result = null;
                int restCallTime;
                try
                {
                    result = await _variantMarketDescriptionProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName, variant).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;

                    if (result != null)
                    {
                        if (!result.Id.Equals(id) || !result.Variant.Equals(variant))
                        {
                            UofSdkTelemetry.DataRouterManagerGetVariantMarketDescriptionDiff.Add(1);
                            _executionLog.LogWarning("Received different market variant description then requested. ({VariantDiff})", $"{id}?{variant} vs {result.Id}?{result.Variant}");
                        }
                    }
                    else
                    {
                        _executionLog.LogError("Error getting market variant description for market id={MarketId}, variant={Variant} and language:[{Language}] (not found)", id, variant, culture.TwoLetterISOLanguageName);
                    }
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    if (e.Message.Contains("NotFound", StringComparison.InvariantCultureIgnoreCase)
                        || e.Message.Contains("Not_Found", StringComparison.InvariantCultureIgnoreCase))
                    {
                        message = message.Contains(".")
                            ? message.Substring(0,
                                message.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)
                            : message;
                        _executionLog.LogError(
                            "Error getting market variant description for market id={MarketId}, variant={MarketVariant} and language:[{Language}]. Not found. Message={ErrorMessage}", id, variant, culture.TwoLetterISOLanguageName, message);
                    }
                    else if (e.Message.Contains("name cannot be null"))
                    {
                        message = message.Contains(".")
                            ? message.Substring(0,
                                message.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)
                            : message;
                        _executionLog.LogError(
                            "Error getting market variant description for market id={MarketId}, variant={MarketVariant} and language:[{Language}]. Outcome missing name. Message={ErrorMessage}", id, variant, culture.TwoLetterISOLanguageName, message);
                    }
                    else
                    {
                        _executionLog.LogError(e.InnerException ?? e,
                            "Error getting market variant description for market id={MarketId}, variant={MarketVariant} and language:[{Language}]. Message={ErrorMessage}", id, variant, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse("sr:variant:" + result.Id, true), result, culture, DtoType.MarketDescription, null).ConfigureAwait(false);
                    if (!result.Id.Equals(id))
                    {
                        WriteLog($"Executing GetVariantMarketDescriptionAsync for id={id}, variant={variant} and language={culture.TwoLetterISOLanguageName} received data for market {result.Id}.", true);
                        result.OverrideId(id);
                        await _cacheManager.SaveDtoAsync(Urn.Parse("sr:variant:" + result.Id, true), result, culture, DtoType.MarketDescription, null).ConfigureAwait(false);
                    }
                }

                WriteLog($"Executing GetVariantMarketDescriptionAsync for id={id}, variant={variant} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the variant descriptions (static)
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task GetVariantDescriptionsAsync(CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "VariantDescriptions"))
            {
                WriteLog($"Executing GetVariantDescriptionsAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<VariantDescriptionDto> result = null;
                int restCallTime;
                try
                {
                    result = await _variantDescriptionsProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting variant descriptions for language:[{Language}]. Message={ErrorMessage}", culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse("sr:variants:" + result.Items?.Count(), true), result, culture, DtoType.VariantDescriptionList, null).ConfigureAwait(false);
                }

                WriteLog($"Executing GetVariantDescriptionsAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the <see cref="DrawDto" /> from lottery draw summary endpoint
        /// </summary>
        /// <param name="id">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task GetDrawSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            if (!_isWnsAvailable)
            {
                return;
            }

            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "DrawSummary"))
            {
                WriteLog($"Executing GetDrawSummaryAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                DrawDto result = null;
                int restCallTime;
                try
                {
                    result = await _lotteryDrawSummaryProvider
                        .GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting draw summary for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetDrawSummaryAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the <see cref="DrawDto" /> from the lottery draw fixture endpoint
        /// </summary>
        /// <param name="id">The id of the draw to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>Task</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task GetDrawFixtureAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester)
        {
            if (!_isWnsAvailable)
            {
                return;
            }

            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "DrawFixture"))
            {
                WriteLog($"Executing GetDrawFixtureAsync for id={id} and language={culture.TwoLetterISOLanguageName}.", true);

                DrawDto result = null;
                int restCallTime;
                try
                {
                    result = await _lotteryDrawFixtureProvider.GetDataAsync(id.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting draw fixture for id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(result.Id, result, culture, DtoType.LotteryDraw, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetDrawFixtureAsync for id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the lottery draw schedule
        /// </summary>
        /// <param name="lotteryId">The id of the lottery</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <returns>The lottery with its schedule</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task GetLotteryScheduleAsync(Urn lotteryId, CultureInfo culture, ISportEventCacheItem requester)
        {
            if (!_isWnsAvailable)
            {
                return;
            }

            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "LotterySchedule"))
            {
                WriteLog($"Executing GetLotteryScheduleAsync for id={lotteryId} and language={culture.TwoLetterISOLanguageName}.", true);

                LotteryDto result = null;
                int restCallTime;
                try
                {
                    result = await _lotteryScheduleProvider.GetDataAsync(lotteryId.ToString(), culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting lottery schedule for id={EventId} and language:[{Language}]. Message={ErrorMessage}", lotteryId, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(result.Id, result, culture, DtoType.Lottery, null).ConfigureAwait(false);
                }

                WriteLog($"Executing GetLotteryScheduleAsync for id={lotteryId} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
            }
        }

        /// <summary>
        /// Gets the list of available lotteries
        /// </summary>
        /// <param name="culture">The culture to be fetched</param>
        /// <param name="ignoreFail">if the fail should be ignored - when user does not have access</param>
        /// <returns>The list of combination of id of the lottery and associated sport id</returns>
        /// <remarks>This gets called only if WNS is available</remarks>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetAllLotteriesAsync(CultureInfo culture, bool ignoreFail)
        {
            if (!_isWnsAvailable)
            {
                return new List<Tuple<Urn, Urn>>();
            }

            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "AllLotteries"))
            {
                WriteLog($"Executing GetAllLotteriesAsync for language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<LotteryDto> result = null;
                int restCallTime;
                try
                {
                    result = await _lotteryListProvider.GetDataAsync(culture.TwoLetterISOLanguageName).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e, "Error getting all lotteries for language:[{Language}]. Message={ErrorMessage}", culture.TwoLetterISOLanguageName, message);
                    if (ignoreFail && ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                if (result?.Items != null)
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse($"sr:lotteries:{result.Items.Count()}", true), result, culture, DtoType.LotteryList, null).ConfigureAwait(false);
                    var urns = new List<Tuple<Urn, Urn>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<Urn, Urn>(item.Id, item.SportId));
                    }
                    WriteLog($"Executing GetAllLotteriesAsync for tournament language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                    return urns;
                }
                WriteLog($"Executing GetAllLotteriesAsync for language={culture.TwoLetterISOLanguageName} took {restCallTime} ms. {SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)} No data.");
            }
            return new List<Tuple<Urn, Urn>>();
        }

        /// <summary>
        /// Get available selections as an asynchronous operation.
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>The available selections for event</returns>
        public async Task<IAvailableSelections> GetAvailableSelectionsAsync(Urn id)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "AvailableSelections"))
            {
                WriteLog($"Executing GetAvailableSelectionsAsync for id={id}.", true);

                AvailableSelectionsDto result = null;
                int restCallTime;
                try
                {
                    result = await _availableSelectionsProvider.GetDataAsync(id.ToString()).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (CommunicationException e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw new CommunicationException(filteredResponse, e.Url, e.ResponseCode, null);
                    }

                    _executionLog.LogError("Error getting available selections for id={EventId}. Message={HttpResponse}", id, filteredResponse);
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }

                    _executionLog.LogError(e.InnerException ?? e, "Error getting available selections for id={EventId}. Message={HttpResponse}", id, filteredResponse);
                }

                AvailableSelections availableSelections = null;
                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(id, result, _defaultLocale, DtoType.AvailableSelections, null).ConfigureAwait(false);
                    availableSelections = new AvailableSelections(result);
                }

                WriteLog($"Executing GetAvailableSelectionsAsync for id={id} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                return availableSelections;
            }
        }

        public async Task<ICalculation> CalculateProbabilityAsync(IEnumerable<ISelection> selections)
        {
            var enumerable = selections.ToList();
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "CalculateProbability"))
            {
                WriteLog("Executing CalculateProbability.", true);

                CalculationDto result = null;
                int restCallTime;
                try
                {
                    result = await _calculateProbabilityProvider.GetDataAsync(enumerable).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (CommunicationException e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw new CommunicationException(filteredResponse, e.Url, e.ResponseCode, null);
                    }

                    _executionLog.LogError("Error calculating probabilities. Message={HttpResponse}", filteredResponse);
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }

                    _executionLog.LogError(e.InnerException ?? e, "Error calculating probabilities. Message={HttpResponse}", filteredResponse);
                }

                Calculation calculation = null;
                if (result != null)
                {
                    calculation = new Calculation(result);
                }

                WriteLog($"Executing CalculateProbability took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                return calculation;
            }
        }

        public async Task<ICalculationFilter> CalculateProbabilityFilteredAsync(IEnumerable<ISelection> selections)
        {
            var enumerable = selections.ToList();
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "CalculateProbabilityFiltered"))
            {
                WriteLog("Executing CalculateProbabilityFiltered.", true);

                FilteredCalculationDto result = null;
                int restCallTime;
                try
                {
                    result = await _calculateProbabilityFilteredProvider.GetDataAsync(enumerable).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (CommunicationException e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw new CommunicationException(filteredResponse, e.Url, e.ResponseCode, null);
                    }

                    _executionLog.LogError("Error calculating probabilities (filtered). Message={HttpResponse}", filteredResponse);
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    var filteredResponse = SdkInfo.ExtractHttpResponseMessage(message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }

                    _executionLog.LogError(e.InnerException ?? e, "Error calculating probabilities (filtered). Message={HttpResponse}", filteredResponse);
                }

                CalculationFilter calculation = null;
                if (result != null)
                {
                    calculation = new CalculationFilter(result);
                }

                WriteLog($"Executing CalculateProbabilityFiltered took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                return calculation;
            }
        }

        /// <summary>
        /// Gets the list of almost all events we are offering prematch odds for.
        /// </summary>
        /// <param name="startIndex">Starting record (this is an index, not time)</param>
        /// <param name="limit">How many records to return (max: 1000)</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the sport event ids with the sportId it belongs to</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetListOfSportEventsAsync(int startIndex, int limit, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "ListOfSportEvents"))
            {
                WriteLog($"Executing GetListOfSportEventsAsync with startIndex={startIndex}, limit={limit} and language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<SportEventSummaryDto> apiData = null;
                int restCallTime;
                try
                {
                    apiData = await _listSportEventProvider
                        .GetDataAsync(culture.TwoLetterISOLanguageName, startIndex.ToString(CultureInfo.InvariantCulture), limit.ToString(CultureInfo.InvariantCulture))
                        .ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    if (e.Message.Contains("NotFound"))
                    {
                        var message = e.InnerException?.Message ?? e.Message;
                        _executionLog.LogDebug(e.InnerException ?? e,
                            "Error getting list of sport events for startIndex={StartIndex}, limit={lLmit} and language:[{Language}]. Message={ErrorMessage}", startIndex, limit, culture.TwoLetterISOLanguageName, message);
                    }
                    else
                    {
                        var message = e.InnerException?.Message ?? e.Message;
                        _executionLog.LogError(e.InnerException ?? e,
                            "Error getting list of sport events for startIndex={StartIndex}, limit={Limit} and language:[{Language}]. Message={ErrorMessage}", startIndex, limit, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                }

                var logMsg = $"Executing ListOfSportEventsAsync with startIndex={startIndex}, limit={limit} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}";
                var result = await FilterEventSportFromSportEventSummaryList(apiData, culture, logMsg).ConfigureAwait(false);

                return result;
            }
        }

        /// <summary>
        /// Gets the list of all the available tournaments for a specific sport
        /// </summary>
        /// <param name="sportId">The specific sport id</param>
        /// <param name="culture">The culture</param>
        /// <returns>The list of the available tournament ids with the sportId it belongs to</returns>
        public async Task<IEnumerable<Tuple<Urn, Urn>>> GetSportAvailableTournamentsAsync(Urn sportId, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "SportAvailableTournaments"))
            {
                WriteLog($"Executing GetSportAvailableTournamentsAsync with sportId={sportId} and language={culture.TwoLetterISOLanguageName}.", true);

                EntityList<TournamentInfoDto> result = null;
                int restCallTime;
                try
                {
                    result = await _availableSportTournamentsProvider.GetDataAsync(culture.TwoLetterISOLanguageName, sportId.ToString()).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    if (e.Message.Contains("NotFound"))
                    {
                        var message = e.InnerException?.Message ?? e.Message;
                        _executionLog.LogDebug(e.InnerException ?? e, "Error getting sport available tournaments for sportId={SportId} and language:[{Language}]. Message={ErrorMessage}", sportId, culture.TwoLetterISOLanguageName, message);
                    }
                    else
                    {
                        var message = e.InnerException?.Message ?? e.Message;
                        _executionLog.LogError(e.InnerException ?? e, "Error getting sport available tournaments for sportId={SportId} and language:[{Language}]. Message={ErrorMessage}", sportId, culture.TwoLetterISOLanguageName, message);
                        if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                        {
                            throw;
                        }
                    }
                }

                if (result != null && result.Items.Any())
                {
                    await _cacheManager.SaveDtoAsync(Urn.Parse($"sr:tournaments:{result.Items.Count()}", true), result, culture, DtoType.TournamentInfoList, null).ConfigureAwait(false);
                    var urns = new List<Tuple<Urn, Urn>>();
                    foreach (var item in result.Items)
                    {
                        urns.Add(new Tuple<Urn, Urn>(item.Id, item.Sport.Id));
                    }
                    WriteLog($"Executing GetSportAvailableTournamentsAsync with sportId={sportId} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                    return urns;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the list of all fixtures that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all fixtures that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IFixtureChange>> GetFixtureChangesAsync(DateTime? after, Urn sportId, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "FixtureChanges"))
            {
                WriteLog("Executing GetFixtureChangesAsync.", true);

                EntityList<FixtureChangeDto> result = null;
                int restCallTime;
                try
                {
                    var query = GetChangesQueryString(after, sportId);
                    result = await _fixtureChangesProvider.GetDataAsync(culture.TwoLetterISOLanguageName, query)
                        .ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    if (e.Message.Contains("Value cannot be null"))
                    {
                        _executionLog.LogInformation("No fixture changes for after={After}, sportId={SportId} and language={Language}", after, sportId, culture.TwoLetterISOLanguageName);
                        return null;
                    }

                    _executionLog.LogError(e.InnerException ?? e, "Error getting fixture changes. Message={ErrorMessage}", message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                WriteLog($"Executing GetFixtureChangesAsync took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                return result?.Items.Select(f => new FixtureChange(f)).ToList();
            }
        }

        /// <summary>
        /// Gets the list of all results that have changed in the last 24 hours
        /// </summary>
        /// <param name="after">A <see cref="DateTime"/> specifying the starting date and time for filtering</param>
        /// <param name="sportId">A <see cref="Urn"/> specifying the sport for which the fixtures should be returned</param>
        /// <param name="culture">The culture to be fetched</param>
        /// <returns>The list of all results that have changed in the last 24 hours</returns>
        public async Task<IEnumerable<IResultChange>> GetResultChangesAsync(DateTime? after, Urn sportId, CultureInfo culture)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "ResultChanges"))
            {
                WriteLog("Executing GetResultChangesAsync.", true);

                EntityList<ResultChangeDto> result = null;
                int restCallTime;
                try
                {
                    var query = GetChangesQueryString(after, sportId);
                    result = await _resultChangesProvider.GetDataAsync(culture.TwoLetterISOLanguageName, query)
                        .ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e, "Error getting result changes. Message={ErrorMessage}", message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                    {
                        throw;
                    }
                }

                WriteLog($"Executing GetResultChangesAsync took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");
                return result?.Items.Select(f => new ResultChange(f)).ToList();
            }
        }

        /// <summary>
        /// Get stage event period summary as an asynchronous operation
        /// </summary>
        /// <param name="id">The id of the sport event to be fetched</param>
        /// <param name="culture">The language to be fetched</param>
        /// <param name="requester">The cache item which invoked request</param>
        /// <param name="competitorIds">The list of competitor ids to fetch the results for</param>
        /// <param name="periods">The list of period ids to fetch the results for</param>
        /// <returns>Task</returns>
        public async Task<PeriodSummaryDto> GetPeriodSummaryAsync(Urn id, CultureInfo culture, ISportEventCacheItem requester, ICollection<Urn> competitorIds = null, ICollection<int> periods = null)
        {
            using (var t = new TelemetryTracker(UofSdkTelemetry.DataRouterManager, "endpoint", "PeriodSummary"))
            {

                var compIds = competitorIds.IsNullOrEmpty() ? "null" : string.Join(", ", competitorIds);
                var periodIds = periods.IsNullOrEmpty() ? "null" : string.Join(", ", periods);

                WriteLog($"Executing GetPeriodSummaryAsync for event id={id} and language={culture.TwoLetterISOLanguageName}, Competitors={compIds}, Periods={periodIds}", true);

                //host/v1/sports/en/sport_events/sr:stage:{id}/period_summary.xml?competitors=sr:competitor:{id}&competitors=sr:competitor:{id}&periods=2&periods=3&periods=4
                var query = string.Empty;
                var compQuery = string.Empty;
                var periodQuery = string.Empty;
                if (!competitorIds.IsNullOrEmpty())
                {
                    compQuery = string.Join("&", competitorIds.Select(s => $"competitors={s}"));
                }

                if (!periods.IsNullOrEmpty())
                {
                    periodQuery = string.Join("&", periods.Select(s => $"periods={s}"));
                }

                if (!compQuery.IsNullOrEmpty())
                {
                    query = "?" + compQuery;
                }

                if (!periodQuery.IsNullOrEmpty())
                {
                    query = query.IsNullOrEmpty() ? "?" + periodQuery : query + "&" + periodQuery;
                }

                PeriodSummaryDto result = null;
                int restCallTime;
                try
                {
                    result = await _stagePeriodSummaryProvider.GetDataAsync(culture.TwoLetterISOLanguageName, id.ToString(), query).ConfigureAwait(false);
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    restCallTime = (int)t.Elapsed.TotalMilliseconds;
                    var message = e.InnerException?.Message ?? e.Message;
                    _executionLog.LogError(e.InnerException ?? e,
                        "Error getting period summary for event id={EventId} and language:[{Language}]. Message={ErrorMessage}", id, culture.TwoLetterISOLanguageName, message);
                    if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw && requester != null)
                    {
                        throw;
                    }
                }

                if (result != null)
                {
                    await _cacheManager.SaveDtoAsync(id, result, culture, DtoType.SportEventSummary, requester).ConfigureAwait(false);
                }

                WriteLog($"Executing GetPeriodSummaryAsync for event id={id} and language={culture.TwoLetterISOLanguageName} took {restCallTime} ms.{SavingTook(restCallTime, (int)t.Elapsed.TotalMilliseconds)}");

                return result;
            }
        }

        private int HandleExceptionForSportList(string name, CultureInfo culture, int elapsedMs, Exception e)
        {
            var message = e.InnerException?.Message ?? e.Message;
            _executionLog.LogError(e.InnerException ?? e, "Error getting {MethodName} for language:[{Language}]. Message={ErrorMessage}", name, culture.TwoLetterISOLanguageName, message);
            if (ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
            {
                throw e;
            }
            return elapsedMs;
        }

        private async Task DistributeSportList(EntityList<SportDto> sports, CultureInfo culture)
        {
            if (sports != null && sports.Items.Any())
            {
                await _cacheManager.SaveDtoAsync(Urn.Parse($"sr:sports:{sports.Items.Count()}", true), sports, culture, DtoType.SportList, null).ConfigureAwait(false);
            }
        }

        private async Task<List<Tuple<Urn, Urn>>> FilterEventSportFromSportEventSummaryList(EntityList<SportEventSummaryDto> eventSummaries, CultureInfo culture, string logMsg)
        {
            if (eventSummaries != null && eventSummaries.Items.Any())
            {
                await _cacheManager.SaveDtoAsync(Urn.Parse($"sr:sportevents:{eventSummaries.Items.Count()}", true), eventSummaries, culture, DtoType.SportEventSummaryList, null).ConfigureAwait(false);
                var urns = new List<Tuple<Urn, Urn>>();
                foreach (var item in eventSummaries.Items)
                {
                    urns.Add(new Tuple<Urn, Urn>(item.Id, item.SportId));
                }
                WriteLog(logMsg);
                return urns;
            }

            return null;
        }

        private string GetChangesQueryString(DateTime? after, Urn sportId)
        {
            var paramList = new List<string>();
            if (after.HasValue)
            {
                paramList.Add("afterDateTime=" + HttpUtility.UrlEncode(after.Value.ToString("o")));
            }
            if (sportId != null)
            {
                paramList.Add("sportId=" + HttpUtility.UrlEncode(sportId.ToString()));
            }

            if (paramList.Count == 0)
            {
                return "";
            }

            return "?" + string.Join("&", paramList);
        }

        [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
        private void WriteLog(string text, bool useDebug = false)
        {
            if (useDebug)
            {
                _executionLog.LogDebug(text);
            }
            else
            {
                _executionLog.LogInformation(text);
            }
        }

        private static string SavingTook(int restTime, int totalTime)
        {
            var difference = totalTime - restTime;
            if (difference > 10)
            {
                return $" Saving took {difference.ToString(CultureInfo.InvariantCulture)} ms";
            }
            return string.Empty;
        }
    }
}
