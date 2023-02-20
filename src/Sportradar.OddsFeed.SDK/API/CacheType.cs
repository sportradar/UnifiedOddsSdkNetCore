/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Enumerates the types of the caches supported by the SDK
    /// </summary>
    [Flags]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1135:Declare enum member with zero value (when enum has FlagsAttribute).", Justification = "Allowed to prevent breaking change")]
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
        /// Cache used to hold profile items (player and team profiles)
        /// </summary>
        Profile = 4,

        /// <summary>
        /// All caches
        /// </summary>
        All = SportData | SportEvent | Profile
    }
}
