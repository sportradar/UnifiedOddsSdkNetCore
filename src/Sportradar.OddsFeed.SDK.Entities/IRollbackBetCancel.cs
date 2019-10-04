/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by bet-cancel-rollback messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IRollbackBetCancel<out T> : IMarketMessage<IMarketCancel, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the start of rollback cancellation period.
        /// A null value indicates the period started with market activation
        /// </summary>
        long? StartTime { get; }

        /// <summary>
        /// Gets number of milliseconds from UTC epoch representing the end of rollback cancellation period.
        /// A null value indicates the period ended when the market was closed
        /// </summary>
        long? EndTime { get; }
    }
}
