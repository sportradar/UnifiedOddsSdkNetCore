/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item object for season coverage
    /// </summary>
    public class SeasonCoverageCI
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
        public URN SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverageCI"/> class.
        /// </summary>
        /// <param name="coverageDTO">A <see cref="SeasonCoverageDTO"/> instance containing information about the coverage</param>
        public SeasonCoverageCI(SeasonCoverageDTO coverageDTO)
        {
            Contract.Requires(coverageDTO != null);

            MaxCoverageLevel = coverageDTO.MaxCoverageLevel;
            MinCoverageLevel = coverageDTO.MinCoverageLevel;
            MaxCovered = coverageDTO.MaxCovered;
            Played = coverageDTO.Played;
            Scheduled = coverageDTO.Scheduled;
            SeasonId = coverageDTO.SeasonId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverageCI"/> class.
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableSeasonCoverageCI"/> instance containing information about the coverage</param>
        public SeasonCoverageCI(ExportableSeasonCoverageCI exportable)
        {
            if (exportable == null)
                throw new ArgumentNullException(nameof(exportable));

            MaxCoverageLevel = exportable.MaxCoverageLevel;
            MinCoverageLevel = exportable.MinCoverageLevel;
            MaxCovered = exportable.MaxCovered;
            Played = exportable.Played;
            Scheduled = exportable.Scheduled;
            SeasonId = URN.Parse(exportable.SeasonId);
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportableSeasonCoverageCI> ExportAsync()
        {
            return Task.FromResult(new ExportableSeasonCoverageCI
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
