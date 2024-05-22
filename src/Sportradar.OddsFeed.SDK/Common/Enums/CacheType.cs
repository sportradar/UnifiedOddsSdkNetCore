// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumerates the types of the caches supported by the SDK
    /// </summary>
    public enum CacheType
    {
        /// <summary>
        /// Cache used to hold sport data items (sports and categories)
        /// </summary>
        SportData = 1,

        /// <summary>
        /// Cache used to hold sport event items (tournaments, matches, seasons...)
        /// </summary>
        SportEvent = 2,

        /// <summary>
        /// Cache used to hold profile items (player and competitor profiles)
        /// </summary>
        Profile = 4,

        /// <summary>
        /// All caches
        /// </summary>
        All = SportData | SportEvent | Profile
    }
}
