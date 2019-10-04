/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Represent a betting market outcome
    /// </summary>
    public interface IOutcomeV1 : IOutcome {

        /// <summary>
        /// Gets the associated outcome definition instance
        /// </summary>
        IOutcomeDefinition OutcomeDefinition { get; }
    }
}