/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Entities.REST.Market;
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
        /// The <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(FeedMessageMapper));

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
        public FeedMessageMapper(ISportEntityFactory sportEntityFactory,
                                 INameProviderFactory nameProviderFactory,
                                 IMarketMappingProviderFactory mappingProviderFactory,
                                 INamedValuesProvider namedValuesProvider,
                                 ExceptionHandlingStrategy externalExceptionStrategy,
                                 IProducerManager producerManager,
                                 IMarketCacheProvider marketCacheProvider,
                                 INamedValueCache voidReasonCache)
        {
            Contract.Requires(sportEntityFactory != null);
            Contract.Requires(nameProviderFactory != null);
            Contract.Requires(namedValuesProvider != null);
            Contract.Requires(producerManager != null);
            Contract.Requires(marketCacheProvider != null);
            Contract.Requires(voidReasonCache != null);

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
        /// Specifies invariants as needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariants()
        {
            Contract.Invariant(_sportEntityFactory != null);
            Contract.Invariant(_nameProviderFactory != null);
            Contract.Invariant(_namedValuesProvider != null);
            Contract.Invariant(_producerManager != null);
            Contract.Invariant(_marketCacheProvider != null);
            Contract.Invariant(_voidReasonCache != null);
        }

        /// <summary>
        /// Builds and returns a <see cref="ISportEvent"/> derived instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the constructed instance should handle potential exceptions</param>
        /// <returns>A <see cref="ISportEvent"/> derived constructed instance</returns>
        protected T BuildEvent<T>(URN eventId, URN sportId, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            Contract.Requires(!string.IsNullOrEmpty(eventId.ToString()));
            Contract.Requires(sportId != null);
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Ensures(Contract.Result<T>() != null);

            var cultureInfos = cultures as CultureInfo[] ?? cultures.ToArray();

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
                    entity = (T) _sportEntityFactory.BuildSportEvent<ISportEvent>(eventId, sportId, cultureInfos, exceptionStrategy);
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
        protected T GetEventForMessage<T>(URN eventId, URN sportId, IEnumerable<CultureInfo> cultures) where T : ISportEvent
        {
            Contract.Requires(!string.IsNullOrEmpty(eventId.ToString()));
            Contract.Requires(sportId != null);
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Ensures(Contract.Result<T>() != null);

            return BuildEvent<T>(eventId, sportId, cultures, _externalExceptionStrategy);
        }

        /// <summary>
        /// Gets the new <see cref="ICompetition"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <returns>Returns the new <see cref="ICompetition"/> instance</returns>
        protected T GetEventForNameProvider<T>(URN eventId, URN sportId, IEnumerable<CultureInfo> cultures) where T : ISportEvent
        {
            Contract.Requires(!string.IsNullOrEmpty(eventId.ToString()));
            Contract.Requires(sportId != null);
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Ensures(Contract.Result<T>() != null);

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
            Contract.Requires(sportEvent != null, "SportEvent missing");
            Contract.Requires(marketSettlement != null, "marketSettlement != null");
            Contract.Ensures(Contract.Result<IMarketWithSettlement>() != null, "Contract.Result<IMarketWithSettlement>() != null");

            var cultureInfos = cultures.ToList();

            var specifiers = string.IsNullOrEmpty(marketSettlement.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.specifiers);

            var additionalInfo = string.IsNullOrEmpty(marketSettlement.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(marketSettlement.extended_specifiers);

            var producer = _producerManager.Get(producerId);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketSettlement.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketSettlement.id, specifiers, _producerManager.Get(producerId), sportId);
            var outcomes = marketSettlement.Items.Select(outcome => new OutcomeSettlement(
                                                                            outcome.dead_heat_factorSpecified
                                                                                ? (double?)outcome.dead_heat_factor
                                                                                : null,
                                                                            outcome.id,
                                                                            outcome.result != 0,
                                                                            MessageMapperHelper.GetVoidFactor(outcome.void_factorSpecified, outcome.void_factor),
                                                                            nameProvider,
                                                                            mappingProvider,
                                                                            cultureInfos,
                                                                                          BuildOutcomeDefinition(marketSettlement.id, sportId, producer, specifiers, outcome.id, cultureInfos)));

            return new MarketWithSettlement(marketSettlement.id,
                                            specifiers,
                                            additionalInfo,
                                            outcomes,
                                            nameProvider,
                                            mappingProvider,
                                            BuildMarketDefinition(marketSettlement.id, sportId, producer, specifiers, cultureInfos),
                                            marketSettlement.void_reasonSpecified ? (int?)marketSettlement.void_reason : null,
                                            _namedValuesProvider.VoidReasons,
                                            cultureInfos);
        }

        /// <summary>
        /// Gets the new <see cref="IMarketWithProbabilities"/> instance
        /// </summary>
        /// <param name="sportEvent">A <see cref="ISportEvent"/> representing the sport event associated with the generated message</param>
        /// <param name="marketOddsChange">The <see cref="oddsChangeMarket"/> instance used</param>
        /// <param name="producerId">A producerId specifying message producer</param>
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

            var producer = _producerManager.Get(producerId);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, _producerManager.Get(producerId), sportId);
            return new MarketWithProbabilities(marketOddsChange.id,
                                               specifiers,
                                               additionalInfo,
                                               nameProvider,
                                               mappingProvider,
                                               MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                                               marketOddsChange.outcome?.Select(outcomeOdds => new OutcomeProbabilities(
                                                                                                                        outcomeOdds.id,
                                                                                                                        outcomeOdds.activeSpecified
                                                                                                                            ? (bool?) (outcomeOdds.active != 0)
                                                                                                                            : null,
                                                                                                                        outcomeOdds.probabilitiesSpecified
                                                                                                                            ? (double?) outcomeOdds.probabilities
                                                                                                                            : null,
                                                                                                                        nameProvider,
                                                                                                                        mappingProvider,
                                                                                                                        cultureInfos,
                                                                                                                        BuildOutcomeDefinition(marketOddsChange.id, sportId, producer, specifiers, outcomeOdds.id, cultureInfos))),
                                               BuildMarketDefinition(marketOddsChange.id, sportId, producer, specifiers, cultureInfos),
                                               cultureInfos);
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

            var marketMetadata = marketOddsChange.market_metadata == null
                             || !marketOddsChange.market_metadata.next_betstopSpecified
                ? new MarketMetadata(null)
                : new MarketMetadata(marketOddsChange.market_metadata.next_betstop);

            var producer = _producerManager.Get(producerId);

            var nameProvider = _nameProviderFactory.BuildNameProvider(sportEvent, marketOddsChange.id, specifiers);
            var mappingProvider = _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, marketOddsChange.id, specifiers, _producerManager.Get(producerId), sportId);

            return new MarketWithOdds(marketOddsChange.id,
                                    specifiers,
                                    additionalInfo,
                                    nameProvider,
                                    mappingProvider,
                                    MessageMapperHelper.GetEnumValue<MarketStatus>(marketOddsChange.status),
                                    marketOddsChange.cashout_statusSpecified ? (CashoutStatus?)MessageMapperHelper.GetEnumValue<CashoutStatus>(marketOddsChange.cashout_status) : null,
                                    marketOddsChange.favouriteSpecified && marketOddsChange.favourite == 1,
                                    marketOddsChange.outcome?.Select(o => GetOutcomeWithOdds(sportEvent, nameProvider, mappingProvider, o, cultureInfos, BuildOutcomeDefinition(marketOddsChange.id, sportId, producer, specifiers, o.id, cultureInfos))),
                                    marketMetadata,
                                    BuildMarketDefinition(marketOddsChange.id, sportId, producer, specifiers, cultureInfos),
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
            Contract.Requires(sportEvent != null);
            Contract.Requires(market != null);
            Contract.Ensures(Contract.Result<IMarket>() != null);


            var specifiers = string.IsNullOrEmpty(market.specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.specifiers);

            var additionalInfo = string.IsNullOrEmpty(market.extended_specifiers)
                ? null
                : FeedMapperHelper.GetSpecifiers(market.extended_specifiers);

            var producer = _producerManager.Get(producerId);

            var cultureInfos = cultures.ToList();
            return new MarketCancel(market.id,
                                  specifiers,
                                  additionalInfo,
                                  _nameProviderFactory.BuildNameProvider(sportEvent, market.id, specifiers),
                                  _mappingProviderFactory.BuildMarketMappingProvider(sportEvent, market.id, specifiers, _producerManager.Get(producerId), sportId),
                                  BuildMarketDefinition(market.id, sportId, producer, specifiers, cultureInfos),
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
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying languages the current instance supports</param>
        /// <param name="outcomeDefinition">The associated <see cref="IOutcomeDefinition"/></param>
        /// <returns>IOutcomeOdds</returns>
        protected virtual IOutcomeOdds GetOutcomeWithOdds(ISportEvent sportEvent, INameProvider nameProvider, IMarketMappingProvider mappingProvider, oddsChangeMarketOutcome outcome, IEnumerable<CultureInfo> cultures, IOutcomeDefinition outcomeDefinition)
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
                                            outcome.probabilitiesSpecified
                                                ? (double?)outcome.probabilities
                                                : null,
                                            nameProvider,
                                            mappingProvider,
                                            match,
                                            outcome.team,
                                            cultures,
                                            outcomeDefinition);
            }

            return new OutcomeOdds(outcome.id,
                                    outcome.activeSpecified ? (bool?)(outcome.active != 0) : null,
                                    outcome.odds,
                                    outcome.probabilitiesSpecified
                                        ? (double?)outcome.probabilities
                                        : null,
                                    nameProvider,
                                    mappingProvider,
                                    cultures,
                                    outcomeDefinition);
        }


        /// <summary>
        /// Maps (converts) the provided <see cref="alive"/> instance to the <see cref="IAlive"/> instance
        /// </summary>
        /// <param name="message">A <see cref="alive"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IAlive"/> instance constructed from information in the provided <see cref="alive"/></returns>
        public IAlive MapAlive(alive message)
        {
            return new Alive(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)), _producerManager.Get(message.product), message.subscribed != 0);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="snapshot_complete"/> instance to the <see cref="ISnapshotCompleted"/> instance
        /// </summary>
        /// <param name="message">A <see cref="snapshot_complete"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="ISnapshotCompleted"/> instance constructed from information in the provided <see cref="snapshot_complete"/></returns>
        public ISnapshotCompleted MapSnapShotCompleted(snapshot_complete message)
        {
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
            return new FixtureChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                        _producerManager.Get(message.product),
                                        GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, cultures),
                                        message.request_idSpecified ? (long?) message.request_id : null,
                                        MessageMapperHelper.GetEnumValue(message.change_typeSpecified, message.change_type, FixtureChangeType.OTHER),
                                        message.next_live_timeSpecified ? (long?) message.next_live_time : null,
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
            return new BetStop<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                  _producerManager.Get(message.product),
                                  GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, cultures),
                                  message.request_idSpecified ? (long?) message.request_id : null,
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
            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetCancel<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                    _producerManager.Get(message.product),
                                    GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                    message.request_idSpecified ? (long?) message.request_id : null,
                                    message.start_timeSpecified ? (long?) message.start_time : null,
                                    message.end_timeSpecified ? (long?) message.end_time : null,
                                    string.IsNullOrEmpty(message.superceded_by)
                                        ? null
                                        : URN.Parse(message.superceded_by),
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
            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new RollbackBetCancel<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt,
                                                                 SdkInfo.ToEpochTime(DateTime.Now)),
                                            _producerManager.Get(message.product),
                                            GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                            message.request_idSpecified ? (long?) message.request_id : null,
                                            message.start_timeSpecified ? (long?) message.start_time : null,
                                            message.end_timeSpecified ? (long?) message.end_time : null,
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
            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetSettlement<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                 _producerManager.Get(message.product),
                                 GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                                 message.request_idSpecified ? (long?) message.request_id : null,
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
            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            var markets = message.odds?.market?.Select(m => GetMarketWithOdds(GetEventForNameProvider<T>(message.EventURN, message.SportId, culturesList), m, message.ProducerId, message.SportId, culturesList)).ToList();

            return new OddsChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                _producerManager.Get(message.product),
                GetEventForMessage<T>(URN.Parse(message.event_id), message.SportId, culturesList),
                message.request_idSpecified
                    ? (long?)message.request_id
                    : null,
                MessageMapperHelper.GetEnumValue(message.odds_change_reasonSpecified, message.odds_change_reason, OddsChangeReason.NORMAL),
                message.odds != null && message.odds.betstop_reasonSpecified
                    ? (int?)message.odds.betstop_reason
                    : null,
                message.odds != null && message.odds.betting_statusSpecified
                    ? (int?)message.odds.betting_status
                    : null,
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
            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();
            var eventId = URN.Parse(message.event_id);
            var sportId = URN.Parse("sr:sport:1");

            return new CashOutProbabilities<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                _producerManager.Get(message.product),
                GetEventForMessage<T>(eventId, sportId, culturesList),
                message.odds != null && message.odds.betstop_reasonSpecified
                    ? (int?)message.odds.betstop_reason
                    : null,
                message.odds != null && message.odds.betting_statusSpecified
                    ? (int?)message.odds.betting_status
                    : null,
                message.odds?.market?.Select(m => GetMarketWithProbabilities(GetEventForNameProvider<T>(eventId, sportId, culturesList), m, message.product, sportId, culturesList)).ToList(),
                _namedValuesProvider,
                rawMessage);
        }

        /// <summary>
        /// Builds a <see cref="IMarketDefinition" /> associated with the provided data
        /// </summary>
        /// <param name="marketId">the id of the market</param>
        /// <param name="sportId">the sport id</param>
        /// <param name="producer">the source of the market</param>
        /// <param name="specifiers">the received market specifiers</param>
        /// <param name="cultures">the cultures in which the market data should be prefetched</param>
        /// <returns></returns>
        private IMarketDefinition BuildMarketDefinition(int marketId, URN sportId, IProducer producer, IReadOnlyDictionary<string, string> specifiers, IEnumerable<CultureInfo> cultures)
        {
            // market descriptor should exist or else the market generation would fail
            // variant always null because we are interested only in the general market attributes
            IMarketDescription marketDescription = null;

            try
            {
                marketDescription = _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, cultures, false).Result;
            }
            catch (CacheItemNotFoundException)
            {
                ExecutionLog.Warn($"Market description for market={marketId}, sport={sportId} and producer={producer.Name} not found.");
                if (_externalExceptionStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }

            return marketDescription == null
                ? null
                : new MarketDefinition(marketDescription, _marketCacheProvider, sportId, producer, specifiers);
        }

        /// <summary>
        /// Builds a <see cref="IOutcomeDefinition" /> associated with the provided data
        /// </summary>
        /// <param name="marketId">the id of the market</param>
        /// <param name="sportId">the sport id</param>
        /// <param name="producer">the source of the market</param>
        /// <param name="specifiers">the received market specifiers</param>
        /// <param name="outcomeId">the id of the market</param>
        /// <param name="cultures">the cultures in which the market data should be prefetched</param>
        /// <returns></returns>
        private IOutcomeDefinition BuildOutcomeDefinition(int marketId, URN sportId, IProducer producer, IReadOnlyDictionary<string, string> specifiers, string outcomeId, IEnumerable<CultureInfo> cultures)
        {
            try
            {
                var cultureInfos = cultures.ToList();
                var marketDescription = _marketCacheProvider.GetMarketDescriptionAsync(marketId, specifiers, cultureInfos, false).Result;
                if (marketDescription == null)
                {
                    throw new CacheItemNotFoundException($"Market description for market={marketId}, sport={sportId} and producer={producer.Name} not found.", $"MarketId={marketId}", null);
                }
                if (marketDescription.Outcomes == null)
                {
                    return new OutcomeDefinition(marketDescription, outcomeId, _marketCacheProvider, specifiers, cultureInfos, _externalExceptionStrategy);
                }
                var outcomeDescription = marketDescription.Outcomes.FirstOrDefault(s => s.Id.Equals(outcomeId, StringComparison.InvariantCultureIgnoreCase));
                return outcomeDescription == null
                       ? new OutcomeDefinition(marketDescription, outcomeId, _marketCacheProvider, specifiers, cultureInfos, _externalExceptionStrategy)
                       : new OutcomeDefinition(marketDescription, outcomeDescription, cultureInfos);
            }
            catch (CacheItemNotFoundException)
            {
                ExecutionLog.Debug($"Market description for market={marketId}, sport={sportId} and producer={producer.Name} not found. Outcome definition not found (yet)");
            }
            return null;
        }
    }
}