// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing a player profile
    /// </summary>
    public interface ICompetitorPlayer : IPlayerProfile
    {
        /// <summary>
        /// Gets the jersey number of the player
        /// </summary>
        int? JerseyNumber { get; }
    }
}
