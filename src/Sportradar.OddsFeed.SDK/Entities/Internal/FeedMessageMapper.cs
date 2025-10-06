// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Enums;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using cashout = Sportradar.OddsFeed.SDK.Messages.Rest.cashout;

namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    /// <summary>
    /// A <see cref="IFeedMessageMapper"/> implementation which maps feed messages to non-specific feed messages
    /// </summary>
    internal class FeedMessageMapper : IFeedMessageMapper
    {
        private readonly ISportEntityFactory _sportEntityFactory;
        private readonly IMarketFactory _marketFactory;
        private readonly IProducerManager _producerManager;
        private readonly INamedValuesProvider _namedValuesProvider;
        private readonly ExceptionHandlingStrategy _externalExceptionStrategy;

        public FeedMessageMapper(ISportEntityFactory sportEntityFactory,
                                 IMarketFactory marketFactory,
                                 IProducerManager producerManager,
                                 INamedValuesProvider namedValuesProvider,
                                 ExceptionHandlingStrategy externalExceptionStrategy)
        {
            _ = Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            _ = Guard.Argument(marketFactory, nameof(marketFactory)).NotNull();
            _ = Guard.Argument(producerManager, nameof(producerManager)).NotNull();
            _ = Guard.Argument(namedValuesProvider, nameof(namedValuesProvider)).NotNull();

            _sportEntityFactory = sportEntityFactory;
            _marketFactory = marketFactory;
            _producerManager = producerManager;
            _namedValuesProvider = namedValuesProvider;
            _externalExceptionStrategy = externalExceptionStrategy;
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="alive"/> instance to the <see cref="IAlive"/> instance
        /// </summary>
        /// <param name="message">A <see cref="alive"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IAlive"/> instance constructed from information in the provided <see cref="alive"/></returns>
        public IAlive MapAlive(alive message)
        {
            _ = Guard.Argument(message, nameof(message)).NotNull();

            return new Alive(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)), _producerManager.GetProducer(message.product), message.subscribed != 0);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="snapshot_complete"/> instance to the <see cref="ISnapshotCompleted"/> instance
        /// </summary>
        /// <param name="message">A <see cref="snapshot_complete"/> instance to be mapped (converted)</param>
        /// <returns>A <see cref="ISnapshotCompleted"/> instance constructed from information in the provided <see cref="snapshot_complete"/></returns>
        public ISnapshotCompleted MapSnapShotCompleted(snapshot_complete message)
        {
            _ = Guard.Argument(message, nameof(message)).NotNull();

            return new SnapshotCompleted(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)), _producerManager.GetProducer(message.product), message.request_id);
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new FixtureChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                        _producerManager.GetProducer(message.product),
                                        GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                        message.request_idSpecified ? (long?)message.request_id : null,
                                        MessageMapperHelper.GetEnumValue(message.change_typeSpecified, message.change_type, FixtureChangeType.Other, FixtureChangeType.NotAvailable),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetStop<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                  _producerManager.GetProducer(message.product),
                                  GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                  message.request_idSpecified ? (long?)message.request_id : null,
                                  MessageMapperHelper.GetEnumValue(message.market_statusSpecified, message.market_status, MarketStatus.Suspended),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetCancel<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                    _producerManager.GetProducer(message.product),
                                    GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                    message.request_idSpecified ? (long?)message.request_id : null,
                                    message.start_timeSpecified ? (long?)message.start_time : null,
                                    message.end_timeSpecified ? (long?)message.end_time : null,
                                    string.IsNullOrEmpty(message.superceded_by) ? null : Urn.Parse(message.superceded_by),
                                    message.market.Select(m => _marketFactory.GetMarketCancel(GetEventForNameProvider<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                                                                              m,
                                                                                              message.ProducerId,
                                                                                              message.SportId,
                                                                                              culturesList)),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new RollbackBetCancel<T>(new MessageTimestamp(message.GeneratedAt,
                                                                 message.SentAt,
                                                                 message.ReceivedAt,
                                                                 SdkInfo.ToEpochTime(DateTime.Now)),
                                            _producerManager.GetProducer(message.product),
                                            GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                            message.request_idSpecified ? (long?)message.request_id : null,
                                            message.start_timeSpecified ? (long?)message.start_time : null,
                                            message.end_timeSpecified ? (long?)message.end_time : null,
                                            message.market.Select(m => _marketFactory.GetMarketCancel(GetEventForNameProvider<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                                                                                      m,
                                                                                                      message.ProducerId,
                                                                                                      message.SportId,
                                                                                                      culturesList)),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new BetSettlement<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                        _producerManager.GetProducer(message.product),
                                        GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                        message.request_idSpecified ? (long?)message.request_id : null,
                                        message.outcomes.Select(m => _marketFactory.GetMarketWithResults(GetEventForNameProvider<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                                                                                         m,
                                                                                                         message.ProducerId,
                                                                                                         message.SportId,
                                                                                                         culturesList)),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            return new RollbackBetSettlement<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                                _producerManager.GetProducer(message.product),
                                                GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                                message.request_idSpecified ? (long?)message.request_id : null,
                                                message.market.Select(m => _marketFactory.GetMarketCancel(GetEventForNameProvider<T>(Urn.Parse(message.event_id), message.SportId, culturesList), m, message.ProducerId, message.SportId, culturesList)),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();

            var sportEvent = GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList);

            var markets = message.odds?.market?.Select(m => _marketFactory.GetMarketWithOdds(sportEvent, m, message.ProducerId, message.SportId, culturesList)).ToList();

            return new OddsChange<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, message.ReceivedAt, SdkInfo.ToEpochTime(DateTime.Now)),
                                     _producerManager.GetProducer(message.product),
                                     GetEventForMessage<T>(Urn.Parse(message.event_id), message.SportId, culturesList),
                                     message.request_idSpecified ? (long?)message.request_id : null,
                                     MessageMapperHelper.GetEnumValue(message.odds_change_reasonSpecified, message.odds_change_reason, OddsChangeReason.Normal),
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
            _ = Guard.Argument(message, nameof(message)).NotNull();

            var culturesList = cultures as List<CultureInfo> ?? cultures.ToList();
            var eventId = Urn.Parse(message.event_id);
            var sportId = Urn.Parse("sr:sport:1");

            var epochTime = SdkInfo.ToEpochTime(DateTime.Now);
            return new CashOutProbabilities<T>(new MessageTimestamp(message.GeneratedAt, message.SentAt, epochTime, epochTime),
                                               _producerManager.GetProducer(message.product),
                                               GetEventForMessage<T>(eventId, sportId, culturesList),
                                               message.odds?.betstop_reasonSpecified == true ? (int?)message.odds.betstop_reason : null,
                                               message.odds?.betting_statusSpecified == true ? (int?)message.odds.betting_status : null,
                                               message.odds?.market?.Select(m => _marketFactory.GetMarketWithProbabilities(GetEventForNameProvider<T>(eventId, sportId, culturesList), m, message.product, sportId, culturesList)).ToList(),
                                               _namedValuesProvider,
                                               rawMessage);
        }

        /// <summary>
        /// Builds and returns a <see cref="ISportEvent"/> derived instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the constructed instance should handle potential exceptions</param>
        /// <returns>A <see cref="ISportEvent"/> derived constructed instance</returns>
        private T BuildEvent<T>(Urn eventId, Urn sportId, List<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent
        {
            _ = Guard.Argument(eventId, nameof(eventId)).NotNull();
            _ = Guard.Argument(sportId, nameof(sportId)).NotNull();
            _ = Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            T entity;
            switch (eventId.TypeGroup)
            {
                case ResourceTypeGroup.Tournament:
                case ResourceTypeGroup.BasicTournament:
                case ResourceTypeGroup.Season:
                case ResourceTypeGroup.Match:
                case ResourceTypeGroup.Stage:
                case ResourceTypeGroup.Draw:
                case ResourceTypeGroup.Lottery:
                case ResourceTypeGroup.Unknown:
                    {
                        entity = (T)_sportEntityFactory.BuildSportEvent<ISportEvent>(eventId, sportId, cultures, exceptionStrategy);
                        break;
                    }
                case ResourceTypeGroup.Other:
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
        private T GetEventForMessage<T>(Urn eventId, Urn sportId, List<CultureInfo> cultures) where T : ISportEvent
        {
            _ = Guard.Argument(eventId, nameof(eventId)).NotNull();
            _ = Guard.Argument(sportId, nameof(sportId)).NotNull();
            _ = Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            return BuildEvent<T>(eventId, sportId, cultures, _externalExceptionStrategy);
        }

        /// <summary>
        /// Gets the new <see cref="ICompetition"/> instance
        /// </summary>
        /// <param name="eventId">A <see cref="string"/> representation of the event id</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}" /> specifying the languages to which the mapped message will be translated</param>
        /// <returns>Returns the new <see cref="ICompetition"/> instance</returns>
        private T GetEventForNameProvider<T>(Urn eventId, Urn sportId, List<CultureInfo> cultures) where T : ISportEvent
        {
            _ = Guard.Argument(eventId, nameof(eventId)).NotNull();
            _ = Guard.Argument(sportId, nameof(sportId)).NotNull();
            _ = Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            return BuildEvent<T>(eventId, sportId, cultures, _externalExceptionStrategy);
        }
    }
}
