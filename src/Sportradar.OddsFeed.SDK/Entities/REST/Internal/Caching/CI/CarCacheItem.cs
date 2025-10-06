// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    internal class CarCacheItem
    {
        public string Name { get; }

        public string Chassis { get; }

        public string EngineName { get; }

        public CarCacheItem(CarDto item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Name = item.Name;
            Chassis = item.Chassis;
            EngineName = item.EngineName;
        }

        public CarCacheItem(ExportableCar exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Name = exportable.Name;
            Chassis = exportable.Chassis;
            EngineName = exportable.EngineName;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCar"/> instance containing all relevant properties</returns>
        public Task<ExportableCar> ExportAsync()
        {
            return Task.FromResult(new ExportableCar
            {
                Name = Name,
                Chassis = Chassis,
                EngineName = EngineName
            });
        }
    }
}
