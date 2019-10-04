/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed indicating that odds of the betting market selections have changed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CashOutProbabilities<T> : MarketMessage<IMarketWithProbabilities, T>, ICashOutProbabilities<T> where T : ISportEvent
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
        /// Gets the <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.INamedValue" /> specifying the reason for betting being stopped, or a null reference if the reason is not known
        /// </summary>
        /// <value>The bet stop reason.</value>
        public INamedValue BetStopReason => _betStopReason == null
            ? null
            : _namedValueProvider.BetStopReasons.GetNamedValue(_betStopReason.Value);


        /// <summary>
        /// Gets a <see cref="T:Sportradar.OddsFeed.SDK.Entities.REST.INamedValue" /> indicating the odds change was triggered by a possible event
        /// </summary>
        /// <value>The betting status.</value>
        public INamedValue BettingStatus => _bettingStatus == null
            ? null
            : _namedValueProvider.BettingStatuses.GetNamedValue(_bettingStatus.Value);

        /// <summary>Initializes a new instance of the <see cref="OddsChange{T}" /> class</summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="betStopReason">The <see cref="BetStopReason" /> enum member specifying the reason for betting being stopped, or a null reference if the reason is not known</param>
        /// <param name="bettingStatus">A <see cref="BettingStatus"/> enum indicating whether the betting should be allowed</param>
        /// <param name="markets">An <see cref="IMarketMessage{T,T1}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="namedValuesProvider">The <see cref="INamedValuesProvider"/> used to provide names for betting status and bet stop reason</param>
        /// <param name="rawMessage">The raw message</param>
        public CashOutProbabilities(IMessageTimestamp timestamp, IProducer producer, T @event, int? betStopReason, int? bettingStatus, IEnumerable<IMarketWithProbabilities> markets, INamedValuesProvider namedValuesProvider, byte[] rawMessage)
            : base(timestamp, producer, @event, null, markets, rawMessage)
        {
            Contract.Requires(namedValuesProvider != null);

            _betStopReason = betStopReason;
            _bettingStatus = bettingStatus;
            _namedValueProvider = namedValuesProvider;
        }
    }
}