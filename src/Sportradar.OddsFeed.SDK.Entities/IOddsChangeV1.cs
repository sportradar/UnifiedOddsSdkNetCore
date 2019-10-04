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
    public interface IOddsChangeV1<out T> : IOddsChange<T> where T : ISportEvent
    {
        /// <summary>
        /// Gets the odds generation properties (contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it)
        /// </summary>
        /// <value>The odds generation properties</value>
        IOddsGeneration OddsGenerationProperties { get; }
    }
}
