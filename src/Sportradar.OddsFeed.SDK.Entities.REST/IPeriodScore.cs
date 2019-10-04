/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a score of a sport event period
    /// </summary>
    public interface IPeriodScore : IEntityPrinter
    {
        /// <summary>
        /// Gets the score of the home team in the period represented by the current <see cref="IPeriodScore"/> instance
        /// </summary>
        decimal HomeScore { get; }

        /// <summary>
        /// Gets the score of the away team in the period represented by the current <see cref="IPeriodScore"/> instance
        /// </summary>
        decimal AwayScore { get; }

        /// <summary>
        /// Type of the period
        /// </summary>
        PeriodType? Type { get; }

        /// <summary>
        /// Number of the period
        /// </summary>
        int? Number { get; }

        /// <summary>
        /// Gets the match status code
        /// </summary>
        /// <value>The match status code</value>
        [Obsolete("Use GetMatchStatusAsync method instead")]
        int? MatchStatusCode { get; }

        /// <summary>
        /// Asynchronously gets the match status
        /// </summary>
        /// <param name="culture">The culture used to get match status id and description</param>
        /// <returns>Returns the match status id and description in selected culture</returns>
        Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture);
    }
}
