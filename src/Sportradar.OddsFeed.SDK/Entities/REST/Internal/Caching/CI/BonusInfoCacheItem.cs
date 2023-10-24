/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for lottery draw bonus info
    /// </summary>
    internal class BonusInfoCacheItem
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

        internal BonusInfoCacheItem(BonusInfoDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            BonusBalls = dto.BonusBalls;
            BonusDrumType = dto.BonusDrumType;
            BonusRange = dto.BonusRange;
        }

        internal BonusInfoCacheItem(ExportableBonusInfo exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            BonusBalls = exportable.BonusBalls;
            BonusDrumType = exportable.BonusDrumType;
            BonusRange = exportable.BonusRange;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableBonusInfo> ExportAsync()
        {
            return Task.FromResult(new ExportableBonusInfo
            {
                BonusBalls = BonusBalls,
                BonusDrumType = BonusDrumType,
                BonusRange = BonusRange
            });
        }
    }
}
