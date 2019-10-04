/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing a match status
    /// </summary>
    /// <seealso cref="ICompetitionStatus" />
    public interface IMatchStatus : ICompetitionStatus
    {
        /// <summary>
        /// Gets the <see cref="IEventClock"/> instance describing the timings in the current event
        /// </summary>
        /// <value>The <see cref="IEventClock"/> instance describing the timings in the current event</value>
        IEventClock EventClock { get; }

        /// <summary>
        /// Gets the list of <see cref="IPeriodScore"/>
        /// </summary>
        /// <value>The list of <see cref="IPeriodScore"/></value>
        IEnumerable<IPeriodScore> PeriodScores { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the home competitor competing on the associated sport event</value>
        decimal HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        /// <value>The score of the away competitor competing on the associated sport event</value>
        decimal AwayScore { get; }

        /// <summary>
        /// Asynchronously gets the match status
        /// </summary>
        /// <param name="culture">The culture used to get match status id and description</param>
        /// <returns>Returns the match status id and description in selected culture</returns>
        Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture);
    }
}
