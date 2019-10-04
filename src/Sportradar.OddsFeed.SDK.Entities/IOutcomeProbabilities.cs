/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represents a selection with probabilities information
    /// </summary>
    /// <seealso cref="IOutcome" />
    public interface IOutcomeProbabilities : IOutcome
    {
        /// <summary>
        /// Gets a value indicating whether the current <see cref="IOutcomeOdds"/> is active - i.e. should bets on it be accepted
        /// </summary>
        bool? Active { get; }

        /// <summary>
        /// Gets the probabilities for the current <see cref="IOutcomeOdds"/> instance
        /// </summary>
        double? Probabilities { get; }
    }
}