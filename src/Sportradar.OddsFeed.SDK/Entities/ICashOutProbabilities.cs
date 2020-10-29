/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by cash-out probability messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ISportEvent"/> derived type specifying the type of the associated sport event</typeparam>
    public interface ICashOutProbabilities<out T> : IMarketMessage<IMarketWithProbabilities, T>
        where T : ISportEvent
    {
        /// <summary>
        /// Gets the <see cref="INamedValue"/> specifying the reason for betting being stopped, or a null reference if the reason is not known
        /// </summary>
        /// <value>The bet stop reason.</value>
        INamedValue BetStopReason { get; }

        /// <summary>
        /// Gets a <see cref="INamedValue"/> indicating the odds change was triggered by a possible event
        /// </summary>
        INamedValue BettingStatus { get; }
    }
}