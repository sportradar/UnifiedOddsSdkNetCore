// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Cache item for current season info
    /// </summary>
    /// <seealso cref="CacheItem" />
    internal class CurrentSeasonInfoCacheItem : CacheItem
    {
        /// <summary>
        /// Gets a <see cref="string"/> representation of the current season year
        /// </summary>
        public string Year { get; private set; }

        /// <summary>
        /// Gets the start date of the season represented by the current instance
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// Gets the end date of the season represented by the current instance
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Gets the <see cref="SeasonCoverageCacheItem"/> instance containing information about coverage available for the season associated with the current instance
        /// </summary>
        /// <returns>The <see cref="SeasonCoverageCacheItem"/> instance containing information about coverage available for the season associated with the current instance</returns>
        public SeasonCoverageCacheItem SeasonCoverage { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{GroupCacheItem}"/> specifying groups of tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="IEnumerable{GroupCacheItem}"/> specifying groups of tournament associated with the current instance</returns>
        public IEnumerable<GroupCacheItem> Groups { get; private set; }

        /// <summary>
        /// Gets the <see cref="RoundCacheItem"/> specifying the current round of the tournament associated with the current instance
        /// </summary>
        /// <returns>The <see cref="RoundCacheItem"/> specifying the current round of the tournament associated with the current instance</returns>
        public RoundCacheItem CurrentRound { get; private set; }

        /// <summary>
        /// Gets the list of competitors ids
        /// </summary>
        /// <value>The list of competitors ids</value>
        public IEnumerable<Urn> CompetitorsIds { get; private set; }

        /// <summary>
        /// Gets the list of all <see cref="CompetitionCacheItem"/> that belongs to the season schedule
        /// </summary>
        /// <returns>The list of all <see cref="CompetitionCacheItem"/> that belongs to the season schedule</returns>
        public IEnumerable<Urn> Schedule { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoCacheItem"/> class
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public CurrentSeasonInfoCacheItem(CurrentSeasonInfoDto dto, CultureInfo culture)
            : base(dto.Id, dto.Name, culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Year = dto.Year;
            StartDate = dto.StartDate;
            EndDate = dto.EndDate;

            SeasonCoverage = dto.SeasonCoverage == null ? null : new SeasonCoverageCacheItem(dto.SeasonCoverage);
            Groups = dto.Groups == null ? null : dto.Groups.Select(s => new GroupCacheItem(s, culture));
            CurrentRound = dto.CurrentRound == null ? null : new RoundCacheItem(dto.CurrentRound, culture);
            CompetitorsIds = dto.Competitors == null ? null : dto.Competitors.Select(s => s.Id);
            Schedule = dto.Schedule == null ? null : dto.Schedule.Select(s => s.Id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSeasonInfoCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The exportable</param>
        public CurrentSeasonInfoCacheItem(ExportableCurrentSeasonInfo exportable)
            : base(exportable)
        {
            Year = exportable.Year;
            StartDate = exportable.StartDate;
            EndDate = exportable.EndDate;
            SeasonCoverage = exportable.SeasonCoverage == null ? null : new SeasonCoverageCacheItem(exportable.SeasonCoverage);
            Groups = exportable.Groups?.Select(g => new GroupCacheItem(g)).ToList();
            CurrentRound = exportable.CurrentRound == null ? null : new RoundCacheItem(exportable.CurrentRound);
            CompetitorsIds = exportable.Competitors?.Select(Urn.Parse).ToList();
            Schedule = exportable.Schedule?.Select(Urn.Parse).ToList();
        }

        /// <summary>
        /// Merges the specified dto
        /// </summary>
        /// <param name="dto">The dto</param>
        /// <param name="culture">The culture</param>
        public void Merge(CurrentSeasonInfoDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            base.Merge(dto, culture);

            Year = dto.Year;
            StartDate = dto.StartDate;
            EndDate = dto.EndDate;

            if (dto.SeasonCoverage != null)
            {
                SeasonCoverage = new SeasonCoverageCacheItem(dto.SeasonCoverage);
            }
            if (dto.Groups != null)
            {
                Groups = dto.Groups.Select(s => new GroupCacheItem(s, culture));
            }
            if (dto.CurrentRound != null)
            {
                CurrentRound = new RoundCacheItem(dto.CurrentRound, culture);
            }
            if (dto.Competitors != null)
            {
                CompetitorsIds = dto.Competitors.Select(s => s.Id);
            }
            if (dto.Schedule != null)
            {
                Schedule = dto.Schedule.Select(s => s.Id);
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public async Task<ExportableCurrentSeasonInfo> ExportAsync()
        {
            var groupsTask = Groups?.Select(async g => await g.ExportAsync().ConfigureAwait(false));
            return new ExportableCurrentSeasonInfo
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Name),
                Year = Year,
                StartDate = StartDate,
                EndDate = EndDate,
                SeasonCoverage = SeasonCoverage != null ? await SeasonCoverage.ExportAsync().ConfigureAwait(false) : null,
                Groups = groupsTask != null ? await Task.WhenAll(groupsTask) : null,
                CurrentRound = CurrentRound != null ? await CurrentRound.ExportAsync().ConfigureAwait(false) : null,
                Competitors = CompetitorsIds?.Select(s => s.ToString()),
                Schedule = Schedule?.Select(s => s.ToString()).ToList()
            };
        }
    }
}
