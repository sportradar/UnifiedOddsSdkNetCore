// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    internal class Division : IDivision
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public Division(DivisionCacheItem divisionCi)
        {
            if (divisionCi == null)
            {
                throw new ArgumentNullException(nameof(divisionCi));
            }

            Id = divisionCi.Id;
            Name = divisionCi.Name;
        }
    }
}
