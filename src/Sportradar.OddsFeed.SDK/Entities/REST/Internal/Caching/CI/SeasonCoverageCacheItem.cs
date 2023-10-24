/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for season coverage
    /// </summary>
    internal class SeasonCoverageCacheItem
    {
        /// <summary>
        /// Gets the string representation of the maximum coverage available for the season associated with the current instance
        /// </summary>
        /// <value>The maximum coverage level.</value>
        public string MaxCoverageLevel { get; }

        /// <summary>
        /// Gets the name of the minimum coverage guaranteed for the season associated with the current instance
        /// </summary>
        public string MinCoverageLevel { get; }

        /// <summary>
        /// Gets the max covered value
        /// </summary>
        public int? MaxCovered { get; }

        /// <summary>
        /// Gets the played value
        /// </summary>
        public int Played { get; }

        /// <summary>
        /// Gets the scheduled value
        /// </summary>
        public int Scheduled { get; }

        /// <summary>
        /// Gets the identifier of the season
        /// </summary>
        public Urn SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverageCacheItem"/> class.
        /// </summary>
        /// <param name="coverageDto">A <see cref="SeasonCoverageDto"/> instance containing information about the coverage</param>
        public SeasonCoverageCacheItem(SeasonCoverageDto coverageDto)
        {
            Guard.Argument(coverageDto, nameof(coverageDto)).NotNull();

            MaxCoverageLevel = coverageDto.MaxCoverageLevel;
            MinCoverageLevel = coverageDto.MinCoverageLevel;
            MaxCovered = coverageDto.MaxCovered;
            Played = coverageDto.Played;
            Scheduled = coverageDto.Scheduled;
            SeasonId = coverageDto.SeasonId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverageCacheItem"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSeasonCoverage"/> instance containing information about the coverage</param>
        public SeasonCoverageCacheItem(ExportableSeasonCoverage exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            MaxCoverageLevel = exportable.MaxCoverageLevel;
            MinCoverageLevel = exportable.MinCoverageLevel;
            MaxCovered = exportable.MaxCovered;
            Played = exportable.Played;
            Scheduled = exportable.Scheduled;
            SeasonId = Urn.Parse(exportable.SeasonId);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableSeasonCoverage> ExportAsync()
        {
            return Task.FromResult(new ExportableSeasonCoverage
            {
                Scheduled = Scheduled,
                MaxCoverageLevel = MaxCoverageLevel,
                MaxCovered = MaxCovered,
                MinCoverageLevel = MinCoverageLevel,
                Played = Played,
                SeasonId = SeasonId?.ToString()
            });
        }
    }
}
