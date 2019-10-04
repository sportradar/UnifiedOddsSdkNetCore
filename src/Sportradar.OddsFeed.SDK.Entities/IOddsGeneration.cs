/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Provided by the prematch odds producer only, and contains a few key-parameters that can be used in a client’s own special odds model, or even offer spread betting bets based on it.
    /// </summary>
    public interface IOddsGeneration {

        /// <summary>
        /// Gets the expected totals (how many goals are expected in total?)
        /// </summary>
        /// <value>The expected totals</value>
        double? ExpectedTotals { get; }

        /// <summary>
        /// Gets the expected supremacy (how big is the expected goal supremacy)
        /// </summary>
        /// <value>The expected supremacy</value>
        double? ExpectedSupremacy { get; }
    }
}