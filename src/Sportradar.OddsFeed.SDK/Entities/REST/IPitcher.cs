/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport event pitcher
    /// </summary>
    public interface IPitcher : IEntityPrinter
    {
        /// <summary>
        /// Gets a <see cref="URN"/> used to uniquely identify the current <see cref="IPitcher"/> instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets the name of the referee represented by the current <see cref="IPitcher"/> instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the hand with which player pitches
        /// </summary>
        /// <value>The hand with which player pitches</value>
        PlayerHand Hand { get; }

        /// <summary>
        /// Gets the indicator if the competitor is Home or Away
        /// </summary>
        /// <value>The indicator if the competitor is Home or Away</value>
        HomeAway Competitor { get; }
    }
}