/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Dawn;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for lottery draw bonus info
    /// </summary>
    public class BonusInfoCI
    {
        /// <summary>
        /// Gets the bonus balls info
        /// </summary>
        /// <value>The bonus balls info or null if not known</value>
        public int? BonusBalls { get; }

        /// <summary>
        /// Gets the type of the bonus drum
        /// </summary>
        /// <value>The type of the bonus drum or null if not known</value>
        public BonusDrumType? BonusDrumType { get; }

        /// <summary>
        /// Gets the bonus range
        /// </summary>
        /// <value>The bonus range</value>
        public string BonusRange { get; }

        internal BonusInfoCI(BonusInfoDTO dto)
        {
            Guard.Argument(dto).NotNull();

            BonusBalls = dto.BonusBalls;
            BonusDrumType = dto.BonusDrumType;
            BonusRange = dto.BonusRange;
        }

        internal BonusInfoCI(ExportableBonusInfoCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            BonusBalls = exportable.BonusBalls;
            BonusDrumType = exportable.BonusDrumType;
            BonusRange = exportable.BonusRange;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableBonusInfoCI> ExportAsync()
        {
            return Task.FromResult(new ExportableBonusInfoCI
            {
                BonusBalls = BonusBalls,
                BonusDrumType = BonusDrumType,
                BonusRange = BonusRange
            });
        }
    }
}
