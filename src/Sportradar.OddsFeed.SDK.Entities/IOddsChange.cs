/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by odds-change messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IOddsChange<out T> : IMarketMessage<IMarketWithOdds, T> where T : ISportEvent
    {
        /// <summary>
        /// Gets a <see cref="OddsChangeReason"/> enum member specifying the reason for odds change or a null reference if the reason is not known
        /// </summary>
        OddsChangeReason? ChangeReason { get; }

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
