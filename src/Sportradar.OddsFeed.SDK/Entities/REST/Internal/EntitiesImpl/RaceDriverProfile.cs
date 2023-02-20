/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a race drive profile
    /// </summary>
    [DataContract]
    internal class RaceDriverProfile : IRaceDriverProfile
    {
        public RaceDriverProfile(RaceDriverProfileCI raceDriverProfileCI)
        {
            if (raceDriverProfileCI == null)
            {
                throw new ArgumentNullException(nameof(raceDriverProfileCI));
            }

            RaceDriverId = raceDriverProfileCI.RaceDriverId;
            RaceTeamId = raceDriverProfileCI.RaceTeamId;
            Car = raceDriverProfileCI.Car != null ? new Car(raceDriverProfileCI.Car) : null;
        }

        /// <summary>
        /// Gets the race driver id
        /// </summary>
        /// <value>The race driver id</value>
        public URN RaceDriverId { get; }

        /// <summary>
        /// Gets the race team id
        /// </summary>
        /// <value>The race team id</value>
        public URN RaceTeamId { get; }

        /// <summary>
        /// Gets the car info
        /// </summary>
        /// <value>The car info</value>
        public ICar Car { get; }
    }
}
