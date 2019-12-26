/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

// ReSharper disable InconsistentNaming

namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Enumerates groups of resources represented by the <see cref="URN"/>
    /// </summary>
    public enum ResourceTypeGroup
    {
        /// <summary>
        /// The resource represents a sport event of match type
        /// </summary>
        MATCH,

        /// <summary>
        /// The resource represents a sport event of stage type
        /// </summary>
        STAGE,

        /// <summary>
        /// The resource represents a tournament
        /// </summary>
        TOURNAMENT,

        /// <summary>
        /// The basic tournament
        /// </summary>
        BASIC_TOURNAMENT,

        /// <summary>
        /// The resource represents a (tournament) season
        /// </summary>
        SEASON,

        /// <summary>
        /// The non-specific URN type specifier
        /// </summary>
        OTHER,

        /// <summary>
        /// The unknown resource type
        /// </summary>
        UNKNOWN,

        /// <summary>
        /// The resource represents lottery draw
        /// </summary>
        DRAW,

        /// <summary>
        /// The resource represents lottery
        /// </summary>
        LOTTERY
    }
}