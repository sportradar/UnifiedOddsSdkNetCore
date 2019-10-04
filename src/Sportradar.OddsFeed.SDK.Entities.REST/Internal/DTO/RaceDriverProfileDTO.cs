/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object for race driver profile
    /// </summary>
    public class RaceDriverProfileDTO
    {
        public URN RaceDriverId { get; }

        public URN RaceTeamId { get; }

        public CarDTO Car { get; }

        public RaceDriverProfileDTO(raceDriverProfile item)
        {
            Contract.Requires(item != null);

            RaceDriverId = item.race_driver != null ? URN.Parse(item.race_driver.id) : null;
            RaceTeamId = item.race_team != null ? URN.Parse(item.race_team.id) : null;
            Car = item.car != null ? new CarDTO(item.car) : null; 
        }
    }
}
