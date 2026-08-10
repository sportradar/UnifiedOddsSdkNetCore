// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Represents a single selection entry in recommendation.
    /// </summary>
    public interface IPrebuiltBetSelection
    {
        /// <summary>
        /// Returns the market ID
        /// </summary>
        int MarketId { get; }

        /// <summary>
        /// Returns the outcome ID
        /// </summary>
        string OutcomeId { get; }

        /// <summary>
        /// Returns the specifiers
        /// </summary>
        string Specifiers { get; }
    }
}
