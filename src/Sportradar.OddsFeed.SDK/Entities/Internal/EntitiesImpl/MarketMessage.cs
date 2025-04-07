﻿// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl
{
    /// <summary>
    /// A base class for all messages containing information about betting markets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T1"></typeparam>
    internal abstract class MarketMessage<T, T1> : EventMessage<T1>, IMarketMessage<T, T1>
        where T : IMarket
        where T1 : ISportEvent
    {
        /// <summary>
        /// A <see cref="Markets" /> property backing field
        /// </summary>
        private readonly IReadOnlyCollection<T> _markets;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketMessage{T,T1}" /> class
        /// </summary>
        /// <param name="timestamp">The value specifying timestamps related to the message (in the milliseconds since EPOCH UTC)</param>
        /// <param name="producer">The <see cref="IProducer" /> specifying the producer / service which dispatched the current <see cref="Message" /> message</param>
        /// <param name="sportEvent">An <see cref="ICompetition" /> derived instance representing the sport event associated with the current <see cref="EventMessage{T}" /></param>
        /// <param name="requestId">The id of the request which triggered the current <see cref="EventMessage{T}" /> message or a null reference</param>
        /// <param name="markets">An <see cref="IEnumerable{IMarket}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></param>
        /// <param name="rawMessage">The raw message</param>
        protected MarketMessage(IMessageTimestamp timestamp, IProducer producer, T1 sportEvent, long? requestId, IEnumerable<T> markets, byte[] rawMessage)
            : base(timestamp, producer, sportEvent, requestId, rawMessage)
        {
            _markets = markets == null ? null : new ReadOnlyCollection<T>(markets.ToList());
        }

        /// <summary>Gets a <see cref="IEnumerable{IMarket}" /> describing markets associated with the current <see cref="IMarketMessage{T, R}" /></summary>
        public IEnumerable<T> Markets => _markets;
    }
}
