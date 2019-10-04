/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about season coverage
    /// </summary>
    /// <seealso cref="ISeasonCoverage" />
    internal class SeasonCoverage : ISeasonCoverage
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
        /// Gets the identifier of the season.
        /// </summary>
        public URN SeasonId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonCoverage"/> class.
        /// </summary>
        /// <param name="cacheItem">The <see cref="SeasonCoverageCI"/> instance containing information about tournament coverage</param>
        public SeasonCoverage(SeasonCoverageCI cacheItem)
        {
            Contract.Requires(cacheItem != null);

            MaxCoverageLevel = cacheItem.MinCoverageLevel;
            MinCoverageLevel = cacheItem.MinCoverageLevel;
            MaxCovered = cacheItem.MaxCovered;
            Played = cacheItem.Played;
            Scheduled = cacheItem.Scheduled;
            SeasonId = cacheItem.SeasonId;
        }
    }
}