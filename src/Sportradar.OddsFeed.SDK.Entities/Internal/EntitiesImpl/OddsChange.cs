/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed indicating that odds of the betting market selections have changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OddsChange<T> : MarketMessage<IMarketWithOdds, T>, IOddsChangeV1<T> where T : ISportEvent
    {
        /// <summary>
        /// The id of the betting status or a null reference if betting status is not specified
        /// </summary>
        private readonly int? _bettingStatus;

        /// <summary>
        /// The id of the bet stop reason or a null reference if bet stop reason is not specified
        /// </summary>
        private readonly int? _betStopReason;

        /// <summary>
        /// The <see cref="INamedValuesProvider"/> used to provide names for betting status and bet stop reason
        /// </summary>
        private readonly INamedValuesProvider _namedValueProvider;

        /// <summary>
        ///     Gets a <see cref="OddsChangeReason" /> member specifying the reason for the odds change
        /// </summary>
        public OddsChangeReason? ChangeReason { get; }

        /// <summary>
        /// Gets the <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.INamedValue" /> specifying the reason for betting being stopped, or a null reference if the reason is not known
        /// </summary>
        /// <value>The bet stop reason.</value>
        public INamedValue BetStopReason => _betStopReason == null
            ? null
            : _namedValueProvider.BetStopReasons.GetNamedValue(_betStopReason.Value);


        /// <summary>
        /// Gets a <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.INamedValue" /> indicating the odds change was triggered by a possible event
        /// </summary>
        /// <value>The betting status</value>
        public INamedValue BettingStatus => _bettingStatus == null
            ? null
            : _namedValueProvider.BettingStatuses.GetNamedValue(_bettingStatus.Value);

        /// <summary>
        /// Gets the odds generation properties (contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it)
        /// </summary>
        /// <value>The odds generation properties</value>
        public IOddsGeneration OddsGenerationProperties { get; }

        /// <summary>Initializes a new instance of the <see cref="OddsChange{T}" /> class</summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="changeReason">a <see cref="EventMessage{T}" /> member specifying the reason for the odds change</param>
        /// <param name="betStopReason">the <see cref="BetStopReason" /> enum member specifying the reason for betting being stopped, or a null reference if the reason is not known</param>
        /// <param name="bettingStatus">a <see cref="BettingStatus"/> enum indicating whether the betting should be allowed</param>
        /// <param name="markets">An <see cref="IMarketMessage{T,T1}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="oddsGenerationProperties">Provided by the prematch odds producer only, and contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it</param>
        /// <param name="namedValuesProvider">The <see cref="INamedValuesProvider"/> used to provide names for betting status and bet stop reason</param>
        /// <param name="rawMessage">The raw message</param>
        public OddsChange(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, OddsChangeReason? changeReason, int? betStopReason, int? bettingStatus, IEnumerable<IMarketWithOdds> markets, oddsGenerationProperties oddsGenerationProperties, INamedValuesProvider namedValuesProvider, byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, markets, rawMessage)
        {
            Contract.Requires(namedValuesProvider != null);

            _namedValueProvider = namedValuesProvider;
            ChangeReason = changeReason;
            _betStopReason = betStopReason;
            _bettingStatus = bettingStatus;
            OddsGenerationProperties = oddsGenerationProperties == null
                                           ? null
                                           : new OddsGeneration(oddsGenerationProperties);
        }
    }
}