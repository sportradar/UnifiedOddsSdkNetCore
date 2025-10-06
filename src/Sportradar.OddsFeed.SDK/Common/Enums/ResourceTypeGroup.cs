// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common.Enums
{
    /// <summary>
    /// Enumerates groups of resources represented by the <see cref="Urn"/>
    /// </summary>
    public enum ResourceTypeGroup
    {
        /// <summary>
        /// The resource represents a sport event of match type
        /// </summary>
        Match,

        /// <summary>
        /// The resource represents a sport event of stage type
        /// </summary>
        Stage,

        /// <summary>
        /// The resource represents a tournament
        /// </summary>
        Tournament,

        /// <summary>
        /// The basic tournament
        /// </summary>
        BasicTournament,

        /// <summary>
        /// The resource represents a (tournament) season
        /// </summary>
        Season,

        /// <summary>
        /// The non-specific urn type specifier
        /// </summary>
        Other,

        /// <summary>
        /// The unknown resource type
        /// </summary>
        Unknown,

        /// <summary>
        /// The resource represents lottery draw
        /// </summary>
        Draw,

        /// <summary>
        /// The resource represents lottery
        /// </summary>
        Lottery
    }
}
