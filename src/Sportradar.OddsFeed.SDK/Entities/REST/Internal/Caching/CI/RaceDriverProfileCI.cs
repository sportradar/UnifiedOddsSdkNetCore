/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    public class RaceDriverProfileCI
    {
        public URN RaceDriverId { get; }

        public URN RaceTeamId { get; }

        public CarCI Car { get; }

        public RaceDriverProfileCI(RaceDriverProfileDTO item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            RaceDriverId = item.RaceDriverId;
            RaceTeamId = item.RaceTeamId;
            Car = item.Car != null ? new CarCI(item.Car) : null;
        }

        public RaceDriverProfileCI(ExportableRaceDriverProfileCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));
            RaceDriverId = exportable.RaceDriverId != null ? URN.Parse(exportable.RaceDriverId) : null;
            RaceTeamId = exportable.RaceTeamId != null ? URN.Parse(exportable.RaceTeamId) : null;
            Car = exportable.Car != null ? new CarCI(exportable.Car) : null;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableRaceDriverProfileCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableRaceDriverProfileCI> ExportAsync()
        {
            return new ExportableRaceDriverProfileCI
            {
                RaceDriverId = RaceDriverId?.ToString(),
                RaceTeamId = RaceTeamId?.ToString(),
                Car = Car != null ? await Car.ExportAsync().ConfigureAwait(false) : null
            };
        }
    }
}
