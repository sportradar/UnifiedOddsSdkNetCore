/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by bet-settlement messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IBetSettlement<out T> : IMarketMessage<IMarketWithSettlement, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets the certainty of the <see cref="IBetSettlement{T}"/>
        /// </summary>
        /// <value>The certainty of the <see cref="IBetSettlement{T}"/></value>
        BetSettlementCertainty Certainty { get; }
    }
}
