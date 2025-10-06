// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

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

        /// <summary>
        /// Gets the odds generation properties (contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it)
        /// </summary>
        /// <value>The odds generation properties</value>
        IOddsGeneration OddsGenerationProperties { get; }
    }
}
