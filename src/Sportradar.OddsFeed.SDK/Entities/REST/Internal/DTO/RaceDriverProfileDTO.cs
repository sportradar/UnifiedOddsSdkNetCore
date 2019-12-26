/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
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
            Guard.Argument(item, nameof(item)).NotNull();

            RaceDriverId = item.race_driver != null ? URN.Parse(item.race_driver.id) : null;
            RaceTeamId = item.race_team != null ? URN.Parse(item.race_team.id) : null;
            Car = item.car != null ? new CarDTO(item.car) : null; 
        }
    }
}
