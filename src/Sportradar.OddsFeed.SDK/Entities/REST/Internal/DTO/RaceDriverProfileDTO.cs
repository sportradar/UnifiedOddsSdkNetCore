// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object for race driver profile
    /// </summary>
    internal class RaceDriverProfileDto
    {
        public Urn RaceDriverId { get; }

        public Urn RaceTeamId { get; }

        public CarDto Car { get; }

        public RaceDriverProfileDto(raceDriverProfile item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            RaceDriverId = item.race_driver != null ? Urn.Parse(item.race_driver.id) : null;
            RaceTeamId = item.race_team != null ? Urn.Parse(item.race_team.id) : null;
            Car = item.car != null ? new CarDto(item.car) : null;
        }
    }
}
