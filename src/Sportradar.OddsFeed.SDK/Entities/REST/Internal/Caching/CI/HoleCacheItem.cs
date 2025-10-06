// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A cache item representing a hole (used in golf course)
    /// </summary>
    internal class HoleCacheItem
    {
        /// <summary>
        /// Gets the number of the hole
        /// </summary>
        /// <value>The number</value>
        internal int Number { get; }

        /// <summary>
        /// Gets the par
        /// </summary>
        /// <value>The par</value>
        internal int Par { get; }

        internal HoleCacheItem(HoleDto hole)
        {
            Number = hole.Number;
            Par = hole.Par;
        }

        internal HoleCacheItem(ExportableHole hole)
        {
            Number = hole.Number;
            Par = hole.Par;
        }
    }
}
