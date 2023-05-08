/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages;
// ReSharper disable MemberCanBePrivate.Global

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// URN extensions
    /// </summary>
    internal static class UrnExtensions
    {
        /// <summary>
        /// Determines whether URN represents a competition sport event
        /// </summary>
        /// <param name="urn"></param>
        /// <returns><c>true</c> if represents competition; otherwise, <c>false</c>.</returns>
        public static bool IsCompetition(this URN urn)
        {
            return urn.TypeGroup == ResourceTypeGroup.MATCH || urn.TypeGroup == ResourceTypeGroup.STAGE;
        }

        /// <summary>
        /// Determines whether URN represents a long term event
        /// </summary>
        /// <param name="urn"></param>
        /// <returns><c>true</c> if represents long term event; otherwise, <c>false</c>.</returns>
        public static bool IsLongTermEvent(this URN urn)
        {
            return urn.TypeGroup == ResourceTypeGroup.BASIC_TOURNAMENT || urn.TypeGroup == ResourceTypeGroup.TOURNAMENT || urn.TypeGroup == ResourceTypeGroup.SEASON;
        }
    }
}
