/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Represents a race driver profile
    /// </summary>
    public interface IRaceDriverProfile
    {
        /// <summary>
        /// Gets the race driver id
        /// </summary>
        /// <value>The race driver id</value>
        URN RaceDriverId { get; }

        /// <summary>
        /// Gets the race team id
        /// </summary>
        /// <value>The race team id</value>
        URN RaceTeamId { get; }

        /// <summary>
        /// Gets the car info
        /// </summary>
        /// <value>The car info</value>
        ICar Car { get; }
    }
}
