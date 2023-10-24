/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Common.Enums;

// ReSharper disable MemberCanBePrivate.Global

namespace Sportradar.OddsFeed.SDK.Common.Internal.Extensions
{
    /// <summary>
    /// Urn extensions
    /// </summary>
    internal static class UrnExtensions
    {
        /// <summary>
        /// Determines whether Urn represents a competition sport event
        /// </summary>
        /// <param name="urn"></param>
        /// <returns><c>true</c> if represents competition; otherwise, <c>false</c>.</returns>
        public static bool IsCompetition(this Urn urn)
        {
            return urn.TypeGroup == ResourceTypeGroup.Match || urn.TypeGroup == ResourceTypeGroup.Stage;
        }

        /// <summary>
        /// Determines whether Urn represents a long term event
        /// </summary>
        /// <param name="urn"></param>
        /// <returns><c>true</c> if represents long term event; otherwise, <c>false</c>.</returns>
        public static bool IsLongTermEvent(this Urn urn)
        {
            return urn.TypeGroup == ResourceTypeGroup.BasicTournament || urn.TypeGroup == ResourceTypeGroup.Tournament || urn.TypeGroup == ResourceTypeGroup.Season;
        }
    }
}
