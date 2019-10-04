/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about race driver profile
    /// </summary>
    public class CarCI
    {
        public string Name { get; }

        public string Chassis { get; }

        public string EngineName { get; }

        public CarCI(CarDTO item)
        {
            Contract.Requires(item != null);

            Name = item.Name;
            Chassis = item.Chassis;
            EngineName = item.EngineName;
        }

        public CarCI(ExportableCarCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));
            Name = exportable.Name;
            Chassis = exportable.Chassis;
            EngineName = exportable.EngineName;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCarCI"/> instance containing all relevant properties</returns>
        public Task<ExportableCarCI> ExportAsync()
        {
            return Task.FromResult(new ExportableCarCI
            {
                Name = Name,
                Chassis = Chassis,
                EngineName = EngineName
            });
        }
    }
}