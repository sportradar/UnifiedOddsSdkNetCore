// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Class TournamentInfoBasicCacheItem
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class TournamentInfoBasicCacheItem : CacheItem
    {
        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public Urn Category { get; }

        /// <summary>
        /// Gets the current season
        /// </summary>
        /// <value>The current season</value>
        public CurrentSeasonInfoCacheItem CurrentSeason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoBasicCacheItem"/> class
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public TournamentInfoBasicCacheItem(TournamentInfoDto dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            if (dto.Category != null)
            {
                Category = dto.Category.Id;
            }
            if (dto.CurrentSeason != null)
            {
                CurrentSeason = new CurrentSeasonInfoCacheItem(dto.CurrentSeason, culture);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoBasicCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The exportable</param>
        public TournamentInfoBasicCacheItem(ExportableTournamentInfoBasic exportable)
            : base(exportable)
        {
            Category = exportable.Category != null ? Urn.Parse(exportable.Category) : null;
            CurrentSeason = exportable.CurrentSeason != null
                                ? new CurrentSeasonInfoCacheItem(exportable.CurrentSeason)
                                : null;
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public void Merge(TournamentInfoDto dto, CultureInfo culture)
        {
            base.Merge(new CacheItem(dto.Id, dto.Name, culture), culture);

            if (dto.Category != null)
            {
                if (!Category.Equals(dto.Category.Id))
                {
                    // WRONG
                }
            }
            if (dto.CurrentSeason != null)
            {
                if (CurrentSeason == null)
                {
                    CurrentSeason = new CurrentSeasonInfoCacheItem(dto.CurrentSeason, culture);
                }
                else
                {
                    CurrentSeason.Merge(dto.CurrentSeason, culture);
                }
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public async Task<ExportableTournamentInfoBasic> ExportAsync()
        {
            return new ExportableTournamentInfoBasic
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Name ?? new Dictionary<CultureInfo, string>()),
                CurrentSeason = CurrentSeason != null ? await CurrentSeason.ExportAsync().ConfigureAwait(false) : null,
                Category = Category?.ToString()
            };
        }
    }
}
