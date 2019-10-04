/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a message dispatched by the feed when betting on some markets have to be stopped
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class BetStop<T> : EventMessage<T>, IBetStop<T> where T : ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="MarketStatus" /> specifying the new status of the associated markets
        /// </summary>
        /// <value>The market status</value>
        public MarketStatus MarketStatus { get; }

        /// <summary>
        /// Get a groups specifying that all markets with that groups needs to be stopped
        /// </summary>
        public IEnumerable<string> Groups { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetStop{T}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="event">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="marketStatus">a <see cref="MarketStatus" /> specifying the new status of the associated markets</param>
        /// <param name="groups">a list of <see cref="string"/> specifying which market groups needs to be stopped</param>
        /// <param name="rawMessage">The raw message</param>
        public BetStop(IMessageTimestamp timestamp, IProducer producer, T @event, long? requestId, MarketStatus marketStatus, IEnumerable<string> groups, byte[] rawMessage)
            : base(timestamp, producer, @event, requestId, rawMessage)
        {
            MarketStatus = marketStatus;
            Groups = groups;
        }
    }
}