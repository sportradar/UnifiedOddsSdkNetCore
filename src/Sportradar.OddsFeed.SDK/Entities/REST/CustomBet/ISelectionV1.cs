// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet
{
    /// <summary>
    /// Provides an requested selection
    /// </summary>
    public interface ISelectionV1 : ISelection
    {
        /// <summary>
        /// Gets the odds
        /// </summary>
        double? Odds { get; }
    }
}
