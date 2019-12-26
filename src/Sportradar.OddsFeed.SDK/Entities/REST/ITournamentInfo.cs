/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract for classes representing tournament info
    /// </summary>
    public interface ITournamentInfo
    {
        /// <summary>
        /// Gets a <see cref="URN"/> uniquely identifying the current season
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing names of the season in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name for specific culture
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>the name in specific culture</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the <see cref="ICategorySummary"/> representing the category associated with the current instance
        /// </summary>
        /// <value>The <see cref="ICategorySummary"/> representing the category associated with the current instance</value>
        ICategorySummary Category { get; }

        /// <summary>
        /// Gets the <see cref="ICurrentSeasonInfo"/> which contains data for the season in which the current tournament is happening
        /// </summary>
        /// <value>The <see cref="ICurrentSeasonInfo"/> which contains data for the season in which the current tournament is happening</value>
        ICurrentSeasonInfo CurrentSeason { get; }
    }
}
