// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Defines a data-transfer object for season coverage info
    /// </summary>
    internal class SeasonCoverageDto
    {
        /// <summary>
        /// Gets the maximum covered value
        /// </summary>
        public int? MaxCovered { get; }

        /// <summary>
        /// Gets the string representation of the maximum coverage available for the season associated with the current instance
        /// </summary>
        public string MaxCoverageLevel { get; }

        /// <summary>
        /// Gets the name of the minimum coverage guaranteed for the season associated with the current instance
        /// </summary>
        public string MinCoverageLevel { get; }

        /// <summary>
        /// Gets the played value
        /// </summary>
        public int Played { get; }

        /// <summary>
        /// Gets the scheduled value
        /// </summary>
        public int Scheduled { get; }

        /// <summary>
        /// Gets a <see cref="Urn"/> representing the ID of the season
        /// </summary>
        /// <value>The identifier.</value>
        public Urn SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverageDto"/> class.
        /// </summary>
        /// <param name="coverageInfo">The coverage message</param>
        internal SeasonCoverageDto(seasonCoverageInfo coverageInfo)
        {
            Guard.Argument(coverageInfo, nameof(coverageInfo)).NotNull();

            MaxCovered = coverageInfo.max_coveredSpecified
                             ? (int?)coverageInfo.max_covered
                             : null;

            MaxCoverageLevel = coverageInfo.max_coverage_level;
            MinCoverageLevel = coverageInfo.min_coverage_level;
            Played = coverageInfo.played;
            Scheduled = coverageInfo.scheduled;
            SeasonId = string.IsNullOrEmpty(coverageInfo.season_id)
                           ? null
                           : Urn.Parse(coverageInfo.season_id);
        }
    }
}
