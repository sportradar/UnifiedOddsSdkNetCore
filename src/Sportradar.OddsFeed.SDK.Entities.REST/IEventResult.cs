/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines methods used by classes that provide event result information
    /// </summary>
    public interface IEventResult
    {
        /// <summary>
        /// Gets the id of the event result
        /// </summary>
        /// <value>The id of the event result</value>
        string Id { get; }

        /// <summary>
        /// Gets the position of the result
        /// </summary>
        /// <value>The position of the result</value>
        int? Position { get; }

        /// <summary>
        /// Gets the points of the result
        /// </summary>
        /// <value>The points of the result</value>
        [Obsolete]
        int? Points { get; }

        /// <summary>
        /// Gets the points
        /// </summary>
        /// <value>The points</value>
        decimal? PointsDecimal { get; }

        /// <summary>
        /// Gets the wc?points
        /// </summary>
        /// <value>The wc?points</value>
        decimal? WcPoints { get; }

        /// <summary>
        /// Gets the time of the result
        /// </summary>
        /// <value>The time of the result</value>
        string Time { get; }

        /// <summary>
        /// Gets the time ranking
        /// </summary>
        /// <value>The time ranking</value>
        int? TimeRanking { get; }

        /// <summary>
        /// Gets the status of the result
        /// </summary>
        /// <value>The status of the result</value>
        string Status { get; }

        /// <summary>
        /// Gets the status comment
        /// </summary>
        /// <value>The status comment</value>
        string StatusComment { get; }

        /// <summary>
        /// Gets the sprint of the result
        /// </summary>
        /// <value>The sprint of the result</value>
        [Obsolete]
        int? Sprint { get; }

        /// <summary>
        /// Gets the sprint
        /// </summary>
        /// <value>The sprint</value>
        decimal? SprintDecimal { get; }

        /// <summary>
        /// Gets the sprint ranking
        /// </summary>
        /// <value>The sprint ranking</value>
        int? SprintRanking { get; }

        /// <summary>
        /// Gets the climber
        /// </summary>
        /// <value>The climber</value>
        [Obsolete]
        int? Climber { get; }

        /// <summary>
        /// Gets the climber
        /// </summary>
        /// <value>The climber</value>
        decimal? ClimberDecimal { get; }

        /// <summary>
        /// Gets the climber ranking
        /// </summary>
        /// <value>The climber ranking</value>
        int? ClimberRanking { get; }

        /// <summary>
        /// Gets the score of the home competitor competing on the associated sport event
        /// </summary>
        decimal? HomeScore { get; }

        /// <summary>
        /// Gets the score of the away competitor competing on the associated sport event
        /// </summary>
        decimal? AwayScore { get; }

        /// <summary>
        /// Gets the match status
        /// </summary>
        /// <value>The match status</value>
        [Obsolete("Use GetMatchStatusAsync method instead")]
        int MatchStatus { get; }

        /// <summary>
        /// Asynchronously gets the match status
        /// </summary>
        /// <param name="culture">The culture used to get match status id and description</param>
        /// <returns>Returns the match status id and description in selected culture</returns>
        Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture);
    }
}
