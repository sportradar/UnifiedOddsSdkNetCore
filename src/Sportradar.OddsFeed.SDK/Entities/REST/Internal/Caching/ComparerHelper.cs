/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    internal sealed class CompetitorCIEqualityComparer : IEqualityComparer<CompetitorCI>
    {
        public bool Equals(CompetitorCI x, CompetitorCI y)
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

        public int GetHashCode(CompetitorCI obj)
        {
            return obj.Id != null ? obj.Id.GetHashCode() : 0;
        }
    }

    //public static IEqualityComparer<CompetitorCI> CompetitorCIComparer { get; } = new CompetitorCIEqualityComparer();
}
