/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by bet-cancel messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IBetCancel<out T> : IMarketMessage<IMarketCancel, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the start of cancellation period.
        /// A null value indicates the period started with market activation
        /// </summary>
        long? StartTime { get; }

        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the end of cancellation period.
        /// A null value indicates the period ended when the market was closed
        /// </summary>
        long? EndTime { get; }

        /// <summary>
        /// If the market was cancelled because of a migration from a different sport event, it gets a <see cref="URN"/> specifying the sport event from which the market has migrated.
        /// </summary>
        /// <value>The superseded by.</value>
        URN SupersededBy { get; }
    }
}
