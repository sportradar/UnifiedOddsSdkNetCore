// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Represents a single recommendation entry in prebuilt bets response.
    /// </summary>
    public interface IRecommendation
    {
        /// <summary>
        /// Returns the list of selections for this recommendation
        /// </summary>
        IPrebuiltBetSelection[] Selections { get; }

        /// <summary>
        /// Returns the odds for this recommendation
        /// </summary>
        double Odds { get; }

        /// <summary>
        /// Returns the probability for this recommendation
        /// </summary>
        double Probability { get; }
    }
}
