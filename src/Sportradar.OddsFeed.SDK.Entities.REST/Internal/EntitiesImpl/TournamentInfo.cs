/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class TournamentInfo
    /// </summary>
    /// <seealso cref="ITournamentInfo" />
    internal class TournamentInfo : BaseEntity, ITournamentInfo
    {
        /// <summary>
        /// Gets the <see cref="ICategorySummary" /> representing the category associated with the current instance
        /// </summary>
        /// <value>The <see cref="ICategorySummary" /> representing the category associated with the current instance</value>
        public ICategorySummary Category { get; }
        /// <summary>
        /// Gets the <see cref="ICurrentSeasonInfo" /> which contains data for the season in which the current tournament is happening
        /// </summary>
        /// <value>The <see cref="ICurrentSeasonInfo" /> which contains data for the season in which the current tournament is happening</value>
        public ICurrentSeasonInfo CurrentSeason { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentInfo"/> class.
        /// </summary>
        /// <param name="cacheItem">The cache item</param>
        /// <param name="categorySummary">The category summary</param>
        /// <param name="currentSeasonInfo">The full current season info</param>
        public TournamentInfo(TournamentInfoBasicCI cacheItem, ICategorySummary categorySummary, ICurrentSeasonInfo currentSeasonInfo)
            : base (cacheItem.Id, cacheItem.Name as IReadOnlyDictionary<CultureInfo, string>)
        {
            Category = categorySummary;
            CurrentSeason = currentSeasonInfo;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"Id={base.PrintC()}, Category={Category?.Id}, CurrentSeason={CurrentSeason?.Id}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }
    }
}
