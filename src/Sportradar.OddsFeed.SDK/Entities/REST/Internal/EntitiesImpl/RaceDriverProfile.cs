// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a race drive profile
    /// </summary>
    [DataContract]
    internal class RaceDriverProfile : IRaceDriverProfile
    {
        public RaceDriverProfile(RaceDriverProfileCacheItem raceDriverProfileCacheItem)
        {
            if (raceDriverProfileCacheItem == null)
            {
                throw new ArgumentNullException(nameof(raceDriverProfileCacheItem));
            }

            RaceDriverId = raceDriverProfileCacheItem.RaceDriverId;
            RaceTeamId = raceDriverProfileCacheItem.RaceTeamId;
            Car = raceDriverProfileCacheItem.Car != null ? new Car(raceDriverProfileCacheItem.Car) : null;
        }

        /// <summary>
        /// Gets the race driver id
        /// </summary>
        /// <value>The race driver id</value>
        public Urn RaceDriverId { get; }

        /// <summary>
        /// Gets the race team id
        /// </summary>
        /// <value>The race team id</value>
        public Urn RaceTeamId { get; }

        /// <summary>
        /// Gets the car info
        /// </summary>
        /// <value>The car info</value>
        public ICar Car { get; }
    }
}
