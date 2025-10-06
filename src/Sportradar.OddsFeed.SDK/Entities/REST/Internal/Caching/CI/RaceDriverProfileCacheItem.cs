// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    internal class RaceDriverProfileCacheItem
    {
        public Urn RaceDriverId { get; }

        public Urn RaceTeamId { get; }

        public CarCacheItem Car { get; }

        public RaceDriverProfileCacheItem(RaceDriverProfileDto item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            RaceDriverId = item.RaceDriverId;
            RaceTeamId = item.RaceTeamId;
            Car = item.Car != null ? new CarCacheItem(item.Car) : null;
        }

        public RaceDriverProfileCacheItem(ExportableRaceDriverProfile exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            RaceDriverId = exportable.RaceDriverId != null ? Urn.Parse(exportable.RaceDriverId) : null;
            RaceTeamId = exportable.RaceTeamId != null ? Urn.Parse(exportable.RaceTeamId) : null;
            Car = exportable.Car != null ? new CarCacheItem(exportable.Car) : null;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableRaceDriverProfile"/> instance containing all relevant properties</returns>
        public async Task<ExportableRaceDriverProfile> ExportAsync()
        {
            return new ExportableRaceDriverProfile
            {
                RaceDriverId = RaceDriverId?.ToString(),
                RaceTeamId = RaceTeamId?.ToString(),
                Car = Car != null ? await Car.ExportAsync().ConfigureAwait(false) : null
            };
        }
    }
}
