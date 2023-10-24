/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Caching
{
    internal sealed class CompetitorCacheItemEqualityComparer : IEqualityComparer<CompetitorCacheItem>
    {
        public bool Equals(CompetitorCacheItem x, CompetitorCacheItem y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return Equals(x.Id, y.Id);
        }

        public int GetHashCode(CompetitorCacheItem obj)
        {
            return obj.Id != null ? obj.Id.GetHashCode() : 0;
        }
    }

    //public static IEqualityComparer<CompetitorCacheItem> CompetitorCacheItemComparer { get; } = new CompetitorCacheItemEqualityComparer();
}
