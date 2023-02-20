/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using cashout = Sportradar.OddsFeed.SDK.Messages.REST.cashout;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A <see cref="IFeedMessageMapper"/> implementation which maps feed messages to non-specific feed messages
    /// </summary>
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        /// <summary>
        /// A <see cref="ISportEntityFactory"/> implementation used to construct <see cref="ISportEvent"/> instances
        /// </summary>
        private readonly ISportEntityFactory _sportEntityFactory;

        /// <summary>
        /// A <see cref="INameProviderFactory"/> instance used to build <see cref="INameProvider"/> instances used to get/create market/outcome names
        /// </summary>
        private readonly INameProviderFactory _nameProviderFactory;

        /// <summary>
        /// A <see cref="IMarketMappingProviderFactory"/> instance used to build <see cref="IMarketMappingProvider"/> instances used to get/create mapped market/outcome ids
        /// </summary>
        private readonly IMarketMappingProviderFactory _mappingProviderFactory;

        /// <summary>
        /// A <see cref="INamedValuesProvider"/> used to obtain descriptions for named values
        /// </summary>
        private readonly INamedValuesProvider _namedValuesProvider;

        /// <summary>
        /// A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the publicly available instance constructed by current instance should handle potential exceptions
        /// </summary>
        private readonly ExceptionHandlingStrategy _externalExceptionStrategy;

        private readonly IProducerManager _producerManager;

        private readonly IMarketCacheProvider _marketCacheProvider;

        private readonly INamedValueCache _voidReasonCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedMessageMapper" /> class
        /// </summary>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> implementation used to construct <see cref="ISportEvent"/> instances</param>
        /// <param name="nameProviderFactory">A <see cref="INameProviderFactory"/> instance used build <see cref="INameProvider"/> instances used to get/create market/outcome names</param>
        /// <param name="mappingProviderFactory">A factory used to construct <see cref="IMarketMappingProvider"/> instances</param>
        /// <param name="namedValuesProvider">A <see cref="INamedValuesProvider"/> used to obtain descriptions for named values</param>
        /// <param name="externalExceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the publicly available instance constructed by current instance should handle potential exceptions</param>
        /// <param name="producerManager">An <see cref="IProducerManager"/> used to get <see cref="IProducer"/></param>
        /// <param name="marketCacheProvider">The SDK market cache provider used to retrieve market data</param>
        /// <param name="voidReasonCache">A <see cref="INamedValueCache"/> for possible void reasons</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "Allowed here")]
        public FeedMessageMapper(ISportEntityFactory sportEntityFactory,
                                 INameProviderFactory nameProviderFactory,
                                 IMarketMappingProviderFactory mappingProviderFactory,
                                 INamedValuesProvider namedValuesProvider,
                                 ExceptionHandlingStrategy externalExceptionStrategy,
                                 IProducerManager producerManager,
                                 IMarketCacheProvider marketCacheProvider,
                                 INamedValueCache voidReasonCache)
        {
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(nameProviderFactory, nameof(nameProviderFactory)).NotNull();
            Guard.Argument(namedValuesProvider, nameof(namedValuesProvider)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();
            Guard.Argument(marketCacheProvider, nameof(marketCacheProvider)).NotNull();
            Guard.Argument(voidReasonCache, nameof(voidReasonCache)).NotNull();

            _nameProviderFactory = nameProviderFactory;
            _sportEntityFactory = sportEntityFactory;
            _mappingProviderFactory = mappingProviderFactory;
            _namedValuesProvider = namedValuesProvider;
            _externalExceptionStrategy = externalExceptionStrategy;
            _producerManager = producerManager;
            _marketCacheProvider = marketCacheProvider;
            _voidReasonCache = voidReasonCache;
        }

        /// <summary>
        /// Builds and returns a <see cref="ISportEvent"/> derived instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the constructed instance should handle potential exceptions</param>
        /// <returns>A <see cref="ISportEvent"/> derived constructed instance</returns>
        protected T BuildEvent<T>(URN eventId, URN sportId, List<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();
            Guard.Argument(sportId, nameof(sportId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            T entity;
            switch (eventId.TypeGroup)
            {
                case ResourceTypeGroup.TOURNAMENT:
                case ResourceTypeGroup.BASIC_TOURNAMENT:
                case ResourceTypeGroup.SEASON:
                case ResourceTypeGroup.MATCH:
                case ResourceTypeGroup.STAGE:
                case ResourceTypeGroup.DRAW:
                case ResourceTypeGroup.LOTTERY:
                case ResourceTypeGroup.UNKNOWN:
                    {
                        entity = (T)_sportEntityFactory.BuildSportEvent<ISportEvent>(eventId, sportId, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.OTHER:
                    throw new InvalidOperationException($"Other entity with id={eventId} cannot be associated with feed message");
                default:
                    throw new InvalidOperationException($"Entity with id={eventId} cannot be associated with feed message");
            }
            return entity;
        }

        /// <summary>
        /// Gets the new <see cref="ICompetition"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <returns>Returns the new <see cref="ICompetition"/> instance</returns>
        protected T GetEventForMessage<T>(URN eventId, URN sportId, List<CultureInfo> cultures) where T : ISportEvent
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();
            Guard.Argument(sportId, nameof(sportId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            return BuildEvent<T>(eventId, sportId, cultures, _externalExceptionStrategy);
        }

        /// <summary>
        /// Gets the new <see cref="ICompetition"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <returns>Returns the new <see cref="ICompetition"/> instance</returns>
        protected T GetEventForNameProvider<T>(URN eventId, URN sportId, List<CultureInfo> cultures) where T : ISportEvent
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();
            Guard.Argument(sportId, nameof(sportId)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            return BuildEvent<T>(eventId, sportId, cultures, ExceptionHandlingStrategy.THROW);
        }

        /// <summary>
        /// Gets the new <see cref="IMarketWithSettlement"/> instance
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> representing the sport event associated with the generated message</param>
        /// <param name="marketSettlement">The <see cref="betSettlementMarket"/> instance used</param>
        /// <param name="producerId">A producerId of the <see cref="ISportEvent"/></param>
        /// <param name="sportId">A producerId of the <see cref="ISportEvent"/></param>
        /// <param name="cultures">The list of cultures that should be prefetched</param>
        /// <returns>Returns the new <see cref="IMarketWithSettlement"/> instance</returns>
        protected virtual IMarketWithSettlement GetMarketWithResults(ISportEvent sportEvent, betSettlementMarket marketSettlement, int producerId, URN sportId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull("SportEvent missing");
            Guard.Argument(marketSettlement, nameof(marketSettlement)).NotNull("marketSettlement != null");

            var cultureInfos = cultures.ToList();

            var specifiers = string.IsNullOrEmpty(marketSettlement.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketSettlement.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketSettlement.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketSettlement.id, specifiers, producerId, sportId);
            var outcomes = (IEnumerable<IOutcomeSettlement>)marketSettlement.Items?.Select(outcome => new OutcomeSettlement(
                              outcome.dead_heat_factorSpecified ? (double?)outcome.dead_heat_factor : null,
                              outcome.id,
                              outcome.result,
                              MessageMapperHelper.GetVoidFactor(outcome.void_factorSpecified, outcome.void_factor),
                              nameProvider,
                              mappingProvider,
                              cultureInfos,
                              new OutcomeDefinition(marketSettlement.id, outcome.id, _marketCacheProvider, specifiers, cultureInfos, _externalExceptionStrategy)))
                ?? new List<IOutcomeSettlement>();

            var marketDefinition = new MarketDefinition(marketSettlement.id, _marketCacheProvider, sportId, producerId, specifiers, cultureInfos, _externalExceptionStrategy);

            return new MarketWithSettlement(marketSettlement.id,
                                            specifiers,
                                            additionalInfo,
                                            outcomes.ToList(),
                                            nameProvider,
                                            mappingProvider,
                                            marketDefinition,
                                            marketSettlement.void_reasonSpecified ? (int?)marketSettlement.void_reason : null,
                                            _namedValuesProvider.VoidReasons,
                                            cultureInfos);
        }

        /// <summary>
        /// Gets the new <see cref="IMarketWithProbabilities"/> instance
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> representing the sport event associated with the generated message</param>
        /// <param name="marketOddsChange">The <see cref="oddsChangeMarket"/> instance used</param>
        /// <param name="producerId">A producerId specifying message producerId</param>
        /// <param name="sportId">A sportId of the <see cref="ISportEvent"/></param>
        /// <param name="cultures">The list of cultures that should be prefetched</param>
        /// <returns>Returns the new <see cref="IMarketWithOdds"/> instance</returns>
        protected virtual IMarketWithProbabilities GetMarketWithProbabilities(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, URN sportId, IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures.ToList();
            var specifiers = string.IsNullOrEmpty(marketOddsChange.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketOddsChange.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, producerId, sportId);

            var marketDefinition = new MarketDefinition(marketOddsChange.id, _marketCacheProvider, sportId, producerId, specifiers, cultureInfos, _externalExceptionStrategy);

            return new MarketWithProbabilities(marketOddsChange.id,
                                               specifiers,
                                               additionalInfo,
                                               nameProvider,
                                               mappingProvider,
                                               MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                                               marketOddsChange.outcome?.Select(outcomeOdds =>
                                                        new OutcomeProbabilities(outcomeOdds.id,
                                                                                 outcomeOdds.activeSpecified ? (bool?)(outcomeOdds.active != 0) : null,
                                                                                 outcomeOdds.probabilitiesSpecified ? (double?)outcomeOdds.probabilities : null,
                                                                                 nameProvider,
                                                                                 mappingProvider,
                                                                                 cultureInfos,
                                                                                 new OutcomeDefinition(marketOddsChange.id, outcomeOdds.id, _marketCacheProvider, specifiers, cultureInfos, _externalExceptionStrategy),
                                                                                 GetAdditionalProbabilities(outcomeOdds))),
                                               marketDefinition,
                                               cultureInfos,
                                               marketOddsChange.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(marketOddsChange.cashout_status) : null,
                                               GetMarketMetadata(marketOddsChange.market_metadata));
        }

        /// <summary>
        /// Gets the new <see cref="IMarketWithOdds"/> instance
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> representing the sport event associated with the generated message</param>
        /// <param name="marketOddsChange">The <see cref="oddsChangeMarket"/> instance used</param>
        /// <param name="producerId">A producerId of the <see cref="ISportEvent"/></param>
        /// <param name="sportId">A sportId of the <see cref="ISportEvent"/></param>
        /// <param name="cultures">The list of cultures that should be prefetched</param>
        /// <returns>Returns the new <see cref="IMarketWithOdds"/> instance</returns>
        protected virtual IMarketWithOdds GetMarketWithOdds(ISportEvent sportEvent, oddsChangeMarket marketOddsChange, int producerId, URN sportId, IEnumerable<CultureInfo> cultures)
        {
            var cultureInfos = cultures.ToList();
            var specifiers = string.IsNullOrEmpty(marketOddsChange.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketOddsChange.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketOddsChange.extended_specifiers);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, producerId, sportId);

            var outcomes = marketOddsChange.outcome?.Select(outcome =>
                GetOutcomeWithOdds(sportEvent,
                                    nameProvider,
                                    mappingProvider,
                                    outcome,
                                    cultureInfos,
                                    new OutcomeDefinition(marketOddsChange.id, outcome.id, _marketCacheProvider, specifiers, cultureInfos, _externalExceptionStrategy)));

            var marketDefinition = new MarketDefinition(marketOddsChange.id, _marketCacheProvider, sportId, producerId, specifiers, cultureInfos, _externalExceptionStrategy);

            return new MarketWithOdds(marketOddsChange.id,
                                    specifiers,
                                    additionalInfo,
                                    nameProvider,
                                    mappingProvider,
                                    MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                                    marketOddsChange.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(marketOddsChange.cashout_status) : null,
                                    marketOddsChange.favouriteSpecified && marketOddsChange.favourite == 1,
                                    outcomes,
                                    GetMarketMetadata(marketOddsChange.market_metadata),
                                    marketDefinition,
                                    cultureInfos);
        }

        /// <summary>
        /// Gets the new <see cref="IMarket"/> instance
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> representing the sport event associated with the generated message</param>
        /// <param name="market">The <see cref="market"/> instance used</param>
        /// <param name = "producerId" > A producerId of the<see cref="ISportEvent"/></param>
        /// <param name="sportId">A sportId of the <see cref="ISportEvent"/></param>
        /// <param name="cultures">The list of cultures that should be prefetched</param>
        /// <returns>Returns the new <see cref="IMarket"/> instance</returns>
        protected virtual IMarketCancel GetMarketCancel(ISportEvent sportEvent, market market, int producerId, URN sportId, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(sportEvent, nameof(sportEvent)).NotNull();
            Guard.Argument(market, nameof(market)).NotNull();

            var cultureInfos = cultures.ToList();

            var specifiers = string.IsNullOrEmpty(market.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.specifiers);

            var additionalInfo = string.IsNullOrEmpty(market.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.extended_specifiers);

            var marketDefinition = new MarketDefinition(market.id, _marketCacheProvider, sportId, producerId, specifiers, cultureInfos, _externalExceptionStrategy);

            return new MarketCancel(market.id,
                                  specifiers,
                                  additionalInfo,
                                  _nameProviderFactory.BuildNameProvider(sportEvent, market.id, specifiers),
                                  _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, market.id, specifiers, producerId, sportId),
                                  marketDefinition,
                                  market.void_reasonSpecified ? market.void_reason : (int?)null,
                                  _voidReasonCache,
                                  cultureInfos);
        }

        /// <summary>
        /// Gets the new <see cref="IOutcomeOdds"/> instances
        /// </summary>
        /// <param name="sportEvent">The <see cref="ISportEvent"/> associated with the market</param>
        /// <param name="nameProvider">The <see cref="INameProvider"/> used to generate outcome name</param>
        /// <param name="mappingProvider">The <see cref="IMarketMappingProvider"/> used to provide market mapping</param>
        /// <param name="outcome">The <see cref="oddsChangeMarketOutcome"/> representing the outcome to be mapped</param>
        /// <param name="cultures">A <see cref="IReadOnlyCollection{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        /// <returns>IOutcomeOdds</returns>
        protected virtual IOutcomeOdds GetOutcomeWithOdds(ISportEvent sportEvent,
                                                          INameProvider nameProvider,
                                                          IMarketMappingProvider mappingProvider,
                                                          oddsChangeMarketOutcome outcome,
                                                          IReadOnlyCollection<CultureInfo> cultures,
                                                          IOutcomeDefinition outcomeDefinition)
        {
            var isValidPlayerOutcome = false;
            IMatch match = null;

            if (outcome.teamSpecified)
            {
                match = sportEvent as IMatch;
                isValidPlayerOutcome = !(match == null || outcome.team < 1 || outcome.team > 2);
            }

            if (isValidPlayerOutcome)
            {
                return new PlayerOutcomeOdds(outcome.id,
                                             outcome.activeSpecified ? (bool?)(outcome.active != 0) : null,
                                             outcome.odds,
                                             outcome.probabilitiesSpecified ? (double?)outcome.probabilities : null,
                                             nameProvider,
                                             mappingProvider,
                                             match,
                                             outcome.team,
                                             cultures,
                                             outcomeDefinition,
                                             GetAdditionalProbabilities(outcome));
            }

            return new OutcomeOdds(outcome.id,
                                   outcome.activeSpecified ? (bool?)(outcome.active != 0) : null,
                                   outcome.odds,
                                   outcome.probabilitiesSpecified ? (double?)outcome.probabilities : null,
                                   nameProvider,
                                   mappingProvider,
                                   cultures,
                                   outcomeDefinition,
                                   GetAdditionalProbabilities(outcome));
        }

        private IAdditionalProbabilities GetAdditionalProbabilities(oddsChangeMarketOutcome outcome)
        {
            if (!outcome.win_probabilitiesSpecified
                && !outcome.lose_probabilitiesSpecified
                && !outcome.half_win_probabilitiesSpecified
                && !outcome.half_lose_probabilitiesSpecified
                && !outcome.refund_probabilitiesSpecified)
            {
                return null;
            }

            return new AdditionalProbabilities(
                outcome.win_probabilitiesSpecified ? outcome.win_probabilities : (double?)null,
                outcome.lose_probabilitiesSpecified ? outcome.lose_probabilities : (double?)null,
                outcome.half_win_probabilitiesSpecified ? outcome.half_win_probabilities : (double?)null,
                outcome.half_lose_probabilitiesSpecified ? outcome.half_lose_probabilities : (double?)null,
                outcome.refund_probabilitiesSpecified ? outcome.refund_probabilities : (double?)null);
        }

        private IMarketMetadata GetMarketMetadata(marketMetadata marketMetadata)
        {
            if (marketMetadata == null ||
                (!marketMetadata.aams_idSpecified
              && !marketMetadata.start_timeSpecified
              && !marketMetadata.end_timeSpecified
              && !marketMetadata.next_betstopSpecified))
            {
                return null;
            }

            return new MarketMetadata(marketMetadata);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="alive"/> instance to the <see cref="IAlive"/> instance
        /// </summary>
        /// <param name="message">A <see cref="alive"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IAlive"/> instance constructed from information in the provided <see cref="alive"/></returns>
        public IAlive MapAlive(alive message)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            return new Alive(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)), _producerManager.Get(message.product), message.subscribed != 0);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="snapshot_complete"/> instance to the <see cref="ISnapshotCompleted"/> instance
        /// </summary>
        /// <param name="message">A <see cref="snapshot_complete"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="ISnapshotCompleted"/> instance constructed from information in the provided <see cref="snapshot_complete"/></returns>
        public ISnapshotCompleted MapSnapShotCompleted(snapshot_complete message)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            return new SnapshotCompleted(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)), _producerManager.Get(message.product), message.request_id);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="fixture_change" /> instance to the <see cref="IFixtureChange{T}" /> instance
        /// </summary>
        /// <param name="message">A <see cref="fixture_change" /> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IFixtureChange{T}" /> instance constructed from information in the provided <see cref="fixture_change" /></returns>
        public IFixtureChange<T> MapFixtureChange<T>(fixture_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new FixtureChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                        _producerManager.Get(message.product),
                                        GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                        message.request_idSpecified ? (long?)message.request_id : null,
                                        MessageMapperHelper.GetEnumValue(message.change_typeSpecified, message.change_type, FixtureChangeType.OTHER, FixtureChangeType.NA),
                                        message.next_live_timeSpecified ? (long?)message.next_live_time : null,
                                        message.start_time,
                                        rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_stop" /> instance to the <see cref="IBetStop{T}" /> instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">A <see cref="bet_stop" /> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetStop{T}" /> instance constructed from information in the provided <see cref="bet_stop" /></returns>
        public IBetStop<T> MapBetStop<T>(bet_stop message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetStop<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                  _producerManager.Get(message.product),
                                  GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                  message.request_idSpecified ? (long?)message.request_id : null,
                                  MessageMapperHelper.GetEnumValue(message.market_statusSpecified, message.market_status, MarketStatus.SUSPENDED),
                                  message.groups?.Split('|'),
                                  rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_cancel"/> instance to the <see cref="IBetCancel{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="bet_cancel"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetCancel{T}"/> instance constructed from information in the provided <see cref="bet_cancel"/></returns>
        public IBetCancel<T> MapBetCancel<T>(bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetCancel<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                    _producerManager.Get(message.product),
                                    GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                    message.request_idSpecified ? (long?)message.request_id : null,
                                    message.start_timeSpecified ? (long?)message.start_time : null,
                                    message.end_timeSpecified ? (long?)message.end_time : null,
                                    string.IsNullOrEmpty(message.superceded_by) ? null : URN.Parse(message.superceded_by),
                                    message.market.Select(m => GetMarketCancel(GetEventForNameProvider<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                                                              m, message.ProducerId, message.SportId, culturesList)),
                                    rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="rollback_bet_cancel"/> instance to the <see cref="IRollbackBetCancel{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="rollback_bet_cancel"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IRollbackBetCancel{T}"/> instance constructed from information in the provided <see cref="rollback_bet_cancel"/></returns>
        public IRollbackBetCancel<T> MapRollbackBetCancel<T>(rollback_bet_cancel message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new RollbackBetCancel<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt,
                                                                 SdkInfo.ToEpochTime(DateTime.Now)),
                                            _producerManager.Get(message.product),
                                            GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                            message.request_idSpecified ? (long?)message.request_id : null,
                                            message.start_timeSpecified ? (long?)message.start_time : null,
                                            message.end_timeSpecified ? (long?)message.end_time : null,
                                            message.market.Select(m => GetMarketCancel(GetEventForNameProvider<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                                                                      m, message.ProducerId, message.SportId, culturesList)),
                                            rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="bet_settlement"/> instance to the <see cref="IBetSettlement{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="bet_settlement"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IBetSettlement{T}"/> instance constructed from information in the provided <see cref="bet_settlement"/></returns>
        public IBetSettlement<T> MapBetSettlement<T>(bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetSettlement<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                 _producerManager.Get(message.product),
                                 GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                 message.request_idSpecified ? (long?)message.request_id : null,
                                 message.outcomes.Select(m => GetMarketWithResults(GetEventForNameProvider<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                                                                  m, message.ProducerId, message.SportId, culturesList)),
                                 message.certainty,
                                 rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="rollback_bet_settlement"/> instance to the <see cref="IRollbackBetSettlement{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="rollback_bet_settlement"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IRollbackBetSettlement{T}"/> instance constructed from information in the provided <see cref="rollback_bet_settlement"/></returns>
        public IRollbackBetSettlement<T> MapRollbackBetSettlement<T>(rollback_bet_settlement message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new RollbackBetSettlement<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                _producerManager.Get(message.product),
                GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                message.request_idSpecified ? (long?)message.request_id : null,
                message.market.Select(m => GetMarketCancel(GetEventForNameProvider<T>(URN.Parse(message.event_id), message.SportId, culturesList), m, message.ProducerId, message.SportId, culturesList)),
                rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="odds_change"/> instance to the <see cref="IOddsChange{T}"/> instance
        /// </summary>
        /// <param name="message">A <see cref="odds_change"/> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="IOddsChange{T}"/> instance constructed from information in the provided <see cref="odds_change"/></returns>
        public IOddsChange<T> MapOddsChange<T>(odds_change message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            var markets = message.odds?.market?.Select(m => GetMarketWithOdds(GetEventForNameProvider<T>(message.EventURN, message.SportId, culturesList), m, message.ProducerId, message.SportId, culturesList)).ToList();

            return new OddsChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                _producerManager.Get(message.product),
                GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                message.request_idSpecified ? (long?)message.request_id : null,
                MessageMapperHelper.GetEnumValue(message.odds_change_reasonSpecified, message.odds_change_reason, OddsChangeReason.NORMAL),
                message.odds?.betstop_reasonSpecified == true ? (int?)message.odds.betstop_reason : null,
                message.odds?.betting_statusSpecified == true ? (int?)message.odds.betting_status : null,
                markets,
                message.odds_generation_properties,
                _namedValuesProvider,
                rawMessage);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="cashout" /> instance to the <see cref="ICashOutProbabilities{T}" /> instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">A <see cref="cashout" /> instance to be mapped (converted)</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="rawMessage">The raw message</param>
        /// <returns>A <see cref="ICashOutProbabilities{T}" /> instance constructed from information in the provided <see cref="cashout" /></returns>
        public ICashOutProbabilities<T> MapCashOutProbabilities<T>(cashout message, IEnumerable<CultureInfo> cultures, byte[] rawMessage) where T : ISportEvent
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();
            var eventId = URN.Parse(message.event_id);
            var sportId = URN.Parse("sr:sport:1");

            var epochTime = SdkInfo.ToEpochTime(DateTime.Now);
            return new CashOutProbabilities<T>(
                new MessageTimestamp(message.GeneratedAt, message.SentAt, epochTime, epochTime),
                _producerManager.Get(message.product),
                GetEventForMessage<T>(eventId, sportId, culturesList),
                message.odds?.betstop_reasonSpecified == true ? (int?)message.odds.betstop_reason : null,
                message.odds?.betting_statusSpecified == true ? (int?)message.odds.betting_status : null,
                message.odds?.market?.Select(m => GetMarketWithProbabilities(GetEventForNameProvider<T>(eventId, sportId, culturesList), m, message.product, sportId, culturesList)).ToList(),
                _namedValuesProvider,
                rawMessage);
        }
    }
}
