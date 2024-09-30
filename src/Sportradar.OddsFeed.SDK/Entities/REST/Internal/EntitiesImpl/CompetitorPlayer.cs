// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class CompetitorPlayer : PlayerProfile, ICompetitorPlayer
    {
        public int? JerseyNumber
        {
            get;
        }

        public CompetitorPlayer(PlayerProfileCacheItem ci, IReadOnlyCollection<CultureInfo> cultures, int? jerseyNumber)
            : base(ci, cultures)
        {
            JerseyNumber = jerseyNumber;
        }
    }
}
