/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed when some of the market are canceled
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BetCancel<T> : MarketMessage<IMarketCancel, T>, IBetCancel<T> where T : ISportEvent
    {
        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the start of cancellation period
        /// A null value indicates the period starts with market activation
        /// </summary>
        public long? StartTime { get; }

        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the end of cancellation period
        /// A Null value indicates the period ends when the market was closed
        /// </summary>
        public long? EndTime { get; }

        /// <summary>
        /// If the market was canceled because of a migration from a different sport event, it gets a <see cref="Urn"/> specifying the sport event from which the market has migrated
        /// </summary>
        /// <value>The superseded by</value>
        public Urn SupersededBy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetCancel{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="startTime">a number of milliseconds from UTC epoch representing the start of cancellation period</param>
        /// <param name="endTime">a number of milliseconds from UTC epoch representing the end of cancellation period</param>
        /// <param name="supersededBy">If the market was canceled because of a migration from a different sport event, a <see cref="Urn" /> specifying the sport event from which the market has migrated</param>
        /// <param name="markets">An <see cref="IEnumerable{T}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="rawMessage">The raw message</param>
        public BetCancel(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, long? startTime, long? endTime, Urn supersededBy, IEnumerable<IMarketCancel> markets, byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, markets, rawMessage)
        {
            StartTime = startTime;
            EndTime = endTime;
            SupersededBy = supersededBy;
        }
    }
}
