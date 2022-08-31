/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.CustomBet
{
    /// <summary>
    /// Provides an outcomes
    /// </summary>
    public interface IOutcomeFilter
    {
        /// <summary>
        /// Gets the id of the outcome
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Value indicating if this outcome is in conflict
        /// </summary>
        bool? IsConflict { get; }
    }
}
