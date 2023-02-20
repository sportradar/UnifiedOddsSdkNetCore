/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Class TournamentInfoBasicCI
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class TournamentInfoBasicCI : CacheItem
    {
        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public URN Category { get; }

        /// <summary>
        /// Gets the current season
        /// </summary>
        /// <value>The current season</value>
        public CurrentSeasonInfoCI CurrentSeason { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoBasicCI"/> class
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public TournamentInfoBasicCI(TournamentInfoDTO dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            if (dto.Category != null)
            {
                Category = dto.Category.Id;
            }
            if (dto.CurrentSeason != null)
            {
                CurrentSeason = new CurrentSeasonInfoCI(dto.CurrentSeason, culture);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfoBasicCI"/> class
        /// </summary>
        /// <param name="exportable">The exportable</param>
        public TournamentInfoBasicCI(ExportableTournamentInfoBasicCI exportable)
            : base(exportable)
        {
            Category = exportable.Category != null ? URN.Parse(exportable.Category) : null;
            CurrentSeason = exportable.CurrentSeason != null
                ? new CurrentSeasonInfoCI(exportable.CurrentSeason)
                : null;
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public void Merge(TournamentInfoDTO dto, CultureInfo culture)
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
                    CurrentSeason = new CurrentSeasonInfoCI(dto.CurrentSeason, culture);
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public async Task<ExportableTournamentInfoBasicCI> ExportAsync()
        {
            return new ExportableTournamentInfoBasicCI
            {
                Id = Id.ToString(),
                Name = new Dictionary<CultureInfo, string>(Name ?? new Dictionary<CultureInfo, string>()),
                CurrentSeason = CurrentSeason != null ? await CurrentSeason.ExportAsync().ConfigureAwait(false) : null,
                Category = Category?.ToString()
            };
        }
    }
}
